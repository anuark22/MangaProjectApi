using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Models;
using Services.Interfaces;

namespace Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ICommonService _commonService;

        public AuditLogService(ICommonService commonService)
        {
            _commonService = commonService;
        }

        public void Log(
            HttpContext? httpContext,
            string action,
            string entityType,
            int? entityId = null,
            string? entityName = null,
            string? details = null,
            int? overrideScanId = null)
        {
            try
            {
                int? userId = null;
                string? username = null;
                string? userRole = null;
                int? scanId = overrideScanId;
                string? ipAddress = null;

                if (httpContext != null)
                {
                    var userInfo = httpContext.Items["Client"] as UsersDTO;
                    if (userInfo != null)
                    {
                        userId = userInfo.UserId;
                        username = userInfo.Username;
                        userRole = userInfo.RoleId == (int)RolesEnum.Admin ? "Admin" : userInfo.RoleId == (int)RolesEnum.Scan ? "Scan" : "Client";
                    }
                    else
                    {
                        var idClaim = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier) ?? httpContext.User?.FindFirst("UserId");
                        if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
                        {
                            userId = parsedId;
                        }
                        username = httpContext.User?.Identity?.Name ?? httpContext.User?.FindFirst(ClaimTypes.Name)?.Value;
                    }

                    // Si es Scan y aún no tenemos ScanId, intentar resolverlo desde la BD
                    if (!scanId.HasValue && userId.HasValue && string.Equals(userRole, "Scan", StringComparison.OrdinalIgnoreCase))
                    {
                        scanId = _commonService.Query<int?>(
                            "SELECT TOP 1 COALESCE(u.ScanId, sg.ScanId) FROM [dbo].[Users] u LEFT JOIN [dbo].[ScansGroups] sg ON sg.[CreatedBy] = u.[UserId] WHERE u.[UserId] = @UserId",
                            new { UserId = userId.Value },
                            CommandType.Text
                        ).FirstOrDefault();
                    }

                    ipAddress = httpContext.Connection?.RemoteIpAddress?.ToString();
                }

                _commonService.Query<int>(
                    "AuditLogsProcedures",
                    new
                    {
                        Action = 1,
                        UserId = userId,
                        Username = username,
                        UserRole = userRole,
                        ScanId = scanId,
                        AuditAction = action,
                        EntityType = entityType,
                        EntityId = entityId,
                        EntityName = entityName,
                        Details = details,
                        IpAddress = ipAddress
                    },
                    CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditLogService] Error logging audit event: {ex.Message}");
            }
        }

        public Response<AuditLogDTO> GetAuditLogs(
            string? entityType = null,
            string? action = null,
            int? userId = null,
            string? search = null,
            int top = 150)
        {
            var response = new Response<AuditLogDTO>();
            try
            {
                var items = _commonService.Query<AuditLogDTO>(
                    "AuditLogsProcedures",
                    new
                    {
                        Action = 2,
                        UserId = userId,
                        AuditAction = action,
                        EntityType = entityType,
                        Search = search,
                        Top = top > 0 ? top : 150
                    },
                    CommandType.StoredProcedure
                ).ToList();

                response.Items = items;
                response.Status = 200;
            }
            catch (Exception ex)
            {
                response.Error = true;
                response.Status = 500;
                response.ErrorDescription = $"Error al obtener registros de auditoría: {ex.Message}";
            }

            return response;
        }
    }
}
