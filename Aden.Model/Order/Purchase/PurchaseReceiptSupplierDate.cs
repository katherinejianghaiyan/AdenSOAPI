using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Purchase
{
    public class PurchaseReceiptSupplierDate
    {
        /// <summary>
        /// 供应商代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// 采购订单类型
        /// </summary>
        public string poType { get; set; }

        /// <summary>
        /// 状态码, 0:订单未打印, 1: 可接收, 2: 已接收,接收数据正在处理
        /// </summary>
        public int statusCode { get; set; }
    }
}
