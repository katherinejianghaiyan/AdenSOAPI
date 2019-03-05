namespace Aden.Model.Order
{
    /// <summary>
    /// 订单行
    /// </summary>
    public class OrderLine
    {
        /// <summary>
        /// 订单行GUID
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        public int lineNumber { get; set; }

        /// <summary>
        /// 行描述
        /// </summary>
        public string lineDescription { get; set; }

        /// <summary>
        /// 物料代码
        /// </summary>
        public string itemCode { get; set; }

        /// <summary>
        /// 物料说明
        /// </summary>
        public string itemDescription { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public double price { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public double tax { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public double quantity { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public double amount { get; set; }

        /// <summary>
        /// 成本中心代码
        /// </summary>
        public string costCenterCode { get; set; }

        /// <summary>
        /// 成本单位代码
        /// </summary>
        public string costUnitCode { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }
}
