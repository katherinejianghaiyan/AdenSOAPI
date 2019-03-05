using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MastData
{
    public class CCMast
    {
        public int id { get; set; }

        public string dbName { get; set; }

        public string costCenterCode { get; set; }

        public string costCenterName { get; set; }

        public string warehouseCode { get; set; }

        public string poItemEndTime { get; set; }

        public string recipientsofMenu { get; set; }

        public string status { get; set; }

        public string posIp { get; set; }

        public string posDBName { get; set; }

        public string posDBUserName { get; set; }

        public string posDBPassword { get; set; }

        public string rechargeSetting { get; set; }
        public string siteGuid { get; set; }
        public string companyGuid { get; set; }
    }
}
