using System;

namespace Services.Interfaces
{
    public interface IAntiSpamService
    {
        /// <summary>
        /// Verifica si la IP o la cuenta está bloqueada temporalmente por exceso de intentos de login fallidos.
        /// </summary>
        (bool IsBlocked, int SecondsRemaining, int AttemptCount) CheckLoginThrottle(string ipAddress, string? email);

        /// <summary>
        /// Registra un intento fallido de login. Si supera el umbral, bloquea temporalmente.
        /// </summary>
        (bool IsNowBlocked, int SecondsRemaining) RecordFailedLogin(string ipAddress, string? email);

        /// <summary>
        /// Restablece el contador de intentos fallidos tras un login exitoso.
        /// </summary>
        void ResetLoginAttempts(string ipAddress, string? email);

        /// <summary>
        /// Comprueba si el campo trampa (honeypot) fue completado por un bot automatizado.
        /// </summary>
        bool IsHoneypotTriggered(string? honeypotValue);

        /// <summary>
        /// Comprueba si el usuario está enviando acciones (comentarios, posts, registros) demasiado rápido (Flood control).
        /// </summary>
        (bool IsRateLimited, int SecondsRemaining) CheckActionCooldown(string actionType, string identifier, int cooldownSeconds = 5);

        /// <summary>
        /// Registra la marca de tiempo de la última acción del usuario para el control de cooldown.
        /// </summary>
        void RecordAction(string actionType, string identifier);

        /// <summary>
        /// Comprueba si el usuario está enviando exactamente el mismo contenido repetido en una ventana de tiempo corta.
        /// </summary>
        bool IsDuplicateContent(string userIdentifier, string content, int windowSeconds = 60);

        /// <summary>
        /// Valida el token de Google reCAPTCHA v3 contra la API de Google.
        /// Retorna éxito si reCAPTCHA está deshabilitado en configuración (entorno local).
        /// </summary>
        System.Threading.Tasks.Task<(bool IsValid, double Score, string? ErrorMessage)> ValidateRecaptchaV3Async(string? token, string? expectedAction = null, string? clientIp = null);
    }
}
