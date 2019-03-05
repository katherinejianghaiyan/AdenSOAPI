using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Purchase
{
    public class PurchaseReceiptLine
    {
        /// <summary>
        /// 产品大类
        /// </summary>
        public string assortment { get; set; }
        /// <summary>
        /// 产品代码
        /// </summary>
        public string itemCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string itemName { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public string price { get; set; }

        /// <summary>
        /// 订购数量
        /// </summary>
        public string orderQty { get; set; }

        /// <summary>
        /// 收货数量
        /// </summary>
        public string receiptQty { get; set; }

        /// <summary>
        /// 输入类型
        /// </summary>
        public string inputType { get; set; }
    }
}
