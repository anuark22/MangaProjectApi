using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Roles
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
        public int IsActive { get; set; }
        public int IsOwnerRole { get; set; }
        public bool IsDefaultRole { get; set; }
        public int UserCount { get; set; }
        public int? CanSeeAllBp { get; set; }
        public List<int>? Permissions { get; set; }
    }

    public class RoleCreateDTO
    {
        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(45, MinimumLength = 2, ErrorMessage = "El nombre del rol debe tener entre 2 y 45 caracteres")]
        public string RoleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del rol es requerida")]
        [StringLength(100, ErrorMessage = "La descripción no puede superar los 100 caracteres")]
        public string RoleDescription { get; set; } = string.Empty;

        public int IsActive { get; set; } = 1;
        public int IsOwnerRole { get; set; } = 0;
    }

    public class RoleUpdateDTO
    {
        [Required(ErrorMessage = "El ID del rol es requerido")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(45, MinimumLength = 2, ErrorMessage = "El nombre del rol debe tener entre 2 y 45 caracteres")]
        public string RoleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del rol es requerida")]
        [StringLength(100, ErrorMessage = "La descripción no puede superar los 100 caracteres")]
        public string RoleDescription { get; set; } = string.Empty;

        public int IsActive { get; set; } = 1;
    }

    public class AssignRoleDTO
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El ID del rol es requerido")]
        public int RoleId { get; set; }
    }

    public class UserRoleInfoDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Permissions
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }
}
