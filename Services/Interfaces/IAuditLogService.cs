using Microsoft.AspNetCore.Http;
using Models;

namespace Services.Interfaces
{
    public interface IAuditLogService
    {
        void Log(
            HttpContext? httpContext,
            string action,
            string entityType,
            int? entityId = null,
            string? entityName = null,
            string? details = null,
            int? overrideScanId = null);

        Response<AuditLogDTO> GetAuditLogs(
            string? entityType = null,
            string? action = null,
            int? userId = null,
            string? search = null,
            int top = 150);
    }
}
