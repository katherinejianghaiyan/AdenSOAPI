using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class MultiCoCostCenterMatch
    {
        public string OwnerCompanyCode { get; set; }
        public string OwnerCostCenterCode { get; set; }
        public string CompanyCode { get; set; }
        public string CostCenterCode { get; set; }
        public string HeadGuid { get; set; }
        public string ChangeUserGuid { get; set; }
        public string ChangeUserID { get; set; }
    }
}
