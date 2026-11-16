using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
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

        public UsersService(
            ICommonService commonService, 
            IJwtService jwtService, 
            IConfiguration config,
            IEmailService emailService)
        {
            _commonService = commonService;
            _jwtService = jwtService;
            _config = config;
            _emailService = emailService;
        }

        public Response<UsersDTO> Login(Models.UsersDTO model, bool recaptchaCheck = true, double hours = 8)
        {
            Response<UsersDTO> response = new();
            object param = new { model.Email };
            var query = "SELECT * FROM Users WHERE Email = @Email";
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
