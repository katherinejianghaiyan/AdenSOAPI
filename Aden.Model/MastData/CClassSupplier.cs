using System;
using System.Collections.Generic;
using Aden.Model.Common;
using System.Linq;
using System.Text;

namespace Aden.Model.MastData
{
    public class CClassSupplier
    {
        // 成本中心Code
        public string costCenterCode { get; set; }
        // 成本中心Name
        public string costCenterName { get; set; }
        // DBName
        public string dbName { get; set; }
        // 供应商Code
        public string supplierCode { get; set; }
        // 供应商Code备份
        public string supplierCode_bak { get; set; }
        // 默认供应商
        public bool defaultSupplier { get; set; }
        // 默认供应商备份
        public bool defaultSupplier_bak { get; set; }
        // 类别Guid
        public string classGuid { get; set; }
        // 开始日期
        public string startDate { get; set; }
        // 开始日期备份
        public string startDate_bak { get; set; }
        // 结束日期
        public string endDate { get; set; }
        // 结束日期备份
        public string endDate_bak { get; set; }
        // 修改标志
        public string changeFlag { get; set; }

        public List<CClassSupplier> lstSupplierCode { get; set; }
    }
}
