using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aden.Model.SiteDIY
{
    public class SitePost
    {
        public int id { get; set; }
        public string dbName { get; set; }
        public string siteGuid { get; set; }
        public string sortName { get; set; }
        public string costCenterCode { get; set; }
        public string postName { get; set; }
        public string businessType { get; set; }
        public string imageType { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int itemStatus { get; set; }
        public string picAddress { get; set; }

    }
}