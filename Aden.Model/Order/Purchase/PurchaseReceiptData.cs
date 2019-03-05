using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Purchase
{
    public class PurchaseReceiptData
    {
        /// <summary>
        /// 用户GUID
        /// </summary>
        public string userGuid { get; set; }

        /// <summary>
        /// 公司GUID
        /// </summary>
        public string companyGuid { get; set; }

        /// <summary>
        /// 仓库代码
        /// </summary>
        public string warehouseCode { get; set; }

        /// <summary>
        /// 供应商代码
        /// </summary>
        public string supplierCode { get; set; }

        /// <summary>
        /// 收货日期
        /// </summary>
        public string receiptDate { get; set; }

        /// <summary>
        /// 采购订单方式
        /// </summary>
        public string poType { get; set; }

        /// <summary>
        /// 收货行
        /// </summary>
        public List<PurchaseReceiptLine> lines { get; set; }
    }
}
