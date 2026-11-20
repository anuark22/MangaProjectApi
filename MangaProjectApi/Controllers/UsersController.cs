using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;
using MangaProjectApi.CustomMiddleware;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MangaProjectApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UsersController : HandlerController
    {
        private readonly IUsersService _usersService;
        private readonly IAntiSpamService _antiSpamService;

        public UsersController(IUsersService usersService, IAntiSpamService antiSpamService)
        {
            _usersService = usersService;
            _antiSpamService = antiSpamService;
        }

        private string GetClientIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded) && !string.IsNullOrWhiteSpace(forwarded))
            {
                return forwarded.ToString().Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        [HttpGet]
        [AdminOnly]
        public IActionResult GetUsers(int? userId)
        {
            Response<Users> users = _usersService.GetUsers(userId);
            return ResponseHandler(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Users model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Detección de bot automatizado mediante campo trampa honeypot
            if (_antiSpamService.IsHoneypotTriggered(model.Honeypot))
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = "Solicitud rechazada." });
            }

            string clientIp = GetClientIpAddress();

            // Validación de Google reCAPTCHA v3 (si está habilitado en configuración)
            var (recaptchaValid, recaptchaScore, recaptchaError) = await _antiSpamService.ValidateRecaptchaV3Async(model.RecaptchaToken, "register", clientIp);
            if (!recaptchaValid)
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = recaptchaError ?? "Verificación de reCAPTCHA fallida." });
            }

            // Flood control: máximo 1 registro cada 10 segundos por IP
            var (isRateLimited, secondsRemaining) = _antiSpamService.CheckActionCooldown("register", clientIp, 10);
            if (isRateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    Status = 429,
                    Error = true,
                    SecondsRemaining = secondsRemaining,
                    ErrorDescription = $"Por favor espera {secondsRemaining} segundos antes de enviar otro registro."
                });
            }

            _antiSpamService.RecordAction("register", clientIp);

            Response<Users> users = await _usersService.InsertRegister(model);
            return ResponseHandler(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Models.UsersDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Detección de bot automatizado mediante campo trampa honeypot
            if (_antiSpamService.IsHoneypotTriggered(model.Honeypot))
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = "Solicitud rechazada." });
            }

            string clientIp = GetClientIpAddress();

            // Validación de Google reCAPTCHA v3 (si está habilitado en configuración)
            var (recaptchaValid, recaptchaScore, recaptchaError) = await _antiSpamService.ValidateRecaptchaV3Async(model.RecaptchaToken, "login", clientIp);
            if (!recaptchaValid)
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = recaptchaError ?? "Verificación de reCAPTCHA fallida." });
            }

            // Protección de fuerza bruta / Bloqueo temporal de intentos de login
            var (isBlocked, secondsRemaining, attemptCount) = _antiSpamService.CheckLoginThrottle(clientIp, model.Email);
            if (isBlocked)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    Status = 429,
                    Error = true,
                    SecondsRemaining = secondsRemaining,
                    ErrorDescription = $"Demasiados intentos fallidos. Tu acceso está temporalmente pausado por seguridad. Intenta de nuevo en {secondsRemaining} segundos."
                });
            }

            Response<UsersDTO> users = _usersService.Login(model);

            // Verificación del resultado del login
            if (users.Status != 200 || users.Error || users.Items == null || !users.Items.Any())
            {
                var (isNowBlocked, lockoutSeconds) = _antiSpamService.RecordFailedLogin(clientIp, model.Email);
                if (isNowBlocked)
                {
                    return StatusCode(StatusCodes.Status429TooManyRequests, new
                    {
                        Status = 429,
                        Error = true,
                        SecondsRemaining = lockoutSeconds,
                        ErrorDescription = $"Has superado el número máximo de intentos permitidos. Por seguridad, tu acceso ha sido suspendido por {lockoutSeconds} segundos."
                    });
                }
            }
            else
            {
                // Login exitoso: restablecer intentos
                _antiSpamService.ResetLoginAttempts(clientIp, model.Email);
            }

            return ResponseHandler(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_antiSpamService.IsHoneypotTriggered(model.Honeypot))
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = "Solicitud rechazada." });
            }

            string clientIp = GetClientIpAddress();
            var (isRateLimited, secondsRemaining) = _antiSpamService.CheckActionCooldown("google_auth", clientIp, 3);
            if (isRateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    Status = 429,
                    Error = true,
                    SecondsRemaining = secondsRemaining,
                    ErrorDescription = $"Por favor espera {secondsRemaining} segundos antes de reintentar."
                });
            }

            _antiSpamService.RecordAction("google_auth", clientIp);

            Response<UsersDTO> response = await _usersService.GoogleAuthAsync(model.IdToken);
            return ResponseHandler(response);
        }

        [HttpGet]
        public IActionResult GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<UserProfileDTO> response = _usersService.GetUserProfile(userId);
            return ResponseHandler(response);
        }

        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDTO model)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                Status = 403,
                Error = true,
                ErrorDescription = "El nombre de usuario y el correo electrónico no pueden ser modificados."
            });
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<Users> response = await _usersService.UploadAvatarAsync(file, userId);
            return ResponseHandler(response);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { Status = 401, ErrorDescription = "Token inválido o usuario no identificado." });
            }

            Response<Users> response = await _usersService.ChangePassword(model, userId);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_antiSpamService.IsHoneypotTriggered(model.Honeypot))
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = "Solicitud rechazada." });
            }

            string clientIp = GetClientIpAddress();

            // Validación de Google reCAPTCHA v3
            var (recaptchaValid, recaptchaScore, recaptchaError) = await _antiSpamService.ValidateRecaptchaV3Async(model.RecaptchaToken, "forgot_password", clientIp);
            if (!recaptchaValid)
            {
                return BadRequest(new { Status = 400, Error = true, ErrorDescription = recaptchaError ?? "Verificación de reCAPTCHA fallida." });
            }

            string spamKey = $"{clientIp}:{model.Email?.Trim().ToLowerInvariant()}";
            var (isRateLimited, secondsRemaining) = _antiSpamService.CheckActionCooldown("forgot_password", spamKey, 30);
            if (isRateLimited)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new
                {
                    Status = 429,
                    Error = true,
                    SecondsRemaining = secondsRemaining,
                    ErrorDescription = $"Por favor espera {secondsRemaining} segundos antes de solicitar otro correo de recuperación."
                });
            }

            _antiSpamService.RecordAction("forgot_password", spamKey);

            Response<Users> response = await _usersService.ForgotPassword(model);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ValidateRecoveryToken([FromBody] ValidateTokenDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Users> response = _usersService.ValidateRecoveryToken(model);
            return ResponseHandler(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response<Users> response = await _usersService.ResetPassword(model);
            return ResponseHandler(response);
        }

        [HttpPut]
        [AdminOnly]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            int adminUserId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int id) ? id : 0;

            Response<DBClass> response = await _usersService.UpdateUserRole(model, adminUserId);
            return ResponseHandler(response);
        }
    }
}
