using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogNew.Models
{
    public class ArchiveMonthModel
    {
        public string Month { get; set; }
        public int TotalPosts { get { return Posts.Count; } }
        public List<ArchivePostModel> Posts { get; set; } = new List<ArchivePostModel>();
    }
}