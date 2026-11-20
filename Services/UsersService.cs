using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Services.Interfaces;
using Models;

namespace Services
{
    public class UsersService : IUsersService
    {
        private readonly ICommonService _commonService;
        private readonly IConfiguration _config;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IImageStorageService _imageStorageService;

        public UsersService(
            ICommonService commonService, 
            IJwtService jwtService, 
            IConfiguration config,
            IEmailService emailService,
            IImageStorageService imageStorageService)
        {
            _commonService = commonService;
            _jwtService = jwtService;
            _config = config;
            _emailService = emailService;
            _imageStorageService = imageStorageService;
        }

        public Response<UsersDTO> Login(Models.UsersDTO model, bool recaptchaCheck = true, double hours = 8)
        {
            Response<UsersDTO> response = new();
            string identifier = (model.Email ?? model.Username ?? string.Empty).Trim();
            object param = new { Identifier = identifier };
            var query = "SELECT * FROM Users WHERE Email = @Identifier OR Username = @Identifier";
            List<UsersDTO> u = _commonService.Query<UsersDTO>(query, param).ToList();

            if (u.Count == 0)
            {
                response.ParseExecution(404, "User Not Found");
                return response;
            }

            UsersDTO user = u.First();

            if (user.IsBlocked == 1)
            {
                response.ParseExecution(403, "User Banned");
                return response;
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                response.ParseExecution(403, "Wrong Password");
                return response;
            }

            u.ForEach(us =>
            {
                us.Token = _jwtService.Generate(us);
                us.Password = null;
            });

            response.Items = u;
            response.ParseResponse();
            return response;
        }

        public async Task<Response<UsersDTO>> GoogleAuthAsync(string idToken)
        {
            Response<UsersDTO> response = new();
            if (string.IsNullOrWhiteSpace(idToken))
            {
                response.ParseExecution(400, "Token de Google inválido.");
                return response;
            }

            string? email = null;
            string? displayName = null;
            string? aud = null;
            string? emailVerified = null;
            string? iss = null;

            try
            {
                using var http = new HttpClient();
                var res = await http.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}");
                if (!res.IsSuccessStatusCode)
                {
                    response.ParseExecution(401, "El token de Google no es válido o ha expirado.");
                    return response;
                }

                string json = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                email = root.TryGetProperty("email", out var pEmail) ? pEmail.GetString() : null;
                displayName = root.TryGetProperty("name", out var pName) ? pName.GetString() : null;
                aud = root.TryGetProperty("aud", out var pAud) ? pAud.GetString() : null;
                emailVerified = root.TryGetProperty("email_verified", out var pVer) ? pVer.ToString() : null;
                iss = root.TryGetProperty("iss", out var pIss) ? pIss.GetString() : null;
            }
            catch (Exception ex)
            {
                response.ParseExecution(500, $"Error validando cuenta con Google: {ex.Message}");
                return response;
            }

            string? configuredClientId = _config["GoogleAuth:ClientId"];
            if (!string.IsNullOrWhiteSpace(configuredClientId) && !string.Equals(aud, configuredClientId.Trim(), StringComparison.Ordinal))
            {
                response.ParseExecution(403, "El Client ID del token de Google no coincide con la aplicación.");
                return response;
            }

            if (!string.IsNullOrWhiteSpace(iss) && iss != "https://accounts.google.com" && iss != "accounts.google.com")
            {
                response.ParseExecution(403, "Emisor del token de Google no autorizado.");
                return response;
            }

            if (string.IsNullOrWhiteSpace(email) || !string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
            {
                response.ParseExecution(403, "Tu cuenta de Gmail debe tener el correo verificado.");
                return response;
            }

            // Buscar si el usuario ya existe por Email
            List<UsersDTO> existing = _commonService.Query<UsersDTO>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = email.Trim() }
            ).ToList();

            if (existing.Count == 0)
            {
                // Generar Username único a partir del nombre de Google o prefijo del email
                string rawBase = !string.IsNullOrWhiteSpace(displayName)
                    ? displayName
                    : email.Split('@')[0];
                string sanitized = Regex.Replace(rawBase, @"[^a-zA-Z0-9_]", "");
                if (sanitized.Length < 3) sanitized = "reader_" + sanitized;
                if (sanitized.Length > 20) sanitized = sanitized.Substring(0, 20);

                string candidateUsername = sanitized;
                int attempts = 0;
                while (attempts < 5)
                {
                    var dup = _commonService.Query<int>(
                        "SELECT COUNT(1) FROM Users WHERE Username = @Username",
                        new { Username = candidateUsername }
                    ).FirstOrDefault();

                    if (dup == 0) break;
                    attempts++;
                    candidateUsername = $"{sanitized}_{RandomNumberGenerator.GetInt32(1000, 9999)}";
                }

                string randomPassHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N") + "G!");
                var dbRes = await _commonService.QueryAsync<DBClass>(
                    "UsersProcedures",
                    new
                    {
                        Action = 2,
                        Username = candidateUsername,
                        Email = email.Trim(),
                        Password = randomPassHash,
                        AceptTerms = true
                    },
                    CommandType.StoredProcedure
                );

                var first = dbRes.FirstOrDefault();
                if (first == null || first.Status != 200)
                {
                    response.ParseExecution(first?.Status ?? 500, first?.ErrorDescription ?? "No se pudo registrar la cuenta de Google.");
                    return response;
                }

                existing = _commonService.Query<UsersDTO>(
                    "SELECT * FROM Users WHERE Email = @Email",
                    new { Email = email.Trim() }
                ).ToList();
            }

