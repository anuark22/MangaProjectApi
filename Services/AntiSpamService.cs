using Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Services
{
    /// <summary>
    /// Servicio de protección anti-spam y limitación de tasa diseñado para alto tráfico concurrente.
    /// Opera de forma estrictamente individual (por usuario/cuenta/IP) sin bloquear a otros usuarios legítimos.
    /// Implementa recolección y poda periódica automática para mantener el consumo de memoria en niveles mínimos.
    /// </summary>
    public class AntiSpamService : IAntiSpamService, IDisposable
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

        // Almacén seguro para intentos de login por clave combinada INDIVIDUAL (IP + Email)
        private readonly ConcurrentDictionary<string, LoginAttemptRecord> _loginAttempts = new(StringComparer.OrdinalIgnoreCase);

        // Almacén para control de frecuencia/cooldown por acción e identificador INDIVIDUAL (UserId o IP)
        private readonly ConcurrentDictionary<string, DateTimeOffset> _actionTimestamps = new(StringComparer.OrdinalIgnoreCase);

        // Almacén para detección de contenido duplicado reciente por usuario INDIVIDUAL
        private readonly ConcurrentDictionary<string, ContentRecord> _lastContents = new(StringComparer.OrdinalIgnoreCase);

        // Temporizador en segundo plano para limpiar periódicamente registros expirados
        private readonly Timer _cleanupTimer;
        private bool _isDisposed;

        // Umbrales de seguridad individuales
        private const int MaxFailedAttemptsPerAccount = 5;
        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(5);
        private const int StandardLockoutSeconds = 120; // 2 minutos
        private const int ExtendedLockoutSeconds = 300; // 5 minutos tras 8+ intentos
        private const int MaxEntriesThreshold = 50000;  // Techo máximo de seguridad contra ataques de memoria

        public AntiSpamService()
        {
            // Ejecutar poda de memoria cada 2 minutos en segundo plano
            _cleanupTimer = new Timer(PruneExpiredRecords, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        /// <summary>
        /// Genera una clave individual combinando la IP del cliente y el correo destino.
        /// Esto garantiza que si un usuario falla en una red compartida (universidad, oficina, WiFi),
        /// los demás usuarios en esa misma red NO sean afectados para sus propias cuentas.
        /// </summary>
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
                // Si la cuenta individual en esta IP está actualmente bloqueada
                if (record.BlockedUntil.HasValue && record.BlockedUntil.Value > now)
                {
                    int remaining = (int)Math.Ceiling((record.BlockedUntil.Value - now).TotalSeconds);
                    return (true, Math.Max(1, remaining), record.FailedCount);
                }

                // Si ya expiró la ventana de 5 minutos, reiniciar ventana individual
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

            // Protección de saturación global de memoria si supera el umbral de registros
            if (_loginAttempts.Count > MaxEntriesThreshold)
            {
                PruneExpiredRecords(null);
            }

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

                    // Superó el umbral de intentos fallidos individuales
                    if (existing.FailedCount >= MaxFailedAttemptsPerAccount)
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
            // Evaluación puramente individual por petición HTTP. Si contiene texto, es un bot.
            return !string.IsNullOrWhiteSpace(honeypotValue);
        }

        public (bool IsRateLimited, int SecondsRemaining) CheckActionCooldown(string actionType, string identifier, int cooldownSeconds = 5)
        {
            // Cooldown estrictamente individual por UserId (o IP en anonimo). Nunca afecta a otros usuarios.
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

            // Comparación de contenido duplicado estrictamente por usuario individual
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

        /// <summary>
        /// Tarea de mantenimiento que corre cada 2 minutos en segundo plano.
        /// Poda cualquier registro cuya ventana o tiempo de cooldown ya haya expirado,
        /// manteniendo el uso de memoria RAM mínimo sin importar cuántos miles de usuarios concurran.
        /// </summary>
        private void PruneExpiredRecords(object? state)
        {
            try
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                // 1. Poda de intentos de login expirados
                foreach (var kvp in _loginAttempts)
                {
                    bool isLocked = kvp.Value.BlockedUntil.HasValue && kvp.Value.BlockedUntil.Value > now;
                    bool isWindowActive = (now - kvp.Value.FirstAttemptAt) <= AttemptWindow;

                    if (!isLocked && !isWindowActive)
                    {
                        _loginAttempts.TryRemove(kvp.Key, out _);
                    }
                }

                // 2. Poda de timestamps de cooldown (> 5 minutos)
                foreach (var kvp in _actionTimestamps)
                {
                    if ((now - kvp.Value).TotalMinutes > 5)
                    {
                        _actionTimestamps.TryRemove(kvp.Key, out _);
                    }
                }

                // 3. Poda de hashes de contenido reciente (> 5 minutos)
                foreach (var kvp in _lastContents)
                {
                    if ((now - kvp.Value.Timestamp).TotalMinutes > 5)
                    {
                        _lastContents.TryRemove(kvp.Key, out _);
                    }
                }
            }
            catch
            {
                // Evita que excepciones en segundo plano afecten la estabilidad del servidor
            }
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _cleanupTimer?.Dispose();
                _isDisposed = true;
            }
        }
    }
}
