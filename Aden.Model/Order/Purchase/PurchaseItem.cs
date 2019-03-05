using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Purchase
{
   public class PurchaseItem
    {
        /// <summary>
        /// 产品代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 单价,含税
        /// </summary>
        public double price { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// 输入控制
        /// </summary>
        public string inputType { get; set; }
    }
}
