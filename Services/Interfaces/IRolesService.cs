using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IRolesService
    {
        Response<Roles> GetRoles(int? roleId = null, string? roleName = null, bool? isActive = null);
        Task<Response<Roles>> CreateRole(RoleCreateDTO model);
        Task<Response<Roles>> UpdateRole(RoleUpdateDTO model);
        Task<Response<Roles>> DeleteRole(int roleId);
        Task<Response<DBClass>> AssignRole(AssignRoleDTO model);
        Response<UserRoleInfoDTO> GetUsersByRole(int roleId);
    }
}
