using Microsoft.Extensions.Configuration;
using Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class UserMangaListsService : IUserMangaListsService
    {
        private readonly ICommonService _commonService;
        private readonly IConfiguration _config;

        public UserMangaListsService(ICommonService commonService, IConfiguration config)
        {
            _commonService = commonService;
            _config = config;
        }

        public Response<UserMangaLists> GetUserList(int userId, string? readingStatus = null)
        {
            Response<UserMangaLists> response = new();
            int action = 1;
            object param = new { Action = action, UserId = userId, ReadingStatus = readingStatus };

            response.Items = _commonService.Query<UserMangaLists>("UserMangaListsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            string baseUrl = _config["MultimediaBaseUrl"] ?? string.Empty;

            Parallel.ForEach(response.Items, new ParallelOptions { MaxDegreeOfParallelism = 4 }, item =>
            {
                if (!string.IsNullOrEmpty(item.MangaFrontImgUrl) && !item.MangaFrontImgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    item.MangaFrontImgUrl = $"{baseUrl.TrimEnd('/')}/{item.MangaFrontImgUrl.TrimStart('/')}";
                }
            });

            return response;
        }

        public async Task<Response<UserMangaLists>> AddOrUpdateMangaInList(AddUserMangaDTO model, int userId)
        {
            Response<UserMangaLists> response = new();
            int action = 2;
            object param = new
            {
                Action = action,
                UserId = userId,
                model.MangaId,
                model.ReadingStatus
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("UserMangaListsProcedures", param, CommandType.StoredProcedure);
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

        public async Task<Response<UserMangaLists>> RemoveMangaFromList(int mangaId, int userId)
        {
            Response<UserMangaLists> response = new();
            int action = 3;
            object param = new { Action = action, UserId = userId, MangaId = mangaId };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("UserMangaListsProcedures", param, CommandType.StoredProcedure);
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

        public Response<UserMangaStatusCheckDTO> CheckMangaInList(int mangaId, int userId)
        {
            Response<UserMangaStatusCheckDTO> response = new();
            int action = 4;
            object param = new { Action = action, UserId = userId, MangaId = mangaId };

            response.Items = _commonService.Query<UserMangaStatusCheckDTO>("UserMangaListsProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            return response;
        }

        public Response<UserReadChapterDTO> GetReadChapters(int userId, int? mangaId = null)
        {
            Response<UserReadChapterDTO> response = new();
            object param = new { Action = 1, UserId = userId, MangaId = mangaId };
            response.Items = _commonService.Query<UserReadChapterDTO>("UserReadChaptersProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);
            return response;
        }

        public async Task<Response<UserReadChapterDTO>> MarkChapterAsRead(int userId, int mangaId, int chapterId)
        {
            Response<UserReadChapterDTO> response = new();
            object param = new { Action = 2, UserId = userId, MangaId = mangaId, ChapterId = chapterId };
            var rows = await _commonService.QueryAsync<UserReadChapterDTO>("UserReadChaptersProcedures", param, CommandType.StoredProcedure);
            response.Items = rows.ToList();
            response.ParseResponse(true);
            return response;
        }

        public async Task<Response<DBClass>> UnmarkChapterAsRead(int userId, int chapterId)
        {
            Response<DBClass> response = new();
            object param = new { Action = 3, UserId = userId, ChapterId = chapterId };
            var rows = await _commonService.QueryAsync<DBClass>("UserReadChaptersProcedures", param, CommandType.StoredProcedure);
            var first = rows.FirstOrDefault();
            response.ParseExecution(first?.Status ?? 200, first?.ErrorDescription);
            return response;
        }

        public Response<UserMangaProgressDTO> GetMyReadingProgress(int userId)
        {
            Response<UserMangaProgressDTO> response = new();
            object param = new { Action = 4, UserId = userId };
            response.Items = _commonService.Query<UserMangaProgressDTO>("UserReadChaptersProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);
            return response;
        }
    }
}
