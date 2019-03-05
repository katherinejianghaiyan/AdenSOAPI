using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Aden.Util.Common;
using Aden.Util.Database;
using Aden.Model.Order.Warehouse;

namespace Aden.DAL.Order
{
    public class ItemCostFactory
    {
        private DataTable dtIns = null;
        private DataTable dtOuts = null;
        private DataTable dtRets = null;
        private DateTime startDate = DateTime.Parse("1900-01-01");

        public ItemCostFactory() { }
        public ItemCostFactory(string erpCode, string ip, string warehouseCode, DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            SetData(erpCode, ip, warehouseCode, this.startDate, endDate);
        }

        public ItemCostFactory(string erpCode, string ip, string warehouseCode, DateTime date) 
            : this(erpCode, ip, warehouseCode, date, date) { }

        private void SetData(string erpCode, string ip, string warehouseCode, DateTime startDate, DateTime endDate)
        {
            List<SqlDic> dics = new List<SqlDic>();

            #region enddate及之前的入库明细
            string sql = "SELECT gbkmut.artcode ItemCode,gbkmut.datum Transdate,round(SUM(isnull(gbkmut.aantal,0)),6) Qty, round(SUM(isnull(bdr_hfl,0)),6) Amt "
                     + "FROM gbkmut (nolock),items (nolock),grtbk (nolock) "
                     + "WHERE gbkmut.warehouse='{0}' and  gbkmut.reknr = grtbk.reknr AND gbkmut.artcode=items.itemcode and "
                     //+ "(gbkmut.aantal > 0 or (gbkmut.transsubtype='J' and gbkmut.datum >='{1}')) " 退货
                     + "round(isnull(gbkmut.aantal,0),6) > 0.0000001 "
                     + "AND gbkmut.artcode is not null and gbkmut.transtype IN ('X', 'N', 'C', 'P') and grtbk.omzrek='G' AND " // IN ('G', 'K', 'N')
                     + "gbkmut.datum <'{2}' AND gbkmut.remindercount <= 13 and "
                     // + "((Items.Condition = 'A' AND Items.Type IN ('S','B','C')) OR "
                     + "((Items.Type IN ('S','B','C')) OR "
                     + "( Items.Type = 'S' AND Items.PackageDescription IN (SELECT Unit FROM ItemUnits (nolock) WHERE UnitType = 'T') AND "
                     + "items.IsSerialNumberItem = 1 AND Items.IsSalesItem = 1)) "
                     + "and grtbk.bal_vw='B' and grtbk.debcrd='D' "
                     + "group by gbkmut.artcode,gbkmut.datum having abs(round(SUM(isnull(gbkmut.aantal,0)),6))>0.0000001";

            sql = string.Format(sql, warehouseCode, startDate, endDate.AddDays(1));
            dics.Add(new SqlDic()
            {
                TableName = "InQtys",
                Sql = sql
            });
            #endregion

            #region startdate的期末，[startdate, enddate]出库明细,
            sql = "select gbkmut.artcode itemcode,(case when gbkmut.datum < '{1}' then '1900-1-1' else gbkmut.datum end) Transdate,-round(SUM(isnull(gbkmut.aantal,0)),6) qty "
                    + "FROM gbkmut (nolock),items (nolock),grtbk (nolock) "
                    + "WHERE gbkmut.warehouse='{0}' and gbkmut.reknr = grtbk.reknr AND gbkmut.artcode=items.itemcode and "
                    + "gbkmut.transtype IN ('X', 'N', 'C', 'P') and grtbk.omzrek='G' AND round(isnull(gbkmut.aantal,0),6) < 0.0000001 and "
                    + "(gbkmut.datum <'{1}' or gbkmut.transsubtype<>'J') " // IN ('G', 'K', 'N')
                    + "AND gbkmut.datum <'{2}' AND gbkmut.remindercount <= 13 and "
                    // + "((Items.Condition = 'A' AND Items.Type IN ('S','B','C')) OR "
                    + "((Items.Type IN ('S','B','C')) OR "
                    + "( Items.Type = 'S' AND Items.PackageDescription IN (SELECT Unit FROM ItemUnits (nolock) WHERE UnitType = 'T') AND "
                    + "Items.IsSerialNumberItem = 1 AND Items.IsSalesItem = 1)) "
                    + "and grtbk.bal_vw='B' and grtbk.debcrd='D' "
                    + "GROUP BY gbkmut.artcode,(case when gbkmut.datum < '{1}' then '1900-1-1' else gbkmut.datum end) "
                    + "having abs(round(SUM(isnull(gbkmut.aantal,0)),6))>0.0000001";

            sql = string.Format(sql, warehouseCode, startDate, endDate.AddDays(1));
            dics.Add(new SqlDic()
            {
                TableName = "OutQtys",
                Sql = sql
            });
            #endregion

            #region [startdate, enddate]退货明细

            sql = "SELECT gbkmut.artcode ItemCode,gbkmut.datum Transdate,-round(SUM(isnull(gbkmut.aantal,0)),6) Qty "
                + "FROM gbkmut (nolock),items (nolock),grtbk (nolock) "
                + "WHERE gbkmut.warehouse='{0}' and  gbkmut.reknr = grtbk.reknr AND gbkmut.artcode=items.itemcode and "
                + "(gbkmut.transsubtype='J' and gbkmut.datum >='{1}') "
                + "AND gbkmut.artcode is not null and gbkmut.transtype IN ('X', 'N', 'C', 'P') and grtbk.omzrek='G' AND " // IN ('G', 'K', 'N')
                + "gbkmut.datum <'{2}' AND gbkmut.remindercount <= 13 and "
                // + "((Items.Condition = 'A' AND Items.Type IN ('S','B','C')) OR "
                + "((Items.Type IN ('S','B','C')) OR "
                + "( Items.Type = 'S' AND Items.PackageDescription IN (SELECT Unit FROM ItemUnits (nolock) WHERE UnitType = 'T') AND "
                + "items.IsSerialNumberItem = 1 AND Items.IsSalesItem = 1)) "
                + "and grtbk.bal_vw='B' and grtbk.debcrd='D' "
                + "group by gbkmut.artcode,gbkmut.datum having abs(round(SUM(isnull(gbkmut.aantal,0)),6))>0.0000001";

            sql = string.Format(sql, warehouseCode, startDate, endDate.AddDays(1));
            dics.Add(new SqlDic()
            {
                TableName = "RetQtys",
                Sql = sql
            });
            #endregion

            DataSet ds = SqlServerHelper.GetDataSet(string.Format(SqlServerHelper.customerConn(), ip, erpCode), dics);
            double dqty = 0;
            string itemcode = "";
            Func<string, double, double> funcOpenningQty = (code, qty) =>
            {
                if (itemcode != code)
                {
                    itemcode = code;
                    dqty = 0;
                }
                dqty += qty.ToDouble();
                return dqty;
            };

            dqty = 0;
            itemcode = "";

            var qry = (from d1 in ds.Tables["InQtys"].AsEnumerable()
                       orderby d1.Field<string>("itemcode"), d1.Field<DateTime>("transdate")
                       join d2 in ds.Tables["OutQtys"].Select("Transdate=#1900-1-1#").AsEnumerable() on d1.Field<string>("itemcode") equals d2.Field<string>("itemcode") into d12G
                       from d12 in d12G.DefaultIfEmpty()
                       let SumInQty = funcOpenningQty(d1.Field<string>("itemcode"), d1.Field<double>("qty")) - (d12 == null ? 0 : d12.Field<double>("qty").ToDouble())
                       where SumInQty > 0
                       select new
                       {
                           ItemCode = d1.Field<string>("itemcode"),
                           Transdate = d1.Field<DateTime>("transdate"),
                           SumInQty,
                           Cost = Math.Round(d1.Field<double>("Amt") / d1.Field<double>("Qty"), 6),
                           Qty = (SumInQty - d1.Field<double>("qty")) < 0 ? SumInQty : d1.Field<double>("qty"),
                       }).ToList();

            dtIns = qry.AsQueryable().ConvertToDataTable();

            DataRow[] drs = ds.Tables["OutQtys"].Select("Transdate>#1900-1-1#");

            if (drs.Any()) dtOuts = drs.CopyToDataTable();

            dtRets = ds.Tables["RetQtys"];

        }

