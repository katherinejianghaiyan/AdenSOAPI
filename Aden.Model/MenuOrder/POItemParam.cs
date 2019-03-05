using System.Collections.Generic;

namespace Aden.Model.MenuOrder
{
    public class POItemParam
    {
        /// <summary>
        /// changesupplier: 修改供应商 po: 采购单
        /// </summary>
        public string mode { get; set; }
        /// <summary>
        /// 登录人账号
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// DBName  
        /// </summary>
        public string dbName { get; set; }
        /// <summary>
        /// 成本中心
        /// </summary>
        public string costCenterCode { get; set; }
        /// <summary>
        /// 档口Guid
        /// </summary>
        public string windowGuid { get; set; }
        /// <summary>
        /// 一周开始日期
        /// </summary>
        public string startDate { get; set; }
        /// <summary>
        /// 一周结束日期
        /// </summary>
        public string endDate { get; set; }

        public List<POItem> lstChangeSupplier { get; set; }
    }
}
