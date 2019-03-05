using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aden.Model.SOMastData;

namespace Aden.Model.SiteDIY
{
    public class DBSite:Site
    {
        public List<Site> siteList { get; set; }
    }
}