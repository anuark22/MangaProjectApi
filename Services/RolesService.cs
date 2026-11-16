using Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class RolesService : IRolesService
    {
        private readonly ICommonService _commonService;

        public RolesService(ICommonService commonService)
        {
            _commonService = commonService;
        }

        public Response<Roles> GetRoles(int? roleId = null, string? roleName = null, bool? isActive = null)
        {
            Response<Roles> response = new();
            int action = 1;
            object param = new
            {
                Action = action,
                RoleId = roleId,
                RoleName = roleName,
                IsActive = isActive.HasValue ? (isActive.Value ? 1 : 0) : (int?)null
            };

            response.Items = _commonService.Query<Roles>("RolesProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            return response;
        }

        public async Task<Response<Roles>> CreateRole(RoleCreateDTO model)
        {
            Response<Roles> response = new();
            int action = 2;
            object param = new
            {
                Action = action,
                model.RoleName,
                model.RoleDescription,
                model.IsActive,
                model.IsOwnerRole
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("RolesProcedures", param, CommandType.StoredProcedure);
            var firstResult = dbResponse.FirstOrDefault();
            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Unexpected database error");
            }

            return response;
        }

        public async Task<Response<Roles>> UpdateRole(RoleUpdateDTO model)
        {
            Response<Roles> response = new();
            int action = 3;
            object param = new
            {
                Action = action,
                model.RoleId,
                model.RoleName,
                model.RoleDescription,
                model.IsActive
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("RolesProcedures", param, CommandType.StoredProcedure);
            var firstResult = dbResponse.FirstOrDefault();
            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Unexpected database error");
            }

            return response;
        }

        public async Task<Response<Roles>> DeleteRole(int roleId)
        {
            Response<Roles> response = new();
            int action = 4;
            object param = new { Action = action, RoleId = roleId };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("RolesProcedures", param, CommandType.StoredProcedure);
            var firstResult = dbResponse.FirstOrDefault();
            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Unexpected database error");
            }

            return response;
        }

        public async Task<Response<DBClass>> AssignRole(AssignRoleDTO model)
        {
            Response<DBClass> response = new();
            int action = 5;
            object param = new
            {
                Action = action,
                model.UserId,
                model.RoleId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("RolesProcedures", param, CommandType.StoredProcedure);
            var firstResult = dbResponse.FirstOrDefault();
            if (firstResult != null)
            {
                response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
            }
            else
            {
                response.ParseExecution(500, "Unexpected database error");
            }

            return response;
        }

        public Response<UserRoleInfoDTO> GetUsersByRole(int roleId)
        {
            Response<UserRoleInfoDTO> response = new();
            int action = 6;
            object param = new { Action = action, RoleId = roleId };

            response.Items = _commonService.Query<UserRoleInfoDTO>("RolesProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            return response;
        }
    }
}
