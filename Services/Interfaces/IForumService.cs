using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IForumService
    {
        Response<ForumPosts> GetPosts(int? postId = null, string? topic = null, string? subTopic = null, string sortBy = "Latest");
        Task<Response<ForumPosts>> CreatePost(CreateForumPostDTO model, int userId);
        Task<Response<ForumPosts>> UpdatePost(UpdateForumPostDTO model, int userId);
        Task<Response<ForumPosts>> DeletePost(int postId, int userId, bool isAdmin = false);
        Task<Response<DBClass>> IncrementViews(int postId);
        Response<ForumComments> GetComments(int postId);
        Task<Response<ForumComments>> AddComment(CreateForumCommentDTO model, int userId);
        Task<Response<ForumComments>> DeleteComment(int commentId, int userId, bool isAdmin = false);
    }
}
