using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model.Account;

namespace Aden.Model.Order.Warehouse
{
    public class CountsData
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
        /// 供应商代码
        /// </summary>
        public string supplierCode { get; set; }

        /// <summary>
        /// 消耗日期
        /// </summary>
        public string countsDate { get; set; }

        public List<AccountCostUnit> costUnits { get; set; }

        public List<CountsLine> lines { get; set; }
    }
}
