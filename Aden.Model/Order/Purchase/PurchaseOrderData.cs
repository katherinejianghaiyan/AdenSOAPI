using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Purchase
{
    public class PurchaseOrderData
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
        /// 成本中心代码
        /// </summary>
        public string costCenterCode { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string orderDate { get; set; }

        /// <summary>
        /// 订单行
        /// </summary>

        public List<PurchaseOrderLine> lines { get; set; }

    }
}
