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
    public class SalesOrderItem
    {
        public string itemGuid { get; set; }

        public string headGuid { get; set; }

        public string itemNo { get; set; }

        public string version { get; set; }

        public string contract { get; set; }

        public string costCenterCode { get; set; }

        public string invoiceCostCenterCode { get; set; }

        public string serviceType { get; set; }

        public string serviceTypeValue { get; set; }

        public string productCode { get; set; }

        public string productDesc { get; set; }

        public decimal qty { get; set; }

        public decimal price { get; set; }

        public string priceUnitCode { get; set; }

        public string taxCode { get; set; }

        public decimal taxRate { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public string remark { get; set; }

        public string createDate { get; set; }

        public string userGuid { get; set; }

        public string userID { get; set; }

        public string changeDate { get; set; }

        public string changeUserGuid { get; set; }

        public string changeUserID { get; set; }

        public string expiryDate { get; set; }

        public int gladisID { get; set; }

        public string status { get; set; }

        public string changeFlag { get; set; }

        public string ownerCompanyCode { get; set; }
    }
}