        public double GetBeginning(string ItemCode)
        {
            return GetBeginning(ItemCode, this.startDate);
        }

        public double GetBeginning(string ItemCode, DateTime date)
        {
            return GetClosing(ItemCode, date.AddDays(-1));
        }

        public double GetClosing(string ItemCode, DateTime date)
        {
            List<ItemTrans> list = GetStocks(ItemCode, date.AddDays(1), date);
            if (list == null) return 0;
            return Math.Round(list.Sum(q => q.qty * q.cost), 6);
        }

        public double GetConsume(string ItemCode, DateTime date)
        {
            return GetConsume(ItemCode, date, date);
        }

        public double GetConsume(string ItemCode, DateTime date1, DateTime date2)
        {
            double? d = GetAmts(ItemCode, date1, date2);
            if (d == null) return 0;

            return (double)d;// Math.Round(d.Sum(q => q), 6);
        }

        public double GetConsume(string ItemCode, DateTime date1, DateTime date2, double outqty)
        {
            double? d = GetAmts(ItemCode, date1, date2, outqty);
            if (d == null) return 0;

            return (double)d;// Math.Round(d.Sum(q => q), 6);
        }

        private double? GetAmts(string ItemCode, DateTime date1, DateTime date2)
        {
            if (this.startDate.CompareTo(date1) > 0 || date1.CompareTo(date2) > 0) return null;

            return GetAmts(ItemCode, date1, date2, null);
        }

        private double? GetAmts(string ItemCode, DateTime date1, DateTime date2, double? outqtys)
        {
            if (this.startDate.CompareTo(date1) > 0 || date1.CompareTo(date2) > 0) return null;

            List<ItemTrans> tmp = null;
            if (outqtys == null)
            {
                tmp = GetCosts(ItemCode, date1, date2);
            }
            else
                tmp = GetCosts(ItemCode, date1, date2, outqtys.ToDouble());
            if (tmp == null) return 0;

            return Math.Round(tmp.Sum(q => q.qty * q.cost), 6);
        }

