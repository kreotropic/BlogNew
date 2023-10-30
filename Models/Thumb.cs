using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlogNew.Models
{
    public class Thumb
    {
        [Key]
        public int ThumbId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }

        public ApplicationUser User { get; set; }
        public Post Post { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}