using Models;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IChapterCommentsService
    {
        Response<ChapterComment> GetComments(int chapterId);
        Task<Response<ChapterComment>> AddComment(CreateChapterCommentDTO model, int userId);
        Response<ChapterComment> DeleteComment(int commentId, int userId, bool isAdmin);
    }
}
