﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;


namespace BlogNew.Models
{
    public class Post
    {
       
        public int PostId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Display(Name = "Title")]
        [Required(ErrorMessage = "Please insert the title of the post")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long and at most {1} characters long.", MinimumLength = 5)]
        public string Title { get; set; }

        [Display(Name = "Sinopse")]
        public string Sinopse { get; set; }

        [Display(Name = "Content")]
        [Required(ErrorMessage = "Please insert the content of the post")]
        [StringLength(500000, ErrorMessage = "The {0} must be at least {2} characters long and at most {1} characters long ", MinimumLength = 100)]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Content { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }
        [Column(TypeName = "datetime2")]
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

     


    }
}