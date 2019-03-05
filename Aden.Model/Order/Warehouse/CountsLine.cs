using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Warehouse
{
    public class CountsLine
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
        /// 税率
        /// </summary>
        public double rate { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public string stockQty { get; set; }

        /// <summary>
        /// 输入类型
        /// </summary>
        public string inputType { get; set; }

        public List<CountsUnit> unitQtys { get; set; }

        /// <summary>
        /// 产品数量成本
        /// </summary>
        public List<ItemQtyCost> itemCosts { get; set; }
    }
}
