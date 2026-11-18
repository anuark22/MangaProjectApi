using Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Services
{
    public class AntiSpamService : IAntiSpamService
    {
        private class LoginAttemptRecord
        {
            public int FailedCount { get; set; }
            public DateTimeOffset FirstAttemptAt { get; set; }
            public DateTimeOffset? BlockedUntil { get; set; }
        }

        private class ContentRecord
        {
            public string ContentHash { get; set; } = string.Empty;
            public DateTimeOffset Timestamp { get; set; }
        }

        // Almacén seguro para intentos de login por clave combinada (IP + Email)
        private readonly ConcurrentDictionary<string, LoginAttemptRecord> _loginAttempts = new(StringComparer.OrdinalIgnoreCase);

        // Almacén para control de frecuencia/cooldown por acción e identificador
        private readonly ConcurrentDictionary<string, DateTimeOffset> _actionTimestamps = new(StringComparer.OrdinalIgnoreCase);

        // Almacén para detección de contenido duplicado reciente
        private readonly ConcurrentDictionary<string, ContentRecord> _lastContents = new(StringComparer.OrdinalIgnoreCase);

        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(5);
        private const int StandardLockoutSeconds = 120; // 2 minutos
        private const int ExtendedLockoutSeconds = 300; // 5 minutos tras 8+ intentos

        private static string BuildLoginKey(string ipAddress, string? email)
        {
            string cleanIp = string.IsNullOrWhiteSpace(ipAddress) ? "unknown_ip" : ipAddress.Trim();
            string cleanEmail = string.IsNullOrWhiteSpace(email) ? "no_email" : email.Trim().ToLowerInvariant();
            return $"login:{cleanIp}:{cleanEmail}";
        }

        public (bool IsBlocked, int SecondsRemaining, int AttemptCount) CheckLoginThrottle(string ipAddress, string? email)
        {
            string key = BuildLoginKey(ipAddress, email);
            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (_loginAttempts.TryGetValue(key, out var record))
            {
                // Si está bloqueado actualmente
                if (record.BlockedUntil.HasValue && record.BlockedUntil.Value > now)
                {
                    int remaining = (int)Math.Ceiling((record.BlockedUntil.Value - now).TotalSeconds);
                    return (true, Math.Max(1, remaining), record.FailedCount);
                }

                // Si ya expiró el bloqueo o la ventana de 5 minutos, reiniciar ventana
                if (now - record.FirstAttemptAt > AttemptWindow)
                {
                    _loginAttempts.TryRemove(key, out _);
                    return (false, 0, 0);
                }

                return (false, 0, record.FailedCount);
            }

            return (false, 0, 0);
        }

        public (bool IsNowBlocked, int SecondsRemaining) RecordFailedLogin(string ipAddress, string? email)
        {
            string key = BuildLoginKey(ipAddress, email);
            DateTimeOffset now = DateTimeOffset.UtcNow;

            var record = _loginAttempts.AddOrUpdate(
                key,
                _ => new LoginAttemptRecord
                {
                    FailedCount = 1,
                    FirstAttemptAt = now,
                    BlockedUntil = null
                },
                (_, existing) =>
                {
                    // Si la ventana expiró, reiniciar conteo
                    if (now - existing.FirstAttemptAt > AttemptWindow)
                    {
                        existing.FailedCount = 1;
                        existing.FirstAttemptAt = now;
                        existing.BlockedUntil = null;
                        return existing;
                    }

                    existing.FailedCount++;

                    // Superó el umbral de intentos fallidos
                    if (existing.FailedCount >= MaxFailedAttempts)
                    {
                        int duration = existing.FailedCount >= 8 ? ExtendedLockoutSeconds : StandardLockoutSeconds;
                        existing.BlockedUntil = now.AddSeconds(duration);
                    }

                    return existing;
                });

            if (record.BlockedUntil.HasValue && record.BlockedUntil.Value > now)
            {
                int remaining = (int)Math.Ceiling((record.BlockedUntil.Value - now).TotalSeconds);
                return (true, Math.Max(1, remaining));
            }

            return (false, 0);
        }

        public void ResetLoginAttempts(string ipAddress, string? email)
        {
            string key = BuildLoginKey(ipAddress, email);
            _loginAttempts.TryRemove(key, out _);
        }

        public bool IsHoneypotTriggered(string? honeypotValue)
        {
            // Un humano normal nunca ve ni llena este campo. Si contiene texto, es un bot.
            return !string.IsNullOrWhiteSpace(honeypotValue);
        }

        public (bool IsRateLimited, int SecondsRemaining) CheckActionCooldown(string actionType, string identifier, int cooldownSeconds = 5)
        {
            string key = $"{actionType.Trim().ToLowerInvariant()}:{identifier.Trim().ToLowerInvariant()}";
            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (_actionTimestamps.TryGetValue(key, out var lastTime))
            {
                TimeSpan elapsed = now - lastTime;
                if (elapsed.TotalSeconds < cooldownSeconds)
                {
                    int remaining = cooldownSeconds - (int)elapsed.TotalSeconds;
                    return (true, Math.Max(1, remaining));
                }
            }

            return (false, 0);
        }

        public void RecordAction(string actionType, string identifier)
        {
            string key = $"{actionType.Trim().ToLowerInvariant()}:{identifier.Trim().ToLowerInvariant()}";
            _actionTimestamps[key] = DateTimeOffset.UtcNow;
        }

        public bool IsDuplicateContent(string userIdentifier, string content, int windowSeconds = 60)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;

            string key = $"content:{userIdentifier.Trim().ToLowerInvariant()}";
            string hash = ComputeHash(content.Trim());
            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (_lastContents.TryGetValue(key, out var existing))
            {
                if (string.Equals(existing.ContentHash, hash, StringComparison.Ordinal) && (now - existing.Timestamp).TotalSeconds < windowSeconds)
                {
                    return true;
                }
            }

            _lastContents[key] = new ContentRecord
            {
                ContentHash = hash,
                Timestamp = now
            };

            return false;
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
