namespace Aden.Model.Order
{
    /// <summary>
    /// 订单头
    /// </summary>
    public class OrderHead
    {
        /// <summary>
        /// 订单GUID
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// ERP公司代码
        /// </summary>
        public string erpCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string orderNumber { get; set; }

        /// <summary>
        /// 订单日期
        /// </summary>
        public string orderDate { get; set; }

        /// <summary>
        /// 订单描述
        /// </summary>
        public string orderDescription { get; set; }

        /// <summary>
        /// 仓库代码
        /// </summary>
        public string warehouseCode { get; set; }

        /// <summary>
        /// 成本中心代码
        /// </summary>
        public string costCenterCode { get; set; }

        /// <summary>
        /// 备注, FreeField
        /// </summary>
        public string remark { get; set; }
        
    }
}
