using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogNew.Models
{
    public class PostIndexViewModel
    {
        public bool HasUserLiked { get; set; }
        public ApplicationUser User { get; set; }
        public Post Post { get; set; }
    }
}