using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.MenuOrder
{
    public class CCWhs
    {
        public string siteGuid { get; set; }
        public int id { get; set; }
        public string dbName { get; set; }

        public string catPOEndTime { get; set; }

        public string ip { get; set; }

        public string costCenterCode { get; set; }

        public string costCenterName_ZH { get; set; }

        public string costCenterName_EN { get; set; }

        public string warehouseCode { get; set; }

        public string value { get; set; }

        public string label { get; set; }

        public List<CCWindows> lstCCWindows { get; set; }
    }
}
