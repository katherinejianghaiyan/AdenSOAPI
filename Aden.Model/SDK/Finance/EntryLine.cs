using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SDK.Finance
{
    public class EntryLine
    {
        /// <summary>
        /// 成本中心
        /// </summary>
        public string CostCenter { get; set; }

        /// <summary>
        /// 成本单位
        /// </summary>
        public string CostUnit { get; set; }

        /// <summary>
        /// 贷方科目
        /// </summary>
        public string Creditor { get; set; }

        /// <summary>
        /// 借方科目
        /// </summary>
        public string Debtor { get; set; }

        /// <summary>
        /// 币种代码
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public double CurrencyRate { get; set; }

        /// <summary>
        /// 行说明
        /// </summary> 
        public string Description { get; set; }

        /// <summary>
        /// 凭证日期
        /// </summary>
        public string EntryDate { get; set; }

        /// <summary>
        /// 总账科目
        /// </summary>
        public string GLAccountNumber { get; set; }

        /// <summary>
        /// 总账类型
        /// </summary>
        public string GLAccountType { get; set; }

        /// <summary>
        /// 产品代码
        /// </summary>
        public string ItemCode { get; set; }
        
        /// <summary>
        /// 行号
        /// </summary>
        public string LineNumber { get; set; }
        
        /// <summary>
        /// 数字自由字段1
        /// </summary>
        public double NumberField1 { get; set; }

        /// <summary>
        /// 数字自由字段2
        /// </summary>
        public double NumberField2 { get; set; }

        /// <summary>
        /// 我方参考
        /// </summary>
        public string OurReference { get; set; }

        public string OrderDebtor { get; set; }

        public string OrderNumber { get; set; }

        public string SerialNumber { get; set; }

         
        public string TextField1 { get; set; }

         
        public string TextField2 { get; set; }

         
        public string TextField3 { get; set; }

         
        public string Warehouse { get; set; }

         
        public string ReportingDate { get; set; }

         
        public string FulfillmentDate { get; set; }

         
        public string YourReference { get; set; }

         
        public double Quantity { get; set; }

         
        public double Amount { get; set; }

         
        public decimal Debit { get; set; }

         
        public decimal Credit { get; set; }

         
        public string VATCode { get; set; }

         
        public string EntryType { get; set; }
    }
}
