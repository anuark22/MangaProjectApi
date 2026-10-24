using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Models;
using Services.Interfaces;

namespace Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string Generate(UsersDTO user, double hours = 8)
        {
            var rawKey = _config["Jwt:Key"]?.Trim();
            if (string.IsNullOrEmpty(rawKey))
                throw new ArgumentNullException("Jwt:Key", "La clave JWT no está configurada en appsettings.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Normalizamos emisor y audiencia limpiando espacios
            string issuer = _config["Jwt:Issuer"]?.Trim() ?? "";
            string audience = _config["Jwt:Audience"]?.Trim() ?? "";

            var claims = new[]
            {
                new Claim("sub", user.UserId.ToString()),
                new Claim("unique_name", user.Username),
                new Claim("email", user.Email ?? ""),
                new Claim("role", user.RoleId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(hours), // Se recomienda UtcNow para evitar problemas de zona horaria del servidor
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public UsersDTO? Validate(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var rawKey = _config["Jwt:Key"]?.Trim();
                if (string.IsNullOrEmpty(rawKey)) return null;

                var key = Encoding.UTF8.GetBytes(rawKey);

                string configuredIssuer = _config["Jwt:Issuer"]?.Trim() ?? "";
                string configuredAudience = _config["Jwt:Audience"]?.Trim() ?? "";

                // Asegurar variantes con y sin barra diagonal para el emisor
                var validIssuers = new List<string>
                {
                    configuredIssuer,
                    configuredIssuer.EndsWith("/") ? configuredIssuer.TrimEnd('/') : configuredIssuer + "/"
                };

                // Asegurar variantes con y sin barra diagonal para la audiencia
                var validAudiences = new List<string>
                {
                    configuredAudience,
                    configuredAudience.EndsWith("/") ? configuredAudience.TrimEnd('/') : configuredAudience + "/"
                };

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateIssuer = true,
                    ValidIssuers = validIssuers,

                    ValidateAudience = true,
                    ValidAudiences = validAudiences,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extraemos los claims usando los identificadores JWT estándar
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
                var usernameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;
                var emailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
                var roleClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "role")?.Value;

                if (userIdClaim == null || usernameClaim == null || roleClaim == null)
                {
                    Console.WriteLine("DEBUG: Faltan claims requeridos en el token decodificado.");
                    return null;
                }

                return new UsersDTO
                {
                    UserId = int.Parse(userIdClaim),
                    Username = usernameClaim,
                    Email = emailClaim ?? "",
                    RoleId = int.Parse(roleClaim)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG (Excepción en Validación): {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"DEBUG Inner (Excepción en Validación): {ex.InnerException.Message}");
                }
                return null;
            }
        }

        public Users Authenticate(Users userLogin)
        {
            return null;
        }

        public bool ValidateUser(int userId, UsersDTO requestingUser)
        {
            if (requestingUser == null || (requestingUser.UserId != userId && requestingUser.RoleId == (int)RolesEnum.Client))
                return false;
            return true;
        }

        public bool ValidateRecaptcha(string response)
        {
            using (var client = new WebClient())
            {
                var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", _config["Captcha:Secret"], response));
                var obj = JObject.Parse(result);
                return (bool)obj["success"];
            }
        }
    }
}