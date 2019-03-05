using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Util.Common;
using Aden.Util.Database;
using Aden.Model.MenuData;

using System.Data;

namespace Aden.DAL.MenuData
{
    public class MenuDataFactory
    {

        const bool onlyFoodName = false;

        /// <summary>
        /// 菜单分类
        /// </summary>
        /// <param name="langCode"></param>
        /// <returns></returns>
        public List<Item> menuClass(Item menu)
        {
            try
            {
                string sql = "select cast(a1.itemCodeId as varchar(10)) itemCodeId, a1.guid,ISNULL(a1.pguid,'') pguid, "
                + "Case '" + menu.langCode + "' when 'en' then isnull(a1.className_EN,a1.className_ZH) else isnull(a1.className_ZH,a1.className_EN) end as name,a1.className_EN as name_en, "
                + "a2.name as parentName,a3.pguid as nationGuid,a1.type,convert(varchar(20), a1.sort) as sort,isnull(a1.colorValue,'') value from tblItemClass a1 "
                + "left join (select distinct a2.guid,Case '" + menu.langCode + "' when 'en' then isnull(a2.className_EN,a2.className_ZH) else isnull(a2.className_ZH,a2.className_EN) end as name "
                + "from tblItemClass a2 (nolock)) a2 on a2.guid=a1.pguid "
                + "left join tblItemClass a3 (nolock) on a3.guid=a2.guid "
                + "where a1.status = 'active' ";

                Item data = new Item();
                data.nationClass = NationList(sql);
                data.menuClass = menuList(sql);
                data.rmClass = rmList(sql);
                data.seasonClass = seasonList(sql);
                data.cookwayClass = cookwayList(sql);
                data.ItemShape = itemShape(sql);
                data.itemColorOption = ItemColor(sql);

                List<Item> list = new List<Item>();
                list.Add(data);
                return list;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取产品颜色
        /// </summary>
        /// <returns></returns>
        public List<ItemColor> ItemColor(string sql)
        {
            try
            {
                string cololrfilter = " and a1.type='ItemColor' order by a1.className_ZH";
                sql = sql + cololrfilter;

                List<ItemColor> itemColorOption = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql)
                       .AsEnumerable().Select(dr => new ItemColor()
                       {
                           guid = dr.Field<string>("guid").ToStringTrim(),
                           name = dr.Field<string>("name").ToStringTrim(),
                           value = dr.Field<string>("value").ToStringTrim(),
                           type = dr.Field<string>("type").ToStringTrim(),
                           sort = dr.Field<string>("sort").ToString().Trim(),
                           name_en=dr.Field<string>("name_en").ToStringTrim()
                       }).ToList();
                return itemColorOption;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemClass> NationList(string sql)
        {
            try
            {
                string nationfilter = "and a1.pguid is null and a1.type='Categories' "
                + "order by a1.type,a1.pguid,a1.sort,a2.name";
                string sqlNation = sql + nationfilter;

                List<ItemClass> nationList = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sqlNation)
                   .AsEnumerable().Select(dr => new ItemClass()
                   {
                       itemCodeId = dr.Field<string>("itemCodeId").ToStringTrim(),
                       guid = dr.Field<string>("guid").ToStringTrim(),
                       pguid = dr.Field<string>("pguid").ToStringTrim(),
                       name = dr.Field<string>("name").ToStringTrim(),
                       parentName = dr.Field<string>("parentName").ToStringTrim(),
                       type = dr.Field<string>("type").ToStringTrim(),
                       sort = dr.Field<string>("sort").ToString().Trim()
                   }).ToList();
                return nationList;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemClass> menuList(string sql)
        {
            try
            {
                string menuClassfilter = "and a1.pguid in (select distinct guid from tblItemClass (nolock) where type='Categories' and pguid is null) "
                            + "order by a1.type,a1.pguid,a1.sort,a2.name";
                string sqlMenu = sql + menuClassfilter;
                List<ItemClass> menuList = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sqlMenu)
                   .AsEnumerable().Select(dg => new ItemClass()
                   {
                       itemCodeId = dg.Field<string>("itemCodeId").ToStringTrim(),
                       guid = dg.Field<string>("guid").ToStringTrim(),
                       pguid = dg.Field<string>("pguid").ToStringTrim(),
                       name = dg.Field<string>("name").ToStringTrim(),
                       parentName = dg.Field<string>("parentName").ToStringTrim(),
                       type = dg.Field<string>("type").ToStringTrim(),
                       sort = dg.Field<string>("sort").ToString().Trim()
                   }).ToList();
                return menuList;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemClass> rmList(string sql)
        {
            try
            {
                string rmClassfilter = "and a1.guid not in (select distinct pguid from tblItemClass where pguid is not null) "
                + "and a3.guid not in (select distinct guid from tblItemClass (nolock) where pguid is null) "
                + "and a1.pguid is not null "
                + "order by a1.type,a1.pguid,a1.sort,a2.name";
                string sqlrmClass = sql + rmClassfilter;
                List<ItemClass> rmList = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sqlrmClass)
                    .AsEnumerable().Select(dg => new ItemClass()
                    {
                        itemCodeId = dg.Field<string>("itemCodeId").ToStringTrim(),
                        guid = dg.Field<string>("guid").ToStringTrim(),
                        pguid = dg.Field<string>("pguid").ToStringTrim(),
                        name = dg.Field<string>("name").ToStringTrim(),
                        parentName = dg.Field<string>("parentName").ToStringTrim(),
                        type = dg.Field<string>("type").ToStringTrim(),
                        sort = dg.Field<string>("sort").ToStringTrim(),
                        nationGuid = dg.Field<string>("nationGuid").ToStringTrim()
                    }).ToList();
                return rmList;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemClass> seasonList(string sql)
        {
            try
            {
                string seasonClassfilter = "and a1.type='seasonClass' order by a1.type,a1.pguid,a1.sort,a2.name";
                string sqlSeason = sql + seasonClassfilter;
                List<ItemClass> seasonList = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sqlSeason)
                    .AsEnumerable().Select(dg => new ItemClass()
                    {
                        itemCodeId = dg.Field<string>("itemCodeId").ToStringTrim(),
                        guid = dg.Field<string>("guid").ToStringTrim(),
                        pguid = dg.Field<string>("pguid").ToStringTrim(),
                        name = dg.Field<string>("name").ToStringTrim(),
                        parentName = dg.Field<string>("parentName").ToStringTrim(),
                        type = dg.Field<string>("type").ToStringTrim(),
                        sort = dg.Field<string>("sort").ToStringTrim()
                    }).ToList();
                return seasonList;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemClass> cookwayList(string sql)
        {
            try
            {
                string cookwayfilter = "and a1.type='cookwayClass' order by a1.type,a1.sort,a2.name";

                string sqlCookway = sql + cookwayfilter;
                List<ItemClass> cookwayList = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sqlCookway)
                    .AsEnumerable().Select(dg => new ItemClass()
                    {
                        itemCodeId = dg.Field<string>("itemCodeId").ToStringTrim(),
                        guid = dg.Field<string>("guid").ToStringTrim(),
                        pguid = dg.Field<string>("pguid").ToStringTrim(),
                        name = dg.Field<string>("name").ToStringTrim(),
                        parentName = dg.Field<string>("parentName").ToStringTrim(),
                        type = dg.Field<string>("type").ToStringTrim(),
                        sort = dg.Field<string>("sort").ToString().Trim()
                    }).ToList();
                return cookwayList;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public List<ItemClass> itemShape(string sql)
        {
            try
            {
                string itemshapefilter = "and a1.type='ItemShape' order by a1.type,a1.sort,a2.name";

                string sqlCookway = sql + itemshapefilter;
                List<ItemClass> itemShape = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sqlCookway)
                    .AsEnumerable().Select(dg => new ItemClass()
                    {
                        itemCodeId = dg.Field<string>("itemCodeId").ToStringTrim(),
                        guid = dg.Field<string>("guid").ToStringTrim(),
                        pguid = dg.Field<string>("pguid").ToStringTrim(),
                        name = dg.Field<string>("name").ToStringTrim(),
                        parentName = dg.Field<string>("parentName").ToStringTrim(),
                        type = dg.Field<string>("type").ToStringTrim(),
                        sort = dg.Field<string>("sort").ToString().Trim()
                    }).ToList();
                return itemShape;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public int setMenuItem(Item menu)
        {
            try
            {
                StringBuilder sqlfinal = new StringBuilder();
                if (menu.item[0].changeFlag == "new")
                {
                    if (menu.paramType[0].item == true)
                    {
                        string sqlinsert = Sql(menu);
                        sqlfinal.Append(sqlinsert);
                    }
                }
                else if (menu.item[0].changeFlag == "update")
                {
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    foreach (paramType parameter in menu.paramType)
                    {
                        if (parameter.item == true)
                        {
                            if (menu.itemGuid != null)
                            {
                                string sqlUpdateItem = string.Format("update tblItem set DeleteTime='{0}',DeleteUser='{1}' where GUID='{2}' and deletetime is null ",
                                now, menu.item[0].createUserGUID, menu.itemGuid);
                                sqlfinal.AppendFormat(sqlUpdateItem);
                            }
                            else
                            {
                                foreach (Item item in menu.item)
                                {
                                    if(item.oldGUID!="" && item.oldGUID != null)
                                    {
                                        string sqlUpdateItem = string.Format("update tblItem set DeleteTime='{0}',DeleteUser='{1}' where GUID='{2}' and deletetime is null ",
                                            now, menu.item[0].createUserGUID, item.oldGUID);
                                        sqlfinal.AppendFormat(sqlUpdateItem);
                                    }
                                }
                            }

                        }
                        if (parameter.bom == true)
                        {
                            string filter = "";
                            if (!string.IsNullOrWhiteSpace(menu.costCenterCode))
                                filter = string.Format(" and costCenterCode='{0}' ", menu.costCenterCode); //加菜后另存为食谱，有成本中心标识
                            else
                                filter = " and costCenterCode is null "; //保存菜单，无成本中心标识
                            string sqlUpdateItemBOM = string.Format("update tblItemBOM set  DeleteTime='{0}',DeleteUser='{1}' where ProductGUID='{2}'  and deletetime is null "+filter,
                            now, menu.item[0].createUserGUID, menu.itemGuid);
                            sqlfinal.AppendFormat(sqlUpdateItemBOM);
                        }
                        if (parameter.cookway == true)
                        {
                            string sqlUpdateItemProcess = string.Format("update tblItemProcess set  DeleteTime='{0}',DeleteUser='{1}' where ItemGUID='{2}' and deletetime is null and Type='cookway'",
                            now, menu.item[0].createUserGUID, menu.itemGuid);
                            sqlfinal.AppendFormat(sqlUpdateItemProcess);
                        }
                        if (parameter.menuPoint == true)
                        {
                            string sqlUpdateItemProcess = string.Format("update tblItemProcess set  DeleteTime='{0}',DeleteUser='{1}' where ItemGUID='{2}' and deletetime is null and Type='menuPoint'",
                            now, menu.item[0].createUserGUID, menu.itemGuid);
                            sqlfinal.AppendFormat(sqlUpdateItemProcess);
                        }
                        if (parameter.nutrition == true)
                        {
                            string sqlItemNutrition = string.Format("update tblItemNutrition set  DeleteTime='{0}',DeleteUser='{1}' where ItemGUID='{2}'  and deletetime is null ",
                            now, menu.item[0].createUserGUID, menu.itemGuid);
                            sqlfinal.AppendFormat(sqlItemNutrition);
                        }
                    }
                    string sqlupdate = Sql(menu);
                    sqlfinal.Append(sqlupdate);
                }
                return SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sqlfinal.ToStringTrim());
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public string Sql(Item menu)
        {
            try
            {
                StringBuilder sqlfinal = new StringBuilder();

                var GUID = menu.itemGuid;

                var createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string PurUOM = "";
                string convUOM = "";
                string SaleUOM = "";
                Decimal cost = 0;
                Decimal price = 0;

                if (menu.item[0].purUOMGUID != "") PurUOM = UOMGUID(menu.item[0].purUOMGUID);
                if (menu.item[0].saleUOMGUID != "") SaleUOM = UOMGUID(menu.item[0].saleUOMGUID);

                foreach (paramType parameter in menu.paramType)
                {
                    if (parameter.item == true)
                    {
                        var obj = menu.item.Count;
                        string sqlInsertItem = "";
                        if (GUID != null)
                        {
                            sqlInsertItem = string.Format("insert into tblItem "
                            + "(GUID, ItemCode, ItemName_ZH, ItemName_EN, itemType, ItemWeightperDish, Version,  toBuy, toSell, "
                            + "categoriesClassGUID, cookwayClassGUID, seasonClassGUID, itemColor, itemTaste, Image1, PurUOMGUID, SaleUOMGUID, "
                            + "sort, Status, createTime, CreateUser, Company,shareQty,itemShapeGUID,itemCostperdish,itemSpicy) "
                            + "values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', "
                            + "'{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}','{22}','{23}','{24}','{25}')",
                            GUID, menu.item[0].itemCode, menu.item[0].itemName_ZH, menu.item[0].itemName_EN, menu.item[0].itemType,
                            menu.item[0].weight, menu.item[0].version, menu.item[0].toBuy, menu.item[0].toSell,
                            menu.item[0].categoriesClassGUID, menu.item[0].cookwayClassGUID, menu.item[0].seasonClassGUID, menu.item[0].itemColor,
                            menu.item[0].itemTaste, "", PurUOM, SaleUOM, menu.item[0].sort, menu.item[0].itemStatus,
                            createTime, menu.item[0].createUserGUID, menu.item[0].company, menu.item[0].shareQty, menu.item[0].itemShapeGUID, menu.item[0].itemCostperDish,menu.item[0].itemSpicy.ToInt());

                            sqlfinal.AppendFormat(sqlInsertItem);
                        }
                        else if (GUID == null)
                        {
                            foreach (Item item in menu.item)
                            {
                                if (item.itemStatus == "active")
                                {
                                    sqlInsertItem = string.Format("insert into tblItem "
                                       + "(GUID, ItemCode, ItemName_ZH, ItemName_EN, itemType,toBuy, toSell, "
                                       + "Status, createTime, CreateUser, Company,itemBrand,specification,PurchaseUnitcode,ConversionUnit,ConversionRate) "
                                       + "values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', "
                                       + "'{10}', '{11}', '{12}', '{13}', '{14}', '{15}')",
                                       item.itemGuid, item.itemCode, item.itemName_ZH, item.itemName_EN, item.itemType, item.toBuy, item.toSell, item.itemStatus,
                                       createTime, item.createUserGUID, item.company, item.itemBrand,
                                       item.specification, item.purchaseUnitcode.ToUpper(), item.conversionUnit.ToUpper(), item.conversionRate);

                                    sqlfinal.AppendFormat(sqlInsertItem);
                                }
                            }

                        }

                    }

                    if (parameter.nutrition == true)
                    {
                        foreach (ItemNutrition item in menu.nutrition)
                        {
                            string sqlInsertNutrition = string.Format("insert into tblItemNutrition (ItemGUID,GUID,Name,Qty,CreateUser,createTime) values ('{0}','{1}','{2}','{3}','{4}','{5}')",
                                GUID, Guid.NewGuid().ToString().Trim(), item.Name, item.Qty, menu.item[0].createUserGUID, DateTime.Now.ToString("yyyy-MM-dd h:m:s"));
                            sqlfinal.AppendFormat(sqlInsertNutrition);
                        }
                    }
                    if (parameter.cookway == true)
                    {
                        foreach (ItemProcess item in menu.ItemProcess)
                        {
                            if (item.itemType == "cookway")
                            {
                                string sqlInsertProcess = string.Format("insert into tblItemProcess (ItemGUID,GUID,Step,Content,Type,CreateUser,createTime) values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                                GUID, Guid.NewGuid(), item.step, item.Content, item.itemType, menu.item[0].createUserGUID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:s"));
                                sqlfinal.AppendFormat(sqlInsertProcess);
                            }
                        }
                    }
                    if (parameter.menuPoint == true)
                    {
                        foreach (ItemProcess item in menu.ItemProcess)
                        {
                            if (item.itemType == "menuPoint")
                            {
                                string sqlInsertProcess = string.Format("insert into tblItemProcess (ItemGUID,GUID,Step,Content,Type,CreateUser,createTime) values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                                GUID, Guid.NewGuid(), item.step, item.Content, item.itemType, menu.item[0].createUserGUID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                sqlfinal.AppendFormat(sqlInsertProcess);
                            }
                        }
                    }

                    if (parameter.bom == true)
                    {
                        string sql = string.Format("select shareQty from tblItem where GUID='{0}' and DeleteTime is null", GUID);
                        var shareQty = SqlServerHelper.GetDataScalar(SqlServerHelper.salesorderConn(), sql);

                        foreach (ItemBOM item in menu.ItemBOM)
                        {
                            //if (item.UOMGUID != "") PurUOM = UOMGUID(item.UOMGUID);

                            if (item.conversionUnit != ""&& item.conversionUnit !=null)
                            {
                                if (item.conversionUnit.ToUpper() == "KG")
                                {
                                    convUOM = "G";
                                    PurUOM = "KG";
                                }
                                else if (item.conversionUnit.ToUpper() == "L")
                                {
                                    convUOM = "ML";
                                    PurUOM = "L";
                                }
                                else
                                {
                                    convUOM = item.conversionUnit.ToUpper();
                                    PurUOM = item.conversionUnit.ToUpper();
                                }
                                if (item.purchasePrice != 0 && item.conversionRate != 0)
                                {
                                    price = (item.purchasePrice).ToDecimal();
                                    if (item.conversionUnit.ToUpper() == "KG" || item.conversionUnit.ToUpper() == "L")
                                    {
                                        cost = (item.purchasePrice / item.conversionRate / 1000).ToDecimal() * (!string.IsNullOrWhiteSpace(item.actQty) ? item.actQty.ToDecimal() * shareQty.ToDecimal() : item.bomQty.ToDecimal());
                                    }
                                    else if (item.conversionUnit.ToUpper() == "G" || item.conversionUnit.ToUpper() == "ML")
                                    {
                                        cost = (item.purchasePrice).ToDecimal() * (!string.IsNullOrWhiteSpace(item.actQty)? item.actQty.ToDecimal() * shareQty.ToDecimal() : item.bomQty.ToDecimal());
                                    }
                                }
                            }
                            

                            string filtercostcenter = ""; string num = "";
                            if (!string.IsNullOrWhiteSpace(item.costCenterCode)) { filtercostcenter = ",costCenterCode) "; num = ",'{18}') "; } //修改周菜单明细，插入成本中心
                            else { filtercostcenter = ")";num = ") "; } //新增菜单的修改维护，不插入成本中心

                            string sqlInsertBOM = string.Format("insert into tblItemBom (ProductGUID,GUID,ItemCode,ItemName,OtherName,StdQty,EffectPercent,UOMGUID,Type,supplierCode,CreateUser,createTime,PurchasePolicy,ItemCost,ItemPurchasePrice,ItemPurchaseUnit,conversionUnit,conversionRate " + filtercostcenter  //,costCenterCode) "
                             + "values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}'" + num,
                                GUID, Guid.NewGuid(), item.itemCode, item.itemName, item.itemDesc, !string.IsNullOrWhiteSpace(item.actQty) ? item.actQty.ToDecimal() * shareQty.ToDecimal() : item.bomQty.ToDecimal(),
                                item.effectPercent.ToDecimal(), convUOM, item.itemType, item.supplierCode, menu.item[0].createUserGUID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), item.purchasePolicy, cost, price, string.IsNullOrWhiteSpace(item.costCenterCode) ? PurUOM:PurUOM,item.conversionUnit,item.conversionRate,item.costCenterCode); //, item.costCenterCode);
                           
                            sqlfinal.AppendFormat(sqlInsertBOM);
                        }
                    }
                }
                return sqlfinal.ToStringTrim();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public string GetItemSql(string dbName, string GUID)//, string ItemType,string Status,string CategoriesClassGUID,decimal price_low,decimal price_high,string keyWord,string nationGUID,string menuClassGUID)
        {
            try
            {
                
                string sql = "";
                if (!string.IsNullOrWhiteSpace(GUID))
                    sql = string.Format(" and a1.GUID='{0}' ", GUID);

                if (!string.IsNullOrWhiteSpace(dbName))
                    sql += string.Format(" and a1.Company ='{0}' ", dbName);
                
                sql = "select distinct c1.guid as nationGUID,c1.className_ZH as nation,c2.guid as menuClassGUID,c2.className_ZH as menuClassName,cast(((1000+ convert(decimal(18,0),c1.sort)) * 1000 + convert(decimal(18,0),c2.sort)) as varchar) as classSort, "
                                 + "a1.GUID,a1.ItemCode,a1.ItemName_ZH,a1.ItemName_EN,a1.itemType,isnull(a1.itemColor,'White') itemColor,a1.itemTaste,convert(int,isnull(a1.itemSpicy,-1)) itemSpicy, "
                                 + "a1.Version,a1.categoriesClassGUID,c3.className_ZH as categoriesClass,a1.cookwayClassGUID,a1.seasonClassGUID,a1.itemShapeGUID,a1.ItemWeightperDish as Weight,a1.itemCostperDish,a1.shareQty, "
                                 + "b1.guid as UOMGUID,b1.NameCn as UOMName, a1.itemBrand,a1.specification,isnull(a1.PurchaseUnitcode,'') PurchaseUnitcode,null purchasePrice,a1.ConversionUnit,a1.ConversionRate, "
                                 + "a1.sort,a1.Status as itemStatus,a1.Company,a1.CreateUser,a2.UserName as createUserName,convert(varchar(100),a1.createTime,20) createTime from tblItem a1 (nolock)"
                                 + "join (tblitemclass c1 (nolock) join tblitemclass c2 (nolock) on c1.guid = c2.pguid left join tblItemClass c3 (nolock) on c2.guid = c3.pguid) "
                                 + "on a1.categoriesClassGUID = ISNULL(c3.guid, ISNULL(c2.guid, c1.guid))"
                                 + "left join UserMast a2 (nolock) on a1.CreateUser=a2.Guid "
                                 + "left join tblUOM b1 (nolock) "
                                 + "on b1.guid = a1.saleUOMGUID where a1.DeleteUser is null and c1.pguid is null and a1.Status='active' "
                                 + sql
                                 + "order by a1.ItemCode";
                return sql;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
       
         /// <summary>
         /// 菜单主数据和明细数据查询、列示
         /// </summary>
         /// <param name="menu"> 
         /// dbName: costCenterCode: requiredDate:
         /// </param>
         /// <returns></returns>
        public List<Item> searchItem(Dictionary<string,dynamic> menu)
        {
            try
            {
                List<Item> purchaseItems = null;
                List<ItemBOM> itemsbom = null;
                if (menu.ContainsKey("costCenterCode"))
                {
                    purchaseItems = itemSource(menu["dbName"],
                        menu.ContainsKey("costCenterCode") ? (string)menu["costCenterCode"] : null,
                        menu.ContainsKey("requiredDate") ? (string)menu["requiredDate"] : null); //, false
                    itemsbom = GetBOMs(menu, purchaseItems);

                }

                string sql = GetItemSql(menu["dbName"],"");

                var itemsData = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql).AsEnumerable();
                
                
                var items = new List<Item>();

                IEnumerable<ItemBOM> tmpbom1 = null;
                List<ItemBOM> tmpbom2 = null;

                if (itemsbom!=null)
                {
                     items = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql).AsEnumerable()
                        .GroupJoin(itemsbom, a => a.Field<string>("GUID").ToStringTrim(), b => b.productGUID, (a, b) => new { a, b })
                        .Select(dr =>
                        {
                            if (dr.b != null)
                            {
                                if (menu.ContainsKey("costCenterCode"))
                                    tmpbom2 = dr.b.Where(q => q.purchasePolicy == "OnDemand" && q.costCenterCode.ToStringTrim() == menu["costCenterCode"]).ToList();
                                if (tmpbom2 == null || !tmpbom2.Any())
                                    tmpbom2 = dr.b.Where(q => q.purchasePolicy == "OnDemand" && string.IsNullOrWhiteSpace(q.costCenterCode)).ToList();
                            }
                            #region
                            //return new Item()
                            //{
                            //    itemSpicy = dr.a.Field<int>("itemSpicy"),
                            //    itemGuid = dr.a.Field<string>("GUID").ToStringTrim(),
                            //    itemCode = dr.a.Field<string>("ItemCode").ToStringTrim(),
                            //    itemName_ZH = string.IsNullOrWhiteSpace(dr.a.Field<string>("ItemName_ZH").ToString()) ? "" : dr.a.Field<string>("ItemName_ZH").ToStringTrim(),
                            //    itemName_EN = string.IsNullOrWhiteSpace(dr.a.Field<string>("ItemName_EN").ToString()) ? "" : dr.a.Field<string>("ItemName_EN").ToStringTrim(),
                            //    itemType = string.IsNullOrWhiteSpace(dr.a.Field<string>("itemType").ToString()) ? "" : dr.a.Field<string>("itemType").ToStringTrim(),
                            //    itemColor = string.IsNullOrWhiteSpace(dr.a.Field<string>("itemColor").ToString()) ? "" : dr.a.Field<string>("itemColor").ToStringTrim(),
                            //    itemTaste = string.IsNullOrWhiteSpace(dr.a.Field<string>("itemTaste").ToString()) ? "" : dr.a.Field<string>("itemTaste").ToStringTrim(),
                            //    version = dr.a.Field<string>("Version").ToStringTrim(),
                            //    categoriesClassGUID = dr.a.Field<string>("categoriesClassGUID").ToStringTrim(),
                            //    categoriesClass = dr.a.Field<string>("categoriesClass").ToStringTrim(),
                            //    cookwayClassGUID = dr.a.Field<string>("cookwayClassGUID").ToStringTrim(),
                            //    seasonClassGUID = dr.a.Field<string>("seasonClassGUID").ToStringTrim(),
                            //    itemShapeGUID = dr.a.Field<string>("itemShapeGUID").ToStringTrim(),
                            //    weight = dr.a.Field<decimal?>("weight") == null ? 0 : dr.a.Field<decimal>("weight"),
                            //    itemCostperDish = menu.ContainsKey("costCenterCode") ? (tmpbom1 == null || !tmpbom1.Any() ? 0 : tmpbom1.Sum(q => q.itemCost.ToDecimal())) : (dr.a.Field<decimal?>("itemCostperDish") == null ? 0 : dr.a.Field<decimal>("itemCostperDish")),
                            //    itemUnitCode = "PC",
                            //    uomGuid = dr.a.Field<string>("UOMGUID").ToStringTrim(),
                            //    UOMName = dr.a.Field<string>("UOMName").ToStringTrim(),
                            //    sort = dr.a.Field<int?>("sort") == null ? 0 : dr.a.Field<int>("sort"),
                            //    itemStatus = dr.a.Field<string>("itemStatus").ToStringTrim(),
                            //    company = dr.a.Field<string>("Company").ToStringTrim(),
                            //    createUserName = dr.a.Field<string>("createUserName").ToStringTrim(),
                            //    shareQty = dr.a.Field<string>("shareQty").ToStringTrim(),
                            //    itemBrand = dr.a.Field<string>("itemBrand").ToStringTrim(),
                            //    specification = dr.a.Field<string>("specification").ToStringTrim(),

                            //    purchasePrice = dr.a.Field<decimal?>("purchasePrice") == null ? 0 : dr.a.Field<decimal>("purchasePrice"),
                            //    purchaseUnitcode = dr.a.Field<string>("PurchaseUnitcode").ToStringTrim(),
                            //    conversionUnit = dr.a.Field<string>("ConversionUnit").ToStringTrim(),
                            //    conversionRate = dr.a.Field<decimal?>("ConversionRate") == null ? 0 : dr.a.Field<decimal>("ConversionRate"),
                            //    createTime = dr.a.Field<string>("createTime").ToStringTrim(),
                            //    nationGUID = dr.a.Field<string>("nationGUID").ToStringTrim(),
                            //    nation = dr.a.Field<string>("nation").ToStringTrim(),
                            //    menuClassGUID = dr.a.Field<string>("menuClassGUID").ToStringTrim(),
                            //    menuClassName = dr.a.Field<string>("menuClassName").ToStringTrim(),
                            //    classSort = dr.a.Field<string>("classSort").ToStringTrim(),
                            //    ItemBOM = tmpbom2,
                            //    otherCost = dr.b == null || !dr.b.Any() ? 0 : dr.b.Where(q => q.purchasePolicy == "NotDemand").Sum(q => q.itemCost.ToDecimal()),
                            //    itemValidity = tmpbom2 != null && tmpbom2.Any() && !tmpbom2.Where(qq => string.IsNullOrWhiteSpace(qq.supplierCode)).Any() ? "valid" : "invalid"
                            //};
                            #endregion
                            return ItemList(dr.a, dr.b, tmpbom2, menu.ContainsKey("costCenterCode"));

                        }).ToList<Item>();
                }
                else
                {
                    items = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql).AsEnumerable().Select(dr =>
                    {
                        #region
                        //if (itemsbom != null)
                        //{
                        //    tmpbom1 = itemsbom.Where(q => q.productGUID == dr.Field<string>("GUID").ToStringTrim());

                        //    if (menu.ContainsKey("costCenterCode"))
                        //        tmpbom2 = tmpbom1.Where(q => q.purchasePolicy == "OnDemand" && q.costCenterCode.ToStringTrim() == menu["costCenterCode"]).ToList();
                        //    if (tmpbom2 == null || !tmpbom2.Any())
                        //        tmpbom2 = tmpbom1.Where(q => q.purchasePolicy == "OnDemand" && string.IsNullOrWhiteSpace(q.costCenterCode)).ToList();
                        //}

                        
                        //return new Item()
                        //{
                        //    itemSpicy = dr.Field<int>("itemSpicy"),
                        //    itemGuid = dr.Field<string>("GUID").ToStringTrim(),
                        //    itemCode = dr.Field<string>("ItemCode").ToStringTrim(),
                        //    itemName_ZH = dr.Field<string>("ItemName_ZH").ToStringTrim(),
                        //    itemName_EN = dr.Field<string>("ItemName_EN").ToStringTrim(),
                        //    itemType = dr.Field<string>("itemType").ToStringTrim(),
                        //    itemColor = dr.Field<string>("itemColor").ToStringTrim(),
                        //    itemTaste = dr.Field<string>("itemTaste").ToStringTrim(),
                        //    version = dr.Field<string>("Version").ToStringTrim(),
                        //    categoriesClassGUID = dr.Field<string>("categoriesClassGUID").ToStringTrim(),
                        //    categoriesClass = dr.Field<string>("categoriesClass").ToStringTrim(),
                        //    cookwayClassGUID = dr.Field<string>("cookwayClassGUID").ToStringTrim(),
                        //    seasonClassGUID = dr.Field<string>("seasonClassGUID").ToStringTrim(),
                        //    itemShapeGUID = dr.Field<string>("itemShapeGUID").ToStringTrim(),
                        //    weight = dr.Field<decimal?>("weight") == null ? 0 : dr.Field<decimal>("weight"),
                        //    itemCostperDish = menu.ContainsKey("costCenterCode") ? (tmpbom1 == null || !tmpbom1.Any() ? 0 : tmpbom1.Sum(q => q.itemCost.ToDecimal())) : (dr.Field<decimal?>("itemCostperDish") == null ? 0 : dr.Field<decimal>("itemCostperDish")),
                        //    itemUnitCode = "PC",
                        //    uomGuid = dr.Field<string>("UOMGUID").ToStringTrim(),
                        //    UOMName = dr.Field<string>("UOMName").ToStringTrim(),
                        //    sort = dr.Field<int?>("sort") == null ? 0 : dr.Field<int>("sort"),
                        //    itemStatus = dr.Field<string>("itemStatus").ToStringTrim(),
                        //    company = dr.Field<string>("Company").ToStringTrim(),
                        //    createUserName = dr.Field<string>("createUserName").ToStringTrim(),
                        //    shareQty = dr.Field<string>("shareQty").ToStringTrim(),
                        //    itemBrand = dr.Field<string>("itemBrand").ToStringTrim(),
                        //    specification = dr.Field<string>("specification").ToStringTrim(),
                        //    purchasePrice = dr.Field<decimal?>("purchasePrice") == null ? 0 : dr.Field<decimal>("purchasePrice"),
                        //    purchaseUnitcode = dr.Field<string>("PurchaseUnitcode").ToStringTrim(),
                        //    conversionUnit = dr.Field<string>("ConversionUnit").ToStringTrim(),
                        //    conversionRate = dr.Field<decimal?>("ConversionRate") == null ? 0 : dr.Field<decimal>("ConversionRate"),
                        //    createTime = dr.Field<string>("createTime").ToStringTrim(),
                        //    nationGUID = dr.Field<string>("nationGUID").ToStringTrim(),
                        //    nation = dr.Field<string>("nation").ToStringTrim(),
                        //    menuClassGUID = dr.Field<string>("menuClassGUID").ToStringTrim(),
                        //    menuClassName = dr.Field<string>("menuClassName").ToStringTrim(),
                        //    classSort = dr.Field<string>("classSort").ToStringTrim(),
                        //    ItemBOM = tmpbom2,
                        //    otherCost = tmpbom1 == null || !tmpbom1.Any() ? 0 : tmpbom1.Where(q => q.purchasePolicy == "NotDemand").Sum(q => q.itemCost.ToDecimal()),
                        //    itemValidity = tmpbom2 != null && tmpbom2.Any() && !tmpbom2.Where(qq => string.IsNullOrWhiteSpace(qq.supplierCode)).Any() ? "valid" : "invalid"
                        //};
                        #endregion

                        return ItemList(dr, tmpbom1, tmpbom2, menu.ContainsKey("costCenterCode"));

                    }).ToList<Item>();
                }
                //显示无BOM的记录
                if (onlyFoodName == true)
                    ReDefine(items);
                    
               //设计菜单
                if (!menu.ContainsKey("requiredDate")) return items;

                //加菜
                items = items //items.Where(q=>q.ItemBOM != null && q.ItemBOM.Any() && !q.ItemBOM.Where(qq=>string.IsNullOrWhiteSpace(qq.supplierCode)).Any())
                    .Union(purchaseItems.Where(q=>q.addToMenu).Select(q =>
                   {
                    decimal qty = (decimal)("KG,L".Contains(q.conversionUnit.ToStringTrim().ToUpper()) ? 1000.0 : 1.0);
                    q.ItemBOM = new List<ItemBOM>
                    {
                        new ItemBOM()
                        {
                            itemCode = q.itemCode,
                            itemName = q.itemName_ZH,
                            purchasePolicy = q.purchasePolicy,
                            bomQty = (qty * q.conversionRate).ToString(),
                            actQty = (qty * q.conversionRate).ToString(),
                            itemCost = (q.purchasePrice / qty / q.conversionRate).ToString(),
                            effectPercent = "0",
                            purchasePrice = q.purchasePrice,
                            purchaseUnit = q.purchaseUnitcode,
                            conversionUnit = q.conversionUnit,
                            conversionRate = q.conversionRate,
                            tax = q.tax,
                            supplierCode = q.supplierCode,
                            supplierName = q.supplierName,
                            }
                        };
                    return q;
                })).ToList();
                return items;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
     
        protected Item ItemList(DataRow dr, IEnumerable<ItemBOM> tmpbom1, List<ItemBOM> tmpbom2,bool costCenterCode)
        {
            return  new Item()
            {
                itemSpicy = dr.Field<int>("itemSpicy"),
                itemGuid = dr.Field<string>("GUID").ToStringTrim(),
                itemCode = dr.Field<string>("ItemCode").ToStringTrim(),
                itemName_ZH = dr.Field<string>("ItemName_ZH").ToStringTrim(),
                itemName_EN = dr.Field<string>("ItemName_EN").ToStringTrim(),
                itemType = dr.Field<string>("itemType").ToStringTrim(),
                itemColor = dr.Field<string>("itemColor").ToStringTrim(),
                itemTaste = dr.Field<string>("itemTaste").ToStringTrim(),
                version = dr.Field<string>("Version").ToStringTrim(),
                categoriesClassGUID = dr.Field<string>("categoriesClassGUID").ToStringTrim(),
                categoriesClass = dr.Field<string>("categoriesClass").ToStringTrim(),
                cookwayClassGUID = dr.Field<string>("cookwayClassGUID").ToStringTrim(),
                seasonClassGUID = dr.Field<string>("seasonClassGUID").ToStringTrim(),
                itemShapeGUID = dr.Field<string>("itemShapeGUID").ToStringTrim(),
                weight = dr.Field<decimal?>("weight") == null ? 0 : dr.Field<decimal>("weight"),
                itemCostperDish = costCenterCode ? (tmpbom1 == null || !tmpbom1.Any() ? 0 : tmpbom1.Sum(q => q.itemCost.ToDecimal())) : (dr.Field<decimal?>("itemCostperDish") == null ? 0 : dr.Field<decimal>("itemCostperDish")),
                itemUnitCode = "PC",
                uomGuid = dr.Field<string>("UOMGUID").ToStringTrim(),
                UOMName = dr.Field<string>("UOMName").ToStringTrim(),
                sort = dr.Field<int?>("sort") == null ? 0 : dr.Field<int>("sort"),
                itemStatus = dr.Field<string>("itemStatus").ToStringTrim(),
                company = dr.Field<string>("Company").ToStringTrim(),
                createUserName = dr.Field<string>("createUserName").ToStringTrim(),
                shareQty = dr.Field<string>("shareQty").ToStringTrim(),
                itemBrand = dr.Field<string>("itemBrand").ToStringTrim(),
                specification = dr.Field<string>("specification").ToStringTrim(),
                purchasePrice = dr.Field<decimal?>("purchasePrice") == null ? 0 : dr.Field<decimal>("purchasePrice"),
                purchaseUnitcode = dr.Field<string>("PurchaseUnitcode").ToStringTrim(),
                conversionUnit = dr.Field<string>("ConversionUnit").ToStringTrim(),
                conversionRate = dr.Field<decimal?>("ConversionRate") == null ? 0 : dr.Field<decimal>("ConversionRate"),
                createTime = dr.Field<string>("createTime").ToStringTrim(),
                nationGUID = dr.Field<string>("nationGUID").ToStringTrim(),
                nation = dr.Field<string>("nation").ToStringTrim(),
                menuClassGUID = dr.Field<string>("menuClassGUID").ToStringTrim(),
                menuClassName = dr.Field<string>("menuClassName").ToStringTrim(),
                classSort = dr.Field<string>("classSort").ToStringTrim(),
                ItemBOM = tmpbom2,
                otherCost = tmpbom1 == null || !tmpbom1.Any() ? 0 : tmpbom1.Where(q => q.purchasePolicy == "NotDemand").Sum(q => q.itemCost.ToDecimal()),
                itemValidity = tmpbom2 != null && tmpbom2.Any() && !tmpbom2.Where(qq => string.IsNullOrWhiteSpace(qq.supplierCode)).Any() ? "valid" : "invalid"
            };
        }
        public void ReDefine(List<Item> items)
        {
            foreach(var row in items)
            {
                row.itemValidity = "valid";
            }
        }

        public string UOMGUID(string guid)
        {
            try
            {
                string sql = string.Format("select GUID from tblUOM (nolock) where NameEN = '{0}'", guid);

                object data = SqlServerHelper.GetDataScalar(SqlServerHelper.salesorderConn(), sql);
                string GUID = data.ToStringTrim();
                return GUID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public Item recipe(string dbName, string ProductGUID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProductGUID))
                    throw new Exception("No Item GUID");

                //List<Item> purchaseItems = itemSource(dbName);

                List<SqlDic> sqls = new List<SqlDic>();

                //tblitem
                string sql = GetItemSql("", ProductGUID);

                sqls.Add(new SqlDic()
                {
                    TableName = "Item",
                    Sql = sql
                });

                //tblItemBOM
                sql = "select cast(ROW_NUMBER() OVER (Partition by Type ORDER BY ItemCode) as int) AS step, "
                    + "productGUID,GUID as itemGuid,itemCode,itemName,supplierCode, "
                    + "OtherName as itemDesc,PurchasePolicy,cast(isnull(StdQty,0) as varchar) bomQty,cast(isnull(ItemCost,0) as varchar) ItemCost, "
                    + "cast(isnull(EffectPercent,0) as varchar) EffectPercent,cast(Type as varchar) itemType  "
                    + "from tblItemBOM (nolock) where ProductGUID='{0}' and DeleteUser is null and costCenterCode is null";
                sqls.Add(new SqlDic()
                {
                    TableName = "ItemBOM",
                    Sql = string.Format(sql, ProductGUID)
                });

                //tblItemNurtrition
                sql = "select itemGuid, Name, cast(isnull(Qty,0) as varchar) Qty from tblItemNutrition (nolock) where ItemGUID='{0}' and DeleteUser is null";
                sqls.Add(new SqlDic()
                {
                    TableName = "ItemNutrition",
                    Sql = string.Format(sql, ProductGUID)
                });
                //tblItemProcess
                sql = "select itemGuid,GUID,step,Content,type as itemType from tblItemProcess (nolock) where ItemGUID='{0}' and DeleteUser is null order by Step";
                sqls.Add(new SqlDic()
                {
                    TableName = "ItemProcess",
                    Sql = string.Format(sql, ProductGUID)
                });

                DataSet ds = SqlServerHelper.GetDataSet(SqlServerHelper.salesorderConn(), sqls);
                Item recipe = ds.Tables["Item"].ToEntityList<Item>().FirstOrDefault();
                recipe.ItemBOM = ds.Tables["ItemBOM"].ToEntityList<ItemBOM>();

                recipe.nutrition = ds.Tables["ItemNutrition"].ToEntityList<ItemNutrition>();
                recipe.ItemProcess = ds.Tables["ItemProcess"].ToEntityList<ItemProcess>();

                return recipe;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<ItemProcess> Process(string productGuid)
        {
            try
            {
                string sql = "select * from tblItemProcess (nolock) where DeleteUser is null and ItemGUID='" + productGuid + "' order by Step";
                return SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql).AsEnumerable().Select(dr => new ItemProcess()
                {
                    GUID = dr.Field<string>("GUID").ToStringTrim(),
                    step = dr.Field<int>("Step"),
                    Content = dr.Field<string>("Content").ToStringTrim(),
                    itemType = dr.Field<string>("Type").ToStringTrim()
                }).ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemNutrition> nutrition(string productGuid)
        {
            try
            {
                string sql = "select * from tblItemNutrition (nolock) where DeleteUser is null and ItemGUID='" + productGuid + "'";
                return SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql).AsEnumerable().Select(dr => new ItemNutrition()
                {
                    GUID = dr.Field<string>("GUID").ToStringTrim(),
                    Name = dr.Field<string>("Name").ToStringTrim(),
                    Qty = dr.Field<decimal>("Qty").ToStringTrim()
                }).ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }



        public List<Item> itemSource(Dictionary<string,dynamic> menu)
        {
            try
            {
                  //PO单用，菜单查询用默认的一个供应商
                List<Item> list = itemSource(menu["dbName"],
                    menu.ContainsKey("costCenterCode") ? menu["costCenterCode"] : null,
                    menu.ContainsKey("requiredDate") ? menu["requiredDate"] : null);

                //维护菜单，显示所有原料
                if (!menu.ContainsKey("directBuy") || !menu["directBuy"])
                    return list;


                //直接采购，只显示可以直接采购的原料
                return list.Where(q => q.directBuy).ToList();
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<ItemBOM> GetBOMs(Dictionary<string, dynamic> menu, List<Item> purchaseItems)
        {
            try
            {
                if (!menu.ContainsKey("costCenterCode")) return null;

                string sql = "select a1.costCenterCode,a1.productGUID,a1.GUID as itemGuid,a1.itemCode,a1.ItemName, "
                     + "a1.OtherName as itemDesc,a1.PurchasePolicy," +
                     "convert(varchar, convert(decimal(18, 3), (isnull(a1.StdQty, 0) * 1) / (isnull(a2.shareQty, 100) * 1))) bomQty,"
                     //"case when a1.costcentercode is not null then convert(varchar,convert(decimal(18,3),(isnull(a1.StdQty,0)*1)/(isnull(a2.shareQty,100)*1))) "
                     //+ "else convert(varchar,convert(decimal(18, 2), (isnull(a1.StdQty, 0) * 1))) End bomQty, "
                     + "cast(convert(decimal(18,2),(isnull(a1.ItemCost,0)*1)/(isnull(a2.shareQty,100)*1)) as varchar) ItemCost, "
                     + "cast(isnull(a1.EffectPercent,0) as varchar) EffectPercent,cast(a1.Type as varchar) itemType,a1.ItemPurchasePrice,a1.ItemPurchaseUnit "
                     + "from tblItemBOM (nolock) a1, tblitem (nolock) a2 "
                     + "where a1.productguid=a2.guid and a1.DeleteUser is null and a2.deleteuser is null and a2.company='{0}' and abs(round(isnull(a1.StdQty, 0),6)) > 0.0000001" +
                     "and isnull(a1.costcentercode,'{1}') = '{1}' order by a1.itemCode";
                sql = string.Format(sql, menu["dbName"],menu["costCenterCode"]);

                List<ItemBOM> itemsbom = SqlServerHelper.GetEntityList<ItemBOM>(SqlServerHelper.salesorderConn(), sql);

                var tbom = itemsbom.GroupJoin(itemsbom.Where(q => q.purchasePolicy.ToLower() == "ondemand" && !string.IsNullOrWhiteSpace(q.costCenterCode))
                    .Select(q=>q.productGUID).Distinct(),
                    a=>a.productGUID,b=>b,(a,b)=>new { a, b })
                    .Where(q=>q.b == null || !q.b.Any() || q.a.purchasePolicy.ToLower() != "ondemand" || !string.IsNullOrWhiteSpace(q.a.costCenterCode))
                    .Select(q=>q.a).ToList();

                
                List<ItemBOM> tmp = tbom.GroupJoin(purchaseItems, p => p.itemCode, pi => pi.itemCode,
                    (p,pi)=> new { p,pi}).Select(q =>
                 {
                    var tmpitem = q.pi.FirstOrDefault();
                     //q.pi.Join(lstSuppliers, p => p.supplierCode, s => s.supplierCode, (pi, s) => new { pi, s }).OrderBy(p => p.s.index).FirstOrDefault();

                     return new ItemBOM()
                     {
                        costCenterCode=q.p.costCenterCode,
                        productGUID = q.p.productGUID,
                        itemGuid = q.p.itemGuid,
                        itemCode = q.p.itemCode,
                        itemName = q.p.itemName, //tmpitem == null ? null : tmpitem.itemName_ZH,
                        itemDesc = q.p.itemDesc,
                        itemType = q.p.itemType,
                        purchasePolicy = q.p.purchasePolicy,
                        bomQty = q.p.bomQty,
                        actQty = q.p.bomQty,
                        itemCost = tmpitem == null ? q.p.itemCost : (tmpitem.purchasePrice / tmpitem.conversionRate / (decimal)("KG,L".Contains(tmpitem.conversionUnit.ToUpper()) ? 1000.0 : 1.0) *  q.p.bomQty.ToDecimal()).ToString() ,   //q.p.itemCost,
                        effectPercent = q.p.effectPercent,
                        purchasePrice = tmpitem == null ? 0 : tmpitem.purchasePrice,
                        purchaseUnit = tmpitem == null ? null : tmpitem.purchaseUnitcode,
                        conversionUnit = tmpitem == null ? null : tmpitem.conversionUnit,
                        conversionRate = tmpitem == null ? 0 : tmpitem.conversionRate,
                        tax = tmpitem == null ? 0 : tmpitem.tax,
                        supplierCode = tmpitem == null ? null : tmpitem.supplierCode,
                        supplierName = tmpitem == null ? null : tmpitem.supplierName
                     };
                }).ToList();

                return tmp;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<Item> itemSource(string dbName, string costCenterCode, string date) //bool allSuppliersByItem = true
        {
            try
            {
                #region 迁移到 SupplierFactory.getPurchaseItems
                /*
                string sql = " and {0} >= validfrom {1}";
                if (string.IsNullOrWhiteSpace(date))  //维护菜单时，显示所有生效过的价目表 validfrom <= now
                    sql = string.Format(sql, "GETDATE()", "");
                else  //查询某一天的价目表
                    sql = string.Format(sql, string.Format("'{0}'",date), string.Format(" and '{0}' < (validto + 1)", date));

                //string sqlsupplier = "0 supplierindex,";
                #region no use
                //sql = "SELECT distinct "+sqlsupplier+"a2.Supplier_Code,a2.Supplier_Name,b2.Class02_Code as nationGUID, b2.Class02_Name as nation,b1.Class03_Code as menuClassGUID,b1.Class03_Name as menuClassName, "
                //+ "a1.division as Company, a1.database_name as Site,a1.area,a1.city,a1.region,cast(a1.price as decimal(18,3))*(1+cast(a1.btwper/100 as decimal(18,3))) Price, convert(decimal(18,6),a1.btwper) tax,"
                //+ "a1.ItemCode,a1.Items_Name as ItemName_ZH,a1.ItemEnName as ItemName_EN,a1.Condition_Code,a1.Brand itemBrand, "
                //+ "a1.specification as specification,a1.unitcode,cast(a1.ConversionRate as decimal(18,3)) ConversionRate, a1.ConversionUnit, "
                //+ "Case ltrim(rtrim(a1.ConversionUnit)) when 'KG' then cast((1*a1.ConversionRate)*1000 as decimal(18,0)) when 'L' then cast((1*a1.ConversionRate)*1000 as decimal(18,0)) else cast((1*a1.ConversionRate) as decimal(18,0)) End Weight, "
                //+ "case left(a1.ItemCode, 5) when '10302' then 'Source' else 'RM' end as itemType,convert(varchar(10),a1.validfrom,23) validfrom "
                //+ "FROM (DM_D_ERP_PurchaseAgreement (nolock) a1 join d_item(nolock) a0 on a1.Itemcode = a0.Items_Code) "
                //+ "left join d_item_class_03 (nolock) b1 on b1.Class03_Code = a0.class03_code "
                //+ "left join d_item_class_02 (nolock) b2 on b2.Class02_Code = b1.Class02_Code "
                //+ "left join[dbo].[d_supplier] (nolock) a2 on a2.Supplier_Code=a1.supplierCode where a1.ItemCode like '10%' "
                //+ "and a1.Condition_Code='A' "
                //+ "and a1.division = '" + dbName + "'" + sql
                //+ "order by supplierindex,a1.ItemCode";
                #endregion

                sql = "SELECT distinct a1.crdnr,a2.Supplier_Name,b2.Class02_Code as nationGUID, b2.Class02_Name as nation,b1.Class03_Code as menuClassGUID,b1.Class03_Name as menuClassName, "
                + "a1.division as Company, a1.database_name as Site,a1.area,a1.city,a1.region,cast(a1.price as decimal(18,3))*(1+cast(a1.btwper/100 as decimal(18,3))) Price, convert(decimal(18,6),a1.btwper) tax,"
                + "a1.ItemCode,a1.Items_Name as ItemName_ZH,a1.ItemEnName as ItemName_EN,a1.Condition_Code,a1.Brand itemBrand, "
                + "a1.specification as specification,a1.unitcode,cast(a1.ConversionRate as decimal(18,3)) ConversionRate, a1.ConversionUnit, "
                + "Case ltrim(rtrim(a1.ConversionUnit)) when 'KG' then cast((1*a1.ConversionRate)*1000 as decimal(18,0)) when 'L' then cast((1*a1.ConversionRate)*1000 as decimal(18,0)) else cast((1*a1.ConversionRate) as decimal(18,0)) End Weight, "
                + "case left(a1.ItemCode, 5) when '10302' then 'Source' else 'RM' end as itemType,convert(varchar(10),a1.validfrom,23) validfrom,convert(varchar(10),a1.validto,23) validto "
                + "FROM (DM_D_ERP_PurchaseAgreement (nolock) a1 join d_item(nolock) a0 on a1.Itemcode = a0.Items_Code) "
                + "left join d_item_class_03 (nolock) b1 on b1.Class03_Code = a0.class03_code "
                + "left join d_item_class_02 (nolock) b2 on b2.Class02_Code = b1.Class02_Code "
                + "left join[dbo].[d_supplier] (nolock) a2 on a2.Supplier_Code=a1.supplierCode where a1.ItemCode like '10%' "
                + "and a1.Condition_Code='A' "
                + "and a1.division = '" + dbName + "'" + sql
                + "order by a1.crdnr,a1.ItemCode";

                var data = SqlServerHelper.GetDataTable(SqlServerHelper.purchasepriceconn, sql).AsEnumerable().
                Select(dr => new Item()
                {
                    itemGuid = "",
                    //supplierIndex=dr.Field<int>("supplierIndex"),
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
                    itemUnitCode= dr.Field<string>("unitcode").ToStringTrim(),
                    specification = dr.Field<string>("specification").ToStringTrim(),
                    unitcode = dr.Field<string>("unitcode").ToStringTrim(),
                    itemType = dr.Field<string>("itemType").ToStringTrim(),
                    purchasePrice = dr.Field<decimal?>("Price") == null ? 0 : dr.Field<decimal>("Price"),
                    tax = dr.Field<decimal?>("tax") == null ? 0 : dr.Field<decimal>("tax"),
                    purchaseUnitcode = dr.Field<string>("unitcode").ToStringTrim(),
                    weight = dr.Field<decimal?>("Weight") == null ? 0 : dr.Field<decimal>("Weight"),
                    conversionRate = dr.Field<decimal?>("ConversionRate") == null ? 0 : dr.Field<decimal>("ConversionRate"),
                    conversionUnit = dr.Field<string>("ConversionUnit").ToStringTrim(),
                    validfrom = dr.Field<string>("validfrom").ToStringTrim(),
                    validto = dr.Field<string>("validto").ToStringTrim()
                }).ToList();
          
                //获取指定Site的价目表，则不加新增原料
                if (!string.IsNullOrWhiteSpace(costCenterCode))
                {
                    List<dynamic>  lstSuppliers = new MastData.SupplierFactory().getSuppliers(dbName, costCenterCode, DateTime.Parse(date));
                    data = (from a in data
                            from b in lstSuppliers
                            where a.supplierCode == b.supplierCode &&
                                (a.nationGUID == b.class_Code || a.menuClassGUID == b.class_Code)
                            select new { a, b }).Select(q=>
                            {
                                q.a.directBuy = q.b.directBuy;
                                q.a.addToMenu = q.b.addToMenu;
                                return q.a;
                            } ).ToList();
                }

                //一个原料只有一个默认的供应商


                #region nouse
                //var categories = (from p in data
                //                  group p by new
                //                  {
                //                      ItemGUID = p.itemGuid,
                //                      nation = p.nation,
                //                      nationGUID = p.nationGUID,
                //                      menuClassName = p.menuClassName,
                //                      menuClassGUID = p.menuClassGUID,
                //                      Company = p.company,
                //                      ItemCode = p.itemCode,
                //                      ItemName_ZH = p.itemName_ZH,
                //                      ItemName_EN = p.itemName_EN,
                //                      specification = p.specification,
                //                      itemType = p.itemType,
                //                      ConversionRate = p.conversionRate,
                //                      ConversionUnit = p.conversionUnit,
                //                  } into r
                //                  select new Item()
                //                  {
                //                      supplierCode = r.OrderByDescending(q => q.validfrom).FirstOrDefault().supplierCode,
                //                      supplierName = r.OrderByDescending(q => q.validfrom).FirstOrDefault().supplierName,
                //                      itemGuid = r.Key.ItemGUID,
                //                      nation = r.Key.nation,
                //                      nationGUID = r.Key.nationGUID,
                //                      menuClassName = r.Key.menuClassName,
                //                      menuClassGUID = r.Key.menuClassGUID,
                //                      itemCostperDish = r.OrderByDescending(q => q.validfrom).FirstOrDefault().itemCostperDish,
                //                      weight = r.OrderByDescending(q => q.validfrom).FirstOrDefault().weight,
                //                      company = r.Key.Company,
                //                      itemCode = r.Key.ItemCode,
                //                      itemName_ZH = r.Key.ItemName_ZH,
                //                      itemName_EN = r.Key.ItemName_EN,
                //                      specification = r.Key.specification,
                //                      unitcode = r.OrderByDescending(q => q.validfrom).FirstOrDefault().unitcode,
                //                      purchaseUnitcode = r.OrderByDescending(q => q.validfrom).FirstOrDefault().purchaseUnitcode,
                //                      itemType = r.Key.itemType,
                //                      purchasePrice = r.OrderByDescending(q => q.validfrom).FirstOrDefault().purchasePrice,
                //                      tax = r.OrderByDescending(q => q.validfrom).FirstOrDefault().tax,
                //                      conversionRate = r.Key.ConversionRate,
                //                      conversionUnit = r.Key.ConversionUnit,
                //                  });
                #endregion

                var categories = data.GroupBy(p => new
                    {
                        ItemGUID = p.itemGuid,
                        nation = p.nation,
                        nationGUID = p.nationGUID,
                        menuClassName = p.menuClassName,
                        menuClassGUID = p.menuClassGUID,
                        Company = p.company,
                        ItemCode = p.itemCode,
                        ItemName_ZH = p.itemName_ZH,
                        ItemName_EN = p.itemName_EN,
                        specification = p.specification,
                        itemType = p.itemType,
                        ConversionRate = p.conversionRate,
                        ConversionUnit = p.conversionUnit,
                    }).Select(r =>
                    {
                        //按优先级选供应商
                        //维护菜单时，选择最接近的价目表
                        var g = r.OrderByDescending(q => DateTime.Parse(q.validto)).FirstOrDefault();

                        return new Item()
                        {
                            supplierCode = g.supplierCode,
                            supplierName = g.supplierName,
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
                        };
                    });
                */
                #endregion

                var categories = new MastData.ItemFactory().getPurchaseItems(dbName,costCenterCode, date);

                //获取指定Site的价目表且onlyFoodName为非，则不加新增原料
                if (!string.IsNullOrWhiteSpace(costCenterCode)) return categories.ToList();

                #region 维护菜单时新增的原料
                string sql2 = "With C as "
                    + "(select distinct ROW_NUMBER() Over (Partition by a1.ItemCode Order by a1.CreateTime desc) 'seq', "
                    + "a1.GUID,a1.Company,a1.ItemCode,a1.ItemName_ZH,a1.ItemName_EN,b1.ItemPurchasePrice as Price,a1.specification,a1.PurchaseUnitcode as Unitcode,a1.ConversionUnit, "
                    + "cast(a1.ConversionRate as decimal(18,3)) ConversionRate, "
                    + "a1.itemType from tblItem (nolock) a1 left join (select * from tblItemBOM (nolock) where DeleteTime is null) b1 on a1.ItemCode=b1.ItemCode "
                    + "where a1.DeleteTime is null and a1.itemType in ('RM','Flavor') and a1.Company = '" + dbName + "' ) select * from C where seq = '1' order by ItemCode ";

                var data2 = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql2).AsEnumerable().
                    Select(g=>new Item()
                    {
                        itemGuid = g.Field<string>("GUID").ToStringTrim(),
                        company = g.Field<string>("Company").ToStringTrim(),
                        itemCode = g.Field<string>("ItemCode").ToStringTrim(),
                        itemName_ZH = g.Field<string>("ItemName_ZH").ToStringTrim(),
                        itemName_EN = g.Field<string>("ItemName_EN").ToStringTrim(),
                        specification = g.Field<string>("specification").ToStringTrim(),
                        unitcode = g.Field<string>("unitcode").ToStringTrim(),
                        purchaseUnitcode = g.Field<string>("unitcode").ToStringTrim(),
                        itemType = g.Field<string>("itemType").ToStringTrim(),
                        purchasePrice = g.Field<decimal?>("Price") == null ? 0 : g.Field<decimal>("Price"),
                        conversionRate = g.Field<decimal?>("ConversionRate") == null ? 0 : g.Field<decimal>("ConversionRate"),
                        conversionUnit = g.Field<string>("ConversionUnit").ToStringTrim(),
                        itemStatus = "A"
                    }).ToList();
                #endregion  

                return categories.Union(data2).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public int insertSequences(string Code)
        {
            try
            {
                StringBuilder finalsbSql = new StringBuilder();

                string sbSqlInsert = "Begin Transaction insert sequences(type,code,nextnbr) "
                    + "select 'ItemCode','" + Code + "',0 "
                    + "where not exists(select top 1 1 from sequences where type = 'ItemCode' and code = '" + Code + "')";

                string sbSqlUpdate = "update Sequences set NextNbr = NextNbr + 1 "
                    + "where type = 'ItemCode' and code = '" + Code + "'";

                int i = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlInsert.ToString());

                int j = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlUpdate.ToString());

                string sbSqlClose = "";
                if (i + j <= 2)
                {
                    sbSqlClose = "Commit";
                }
                else
                {
                    sbSqlClose = "RollBack";
                }

                return SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlClose.ToString());
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        public List<itemSequence> itemSequence(itemSequence menu)
        {
            try
            {
                string execSQL = "insert sequences(type,code,nextnbr) "
                + "select 'ItemCode','{0}',0 "
                + "where not exists(select top 1 1 from sequences where type = 'ItemCode' and code = '{0}');";

                execSQL += "update Sequences set NextNbr = NextNbr + 1 "
                    + "where type = 'ItemCode' and code = '{0}';";
                execSQL = string.Format(execSQL, menu.Code);
                string selectSQL = "select Code,NextNbr from Sequences where type='ItemCode' and code='{0}'";
                selectSQL = string.Format(selectSQL, menu.Code);

                DataTable dt = SqlServerHelper.ExecAndGetDataTable(SqlServerHelper.salesorderConn(), execSQL, selectSQL);
                return dt.AsEnumerable().Select(dr => new itemSequence()
                {
                    Code = dr.Field<string>("Code").ToStringTrim(),
                    NextNbr = dr.Field<int>("NextNbr").ToStringTrim(),
                    NextGUID = Guid.NewGuid().ToString()
                }).ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemClass> GetnewMenu(string companyCode, string costCenterCode, string date)
        {
            try
            {
                //直接采购产品大类

                List<dynamic> lstItemClass = (new MastData.SupplierFactory()).getSuppliers(companyCode, costCenterCode, DateTime.Parse(date))
                    .Where(q=>q.addToMenu).ToList();
                /*
                sql = "select a2.Class02_Code as value2,a2.Class02_Name as label2,a3.Class03_Code as value3,a3.Class03_Name as label3 "
                    + "from (d_item_class_03 (nolock) a3 join d_item_class_02 (nolock) a2 on a3.Class02_Code= a2.Class02_Code "
                    + "join d_item_class_01 (nolock) a1 on a3.Class01_Code= a1.Class01_Code) "
                    + "where a1.Class01_Code= '10' and (a3.class02_code in ('{1}') or a3.class03_code in ('{1}')) "
                    + "and a3.class03_code in (select distinct a2.Class03_Code "
                    + "from DM_D_ERP_PurchaseAgreement (nolock) a1 join d_item(nolock)a2 on a1.Itemcode = a2.Items_Code "
                    + "where a1.division = '{0}' and a1.Condition_Code = 'A' and GETDATE()>= a1.validfrom and getdate() < (a1.validto+1)"
                    + ") order by a2.Class02_Code,a3.Class03_Code";
                
                string sql = "select distinct a3.Class02_Code as value2,a2.Class02_Name as label2,a3.Class03_Code as value3,a3.Class03_Name as label3, "
                    + "b1.crdnr supplierCode "
                    + "from DM_D_ERP_PurchaseAgreement (nolock) b1 join d_item(nolock)b2 on b1.Itemcode = b2.Items_Code "
                    + "join d_item_class_03 (nolock) a3 on b2.Class03_Code = a3.Class03_Code "
                    + "join d_item_class_02 (nolock)a2 on a3.Class02_Code = a2.Class02_Code "
                    + "where b1.Itemcode like '10%' and b1.division = '{0}' and b1.Condition_Code = 'A' "
                    + "and b1.validfrom <= '{1}' and(b1.validto + 1) > '{1}' "
                    + "order by a3.Class02_Code,a3.Class03_Code";

                sql = string.Format(sql, companyCode,DateTime.Parse(date).ToString("yyyy-MM-dd"));
                DataTable dt = SqlServerHelper.GetDataTable(SqlServerHelper.purchasepriceconn, sql);
                if (dt == null || dt.Rows.Count == 0) return null;
                */
                List<Item> list = (new MastData.ItemFactory()).getPurchaseItems(companyCode, costCenterCode,date);
                if (list == null || !list.Any()) return null;
                //获取最外层Menu, 即所有父GUID为空

                int index = 1;

                var data2 = (from d in list.OrderBy(q=>q.nationGUID).ThenBy(q=>q.menuClassGUID)//dt.AsEnumerable()
                             from a in lstItemClass
                             where /*d.Field<string>("supplierCode").ToStringTrim()*/ d.supplierCode == a.supplierCode &&
                                (d.nationGUID == a.class_Code || d.menuClassGUID == a.class_Code)
                                //(d.Field<string>("value2").ToStringTrim() == a.class_Code || d.Field<string>("value3").ToStringTrim() == a.class_Code)
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
                                 children = g.Select(q=>new { value=q.menuClassGUID, label = q.menuClassName })
                                 .Distinct().Select(q=>  //GetnewChildMenus(g)
                                     {
                                         int x = 0;

                                         return new ItemClass()
                                             {
                                                 itemCodeId = x++.ToString(),
                                                 value = q.value,//dr.Field<string>("value3").ToStringTrim(),
                                                 label = q.label//dr.Field<string>("label3").ToStringTrim()
                                             };
                                     }).ToList(),
                             }).ToList();
                data2.Insert(0, new ItemClass()
                {
                    itemCodeId = "0",
                    value = "",
                    label = "-----------------------",
                    children = new List<ItemClass>(),
                    disabled = "true"
                });
                return data2.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<ItemClass> GetnewChildMenus(IGrouping<object, DataRow> qry)
        {
            try
            {
                int index = 0;

                return qry.Select(dr => new ItemClass()
                {
                    itemCodeId = index++.ToString(),
                    value = dr.Field<string>("value3").ToStringTrim(),
                    label = dr.Field<string>("label3").ToStringTrim()
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 新分类
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="langCode"></param>
        /// <returns></returns>
        public List<ItemClass> GetMenu(string langCode,string itemType, string company, string costCenterCode, string date)
        {
            try
            {
                string sqlFilter = string.Empty;

                StringBuilder sql = new StringBuilder("select cast(a1.itemCodeId as varchar(10)) itemCodeId, a1.guid, ISNULL(a1.pguid, '') pguid, "
                    + "Case '" + langCode + "' when 'en' then isnull(a1.className_EN,a1.className_ZH) else isnull(a1.className_ZH, a1.className_EN) end as name, "
                    + "a1.type,convert(varchar(20), a1.sort) as sort from tblItemClass a1 where a1.type = 'Categories' and a1.status = 'active' "); //+ sqlFilter
                DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql.ToString());
                if (data == null || data.Rows.Count == 0) return null;
                //获取最外层Menu, 即所有父GUID为空
                var query = data.AsEnumerable().Where(dr => dr.Field<string>("pguid").ToStringTrim().Equals(string.Empty));
                if (!query.Any()) throw new Exception("root menuClass not found");

                int index = 0;
                var queries = query.Select(dr => new ItemClass()
                {
                    itemCodeId = index++.ToString(),
                    value = dr.Field<string>("guid").ToStringTrim(),
                    label = dr.Field<string>("name").ToStringTrim(),
                    sort = dr.Field<string>("sort").ToStringTrim(),
                    children = GetChildMenus(dr.Field<string>("guid"), data)
                }).ToList();

                if (itemType != "FG&RM") return queries;

                queries = queries.Union(GetnewMenu(company,costCenterCode,date)).ToList();
                return queries;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private List<ItemClass> GetChildMenus(string parentGuid, DataTable data)
        {
            try
            {
                var query = data.AsEnumerable().OrderBy(dr => dr.Field<string>("pguid")).ThenBy(dr=>dr.Field<string>("sort")).Where(dr => dr.Field<string>("pguid").ToStringTrim().Equals(parentGuid));
                if (query.Any())
                {
                    int index = 0;
                    return query.Select(dr => new ItemClass()
                    {
                        itemCodeId = index++.ToString(),
                        value = dr.Field<string>("guid").ToStringTrim(),
                        label = dr.Field<string>("name").ToStringTrim(),
                        sort = dr.Field<string>("sort").ToStringTrim(),
                        children = GetChildMenus(dr.Field<string>("guid"), data) //递归获取
                    }).ToList();
                }
                else return null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
