using Aden.Model.Common;
using Aden.Model.MastData;
using Aden.Model.Order.Warehouse;
using Aden.Util.Common;
using Aden.Util.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.DAL.Order
{
    public class WarehouseFactory
    {

        /// <summary>
        /// 获取可消耗的日期段
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库IP地址</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <returns>消耗日期集合</returns>
        public List<CountsDate> GetCountsDate(string erpCode, string ip, string warehouseCode, DateTime endDate)
        {
            string maxCountSql = "select Max(gbkmut.datum) sDate from gbkmut (nolock),amutak (nolock) where "
                + "gbkmut.dagbknr=amutak.dagbknr and gbkmut.bkstnr=amutak.bkstnr and gbkmut.transtype='N' and gbkmut.transsubtype='G' "
                + "and amutak.status<>'V' and warehouse='{0}' and len(isnull(gbkmut.freefield1,''))<>36";
            DateTime? cDate = (DateTime?)SqlServerHelper.GetDataScalar(string.Format(SqlServerHelper.customerConn(), ip, erpCode),
                string.Format(maxCountSql, warehouseCode));
            DateTime startDate = endDate.AddMonths(-1);
            if (cDate != null && ((DateTime)cDate).AddDays(1) > startDate) startDate = ((DateTime)cDate).AddDays(1);
            string openReceiptSql = "select Min(a1.afldat) eDate from orsrg (nolock) a1,orkrg (nolock) a2 "
                    + "where a2.ord_soort='B' and a2.ordernr=a1.ordernr and a1.magcode='{0}' and a1.aant_gelev=0 "
                    + "and a2.afgehandld=0 and a2.freefield5=0 and a1.prijslijst='P_CNY' and a2.bstwijze in ('001','003')";
            string pDate = SqlServerHelper.GetDataScalar(string.Format(SqlServerHelper.customerConn(), ip, erpCode),
                string.Format(openReceiptSql, warehouseCode)).ToString();
            DateTime purDate = string.IsNullOrEmpty(pDate) ? endDate : DateTime.Parse(pDate).AddDays(-1);
            List<CountsDate> countsDates = new List<CountsDate>();
            while (startDate <= endDate)
            {
                int statusCode = 1;
                if (startDate > purDate) statusCode = 0;
                countsDates.Add(new CountsDate()
                {
                    date = startDate.ToString("yyyy-MM-dd"),
                    statusCode = statusCode
                });
                startDate = startDate.AddDays(1);
            }
            return countsDates;
        }

        public List<CountsLine> GetCountsLines(string erpCode, string ip, string warehouseCode, DateTime date, string langCode)
        {
            string stockSql = "select b1.itemCode,b1.itemName,b1.assortName,b1.diviable,b1.unit,round(sum(b1.qty),4) stockQty,"
                + "round(case when sum(b1.qty) = 0 then 0 else sum(b1.amt) / sum(b1.qty) end, 4) price from "
                + "(select a2.itemcode,case when 'zh-CN' = 'zh-CN' then case when isnull(a2.description_1,'')= '' then a2.description "
                + "else a2.description_1 end else case when isnull(a2.description_0,'')= '' then a2.description "
                + "else a2.description_0 end end itemName,case when '{0}' = 'zh-CN' then case when isnull(a3.description_1,'')= '' "
                + "then a3.description else a3.description_1 end else case when isnull(a3.description_0,'')= '' "
                + "then a3.description else a3.description_0 end end assortName, a2.isfractionalloweditem diviable,"
                + "upper(ltrim(a2.packagedescription)) unit,a1.aantal qty, a1.bdr_hfl amt from gbkmut (nolock)a1,items(nolock) a2,"
                + "itemassortment(nolock) a3,grtbk(nolock) a4,amutak(nolock) a5 where a1.artcode = a2.itemcode "
                + "and a2.assortment=a3.assortment and a1.dagbknr = a5.dagbknr and a1.bkstnr = a5.bkstnr "
                + "and a1.entryguid = a5.sysguid and a5.status <> 'V' and a4.reknr = a1.reknr and a4.omzrek = 'G' "
                + "and a1.transtype in ('X', 'N', 'C', 'P') and a1.datum <='{1}' and a1.warehouse = '{2}') b1 "
                + "group by b1.itemCode,b1.itemName,b1.assortName,b1.diviable,b1.unit having round(sum(b1.qty), 4) > 0 "
                + "order by b1.assortName,b1.itemCode";
            DataTable data = SqlServerHelper.GetDataTable(string.Format(SqlServerHelper.customerConn(), ip, erpCode),
                string.Format(stockSql, langCode, date.ToString("yyyy-MM-dd"), warehouseCode));
            if (data == null || data.Rows.Count == 0) return null;
            DataTable taxData = null;
            Task task = new Task(()=>
            {
                string taxSql = "select a1.artcode itemCode,max(a2.btwper) taxRate from orsrg (nolock) a1,btwtrs (nolock) a2 "
                        + "where a1.btw_code=a2.btwtrans and a1.afldat between '{0}' and '{1}'"  + " and a1.artcode in ({2}) "
                        + "and a1.btw_code<>'0' and a1.magcode='{3}' group by a1.artcode";
                taxData = SqlServerHelper.GetDataTable(string.Format(SqlServerHelper.customerConn(), ip, erpCode),
                    string.Format(taxSql, date.AddMonths(-1).ToString("yyyy-MM-dd"), date.ToString("yyyy-MM-dd"),
                    string.Join(",", data.AsEnumerable().Select(dr => "'" + dr.Field<string>("ItemCode").ToStringTrim() + "'").ToArray()), warehouseCode));
                if (taxData == null) taxData = new DataTable();
            });
            task.Start();
            ItemCostFactory costFactory = new ItemCostFactory(erpCode, ip, warehouseCode, date);
            task.Wait();
            List<CountsLine> query = (from dr in data.AsEnumerable()
                         join tr in taxData.AsEnumerable()
                         on dr.Field<string>("itemCode").ToStringTrim() equals tr.Field<string>("itemCode").ToStringTrim() into ldr
                         from lr in ldr.DefaultIfEmpty()
                         select new CountsLine()
                         {
                             assortment = dr.Field<string>("assortName").ToStringTrim(),
                             itemCode = dr.Field<string>("itemCode").ToStringTrim(),
                             itemName = dr.Field<string>("itemName").ToStringTrim(),
                             inputType = dr.Field<bool>("diviable") ? "+decimal" : "+int",
                             unit = dr.Field<string>("unit").ToStringTrim(),
                             stockQty = dr.Field<double>("stockQty").ToString("0.###"),
                             price = dr.Field<double>("price").ToString("0.####"),
                             rate = lr == null ? 0 : Math.Round((lr.Field<double>("taxRate") / 100), 2),
                             itemCosts = costFactory.GetCostArray(dr.Field<string>("itemCode").ToStringTrim(), date)
                         }).ToList();
            return query;
        }

        public int SaveCounts(string erpCode, string warehouseCode, string date, string user)
        {
            string sql = "insert into tblCounts(warehouseCode,date,dbCode,type,createUser,createDate) select '{0}','{1}','{2}','purchase','{3}','{4}' "
                + "where not exists(select top 1 id from tblCounts where warehouseCode = '{0}' and date = '{1}' and dbCode = '{2}' and type = 'purchase')";
            return SqlServerHelper.Execute(SqlServerHelper.baseConn(),
                string.Format(sql, warehouseCode, date, erpCode, user, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }
    }
}
