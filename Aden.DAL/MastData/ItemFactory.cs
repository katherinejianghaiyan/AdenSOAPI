using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Aden.Model.MenuData;
using Aden.Util.Database;
using Aden.Util.Common;

namespace Aden.DAL.MastData
{
    public class ItemFactory
    {
        #region 得到一个公司/成本中心对应的所有采购物料
        public List<Item> getPurchaseItemsFromMDM(string dbName, string costCenterCode, string requiredDate)
        {
            try
            {
                string sql = " and {0} >= validfrom {1}";
                if (string.IsNullOrWhiteSpace(requiredDate))  //维护菜单时，显示所有生效过的价目表 validfrom <= now
                    sql = string.Format(sql, "GETDATE()", "");
                else  //查询某一天的价目表
                {
                    DateTime.Parse(requiredDate); //检查日期格式
                    sql = string.Format(sql, string.Format("'{0}'", requiredDate), string.Format(" and '{0}' < (validto + 1)", requiredDate));
                }

                sql = "SELECT distinct a1.crdnr,a2.Supplier_Name,b2.Class02_Code as nationGUID, b2.Class02_Name as nation," +
                    "b1.Class03_Code as menuClassGUID,b1.Class03_Name as menuClassName, "
                    + "a1.division as Company, a1.database_name as Site,a1.area,a1.city,a1.region,"
                    + " cast(a1.price as decimal(18,6)) price,cast(a1.btwper/100 as decimal(8,6)) tax,"
                    + "a1.ItemCode,a1.Items_Name as ItemName_ZH,a1.ItemEnName as ItemName_EN,a1.Condition_Code,a1.Brand itemBrand, "
                    + "a1.specification as specification,a1.unitcode,cast(a1.ConversionRate as decimal(18,3)) ConversionRate, a1.ConversionUnit, "
                    + "Case ltrim(rtrim(a1.ConversionUnit)) when 'KG' then cast((1*a1.ConversionRate)*1000 as decimal(18,0)) " +
                    "when 'L' then cast((1*a1.ConversionRate)*1000 as decimal(18,0)) else cast((1*a1.ConversionRate) as decimal(18,0)) End Weight, "
                    + "case left(a1.ItemCode, 5) when '10302' then 'Source' else 'RM' end as itemType," +
                    "convert(varchar(10),a1.validfrom,23) validfrom,convert(varchar(10),a1.validto,23) validto "
                    + "FROM (DM_D_ERP_PurchaseAgreement (nolock) a1 join d_item(nolock) a0 on a1.Itemcode = a0.Items_Code) "
                    + "join d_item_class_03 (nolock) b1 on b1.Class03_Code = a0.class03_code "
                    + "join d_item_class_02 (nolock) b2 on b2.Class02_Code = b1.Class02_Code "
                    + "join d_supplier (nolock) a2 on a2.Supplier_Code=a1.supplierCode " +
                    "where a1.ItemCode like '10%' and a1.Condition_Code='A' and a1.division = '{0}'" + sql
                    + "order by a1.crdnr,a1.ItemCode";
                sql = string.Format(sql, dbName);
                return SqlServerHelper.GetDataTable(SqlServerHelper.purchasepriceconn(), sql).AsEnumerable().
                Select(dr => new Item()
                {
                    itemGuid = "",
                    supplierCode = dr.Field<string>("crdnr").ToStringTrim(),
                    supplierName = dr.Field<string>("Supplier_Name").ToStringTrim(),
                    nation = dr.Field<string>("nation").ToStringTrim(),
                    nationGUID = dr.Field<string>("nationGUID").ToStringTrim(),
                    menuClassName = dr.Field<string>("menuClassName").ToStringTrim(),
                    menuClassGUID = dr.Field<string>("menuClassGUID").ToStringTrim(),
                    itemCostperDish = dr.Field<decimal?>("Price").ToNumber<decimal>(),
                    //== null ? 0 : dr.Field<decimal>("Price"),//dr.Field<decimal?>("itemCostperDish") == null ? 0 : dr.Field<decimal>("itemCostperDish"),
                    company = dr.Field<string>("Company").ToStringTrim(),
                    itemCode = dr.Field<string>("ItemCode").ToStringTrim(),
                    itemName_ZH = dr.Field<string>("ItemName_ZH").ToStringTrim(),
                    itemName_EN = dr.Field<string>("ItemName_EN").ToStringTrim(),
                    itemUnitCode = dr.Field<string>("unitcode").ToStringTrim(),
                    specification = dr.Field<string>("specification").ToStringTrim(),
                    unitcode = dr.Field<string>("unitcode").ToStringTrim(),
                    itemType = dr.Field<string>("itemType").ToStringTrim(),
                    purchasePrice = dr.Field<decimal?>("Price") == null ? 0 : dr.Field<decimal>("Price"),
                    tax = dr.Field<decimal?>("tax") == null ? 0 : dr.Field<decimal>("tax"),
                    purchaseUnitcode = dr.Field<string>("unitcode").ToStringTrim(),
                    weight = dr.Field<decimal?>("Weight") == null ? 0 : dr.Field<decimal>("Weight"),
                    conversionRate = dr.Field<decimal?>("ConversionRate") == null ? 0 : dr.Field<decimal>("ConversionRate"),
                    conversionUnit = dr.Field<string>("ConversionUnit").ToStringTrim(),
                    validfrom = dr.Field<string>("validfrom"),
                    validto = dr.Field<string>("validto").ToStringTrim(),
                    itemStatus = ((dr.Field<string>("validto").ToDateTime().ToString("yyyy-MM-dd")).CompareTo(DateTime.Now.ToString("yyyy-MM-dd")))>0?"A":"B" //dr.Field<string>("itemStatus").ToStringTrim()
                }).ToList();
            }
            catch(Exception e)
            {
                throw e;
            }
            }
        public List<Item> getPurchaseItems(string dbName, string costCenterCode, string requiredDate)
        {
            try
            {

                List<Item> data = getPurchaseItemsFromMDM(dbName,costCenterCode,requiredDate);

                List<dynamic> lstSuppliers = null;
                var qry = data.Select(q => new { a = q, b = (dynamic)null });

                //获取指定Site的价目表，则不加新增原料
                if (!string.IsNullOrWhiteSpace(costCenterCode))
                {
                    lstSuppliers = new MastData.SupplierFactory().getSuppliers(dbName, costCenterCode, DateTime.Parse(requiredDate));

                    qry = (from a in data
                           from b in lstSuppliers
                           where a.supplierCode == b.supplierCode &&
                               (a.nationGUID == b.class_Code || a.menuClassGUID == b.class_Code)
                           select new { a, b });
                }

                var categories = qry.GroupBy(p => new
                {
                    ItemGUID = p.a.itemGuid,
                    nation = p.a.nation,
                    nationGUID = p.a.nationGUID,
                    menuClassName = p.a.menuClassName,
                    menuClassGUID = p.a.menuClassGUID,
                    Company = p.a.company,
                    ItemCode = p.a.itemCode,
                    ItemName_ZH = p.a.itemName_ZH,
                    ItemName_EN = p.a.itemName_EN,
                    specification = p.a.specification,
                    itemType = p.a.itemType,
                    ConversionRate = p.a.conversionRate,
                    ConversionUnit = p.a.conversionUnit
                }).Select(r =>
                {
                    //1 维护菜单时，选择最接近的价目表
                    var g = r.OrderByDescending(q => DateTime.Parse(q.a.validto)).Select(q => q.a).FirstOrDefault();
                    IEnumerable<Item> suppliers = null;
                    //3 Site指定供应商
                    if (!string.IsNullOrWhiteSpace(costCenterCode))//(lstSuppliers != null)//有明确的成本中心
                    {
                        //多个供应商新消息
                        suppliers = r.Where(q => q.b.class_Code == r.Key.menuClassGUID).Select(q => q.a); //匹配第三级别分类
                        if (!suppliers.Any()) //没有第三级分类，匹配第二级别分类
                            suppliers = r.Where(q => q.b.class_Code == r.Key.nationGUID).Select(q => q.a);
                        
                        //取出一个供应商，先考虑二级分类匹配，再考虑默认供应商
                        g = r.OrderBy(q => q.b.class_Code == r.Key.menuClassGUID ? 0 : 1).ThenBy(q => q.b.defaultSupplier ? 0 : 1)  
                            .Select(q => {
                                q.a.directBuy = q.b.directBuy;
                                q.a.addToMenu = q.b.addToMenu;
                                q.a.defaultSupplier = q.b.defaultSupplier;

                                return q.a;
                            }).FirstOrDefault();
                    }

                    return new Item()
                    {
                        supplierCode = g.supplierCode,
                        supplierName = g.supplierName,
                        defaultSupplier = g.defaultSupplier,
                        itemGuid = r.Key.ItemGUID,
                        nation = r.Key.nation,
                        nationGUID = r.Key.nationGUID,
                        menuClassName = r.Key.menuClassName,
                        menuClassGUID = r.Key.menuClassGUID,
                        itemCostperDish = g.itemCostperDish,
                        weight = g.weight,
                        company = r.Key.Company,
                        itemCode = r.Key.ItemCode,
                        itemName_ZH = r.Key.ItemName_ZH,
                        itemName_EN = r.Key.ItemName_EN,
                        itemUnitCode = g.purchaseUnitcode,
                        addToMenu = g.addToMenu,
                        directBuy = g.directBuy,
                        specification = r.Key.specification,
                        unitcode = g.unitcode,
                        purchaseUnitcode = g.purchaseUnitcode,
                        itemType = r.Key.itemType,
                        purchasePrice = g.purchasePrice,
                        tax = g.tax,
                        conversionRate = r.Key.ConversionRate,
                        conversionUnit = r.Key.ConversionUnit,
                        validto = g.validto,
                        itemStatus =g.itemStatus,
                        suppliers = string.IsNullOrWhiteSpace(costCenterCode) ? null : suppliers.ToList()
                    };
                }).ToList();

                if ((string.IsNullOrWhiteSpace(costCenterCode))) return categories;

                //调用Chris的方法，ItemCode, Supplier
                var itemSupplier = new MenuOrder.WeeklyMenuFactory().GetItemSupplierMapping(costCenterCode, requiredDate);

                if (itemSupplier != null)
                {
                    // 匹配
                    categories = categories.GroupJoin(itemSupplier,
                        adj => new { ITEMCODE = adj.itemCode },
                        tmp => new { ITEMCODE = tmp.itemCode },
                        (adj, tmp) =>
                        {
                            bool check = tmp == null || tmp.Count() == 0;                        
                            // supplierCode
                            adj.supplierCode = check ? adj.supplierCode : tmp.FirstOrDefault().supplierCode;
                            // supplierName
                            adj.supplierName = check ? adj.supplierName : tmp.FirstOrDefault().supplierName;
                            // conversionUnit
                            adj.conversionUnit = check ? adj.conversionUnit : tmp.FirstOrDefault().conversionUnit;
                            // conversionRate
                            adj.conversionRate = check ? adj.conversionRate.ToDecimal() : tmp.FirstOrDefault().conversionRate.ToDecimal();
                            // purchasePrice
                            adj.purchasePrice = check ? adj.purchasePrice.ToDecimal() : tmp.FirstOrDefault().purchasePrice.ToDecimal();
                            // tax
                            adj.tax = check ? adj.tax.ToDecimal() : tmp.FirstOrDefault().purchaseTax.ToDecimal();
                            // purchaseUnit
                            adj.purchaseUnitcode = check ? adj.purchaseUnitcode.ToString() : tmp.FirstOrDefault().purchaseUnit.ToString();

                            return adj;
                        }).ToList();
                }

                return categories;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region 得到一个公司当前有效的所有采购物料分类
        //purchaseType: addToMenu(加菜时可以选择的采购物料)
        public List<ItemClass> getItemClass(string companyCode, string costCenterCode, string date, string purchaseType = null)
        {
            try
            {
                //可以添加到菜单的产品大类

                List<dynamic> lstItemClass = null;
                if(purchaseType.ToStringTrim().ToLower() == "addtomenu")
                    lstItemClass = (new MastData.SupplierFactory()).getSuppliers(companyCode, costCenterCode, DateTime.Parse(date))
                        .Where(q => q.addToMenu).ToList();

                List<Item> list = getPurchaseItems(companyCode, costCenterCode, date);
                if (list == null || !list.Any()) return null;
                //获取最外层Menu, 即所有父GUID为空

                // = list.OrderBy(q => q.nationGUID).ThenBy(q => q.menuClassGUID).Select(q=>q);
                if (lstItemClass != null)
                    list = (from d in list
                            from a in lstItemClass
                            where d.supplierCode == a.supplierCode &&
                                (d.nationGUID == a.class_Code || d.menuClassGUID == a.class_Code)
                            select d).ToList();

                /*
                 * 
                    (from d in list.OrderBy(q => q.nationGUID).ThenBy(q => q.menuClassGUID)//dt.AsEnumerable()
                    from a in lstItemClass
                     where d.supplierCode == a.supplierCode &&
                       (d.nationGUID == a.class_Code || d.menuClassGUID == a.class_Code)
                       */
               

                return getItemClass(list);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 得到一个所有采购物料分类
        //purchaseType: addToMenu(加菜时可以选择的采购物料)
        public List<ItemClass> getItemClass(string companyCode)
        {
            string sql = "select distinct a3.Class02_Code,a4.Class02_Name, a3.Class03_Code,a3.Class03_Name from " +
               // "from (SELECT distinct a2.class03_code  " +
               // "FROM DM_D_ERP_PurchaseAgreement a1,d_item a2 " +
               // "WHERE a1.itemcode LIKE '10%' AND a1.validfrom <= getdate() AND a1.division = '{0}' AND a1.itemcode = a2.Items_Code) a1," +
                "d_item_class_03 a3,d_item_class_02 a4 where " +
               // "WHERE a1.Class03_Code = a3.Class03_Code " +
                "a3.class01_code='10' "+
                "and a3.Class02_Code = a4.Class02_Code " +
                "order by a3.Class02_Code,a3.Class03_Code";

            sql = string.Format(sql, companyCode);
            var data = SqlServerHelper.GetDataTable(SqlServerHelper.purchasepriceconn(), sql).AsEnumerable().
                Select(dr => new Item()
                {
                    nation = dr.Field<string>("Class02_Name").ToStringTrim(),
                    nationGUID = dr.Field<string>("Class02_Code").ToStringTrim(),
                    menuClassName = dr.Field<string>("Class03_Name").ToStringTrim(),
                    menuClassGUID = dr.Field<string>("Class03_Code").ToStringTrim(),

                }).ToList();
            return getItemClass(data);
         }
          #endregion

        private List<ItemClass> getItemClass(List<Item> list)
        {
            try
            {
                int index = 1;
                var data2 = from d in list.OrderBy(q => q.nationGUID).ThenBy(q => q.menuClassGUID)
                            group d by new
                            {
                                value = d.nationGUID,//d.Field<string>("value2").ToStringTrim(),
                                label = d.nation,//d.Field<string>("label2").ToStringTrim(),
                            } into g
                            select new ItemClass()
                            {
                                itemCodeId = index++.ToString(),
                                value = g.Key.value,
                                label = g.Key.label,
                                disabled = "",
                                children = g.Select(q => new { value = q.menuClassGUID, label = q.menuClassName })
                                 .Distinct().Select(q =>  //GetnewChildMenus(g)
                                {
                                     int x = 0;

                                     return new ItemClass()
                                     {
                                         itemCodeId = x++.ToString(),
                                         value = q.value,//dr.Field<string>("value3").ToStringTrim(),
                                        label = q.label//dr.Field<string>("label3").ToStringTrim()
                                    };
                                 }).ToList(),
                            };

                return data2.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
