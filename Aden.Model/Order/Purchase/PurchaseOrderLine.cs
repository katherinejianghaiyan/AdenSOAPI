using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Purchase
{
    public class PurchaseOrderLine
    {
        /// <summary>
        /// 行GUID
        /// </summary>
        public string lineGuid { get; set; }

        /// <summary>
        /// 供应商代码
        /// </summary>
        public string supplierCode { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string supplierName { get; set; }

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
        /// 数量
        /// </summary>
        public string qty { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 数量输入类型
        /// </summary>
        public string inputType { get; set; }
    }
}