            if (existing.Count == 0)
            {
                response.ParseExecution(500, "No se pudo recuperar el usuario tras autenticar con Google.");
                return response;
            }

            UsersDTO user = existing.First();
            if (user.IsBlocked == 1)
            {
                response.ParseExecution(403, "User Banned");
                return response;
            }

            existing.ForEach(us =>
            {
                us.Token = _jwtService.Generate(us);
                us.Password = null;
            });

            response.Items = existing;
            response.ParseResponse();
            return response;
        }

        public async Task<Response<Users>> InsertRegister(Models.Users model)
        {
            Response<Users> response = new();
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            int Action = 2;
            object param = new { Action, model.Username, model.Password, model.AceptTerms, model.Email };
            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("UsersProcedures", param, CommandType.StoredProcedure);
            var first = dbResponse.FirstOrDefault();
            if (first != null)
            {
                response.ParseExecution(first.Status, first.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Database error");
            }
            return response;
        }

        public async Task<Response<Users>> UploadAvatarAsync(IFormFile file, int userId)
        {
            Response<Users> response = new();
            if (file == null || file.Length == 0)
            {
                response.ParseExecution(400, "Debes seleccionar una imagen válida.");
                return response;
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                response.ParseExecution(400, "La imagen no puede superar los 5 MB.");
                return response;
            }

            string fileName = await _imageStorageService.SaveImageAsWebpAsync(file, "Avatars", 85);
            string baseUrl = (_config["DomainSettings:ImageUrl"] ?? "https://localhost:44311/Files/").TrimEnd('/') + "/";
            string avatarUrl = $"{baseUrl}Avatars/{fileName}";

            var dbRes = await _commonService.QueryAsync<DBClass>(
                "UsersProcedures",
                new { Action = 8, UserId = userId, AvatarUrl = avatarUrl },
                CommandType.StoredProcedure
            );

            var first = dbRes.FirstOrDefault();
            if (first != null && first.Status != 200)
            {
                response.ParseExecution(first.Status, first.ErrorDescription);
                return response;
            }

            response.Items = new List<Users>
            {
                new Users { UserId = userId, AvatarUrl = avatarUrl }
            };
            response.ParseExecution(200, "Foto de perfil actualizada con éxito.");
            return response;
        }

        public Response<Users> GetUsers(int? userId)
        {
            Response<Users> response = new();
            int Action = 1;
            object param = new { Action, userId };
            response.Items = _commonService.Query<Users>("UsersProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);
            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, x =>
            {
                x.Password = string.Empty;
                x.RecoveryToken = null;
            });
            return response;
        }

        public Response<UserProfileDTO> GetUserProfile(int userId)
        {
            Response<UserProfileDTO> response = new();
            int action = 1;
            object param = new { Action = action, UserId = userId };

            var users = _commonService.Query<Users>("UsersProcedures", param, CommandType.StoredProcedure).ToList();
            var user = users.FirstOrDefault();

            if (user == null)
            {
                response.ParseExecution(404, "Usuario no encontrado");
                return response;
            }

            UserProfileDTO profile = new()
            {
                UserId = user.UserId ?? 0,
                Username = user.Username ?? string.Empty,
                Email = user.Email,
                RoleId = user.RoleId ?? 1,
                RoleName = user.RoleName ?? "CLIENTE",
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = user.UpdatedAt
            };

            response.Items = new List<UserProfileDTO> { profile };
            response.ParseResponse(true);
            return response;
        }

        public async Task<Response<Users>> UpdateProfile(UpdateProfileDTO model, int userId)
        {
            Response<Users> response = new();
            int action = 7;
            object param = new
            {
                Action = action,
                UserId = userId,
                model.Username,
                model.Email
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("UsersProcedures", param, CommandType.StoredProcedure);
            var first = dbResponse.FirstOrDefault();
            if (first != null)
            {
                response.ParseExecution(first.Status, first.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Database error");
            }
            return response;
        }

        public async Task<Response<Users>> ChangePassword(ChangePasswordDTO model, int userId)
        {
            Response<Users> response = new();

            // 1. Obtener datos actuales del usuario con su hash de contraseña
            int action = 1;
            object paramSelect = new { Action = action, UserId = userId };
            var users = _commonService.Query<Users>("UsersProcedures", paramSelect, CommandType.StoredProcedure).ToList();
            var user = users.FirstOrDefault();

            if (user == null)
            {
                response.ParseExecution(404, "Usuario no encontrado");
                return response;
            }

            // 2. Verificar que la contraseña actual sea correcta
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                response.ParseExecution(400, "La contraseña actual es incorrecta");
                return response;
            }

            // 3. Hashear y guardar la nueva contraseña
            string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            int actionUpdate = 6;
            object paramUpdate = new
            {
                Action = actionUpdate,
                UserId = userId,
                Password = hashedNewPassword
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("UsersProcedures", paramUpdate, CommandType.StoredProcedure);
            var first = dbResponse.FirstOrDefault();
            if (first != null)
            {
                response.ParseExecution(first.Status, first.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Database error");
            }
            return response;
        }

        public async Task<Response<Users>> ForgotPassword(ForgotPasswordRequestDTO model)
        {
            Response<Users> response = new();

            // 1. Buscar si el correo existe
            int actionSelect = 1;
            object paramSelect = new { Action = actionSelect, Email = model.Email.Trim() };
            var users = _commonService.Query<Users>("UsersProcedures", paramSelect, CommandType.StoredProcedure).ToList();
            var user = users.FirstOrDefault();

            if (user == null)
            {
                // Mensaje seguro estándar: no revelar si el correo existe o no
                response.ParseExecution(200, "Si el correo está registrado en la plataforma, recibirás las instrucciones para restablecer tu contraseña.");
                return response;
            }

            // 2. Generar token seguro alfanumérico
            string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            DateTime expiration = DateTime.UtcNow.AddMinutes(30);

            // 3. Guardar el token en la base de datos
            int actionToken = 3;
            object paramToken = new
            {
                Action = actionToken,
                Email = user.Email,
                RecoveryToken = token,
                RecoveryTokenExpiration = expiration
            };

            await _commonService.QueryAsync<DBClass>("UsersProcedures", paramToken, CommandType.StoredProcedure);

            // 4. Enviar correo de recuperación
            bool emailSent = await _emailService.SendPasswordRecoveryEmail(user.Email, user.Username ?? "Usuario", token);

            if (emailSent)
            {
                response.ParseExecution(200, "Se ha enviado un correo con las instrucciones para restablecer tu contraseña.");
            }
            else
            {
                response.ParseExecution(500, "Error al enviar el correo de recuperación. Por favor intenta nuevamente más tarde.");
            }

            return response;
        }

        public Response<Users> ValidateRecoveryToken(ValidateTokenDTO model)
        {
            Response<Users> response = new();
            int action = 4;
            object param = new
            {
                Action = action,
                Email = model.Email.Trim(),
                RecoveryToken = model.Token.Trim()
            };

            var dbResponse = _commonService.Query<DBClass>("UsersProcedures", param, CommandType.StoredProcedure).ToList();
            var first = dbResponse.FirstOrDefault();
            if (first != null)
            {
                response.ParseExecution(first.Status, first.ErrorDescription);
            }
            else
            {
                response.ParseExecution(400, "Token inválido o expirado");
            }
            return response;
        }

        public async Task<Response<Users>> ResetPassword(ResetPasswordDTO model)
        {
            Response<Users> response = new();

            // Hashear nueva contraseña
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            int action = 5;
            object param = new
            {
                Action = action,
                Email = model.Email.Trim(),
                RecoveryToken = model.Token.Trim(),
                Password = hashedPassword
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("UsersProcedures", param, CommandType.StoredProcedure);
            var first = dbResponse.FirstOrDefault();
            if (first != null)
            {
                response.ParseExecution(first.Status, first.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Database error");
            }
            return response;
        }

        public async Task<Response<DBClass>> UpdateUserRole(UpdateUserRoleDTO model, int adminUserId)
        {
            Response<DBClass> response = new();

            // Evitar que el administrador se quite su propio rol de administrador accidentalmente
            if (model.UserId == adminUserId && model.RoleId != (int)RolesEnum.Admin)
            {
                response.ParseExecution(400, "No puedes revocar tu propio rol de administrador.");
                return response;
            }

            int action = 5;
            object param = new
            {
                Action = action,
                model.UserId,
                model.RoleId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("RolesProcedures", param, CommandType.StoredProcedure);
            var first = dbResponse.FirstOrDefault();
            if (first != null)
            {
                response.ParseExecution(first.Status, first.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Database error");
            }

            return response;
        }
    }
}