        private List<ItemTrans> GetCosts(string ItemCode, DateTime date1, DateTime date2)
        {
            if (this.startDate.CompareTo(date1) > 0 || date1.CompareTo(date2) > 0) return null;

            if (dtOuts == null) return null;

            double outqty = Math.Round(dtOuts.AsEnumerable()
                .Where(q => q.Field<string>("itemcode") == ItemCode && q.Field<DateTime>("transdate").CompareTo(date1) >= 0
                && q.Field<DateTime>("transdate").CompareTo(date2.AddDays(1)) < 0)
                .Sum(q => q.Field<double>("qty")).ToDouble(), 6);
            if (outqty == 0) return null;
            return GetCosts(ItemCode, date1, date2, outqty);//0, outqty);
        }

        private List<ItemTrans> GetCosts(string ItemCode, DateTime date1, DateTime date2, double outqty)//1, double outqty2)
        {
            if (this.startDate.CompareTo(date1) > 0 || date1.CompareTo(date2) > 0) return null;
            // outqty1 = Math.Abs(outqty1.ToDouble());
            outqty = Math.Abs(outqty.ToDouble());

            var query = from d in GetStocks(ItemCode, date1, date2)//,outqty1)
                        where Math.Round(d.sumInQty - d.qty - outqty, 6) < 0   //本次入库有对应消耗
                        orderby d.id
                        select new ItemTrans()
                        {
                            qty = Math.Round((Math.Round(outqty - d.sumInQty, 6) > 0 ? d.qty : outqty - d.sumInQty + d.qty), 6),  //总消耗<累计入库
                            cost = d.cost,
                        };
            if (query == null || query.Count() == 0) return null;

            return query.ToList();
        }

        private List<ItemTrans> GetStocks(string ItemCode, DateTime date)
        {
            return GetStocks(ItemCode, date.AddDays(1), date);
        }

        private List<ItemTrans> GetStocks(string ItemCode, DateTime date1, DateTime date2)
        {
            return GetStocks(ItemCode, date1, date2, 0);
        }

        private List<ItemTrans> GetStocks(string ItemCode, DateTime date1, DateTime date2, double outqty)
        {
            if (this.startDate.CompareTo(date1) > 0 || date1.CompareTo(date2.AddDays(1)) > 0) return null;

            outqty = Math.Abs(outqty.ToDouble());
            if (date1.CompareTo(this.startDate) > 0 && dtOuts != null) //[StartDate,date1) 数据库中已发生的消耗
                outqty += Math.Round(dtOuts.AsEnumerable()
                  .Where(q => q.Field<string>("itemcode") == ItemCode && q.Field<DateTime>("transdate").CompareTo(date1) < 0)
                  .Sum(q => q.Field<double>("qty")).ToDouble(), 6);

            //[StartDate,date2) 数据库中已发生的退货
            outqty += Math.Round(dtRets.AsEnumerable()
              .Where(q => q.Field<string>("itemcode") == ItemCode && q.Field<DateTime>("transdate").CompareTo(date2.AddDays(1)) < 0)
              .Sum(q => q.Field<double>("qty")).ToDouble(), 6);

            int x = 0;

            var query = from d in dtIns.AsEnumerable()
                        let Transdate = d.Field<DateTime>("transdate")
                        let SumInQty = d.Field<decimal>("suminqty")//.ToDouble()
                        where d.Field<string>("itemcode") == ItemCode && Transdate.CompareTo(date2.AddDays(1)) < 0 && Math.Round(SumInQty - outqty.ToDecimal(), 6) > 0
                        orderby Transdate
                        select new ItemTrans()
                        {
                            id = x++,
                            qty = Math.Round((Math.Round(SumInQty.ToDouble() - d.Field<decimal>("qty").ToDouble() - outqty, 6) > 0 ? d.Field<decimal>("qty").ToDouble() : SumInQty.ToDouble() - outqty), 6),
                            cost = Math.Round(d.Field<decimal>("cost").ToDouble(), 6),
                            sumInQty = Math.Round(SumInQty.ToDouble() - outqty, 6)
                        };
            if (query == null || query.Count() == 0) return null;
            return query.ToList();
        }

        public List<ItemQtyCost> GetCostArray(string ItemCode, DateTime date)
        {
            List<ItemTrans> list = GetStocks(ItemCode, date);
            if (list == null || list.Count() == 0) return null;
            double qty = 0;
            double cost = 0;
            List<ItemQtyCost> qtyCosts = new List<ItemQtyCost>();
            for (int i = 0; i <= list.Count(); i++)
            {
                if (i == list.Count() || cost != list[i].cost)
                {
                    if (qty != 0 || cost != 0)
                        qtyCosts.Add(new ItemQtyCost()
                        {
                            qty = qty,
                            cost = cost
                        });

                    if (i == list.Count()) break;

                    qty = list[i].qty;
                    cost = list[i].cost;
                    continue;
                }

                qty += list[i].qty;
            }
            return qtyCosts;
        }
    }
}
