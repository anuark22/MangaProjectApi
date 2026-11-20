using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaProjectApi.Hubs
{
    public class ChatMessageDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Hub SignalR (WebSockets) para el Chat en Vivo de la comunidad.
    /// Mantiene historial reciente en memoria (ring buffer de 60 mensajes), contador de usuarios conectados
    /// y protección anti-spam individual por usuario.
    /// </summary>
    public class LiveChatHub : Hub
    {
        private readonly IAntiSpamService _antiSpamService;

        // Ring buffer en memoria de los últimos 60 mensajes globales
        private static readonly ConcurrentQueue<ChatMessageDto> _recentMessages = new();
        private static readonly ConcurrentDictionary<string, string> _connectedClients = new();
        private const int MaxHistoryCount = 60;

        public LiveChatHub(IAntiSpamService antiSpamService)
        {
            _antiSpamService = antiSpamService;
        }

        public override async Task OnConnectedAsync()
        {
            _connectedClients[Context.ConnectionId] = Context.User?.Identity?.Name ?? "Guest";

            // Enviar historial reciente y contador de usuarios en línea al cliente recién conectado
            await Clients.Caller.SendAsync("ReceiveHistory", _recentMessages.ToList());
            await Clients.All.SendAsync("OnlineCountUpdated", _connectedClients.Count);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _connectedClients.TryRemove(Context.ConnectionId, out _);
            await Clients.All.SendAsync("OnlineCountUpdated", _connectedClients.Count);

            await base.OnDisconnectedAsync(exception);
        }

        [Authorize]
        public async Task SendMessage(string text, string? mediaUrl = null)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)
                ?? Context.User?.FindFirst("sub")
                ?? Context.User?.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new HubException("Debes iniciar sesión para enviar mensajes al chat.");
            }

            string cleanText = (text ?? string.Empty).Trim();
            string? cleanMedia = string.IsNullOrWhiteSpace(mediaUrl) ? null : mediaUrl.Trim();

            if (string.IsNullOrEmpty(cleanText) && string.IsNullOrEmpty(cleanMedia))
            {
                throw new HubException("El mensaje no puede estar vacío.");
            }

            if (cleanText.Length > 500)
            {
                cleanText = cleanText.Substring(0, 500);
            }

            if (cleanMedia != null)
            {
                if (!Uri.TryCreate(cleanMedia, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new HubException("La URL de imagen o GIF debe ser http:// o https://");
                }
            }

            // Anti-spam individual por usuario: cooldown de 2 segundos entre mensajes
            var (isRateLimited, secondsRemaining) = _antiSpamService.CheckActionCooldown("live_chat", userId.ToString(), 2);
            if (isRateLimited)
            {
                throw new HubException($"Espera {secondsRemaining}s antes de enviar otro mensaje.");
            }

            _antiSpamService.RecordAction("live_chat", userId.ToString());

            string username = Context.User?.FindFirst("unique_name")?.Value
                ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value
                ?? Context.User?.Identity?.Name
                ?? "Lector";

            string email = Context.User?.FindFirst("email")?.Value
                ?? Context.User?.FindFirst(ClaimTypes.Email)?.Value
                ?? string.Empty;

            int.TryParse(Context.User?.FindFirst("role")?.Value ?? Context.User?.FindFirst(ClaimTypes.Role)?.Value, out int roleId);

            var msg = new ChatMessageDto
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                Username = username,
                Email = email,
                RoleId = roleId,
                Text = cleanText,
                MediaUrl = cleanMedia,
                CreatedAt = DateTime.UtcNow
            };

            _recentMessages.Enqueue(msg);
            while (_recentMessages.Count > MaxHistoryCount)
            {
                _recentMessages.TryDequeue(out _);
            }

            await Clients.All.SendAsync("ReceiveMessage", msg);
        }
    }
}
