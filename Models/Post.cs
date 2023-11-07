using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;


namespace BlogNew.Models
{
    public class Post
    {
       
        public int PostId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "Please insert the title of the post")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        public string Title { get; set; }

        [Display(Name = "Sinopse")]
        //[Required(ErrorMessage = "Please insert the description of the post")]
        //[StringLength(256, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 5)]
        public string Sinopse { get; set; }

        [Display(Name = "Content")]
        [Required(ErrorMessage = "Please insert the content of the post")]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 100)]
        public string Content { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } 
        public bool IsPrivate { get; set; }

        [NotMapped]
        public int ThumbsCount
        {
            get
            {
                using (var db = new ApplicationDbContext())
                {
                    return db.Thumbs.Count(t => t.PostId == this.PostId);
                }
            }
            set { }
        }

        //assign date to created at
        public Post()
        {
            CreatedAt = DateTime.UtcNow;
        }

        //assign date to updated at
        public void Update()
        {
            UpdatedAt = DateTime.UtcNow;
        }

    }
}