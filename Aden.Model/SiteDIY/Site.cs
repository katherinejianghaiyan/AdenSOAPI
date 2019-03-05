using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aden.Model.SOMastData;

namespace Aden.Model.SiteDIY
{
    public class Site:Company
    {
        public string costCenterCode { get; set; }
        public string siteGuid { get; set; }
    }
}