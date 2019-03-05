using Aden.Model.MastData;
using Aden.Model.Order.Purchase;
using Aden.Util.Common;
using Aden.Util.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Aden.DAL.Order
{
    public class PurchaseFactory
    {
        /// <summary>
        /// 根据用户和类型返回采购订单允许操作的天数
        /// </summary>
        /// <param name="type">采购订单类型,周单或调整单</param>
        /// <param name="userGuid">用户唯一标识</param>
        /// <returns>时间段</returns>
        public int GetOrderDateRange(string type, string userGuid)
        {
            string sql = "select top 1 allowDays from tblPurchaseAction where status=1 and type='" + type 
                + "' and userGuid='" + userGuid + "'";
            return SqlServerHelper.GetDataScalar(SqlServerHelper.baseConn(), sql).ToInt();
        }

        /// <summary>
        /// 根据订购日期和仓库代码获取供应商列表对象
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库IP</param>
        /// <param name="langCode">语言代码</param>
        /// <param name="orderDate">订购日期</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <returns></returns>
        public List<Supplier> GetSupplierListFromPriceList(string erpCode, string ip, string langCode, string orderDate, string warehouseCode)
        {
            StringBuilder sql = new StringBuilder("select distinct ltrim(a2.crdcode) as code,(case when ('" + langCode +"' ='zh-CN') then " 
            +"(case when(ISNULL(cmp_fadd1, '') = '')then cmp_name else cmp_fadd1 end) else cmp_name end) as name "
            +"from staffl (nolock)a1,cicmpy(nolock) a2,voorrd(nolock) a3,itemaccounts(nolock) a4 where a2.blocked = 0 "
            +"and a1.prijslijst = 'P_CNY' and a1.accountid = a2.cmp_wwn and a1.artcode = a3.artcode and a2.crdnr = a4.crdnr "
            +"and a4.itemcode = a1.artcode and a1.validfrom <= '"+ orderDate + "' and a1.validto >= '"
            + orderDate +"' and a3.magcode = '"+ warehouseCode + "' order by code");

            return SqlServerHelper.GetEntityList<Supplier>(string.Format(SqlServerHelper.customerConn(), ip, erpCode), sql.ToString());
        }

        /// <summary>
        /// 从采购订单中获取供应商信息,日单使用
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库IP</param>
        /// <param name="langCode">语言代码</param>
        /// <param name="orderDate">日期</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="costCenterCode">成本中心代码</param>
        /// <returns></returns>
        public List<Supplier> GetSupplierListFromOrder(string erpCode, string ip, string langCode, string orderDate, string warehouseCode, string costCenterCode)
        {
            string sql = "select distinct a2.SupplierCode from purchaseOrderhead a1,PurchaseOrderLine a2 where a1.HeadGuid = a2.HeadGuid and a1.dbCode = '{0}' "
                + "and a1.OrderDate = '{1}' and a2.WarehouseCode = '{2}' and a2.CostCenter = '{3}'";
            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.baseConn(), string.Format(sql, erpCode, orderDate, warehouseCode, costCenterCode));
            if (data == null || data.Rows.Count == 0) return null;
            List<string> supplierCodes = data.AsEnumerable().Select(dr => dr.Field<string>("supplierCode").ToStringTrim()).ToList();
            return GetSupplierData(erpCode, ip, langCode, supplierCodes).ToEntityList<Supplier>();
        }

        /// <summary>
        /// 根据供应商获取价目表中产品价格信息
        /// </summary>
        /// <param name="erpCode">数据库代码</param>
        /// <param name="ip">数据库服务器地址</param>
        /// <param name="date">日期</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="langCode">语言代码</param>
        /// <param name="supplierCode">供应商代码</param>
        /// <returns></returns>
        public List<PurchaseItem> GetItemListFromPriceList(string erpCode, string ip, string date, string warehouseCode, string langCode, List<string> supplierCode)
        {
            DataTable dataTable = GetPriceListData(erpCode, ip, date, warehouseCode, langCode, supplierCode, null);
            List<PurchaseItem> items = new List<PurchaseItem>();
            foreach(DataRow row in dataTable.Rows)
            {
                items.Add(new PurchaseItem()
                {
                    code  = row.Field<string>("itemCode").ToStringTrim(),
                    name  = row.Field<string>("itemName").ToStringTrim(),
                    price = row.Field<double>("price"),
                    unit  = row.Field<string>("unit").ToStringTrim(),
                    currency = "￥",
                    inputType = row.Field<bool>("diviable")?"+decimal":"+int"
                });
            }
            return items;
        }

        /// <summary>
        /// 返回采购订单数据
        /// 同一天同一个营运点只有一张采购订单
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库地址</param>
        /// <param name="date">日期</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="costCenterCode">成本中心代码</param>
        /// <param name="supplierCode">供应商代码</param>
        /// <param name="langCode">语言代码</param>
        /// <returns>采购订单数据</returns>
        public List<PurchaseOrderLine> GetPurchaseOrderData(string erpCode, string ip, string date, string warehouseCode, 
            string costCenterCode, string supplierCode, string langCode)
        {
            DataTable data = GetSavedOrderData(erpCode, date, warehouseCode, costCenterCode, supplierCode);
            if (data == null || data.Rows.Count == 0) return null;
            List<string> supplierCodes = data.AsEnumerable().Select(dr => dr.Field<string>("supplierCode").ToStringTrim()).Distinct().ToList();
            DataTable supplierData = GetSupplierData(erpCode, ip, langCode, supplierCodes);
            if (supplierData == null || supplierData.Rows.Count == 0) return null;
            List<string> itemCodes = data.AsEnumerable().Select(dr => dr.Field<string>("itemCode").ToStringTrim()).Distinct().ToList();
            DataTable itemPriceData = GetPriceListData(erpCode, ip, date, warehouseCode, langCode, supplierCodes, itemCodes);
            if (itemPriceData == null || itemPriceData.Rows.Count == 0) return null;
            return (from row in data.AsEnumerable()
                    join sup in supplierData.AsEnumerable()
                    on row.Field<string>("supplierCode").ToStringTrim() equals sup.Field<string>("code").ToStringTrim()
                    join item in itemPriceData.AsEnumerable()
                    on new { supplierCode = row.Field<string>("supplierCode").ToStringTrim(), itemCode = row.Field<string>("itemCode").ToStringTrim() }
                    equals new { supplierCode = item.Field<string>("supplierCode").ToStringTrim(), itemCode = item.Field<string>("itemCode").ToStringTrim() }
                    select new PurchaseOrderLine()
                    {
                        lineGuid = row.Field<string>("lineGuid").ToStringTrim(),
                        supplierCode = row.Field<string>("supplierCode").ToStringTrim(),
                        supplierName = sup.Field<string>("name").ToStringTrim(),
                        itemCode = row.Field<string>("itemCode").ToStringTrim(),
                        itemName = item.Field<string>("itemName").ToStringTrim(),
                        unit = item.Field<string>("unit").ToStringTrim(),
                        remark = row.Field<string>("remark").ToStringTrim(),
                        price = item.Field<double>("price").ToString(),
                        qty = row.Field<decimal>("qty").ToString(),
                        currency = "￥",
                        inputType = item.Field<bool>("diviable") ? "+decimal" : "+int"
                    }).ToList();
                        
        }

        /// <summary>
        /// 新增采购订单
        /// </summary>
        /// <param name="lines">订单行</param>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="orderDate">日期</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="costCenterCode">成本中心代码</param>
        /// <param name="description">说明</param>
        /// <param name="method">订单方式</param>
        /// <param name="employeeId">UserGuid</param>
        /// <param name="processDate">处理日期</param>
        public void AddPurchaseOrder(List<PurchaseOrderLine> lines, string erpCode, string orderDate, string warehouseCode,
           string costCenterCode, string description, string method, string employeeId, string processDate)
        {
            string insertHeadSql = "insert PurchaseOrderHead(HeadGuid,DBCode,OrderDate,OrderDescription,OrderMethod,"
                + "CreateUser,CreateDate) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}');";
            string insertLineSql = "insert PurchaseOrderLine(LineGuid,HeadGuid,LineNumber,SupplierCode,WarehouseCode,ItemDescription,"
                + "ItemCode,Unit,CostCenter,Remark) values('{0}','{1}','{2}','{3}','{4}',N'{5}','{6}','{7}','{8}',N'{9}');"
                + "insert into PurchaseOrderLineDetail(LineGuid,Qty,Price,CreateUser,CreateDate) values('{0}','{10}','{11}','{12}','{13}');";
            string headGuid = Guid.NewGuid().ToString().ToLower().Replace("-", "");
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(insertHeadSql, headGuid, erpCode, orderDate, description, method, employeeId, processDate);
            foreach(PurchaseOrderLine line in lines)
            {
                sql.AppendFormat(insertLineSql, Guid.NewGuid().ToString().ToLower().Replace("-", ""), headGuid, 0, line.supplierCode,
                    warehouseCode, line.itemName, line.itemCode, line.unit, costCenterCode, line.remark.Equals("none") ? "" : line.remark, line.qty, line.price, employeeId,
                    processDate);
            }
            SqlServerHelper.Execute(SqlServerHelper.baseConn(), sql.ToString());
        }

        /// <summary>
        /// 更新采购订单
        /// </summary>
        /// <param name="lines">需更新的订单行,lineGuid 不能为空</param>
        /// <param name="employeeId">UserGuid</param>
        /// <param name="processDate">处理日期</param>
        public void UpdatePurchaseOrder(List<PurchaseOrderLine> lines, string employeeId, string processDate)
        {
            string updateRemarkSql = "update PurchaseOrderLine set Remark='{0}' where LineGuid='{1}';";
            string updateLineDetailSql = "update PurchaseOrderLineDetail set DeleteUser='{0}',DeleteDate='{1}' where LineGuid='{2}';"
                + "insert into PurchaseOrderLineDetail(LineGuid,Qty,Price,CreateUser,CreateDate) values('{2}','{3}','{4}','{0}','{1}');";
            StringBuilder sql = new StringBuilder();
            foreach(PurchaseOrderLine line in lines)
            {
                if (!line.remark.ToStringTrim().Equals("none")) sql.AppendFormat(updateRemarkSql, line.remark.ToStringTrim(), line.lineGuid);
                double qty = line.qty.ToDouble();
                if (qty >= 0) sql.AppendFormat(updateLineDetailSql, employeeId, processDate, line.lineGuid, qty, line.price);
            }
            SqlServerHelper.Execute(SqlServerHelper.baseConn(), sql.ToString());
        }

        /// <summary>
        /// 保存采购收货数据
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="erpCode"></param>
        /// <param name="receiptDate"></param>
        /// <param name="warehouseCode"></param>
        /// <param name="supplierCode"></param>
        /// <param name="poType"></param>
        /// <param name="employeeId"></param>
        public bool AddPurchaseReceipt(List<PurchaseReceiptLine> lines, string erpCode, string receiptDate, 
            string warehouseCode, string supplierCode, string poType, string employeeId, string processDate)
        {
            StringBuilder sql = new StringBuilder();
            string headSql = "declare @guid char(32) select @guid = headGuid from tblPurchaseReceiptHead where dbCode='{0}' "
                + "and warehouseCode='{1}' and receiptDate='{2}' and supplierCode='{3}' and poType='{4}' begin "
                + "if (isnull(@guid, '') = '') begin select @guid = lower(replace(newid(), '-', ''));"
                + "insert into tblPurchaseReceiptHead(headGuid, dbCode, warehouseCode, receiptDate,supplierCode, poType,"
                + "createUser, createDate) values(@guid, '{0}', '{1}', '{2}','{3}', '{4}', '{5}', '{6}');";
            sql.AppendFormat(headSql, erpCode, warehouseCode, receiptDate, supplierCode, poType, employeeId, processDate);
            string lineSql = "insert into tblPurchaseReceiptLine(headGuid,itemCode,itemName,qty) values(@guid,'{0}',N'{1}','{2}');";
            foreach(PurchaseReceiptLine line in lines)
                sql.AppendFormat(lineSql, line.itemCode, line.itemName, line.receiptQty);
            sql.Append("insert into tblSchdule(type,keyValue,processStatus) values('Receipt',@guid,'P');");
            sql.Append("end end");
            int count = SqlServerHelper.Execute(SqlServerHelper.baseConn(), sql.ToString());
            if(count>0) return true;
            return false;
        }

        /// <summary>
        /// 获取收货供应商日期数据
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库IP地址</param>
        /// <param name="langCode">语言代码</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>存在采购订单的供应商收货日期数据</returns>
        public List<PurchaseReceiptSupplierDate> GetReceiptSupplierDate(string erpCode, string ip, string langCode, 
            string warehouseCode, string startDate, string endDate)
        {
            string sql = "select c.poType,c.code,c.name,c.date,case when c.blocked=1 or c.flag=0 then 0 else 1 end statusCode from "
                      + "(select d1.poType,d1.blocked, d1.code, d1.name, d1.date,case when sum(d1.ordproc) < 0 then 0 else 1 end flag "
                      + "from (select distinct a1.bstwijze poType,ltrim(a2.crdcode) as code,case when '{0}' = 'zh-CN' then "
                      + "case when isnull(a2.cmp_fadd1, '') = '' then a2.cmp_name else a2.cmp_fadd1 end else a2.cmp_name end as name,"
                      + "a2.blocked, convert(char(10), a1.orddat, 120) date,case when a1.ordbv_afgd = 0 or a1.fiattering <> 'J' "
                      + "or a3.prijslijst not like 'P_%' then - 1 else 0 end ordproc, sum(a3.esr_aantal) qty from orkrg (nolock)a1,"
                      + "cicmpy(nolock) a2, orsrg(nolock) a3  where a1.ord_soort = 'B' and a1.status = 'V' and a1.afgehandld = 0 "
                      + "and a1.freefield5 = 0 and resulttype <> 'R' and a1.magcode = '{1}' "
                      + "and ltrim(rtrim(a1.crdnr))=ltrim(rtrim(a2.crdnr)) and a1.bstwijze in ('001', '003') "
                      + "and a1.ordernr = a3.ordernr and a1.orddat = a3.afldat and a3.esr_aantal <> 0 and a3.aant_gelev = 0 "
                      + "and a1.orddat >= '{2}' and a1.orddat <= '{3}' group by a1.bstwijze,a2.blocked,a2.crdcode,case when '{0}' = 'zh-CN' then "
                      + "case when isnull(a2.cmp_fadd1, '') = '' then a2.cmp_name else a2.cmp_fadd1 end else a2.cmp_name end,"
                      + "convert(char(10), a1.orddat, 120),case when a1.ordbv_afgd = 0 or a1.fiattering <> 'J' "
                      + "or a3.prijslijst not like 'P_%' then - 1 else 0 end) d1 group by d1.poType,d1.blocked, d1.code, d1.name,"
                      + "d1.date having sum(qty) > 0) c order by c.date,c.code,c.name";
            DataTable data = SqlServerHelper.GetDataTable(string.Format(SqlServerHelper.customerConn(), ip, erpCode),
                string.Format(sql, langCode, warehouseCode, startDate, endDate));
            if (data == null || data.Rows.Count == 0) return null;
            DataTable procData = GetReceiptSchdule(warehouseCode, startDate, endDate);
            if (procData == null || procData.Rows.Count == 0) return data.ToEntityList<PurchaseReceiptSupplierDate>();
            return (from dr in data.AsEnumerable()
                    join dp in procData.AsEnumerable()
                    on new
                    {
                        supplier = dr.Field<string>("code").ToStringTrim(),
                        date = dr.Field<string>("date")
                    }
                    equals new
                    {
                        supplier = dp.Field<string>("supplierCode").ToStringTrim(),
                        date = dp.Field<DateTime>("receiptDate").ToString("yyyy-MM-dd")
                    } into ldr
                    from ld in ldr.DefaultIfEmpty()
                    select new PurchaseReceiptSupplierDate()
                    {
                        poType = dr.Field<string>("poType").ToStringTrim(),
                        code = dr.Field<string>("code").ToStringTrim(),
                        name = dr.Field<string>("name").ToStringTrim(),
                        date = dr.Field<string>("date").ToStringTrim(),
                        statusCode = ld == null ? dr.Field<int>("statusCode") : 2
                    }).ToList();
        }

        /// <summary>
        /// 获取采购订单收货数据
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库IP地址</param>
        /// <param name="date">日期</param>
        /// <param name="poType">采购方式,001食品订单,003食品补单</param>
        /// <param name="supplierCode">供应商代码</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="langCode">语言代码</param>
        /// <returns>返回采购收获行集合</returns>
        public List<PurchaseReceiptLine> GetPurchaseReceiptData(string erpCode, string ip, string date, 
            string poType, string supplierCode, string warehouseCode, string langCode)
        {
            string sql = "select b1.assortment,b1.itemCode,b1.itemName,b1.unit,b1.price,case when b1.diviable=1 then '+decimal' "
                       + "else '+int' end as inputType,sum(qty) as orderQty from (select case when '{0}' = 'zh-CN' then case when "
                       + "isnull(a5.description_1, '') = '' then a5.Description else a5.Description_1 end else case when "
                       + "isnull(a5.Description_0, '') = '' then a5.Description else a5.Description_0 end end assortment,a2.artcode itemCode,"
                       + "ltrim(rtrim(a2.oms45)) itemName,upper(ltrim(rtrim(a2.unitcode))) unit,a3.isfractionalloweditem diviable,"
                       + "round(a2.prijs83 * (1 + a4.btwper / 100), 4) price, a2.esr_aantal qty from orkrg (nolock)a1, orsrg(nolock) a2,"
                       + "items(nolock) a3, btwtrs(nolock) a4, ItemAssortment(nolock) a5,cicmpy (nolock) a6 where a2.btw_code = a4.btwtrans "
                       + "and a3.Assortment = a5.Assortment and(a4.exclus = 'E' or a4.btwper = 0) and a1.ord_soort = 'B' and a1.status = 'V' "
                       + "and a1.bstwijze='{1}' and a1.afgehandld = 0 and a1.freefield5 = 0 and resulttype <> 'R' "
                       + "and ltrim(rtrim(a1.crdnr)) = ltrim(rtrim(a6.crdnr)) and a6.cmp_type = 'S' and a6.blocked = 0 "
                       + "and ltrim(a6.crdcode)='{2}' and a1.magcode = '{3}' and a1.ordernr = a2.ordernr and a2.afldat='{4}' "
                       + "and a2.prijslijst like 'P_%' and a2.artcode = a3.itemcode and a1.ordbv_afgd = 1 and isnull(a2.aant_gelev,0)= 0) b1 "
                       + "group by b1.assortment,b1.itemCode,b1.itemName,b1.unit,b1.price,b1.diviable order by b1.assortment,b1.itemCode";
            DataTable data = SqlServerHelper.GetDataTable(string.Format(SqlServerHelper.customerConn(), ip, erpCode),
                string.Format(sql, langCode, poType, supplierCode, warehouseCode, date));
            if (data == null || data.Rows.Count == 0) return null;
            return data.AsEnumerable().Select(dr => new PurchaseReceiptLine()
            {
                assortment = dr.Field<string>("assortment").ToStringTrim(),
                itemCode = dr.Field<string>("itemCode").ToStringTrim(),
                itemName = dr.Field<string>("itemName").ToStringTrim(),
                price = dr.Field<double>("price").ToString(),
                unit = dr.Field<string>("unit").ToStringTrim(),
                inputType = dr.Field<string>("inputType"),
                orderQty = dr.Field<double>("orderQty").ToString("0.###")
            }).ToList();
        }


        /// <summary>
        /// 获取价目表数据
        /// supplierCodes和itemCodes都为可以输入字段
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库地址</param>
        /// <param name="date">日期</param>
        /// <param name="warehouseCode">公司代码</param>
        /// <param name="langCode">语言代码</param>
        /// <param name="supplierCodes">供应商集合</param>
        /// <param name="itemCodes">产品集合</param>
        /// <returns>价目表数据</returns>
        private DataTable GetPriceListData(string erpCode, string ip, string date, string warehouseCode, string langCode, List<string> supplierCodes, List<string> itemCodes)
        {
            StringBuilder sql = new StringBuilder();
            string supplierFilter = "1=1";
            string itemFilter = "1=1";
            if (supplierCodes != null && supplierCodes.Count > 0)
                supplierFilter = "ltrim(a4.crdcode) in (" + string.Join(",", supplierCodes.Select(s => "'" + s + "'").ToArray()) + ")";
            if (itemCodes != null && itemCodes.Count > 0)
                itemFilter = "ltrim(a1.artcode) in (" + string.Join(",", itemCodes.Select(i => "'" + i + "'").ToArray()) + ")";
            sql.AppendFormat("select a4.crdcode supplierCode,a1.artcode itemCode,(case when '{0}'='zh-CN' then "
                + "case when isnull(a3.description_1, '') = '' then a3.description else a3.description_1 end else "
                + "case when isnull(a3.description_0, '') = '' then a3.description else a3.description_0 end end) itemName,"
                + "upper(a1.unitcode) as unit, a3.isfractionAlloweditem diviable,round(a1.prijs83 * (1 + a6.btwper / 100), 4) "
                + "as price from staffl (nolock)a1, voorrd(nolock) a2, items(nolock) a3,cicmpy(nolock) a4, itemaccounts(nolock) a5,"
                + "btwtrs(nolock) a6,(select a1.accountid, a1.artcode from staffl(nolock)a1, voorrd(nolock) a2 "
                + "where a1.artcode = a2.artcode and a2.magcode = '{1}' and a1.prijslijst = 'P_CNY' and a1.validfrom <= '{2}' "
                + "and a1.validto >= '{2}' group by a1.AccountID, a1.artcode having count(*) = 1) a7 "
                + "where a5.purchasevatcode=a6.btwtrans and(a6.exclus = 'E' or a6.btwper = 0) and a1.artcode = a2.artcode "
                + "and a1.artcode = a3.itemcode and a3.condition = 'A' and a2.magcode = '{1}' and a1.accountid = a4.cmp_wwn "
                + "and a4.crdnr = a5.crdnr and a5.itemcode = a3.itemcode and a1.AccountID = a7.AccountID "
                + "and a1.artcode = a7.artcode and a1.prijslijst = 'P_CNY' and a1.validfrom <= '{2}' and a1.validto >= '{2}' "
                + "and {3} and {4}", langCode, warehouseCode, date, supplierFilter, itemFilter);
            return SqlServerHelper.GetDataTable(string.Format(SqlServerHelper.customerConn(), ip, erpCode), sql.ToString());
        }

        /// <summary>
        /// 获取已经保存的采购订单数据
        /// </summary>
        /// <param name="erpCode">ERP公司代码</param>
        /// <param name="date">日期</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="supplierCode">供应商代码</param>
        /// <returns>采购订单数据</returns>
        private DataTable GetSavedOrderData(string erpCode, string date, string warehouseCode, string costCenterCode, string supplierCode)
        {
            string filter = "1=1";
            if (!string.IsNullOrEmpty(supplierCode)) filter = "l.supplierCode='" + supplierCode + "'";
            string sql = "select l.supplierCode,l.itemCode,l.unit,l.remark,l.lineguid,l.itemdescription as itemName,"
                + "d.qty from PurchaseOrderLine as L join PurchaseOrderLineDetail as D on L.LineGuid = D.LineGuid "
                + "join PurchaseOrderHead as H on H.HeadGuid = L.HeadGuid where l.warehousecode = '{0}' and l.costcenter='{1}' and {4} "
                + "and orderdate = '{2}' and isnull(D.deleteuser,'')='' and isnull(D.deletedate,'')='' and H.dbcode = '{3}' "
                + "order by L.supplierCode,l.itemcode";
            return SqlServerHelper.GetDataTable(SqlServerHelper.baseConn(), string.Format(sql, warehouseCode, costCenterCode, date, erpCode, filter));
        }

        /// <summary>
        /// 获取供应商数据
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库地址</param>
        /// <param name="langCode">语言代码</param>
        /// <param name="supplierCodes">供应商集合</param>
        /// <returns>供应商代码名称数据</returns>
        private DataTable GetSupplierData(string erpCode, string ip, string langCode, List<string> supplierCodes)
        {
            string filter = "1=1";
            if (supplierCodes != null && supplierCodes.Count > 0)
                filter = "ltrim(crdcode) in (" + string.Join(",", supplierCodes.Select(s => "'" + s + "'").ToArray()) + ")";
            string sql = "select ltrim(crdcode) as code,case when '{0}'='zh-CN' then case when isnull(cmp_fadd1,'')= '' then cmp_name else cmp_fadd1 end "
                + "else cmp_name end as name from cicmpy (nolock) where cmp_type = 'S' and blocked = 0 and {1}";
            return SqlServerHelper.GetDataTable(string.Format(SqlServerHelper.customerConn(), ip, erpCode), string.Format(sql, langCode, filter));
        }

        /// <summary>
        /// 获取采购收货处理情况
        /// </summary>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>DataTable</returns>
        private DataTable GetReceiptSchdule(string warehouseCode, string startDate, string endDate)
        {
            string sql = "select a1.receiptDate,a1.supplierCode,a1.poType from tblPurchaseReceiptHead a1,"
                + "tblSchdule a2 where a1.headGuid = a2.keyValue and a2.type = 'Receipt' and a2.processStatus<>'C' "
                + "and a1.warehouseCode='{0}' and a1.receiptDate>='{1}' and a1.receiptDate<='{2}'";
            return SqlServerHelper.GetDataTable(SqlServerHelper.baseConn(), string.Format(sql, warehouseCode, startDate, endDate));
        }
    }
}
