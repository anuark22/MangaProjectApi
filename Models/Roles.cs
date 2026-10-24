namespace Models
{
    public class Roles
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public int IsActive { get; set; }
        public int IsOwnerRole { get; set; }
        public int? CanSeeAllBp { get; set; }
        public List<int> Permissions { get; set; }
    }

    public class Permissions
    {
        public int RoleId { get; set;}
        public int PermissionId { get; set;}
    }
}
