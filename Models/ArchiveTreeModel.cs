using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogNew.Models
{
    public class ArchiveTreeModel
    {
        public List<ArchiveYearModel> Years { get; set; } = new List<ArchiveYearModel>();   
    }
}