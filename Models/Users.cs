using System.ComponentModel.DataAnnotations;

namespace Models
{

    public class Users
    {
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? RoleId { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
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
        public string Password { get; set; }
        public int? RoleId { get; set; }
        public string? Token { get; set; }
        public int? IsBlocked { get; set; }
    }
    public class ChangePassword
    {
        public string? Password { get; set; }
        public string NewPassword { get; set; }

    }
    public class StartRecoveryPassword
    {
        [EmailAddress]
        public string Username { get; set; }
    }
    public class SendEmail
    {
        public string EmailDestiny { get; set; }
    }
    public class RecoveryPasswordValitationToken
    {
        public string Username { get; set; }
        public string TokenRecovery { get; set; }
    }
    public class RecoveryPassword
    {
        public string Username { get; set; }
        public string TokenRecovery { get; set; }
        public string Password { get; set; }
    }
    public class SetRole
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }
    }
}
