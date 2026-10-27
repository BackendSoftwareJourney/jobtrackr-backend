using JobTrackr.Application.Common;
using System.ComponentModel.DataAnnotations;

namespace JobTrackr.Application.Comments
{
    public class CreateTaskCommentRequest
    {
        [Required(ErrorMessage = ErrorMessages.CommentContentRequired)]
        [MaxLength(1000, ErrorMessage = ErrorMessages.CommentContentTooLong)]
        public string Content { get; set; } = string.Empty;
    }
}