
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolaMyFrontend.Models.CommentDTOs
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public CommentContentType ContentType { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public float? Rating { get; set; }
        public string AvatarUrl { get; set; }
    }
}
