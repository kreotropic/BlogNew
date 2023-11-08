using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogNew.Models
{
    public class ArchivePostModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}