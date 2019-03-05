using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class Company
    {
        public string companyGuid { get; set; }
        public string companyCode { get; set; }
        public string companyName_ZH { get; set; }
        public string companyName_EN { get; set; }        
        public string superiorCompany { get; set; }
        public string ip { get; set; }
        public string dbName { get; set; }
        public string interBranchOP { get; set; }
        public string interBranchOR { get; set; }
        public string interBranchAP { get; set; }
        public string interBranchAR { get; set; }
        public string interBranchCost { get; set; }
        public string interBranchRevenue { get; set; }
        public string interCompanyOP { get; set; }
        public string interCompanyOR { get; set; }
        public string interCompanyAP { get; set; }
        public string interCompanyAR { get; set; }
        public string interCompanyCost { get; set; }
        public string interCompanyRevenue { get; set; }
        public string companyGroup { get; set; }
        public string supplierCode { get; set; }
        public string customerCode { get; set; }
        public string status { get; set; }
        
    }
}
