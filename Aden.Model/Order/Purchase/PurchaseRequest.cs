using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Purchase
{
    public class PurchaseRequest:BaseRequest
    {
        /// <summary>
        /// 公司GUID
        /// </summary>
        public string companyGuid { get; set; }

        /// <summary>
        /// 成本中心代码
        /// </summary>
        public string costCenterCode { get; set; }

        /// <summary>
        /// 仓库代码
        /// </summary>
        public string warehouseCode { get; set; }

        /// <summary>
        /// 订购日期
        /// </summary>
        public string orderDate { get; set; }
        
        /// <summary>
        /// 语言代码
        /// </summary>
        public string langCode { get; set; }

        /// <summary>
        /// 供应商代码Crdnr
        /// </summary>
        public string supplierCode { get; set; }

        /// <summary>
        /// 采购订单类型
        /// </summary>
        public string poType { get; set; }
    }
}
