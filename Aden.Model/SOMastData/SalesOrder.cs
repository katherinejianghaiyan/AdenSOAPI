using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    /// <summary>
    /// 合同订单表头
    /// </summary>
    public class SalesOrder
    {
        public string id { get; set; }
        public string headGuid { get; set; }

        public string version { get; set; }

        public string contract { get; set; }

        public bool received { get; set; }

        public int signCount { get; set; }

        public string ownerCompanyCode { get; set; }

        public string ownerCompanyName_ZH { get; set; }

        public string ownerCompanyName_EN { get; set; }

        public string companyCode { get; set; }

        public string companyName_ZH { get; set; }

        public string companyName_EN { get; set; }

        public string cmpCode { get; set; }

        public string customCode { get; set; }

        public string customName_ZH { get; set; }

        public string customName_EN { get; set; }

        public string currCode { get; set; }

        public string currName_ZH { get; set; }

        public string currName_EN { get; set; }

        public string paymentCode { get; set; }

        public string paymentName_ZH { get; set; }

        public string paymentName_EN { get; set; }

        public int deadline { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public string validDate { get; set; }

        public string remark { get; set; }

        public string createDate { get; set; }

        public string userGuid { get; set; }

        public string userID { get; set; }

        public string changeDate { get; set; }

        public string changeUserGuid { get; set; }

        public string changeUserID { get; set; }

        public string crossCompInv { get; set; }

        public string expiryDate { get; set; }

        public string status { get; set; }

        public string changeFlag { get; set; }

        public string costCenterCode { get; set; }

        public int gladisID { get; set; }

        public string rebuildGuid { get; set; }

        public List<SalesOrderItem> items { get; set; }
    }
}
