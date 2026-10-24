using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IScansGroupsService
    {
        Response<ScansGroups> GetScansGroups(int? scanId = null, string? scanName = null);
        Task<Response<ScansGroups>> CreateScanGroup(ScanGroupCreateDTO model, int userId); // <--- Añadimos userId
        Task<Response<ScansGroups>> UpdateScanGroup(ScanGroupUpdateDTO model);
        Task<Response<ScansGroups>> DeleteScanGroup(int scanId);
    }
}