using Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class ForumService : IForumService
    {
        private readonly ICommonService _commonService;

        public ForumService(ICommonService commonService)
        {
            _commonService = commonService;
        }

        public Response<ForumPosts> GetPosts(int? postId = null, string? topic = null, string? subTopic = null, string sortBy = "Latest")
        {
            Response<ForumPosts> response = new();
            int action = 1;
            object param = new
            {
                Action = action,
                PostId = postId,
                Topic = topic,
                SubTopic = subTopic,
                SortBy = sortBy
            };

            response.Items = _commonService.Query<ForumPosts>("ForumProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            return response;
        }

        public async Task<Response<ForumPosts>> CreatePost(CreateForumPostDTO model, int userId)
        {
            Response<ForumPosts> response = new();
            int action = 2;
            object param = new
            {
                Action = action,
                model.Title,
                model.Content,
                model.Topic,
                model.SubTopic,
                UserId = userId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ForumProcedures", param, CommandType.StoredProcedure);
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

        public async Task<Response<ForumPosts>> UpdatePost(UpdateForumPostDTO model, int userId)
        {
            Response<ForumPosts> response = new();
            int action = 3;
            object param = new
            {
                Action = action,
                model.PostId,
                model.Title,
                model.Content,
                model.Topic,
                model.SubTopic,
                UserId = userId
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ForumProcedures", param, CommandType.StoredProcedure);
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

        public async Task<Response<ForumPosts>> DeletePost(int postId, int userId, bool isAdmin = false)
        {
            Response<ForumPosts> response = new();
            int action = 4;
            object param = new { Action = action, PostId = postId };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ForumProcedures", param, CommandType.StoredProcedure);
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

        public async Task<Response<DBClass>> IncrementViews(int postId)
        {
            Response<DBClass> response = new();
            int action = 5;
            object param = new { Action = action, PostId = postId };

            await _commonService.QueryAsync<DBClass>("ForumProcedures", param, CommandType.StoredProcedure);
            response.ParseExecution(200, null);
            return response;
        }

        public Response<ForumComments> GetComments(int postId)
        {
            Response<ForumComments> response = new();
            int action = 6;
            object param = new { Action = action, PostId = postId };

            response.Items = _commonService.Query<ForumComments>("ForumProcedures", param, CommandType.StoredProcedure);
            response.ParseResponse(true);

            return response;
        }

        public async Task<Response<ForumComments>> AddComment(CreateForumCommentDTO model, int userId)
        {
            Response<ForumComments> response = new();
            int action = 7;
            object param = new
            {
                Action = action,
                model.PostId,
                UserId = userId,
                model.Content
            };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ForumProcedures", param, CommandType.StoredProcedure);
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

        public async Task<Response<ForumComments>> DeleteComment(int commentId, int userId, bool isAdmin = false)
        {
            Response<ForumComments> response = new();
            int action = 8;
            object param = new { Action = action, CommentId = commentId, UserId = isAdmin ? (int?)null : userId };

            IEnumerable<DBClass> dbResponse = await _commonService.QueryAsync<DBClass>("ForumProcedures", param, CommandType.StoredProcedure);
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
    }
}
