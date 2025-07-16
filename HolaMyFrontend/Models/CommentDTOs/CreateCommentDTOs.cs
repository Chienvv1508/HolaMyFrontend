using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolaMyFrontend.Models.CommentDTOs
{
    public class CreateCommentDTO
    {
        public int? BuildingId { get; set; }
        public int? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public CommentContentType ContentType { get; set; } = CommentContentType.text;
        public IFormFile? File { get; set; }
        public float? Rating { get; set; }
    }
    public enum CommentContentType
    {
        text,
        image,
        text_image,
        file
    }

    public enum CommentStatus
    {
        active,
        hidden
    }
}
