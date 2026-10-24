using Microsoft.Extensions.Configuration;
using Models;
using Services.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class MangaGenresService : IMangaGenresService
    {
        private readonly ICommonService _commonService;
        private readonly IConfiguration _config;

        public MangaGenresService(ICommonService commonService, IConfiguration config)
        {
            _commonService = commonService;
            _config = config;
        }

        public Response<MangaGenres> GetMangaGenres(int? genreId = null, int? mangaId = null)
        {
            Response<MangaGenres> response = new();
            int action = 1;
            object param = new { Action = action, GenreId = genreId, MangaId = mangaId };

            response.Items = _commonService.Query<MangaGenres>("MangaGenresProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            return response;
        }

        public async Task<Response<MangaGenres>> CreateMangaGenre(MangaGenreCreateDTO model)
        {
            Response<MangaGenres> response = new();
            int action = 2;
            object param = new
            {
                Action = action,
                model.GenreId,
                model.MangaId
            };

            try
            {
                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangaGenresProcedures", param, CommandType.StoredProcedure);

                var firstResult = dbResponse.FirstOrDefault();
                if (firstResult != null)
                {
                    response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
                }
                else
                {
                    response.ParseExecution(500, "Unexpected database error");
                }
            }
            catch (Exception ex)
            {
                response.ParseExecution(500, $"Error al asociar el género al manga: {ex.Message}");
            }

            return response;
        }

        public async Task<Response<MangaGenres>> DeleteMangaGenre(int genreId, int mangaId)
        {
            Response<MangaGenres> response = new();
            int action = 3;
            object param = new
            {
                Action = action,
                GenreId = genreId,
                MangaId = mangaId
            };

            try
            {
                IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("MangaGenresProcedures", param, CommandType.StoredProcedure);

                var firstResult = dbResponse.FirstOrDefault();
                if (firstResult != null)
                {
                    response.ParseExecution(firstResult.Status, firstResult.ErrorDescription);
                }
                else
                {
                    response.ParseExecution(500, "Unexpected database error");
                }
            }
            catch (Exception ex)
            {
                response.ParseExecution(500, $"Error al eliminar la relación: {ex.Message}");
            }

            return response;
        }
    }
}