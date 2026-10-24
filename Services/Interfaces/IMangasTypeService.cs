using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMangasTypeService
    {
        Response<MangasType> GetMangasType(int? mangaTypeId = null, string? mangaTypeName = null);
        Task<Response<MangasType>> CreateMangaType(MangaTypeCreateDTO model);
        Task<Response<MangasType>> UpdateMangaType(MangaTypeUpdateDTO model);
        Task<Response<MangasType>> DeleteMangaType(int mangaTypeId);
    }
}