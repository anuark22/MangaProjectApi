using System;
using System.Collections.Generic;

namespace Models
{
    public class ForumPosts
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string SubTopic { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ViewsCount { get; set; }
        public int PostCommentsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPinned { get; set; }
        public bool IsClosed { get; set; }

        // Aliases exactos para consumirse directamente en Post.jsx
        public int Id => PostId;
        public string PostTittle => Title;
        public string PostContent => Content;
        public int PostComments => PostCommentsCount;
        public int PostViews => ViewsCount;
    }

    public class ForumComments
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateForumPostDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string SubTopic { get; set; } = string.Empty;
    }

    public class UpdateForumPostDTO
    {
        public int PostId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Topic { get; set; }
        public string? SubTopic { get; set; }
    }

    public class CreateForumCommentDTO
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
