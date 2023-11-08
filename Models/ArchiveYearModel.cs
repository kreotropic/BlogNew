using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlogNew.Models
{
    public class ArchiveYearModel
    {
        public int Year { get; set; }
        public List<ArchiveMonthModel> Months { get; set; } = new List<ArchiveMonthModel>();    
    }
}