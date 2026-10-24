using Models;

namespace Services.Interfaces
{
    public interface IDemographicsService
    {
        Response<Demographics> GetDemographics(int? demographyId = null, string? name = null);
        Task<Response<Demographics>> CreateDemographic(DemographicCreateDTO model);
        Task<Response<Demographics>> UpdateDemographic(DemographicUpdateDTO model);
        Task<Response<Demographics>> DeleteDemographic(int demographyId);

    }
}
