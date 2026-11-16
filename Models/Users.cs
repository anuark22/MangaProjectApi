using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Users
    {
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RecoveryToken { get; set; }
        public DateTime? RecoveryTokenExpiration { get; set; }
        public bool? AceptTerms { get; set; }
        public int? IsBlocked { get; set; }
        public int? ScanId { get; set; }
        public string? Token { get; set; }
        public string? RoleName { get; set; } = string.Empty;
        public int? IsOwnerRole { get; set; }
        public int? PaginationCount { get; set; } = null;
    }

    public class UsersDTO
    {
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }
        public string? Token { get; set; }
        public int? IsBlocked { get; set; }
    }

    public class UserProfileDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateProfileDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmPassword { get; set; }
    }

    public class ForgotPasswordRequestDTO
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;
    }

    public class ValidateTokenDTO
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token de recuperación es requerido")]
        public string Token { get; set; } = string.Empty;
    }

    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token de recuperación es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmPassword { get; set; }
    }

    // Clases legacy conservadas para retrocompatibilidad
    public class ChangePassword
    {
        public string? Password { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }

    public class StartRecoveryPassword
    {
        [EmailAddress]
        public string Username { get; set; } = string.Empty;
    }

    public class SendEmail
    {
        public string EmailDestiny { get; set; } = string.Empty;
    }

    public class RecoveryPasswordValitationToken
    {
        public string Username { get; set; } = string.Empty;
        public string TokenRecovery { get; set; } = string.Empty;
    }

    public class RecoveryPassword
    {
        public string Username { get; set; } = string.Empty;
        public string TokenRecovery { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SetRole
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }
    }
}
