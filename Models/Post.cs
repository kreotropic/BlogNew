using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogNew.Models
{
    public class Post
    {
        
        public int PostId { get; set; }

        public string UserId { get; set; } 
        public ApplicationUser User { get; set; }

        public string Title { get; set; }
        public string Sinopse { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPrivate { get; set; }




    }
}