using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Aden.Util.Common;
using Aden.Util.Database;
using Aden.Model.MenuOrder;
using Aden.Model.MenuData;
using Aden.Model.SOMastData;
using Aden.Model.Common;
using Aden.Model.SOAccount;
using Aden.Model.MastData;
using Aden.DAL.SOAccount;
using Aden.DAL.SalesOrder;
using System.Data;

namespace Aden.DAL.MenuOrder
{
    public class WeeklyMenuFactory
    {
        // 成品类别代码
        private const string FG = "FG";
        // 原料类别的排序代码
        private const string othClassSort = "999999";

        #region 取得CostCenter->档口的级联下拉框数据
        /// <summary>
        /// 根据用户权限取得CostCenter
        /// </summary>
        /// <param name="action"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        public List<CCWhs> GetCCWhs(AuthCompany param)
        {
            List<SingleField> lstCC = new List<SingleField>();
            string strSql = string.Empty;

            // 取得Guid集合
            //string strMenuGuid = Aden.Util.AdenCommon.MastData.formatActionToMenuGuid(param.action);

            //string strSql = " SELECT RTRIM(CODE) AS CODE " +
            //                  " FROM TBLUSERMENUDATA " +
            //                 " WHERE USERGUID = '{0}' " +
            //                   " AND TYPE = 'costcenter' " +
            //                   " AND MENUGUID IN {1} ";

            //lstCC = SqlServerHelper.GetEntityList<SingleField>(SqlServerHelper.salesorderConn, string.Format(strSql, param.userGuid, strMenuGuid));
            UserAuthParam uParam = new UserAuthParam
            {
                userGuid = param.userGuid,
                action = param.action,
                fieldName = "costcenter",
                recursion = false
            };
            

            lstCC = (new AccountFactory()).GetAuthList(uParam);

            string strCostCenterCode = string.Join("','", lstCC.Select(r => r.code).Distinct().ToArray());
            strCostCenterCode = "('" + strCostCenterCode + "')";

            // ccWhs数据取得
            strSql = " SELECT DISTINCT T.ID " +
                          " , T.DBNAME " +
                          " , C.IP " +
                          " , C.CATPOENDTIME " +
                          " , T.COSTCENTERCODE " +
                          " , T.WAREHOUSECODE " +
                          " , T.COSTCENTERCODE AS VALUE " +
                       " FROM CCMAST AS T" +
                      " INNER JOIN COMPANY AS C " +
                         " ON T.DBNAME = C.DBNAME " +
                      " WHERE T.STATUS = '1' " +
                        " AND T.COSTCENTERCODE IN " + strCostCenterCode;

            // Get ccWhs
            List<CCWhs> lstCCWhs = SqlServerHelper.GetEntityList<CCWhs>(SqlServerHelper.salesorderConn(), strSql);
            // Get CostCenter Name
            GetCostCenterName(ref lstCCWhs);
            // Get ccWindows
            List<CCWindows> lstCCWindows = GetWindows(strCostCenterCode);
            // Get ccWindowMeals
            List<CCWindowMeals> lstCCWindowMeals = GetWindowMeals(lstCCWindows);

            if (lstCCWindowMeals != null && lstCCWindowMeals.Count > 0)
            {
                foreach (CCWindows win in lstCCWindows)
                {
                    var lstTemp = lstCCWindowMeals.Where(r => win.windowGuid.Equals(r.windowGuid));
                    if (!lstTemp.Any()) continue;
                    win.lstCCWindowMeals = lstTemp.ToList();
                }
            }
            // 匹配成本中心对应的档口信息和供应商信息
            foreach (CCWhs whs in lstCCWhs)
            {
                if (lstCCWindows != null && lstCCWindows.Count > 0)
                {
                    var lstTemp = lstCCWindows.Where(r => whs.costCenterCode.Equals(r.costCenterCode));
                    if (!lstTemp.Any()) continue;
                    whs.lstCCWindows = lstTemp.ToList();
                }
            }

            lstCCWhs = lstCCWhs.OrderBy(r => r.costCenterCode).ToList();

            return lstCCWhs;
        }

        /// <summary>
        /// 设置CostCenter名称
        /// </summary>
        private void GetCostCenterName(ref List<CCWhs> lstCCWhs)
        {
            string strSql = " SELECT KSTPLCODE AS COSTCENTERCODE " +
                                " , OMS25_0 AS COSTCENTERNAME_EN " +
                                " , OMS25_1 AS COSTCENTERNAME_ZH " +
                             " FROM KSTPL (nolock) " +
                            " WHERE ISNULL(CLASS_01,'') <> '' " +
                              " AND KSTPLCODE IN {0} ";

            List<CostCenter> lstCC = new List<CostCenter>();
            List<CostCenter> lstCCWork = new List<CostCenter>();
            string strCCName = string.Empty;

            var lqCCWhs = (from ccwhs in lstCCWhs
                           group ccwhs by new
                           {
                               // 服务器名称
                               dbName = ccwhs.dbName,
                               // 服务器IP
                               ip = ccwhs.ip
                           }).ToList();
            foreach (var gCCWhs in lqCCWhs)
            {
                string strCodeCenterCode = string.Join("','", gCCWhs.ToList().Select(r => r.costCenterCode).Distinct().ToArray());
                strCodeCenterCode = "('" + strCodeCenterCode + "')";

                lstCCWork = SqlServerHelper.GetEntityList<CostCenter>(string.Format(SqlServerHelper.customerConn(), gCCWhs.Key.ip, gCCWhs.Key.dbName), string.Format(strSql, strCodeCenterCode));
                if (lstCCWork != null && lstCCWhs.Count > 0)
                    lstCC.AddRange(lstCCWork);
            }
            // 名称赋值
            foreach(CCWhs cw in lstCCWhs)
            {
                var cc = lstCC.FirstOrDefault(r => cw.costCenterCode.Equals(r.costCenterCode));
                if (cc == null) continue;
                cw.costCenterName_ZH = cc.costCenterName_ZH;
                cw.costCenterName_EN = cc.costCenterName_EN;
            }
        }

        /// <summary>
        /// 根据CostCenterCode取得窗口
        /// </summary>
        /// <param name="lstCostCenterCode"></param>
        /// <returns></returns>
        private List<CCWindows> GetWindows(string strCostCenterCode)
        {
            List<CCWindows> lstCCWindows = new List<CCWindows>();

            string strSql = "SELECT MAX(ID) AS ID " +
                                " , COSTCENTER AS COSTCENTERCODE " +
                                " , WINDOWGUID,WINDOWSORT " +
                                " , MAX(WINDOWNAME) AS WINDOWNAME " +
                                " , WINDOWGUID AS VALUE " +
                                " , MAX(WINDOWNAME) AS LABEL " +
                             " FROM CCWINDOWMEALS " +
                            " WHERE COSTCENTER IN " + strCostCenterCode +
                              " AND DELETETIME IS NULL " +
                            " GROUP BY COSTCENTER, WINDOWGUID,WINDOWSORT" +
                            " ORDER BY COSTCENTER,WINDOWSORT ";

            lstCCWindows = SqlServerHelper.GetEntityList<CCWindows>(SqlServerHelper.salesorderConn(), strSql);

            return lstCCWindows;
        }

        /// <summary>
        /// 取得Meals数据
        /// </summary>
        /// <param name="strWindowGuid"></param>
        /// <returns></returns>
        private List<CCWindowMeals> GetWindowMeals(List<CCWindows> lstCCWindows)
        {
            if (lstCCWindows == null || lstCCWindows.Count == 0)
                return null;
            List<CCWindowMeals> lstWindowMeals = new List<CCWindowMeals>();

            string strWindowGuid = string.Join("','", lstCCWindows.Select(r => r.windowGuid).Distinct().ToArray());
            strWindowGuid = "('" + strWindowGuid + "')";

            string strSql = " SELECT ID " +
                                 " , WINDOWGUID " +
                                 " , SOITEMGUID " +
                                 " , CONVERT(VARCHAR(100), ISNULL(STARTDATE, '2000-01-01 00:00:00.000' ), 20) AS STARTDATE " +
                                 " , CONVERT(VARCHAR(100), ISNULL(ENDDATE, '2099-12-31 23:59:59.000' ), 20) AS ENDDATE " +
                              " FROM CCWINDOWMEALS " +
                             " WHERE WINDOWGUID IN {0} and deletetime is null" +
                               " AND ISNULL(STARTDATE, '2000-01-01 00:00:00.000') <= GETDATE() " +
                               " AND ISNULL(ENDDATE, '2099-12-31 00:00:00.000') >= GETDATE() ";

            lstWindowMeals = SqlServerHelper.GetEntityList<CCWindowMeals>(SqlServerHelper.salesorderConn(), string.Format(strSql, strWindowGuid));

            return lstWindowMeals;
        }
        #endregion

        #region 根据CostCenter和SO ItemGUID集合取得餐次信息
        /// <summary>
        /// 根据CostCenter和SO ItemGUID集合取得餐次信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<SalesOrderItem> GetMealTypeList(MealTypeParam param)
        {
            List<SalesOrderItem> lstMealType = new List<SalesOrderItem>();
            StringBuilder sbSql = new StringBuilder();
            string strItemGuid = string.Empty;

            if(param.lstItemGuid != null && param.lstItemGuid.Count > 0)
            {
                strItemGuid = string.Join("','", param.lstItemGuid.Select(r => r.itemGuid).Distinct().ToArray());
                strItemGuid = "('" + strItemGuid + "')";
            }

            string strDate = SalesOrder.Common.convertDateTime(param.startDate);

            // 拼接Sql文
            sbSql.Append(" SELECT DISTINCT L.HEADGUID,p.sort ");
            sbSql.Append("      , L.ITEMGUID ");
            sbSql.Append("      , L.PRODUCTCODE ");
            sbSql.Append("      , L.PRODUCTDESC ");
            sbSql.Append("   FROM SALESORDERITEM (NOLOCK) AS L ");
            sbSql.Append("  INNER JOIN SALESORDERHEAD (NOLOCK) AS H  ");
            sbSql.Append("     ON L.HEADGUID = H.HEADGUID ");
            sbSql.Append("    AND L.STATUS <> '0' ");
            sbSql.Append("    AND H.STATUS <> '0' ");
            sbSql.Append("  INNER JOIN PRODUCTTYPEDATA (NOLOCK) AS P  ");
            sbSql.Append("     ON L.PRODUCTCODE = P.ID ");
            sbSql.Append("    AND P.PRODUCTTYPE = 'SalesMeals' ");
            sbSql.Append("  WHERE L.COSTCENTERCODE = '" + param.costCenterCode + "' ");
            if (!string.IsNullOrWhiteSpace(strItemGuid))
                sbSql.Append("AND L.ITEMGUID IN " + strItemGuid);
            sbSql.Append("    AND L.STARTDATE <= '" + strDate + "' ");
            sbSql.Append("    AND ISNULL(L.EXPIRYDATE, L.ENDDATE) >= '" + strDate + "' ");
            sbSql.Append("    AND ISNULL(H.VALIDDATE, H.STARTDATE) <= '" + strDate + "' ");
            sbSql.Append("    AND ISNULL(H.EXPIRYDATE, H.ENDDATE) >= '" + strDate + "' ");
            sbSql.Append("order by p.sort,l.PRODUCTDESC");

            lstMealType = SqlServerHelper.GetEntityList<SalesOrderItem>(SqlServerHelper.salesorderConn(), sbSql.ToString());
            lstMealType = lstMealType.GroupBy(r => r.productDesc).Select(t => t.First()).ToList();

            return lstMealType;
        }
        #endregion

        #region 保存MenuOrder
        /// <summary>
        /// 保存MenuOrder
        /// </summary>
        /// <param name="lstMenuOrderHead"></param>
        /// <returns></returns>
        public int SaveMenuOrder(SaveMenuOrderParam param)
        {
            int result = 0;
            StringBuilder sbSql = new StringBuilder();
            string strDateTime = SalesOrder.Common.convertDateTime(DateTime.Now.ToString());
            string strHEADGUID = string.Empty;
            List<string> lstRequireDate = new List<string>();

            // Update Sql
            string strSqlUpdate = " UPDATE MENUORDERHEAD " +
                                     " SET DELETETIME = '{0}' " +
                                       " , DELETEUSER = '{1}' " +
                                   " WHERE HEADGUID = '{2}' " +
                                     " AND DELETEUSER IS NULL ";

            // Delete line Sql
            string strSqlDelete = " UPDATE MENUORDERLINE " +
                                     " SET DELETETIME = '{0}' " +
                                       " , DELETEUSER = '{1}' " +
                                   " WHERE HEADGUID = '{2}' " +
                                     " AND DELETEUSER IS NULL ";

            // Insert Sql
            string strSqlInsert = " INSERT INTO MENUORDERHEAD " +
                                       " ( COSTCENTERCODE " +
                                       " , WINDOWGUID " +
                                       " , REQUIREDDATE " +
                                       " , SOITEMGUID " +
                                       " , ITEMGUID " +
                                       " , ITEMCODE " +
                                       " , ITEMTYPE " +
                                       " , ITEMNAME_ZH " +
                                       " , ITEMNAME_EN " +
                                       " , REQUIREDQTY " +
                                       " , ITEMCOST " +
                                       " , ITEMUNITCODE " +
                                       " , OTHERCOST " +
                                       " , PRODUCTGUID " +
                                       " , PURCHASEPOLICY " +
                                       " , ITEMCOLOR " +
                                       " , HEADGUID " +
                                       " , CREATETIME " +
                                       " , CREATEUSER " +
                                       " , LINKID ) " +
                                  " VALUES " +
                                       " ( '{0}' " +
                                       " , '{1}' " +
                                       " , '{2}' " +
                                       " , '{3}' " +
                                       " , '{4}' " +
                                       " , '{5}' " +
                                       " , '{6}' " +
                                       " , '{7}' " +
                                       " , '{8}' " +
                                       " , {9} " +
                                       " , {10} " +
                                       " , '{11}' " +
                                       " , {12} " +
                                       " , '{13}' " +
                                       " , '{14}' " +
                                       " , '{15}' " +
                                       " , '{16}' " +
                                       " , '{17}' " +
                                       " , '{18}' " +
                                       " , {19} ) ";

            foreach (MenuOrderHead mos in param.lstMenuOrderHead)
            {
                // 如果HEADGUID不为空且changeFlag不为'1'时不做处理
                if (string.IsNullOrWhiteSpace(mos.headGuid) || "1".Equals(mos.changeFlag))
                {
                    // ChangeFlag为'1'时增加删除Sql文
                    if ("1".Equals(mos.changeFlag))
                        sbSql.Append(string.Format(strSqlUpdate, strDateTime, param.userId, mos.headGuid));

                    // 如果DelteUser为空时增加新增Sql文
                    if (string.IsNullOrWhiteSpace(mos.deleteUser))
                    {
                        // 设置HeadGuid
                        strHEADGUID = string.IsNullOrWhiteSpace(mos.headGuid) ? Guid.NewGuid().ToString().ToUpper() : mos.headGuid;
                        // Insert Sql文
                        sbSql.Append(string.Format(strSqlInsert, mos.costCenterCode, mos.windowGuid, mos.requiredDate, mos.soItemGuid, mos.itemGuid
                                    , mos.itemCode, mos.itemType, mos.itemName_ZH, mos.itemName_EN, mos.requiredQty, mos.itemCost
                                    , mos.itemUnitCode, mos.otherCost, mos.productGuid, mos.purchasePolicy, mos.itemColor, strHEADGUID, strDateTime, param.userId 
                                    , mos.linkId));
                    }
                    else
                        // 反之删除该MenuOrder所关联的所有Item
                        sbSql.Append(string.Format(strSqlDelete, strDateTime, param.userId, mos.headGuid));
                }
                if (string.IsNullOrWhiteSpace(mos.deleteUser))
                    // 处理BOM部分
                    GetBomSQL(mos, strHEADGUID, strDateTime, param.userId, ref sbSql, ref lstRequireDate);
            }
            // 执行Sql文
            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSql.ToString());

            if(result > 0)
            {
                List<MenuOrderHead> lstTemp = param.lstMenuOrderHead.OrderBy(r => r.requiredDate).ToList();
                MenuOrderHead impParam = new MenuOrderHead
                {
                    costCenterCode = param.lstMenuOrderHead[0].costCenterCode,
                    requiredDate = lstTemp[0].requiredDate,
                    createUser = param.userId,
                    lstRequiredDate = lstRequireDate,
                };
                
                (new Purchase.PurchaseOrderFactory()).ImportToPurchaseOrder(impParam);
            }

            return result;
        }

        /// <summary>
        /// 取得Bom保存Sql
        /// </summary>
        /// <returns></returns>
        private void GetBomSQL(MenuOrderHead mos, string headGuid, string dateTime, string userId, ref StringBuilder sbSql, ref List<string> lstRequiredDate)
        {
            // Update Sql
            string strSqlUpdate = " UPDATE MENUORDERLINE " +
                                     " SET DELETETIME = '{0}' " +
                                       " , DELETEUSER = '{1}' " +
                                   " WHERE HEADGUID = '{2}' " +
                                     " AND LINEGUID = '{3}' " +
                                     " AND DELETETIME IS NULL ";

            string strSqlInsert = " INSERT INTO MENUORDERLINE " +
                                            " ( HEADGUID " +
                                            " , LINEGUID " +
                                            " , ITEMCODE " +
                                            " , ITEMNAME " +
                                            " , ITEMDESC " +
                                            " , ITEMTYPE " +
                                            " , BOMQTY " +
                                            " , ACTQTY " +
                                            " , SUPPLIERCODE " +
                                            " , SUPPLIERNAME " +
                                            " , CONVERSIONUNIT " +
                                            " , CONVERSIONRATE " +
                                            " , PURCHASEPRICE " +
                                            " , PURCHASEUNIT " +
                                            " , PURCHASETAX " +
                                            " , REQUIREDQTY " +
                                            " , REQUIREDDATE " +
                                            " , CREATEUSER " +
                                            " , CREATETIME " +
                                            " , SORTID ) " +
                                       " VALUES  " +
                                            " ( '{0}' " +
                                            " , '{1}' " +
                                            " , '{2}' " +
                                            " , '{3}' " +
                                            " , '{4}' " +
                                            " , '{5}' " +
                                            " , {6} " +
                                            " , {7} " +
                                            " , '{8}' " +
                                            " , '{9}' " +
                                            " , '{10}' " +
                                            " , {11} " +
                                            " , {12} " +
                                            " , '{13}' " +
                                            " , {14} " +
                                            " , {15} " +
                                            " , {16} " +
                                            " , '{17}' " +
                                            " , '{18}' " +
                                            " , {19} ) ";

            string strLineGuid = string.Empty;
            string strRequiredDate = null;
            string strDate1 = string.Empty;
            string strDate2 = string.Empty;

            if (mos.lstMenuOrderLine == null || mos.lstMenuOrderLine.Count == 0)
                return;

            foreach (MenuOrderLine mol in mos.lstMenuOrderLine)
            {
                // 如果ITEMGUID不为空且changeFlag不为'1'时不做处理
                if (!string.IsNullOrWhiteSpace(mol.lineGuid) && !"1".Equals(mol.changeFlag))
                    continue;

                // ChangeFlag为'1'时增加删除Sql文
                if ("1".Equals(mol.changeFlag))
                    sbSql.Append(string.Format(strSqlUpdate, dateTime, userId, headGuid, mol.lineGuid));

                strLineGuid = string.IsNullOrWhiteSpace(mol.lineGuid) ? Guid.NewGuid().ToString().ToUpper() : mol.lineGuid;

                if (string.IsNullOrWhiteSpace(mol.deleteUser))
                {
                    if (string.IsNullOrWhiteSpace(mol.requiredDate))
                        strRequiredDate = "NULL";
                    else
                        strRequiredDate = "'" + mol.requiredDate + "'";

                    sbSql.Append(string.Format(strSqlInsert, headGuid, strLineGuid, mol.itemCode, mol.itemName, mol.itemDesc
                        , mol.itemType, mol.bomQty, mol.actQty, mol.supplierCode, mol.supplierName, mol.conversionUnit
                        , mol.conversionRate, mol.purchasePrice, mol.purchaseUnit, mol.purchaseTax, mol.requiredQty, strRequiredDate,
                        userId, dateTime, mol.sortId));
                }
                /***记录下行项上有修改过的日期***/
                strDate1 = string.IsNullOrWhiteSpace(mol.requiredDate) ? string.Empty : mol.requiredDate;
                strDate2 = string.IsNullOrWhiteSpace(mol.requiredDate_bak) ? string.Empty : mol.requiredDate_bak;
                if (!strDate1.Equals(strDate2) && 
                    !(mos.requiredDate.Equals(strDate1) && string.Empty.Equals(strDate2)) &&
                    !(mos.requiredDate.Equals(strDate2) && string.Empty.Equals(strDate1)))
                    lstRequiredDate.Add(strDate1);
            }
            if (lstRequiredDate != null && lstRequiredDate.Any())
                lstRequiredDate = lstRequiredDate.Distinct().ToList();
        }

        #endregion

        #region 读取MenuOrder
        /// <summary>
        /// 根据日期、成本中心、档口取得MenuOrder
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<MenuOrderHead> GetMenuOrder(MenuOrderHead param)
        {
            MenuData.MenuDataFactory mdf = new MenuData.MenuDataFactory();
            List<MenuOrderHead> lstMenuOrderHead = new List<MenuOrderHead>();
            string strItemType = "FG&RM";

            string strSql = " SELECT M.ID " +
                                 " , M.COSTCENTERCODE " +
                                 " , M.WINDOWGUID " +
                                 " , CONVERT(VARCHAR(100), M.REQUIREDDATE, 20) AS REQUIREDDATE " +
                                 " , M.SOITEMGUID " +
                                 " , L.PRODUCTDESC " +
                                 " , M.ITEMGUID " +
                                 " , M.ITEMCODE " +
                                 " , M.ITEMTYPE " +
                                 " , M.ITEMNAME_ZH " +
                                 " , M.ITEMNAME_EN " +
                                 " , M.REQUIREDQTY " +
                                 " , M.ITEMCOST " +
                                 " , M.OTHERCOST " +
                                 " , M.ITEMUNITCODE " +
                                 " , M.HEADGUID " +
                                 " , M.PRODUCTGUID " +
                                 " , M.PURCHASEPOLICY " +
                                 " , CONVERT(VARCHAR(100), M.CREATETIME, 20) AS CREATETIME " +
                                 " , M.CREATEUSER " +
                                 " , CONVERT(VARCHAR(100), M.DELETETIME, 20) AS DELETETIME " +
                                 " , M.DELETEUSER " +
                                 " , (CASE WHEN M.LINKID = 0 THEN M.ID ELSE M.LINKID END) AS LINKID " +
                                 " , M.ITEMCOLOR " +
                              " FROM MENUORDERHEAD AS M " +
                             " INNER JOIN SALESORDERITEM AS L " +
                                " ON M.SOITEMGUID = L.ITEMGUID " +
                               " AND L.STATUS <> '0' " +
                             " WHERE M.COSTCENTERCODE = '{0}' " +
                               " AND M.DELETEUSER IS NULL " +
                               " AND M.REQUIREDDATE BETWEEN '{1}' AND '{2}' ";
            string strSqlWindowGuid = " AND M.WINDOWGUID = '{0}' ";

            if (!string.IsNullOrWhiteSpace(param.windowGuid))
                strSql = strSql + string.Format(strSqlWindowGuid, param.windowGuid);

            lstMenuOrderHead = SqlServerHelper.GetEntityList<MenuOrderHead>(SqlServerHelper.salesorderConn(), string.Format(strSql, param.costCenterCode
                , param.startDate, param.endDate));

            if (lstMenuOrderHead.Count == 0) { return lstMenuOrderHead; }

            /****** 获取产品类别 ******/
            // 设置获取类别主数据的参数
            Item itemParam = new Item();

            itemParam.company = param.dbName;
            itemParam.itemType = strItemType;

            // 取得类别主数据信息
            List<Item> lstItemData = mdf.searchItem(new Dictionary<string, dynamic>() {{ "dbName", param.dbName}});

            lstMenuOrderHead.GroupJoin(lstItemData,
                mos => new { GUID = mos.itemGuid, CODE = mos.itemCode },
                itemObj => new { GUID = itemObj.itemGuid, CODE = itemObj.itemCode },
                (mos, itemObj) =>
                {
                    var tempObj = itemObj.Where(r => !string.IsNullOrWhiteSpace(r.nationGUID)).FirstOrDefault();
                    // 星期几
                    mos.dayOfWeek = Convert.ToDateTime(mos.requiredDate).DayOfWeek.ToString().ToLower().Substring(0, 3);
                    // 阶层1 Guid
                    mos.nationGuid = !FG.Equals(mos.itemType) || tempObj == null ? string.Empty : tempObj.nationGUID;
                    // 阶层1 名称
                    mos.nation = !FG.Equals(mos.itemType) || tempObj == null ? string.Empty : tempObj.nation;
                    // 阶层2 Guid
                    mos.menuClassGuid = !FG.Equals(mos.itemType) || tempObj == null ? string.Empty : tempObj.menuClassGUID;
                    // 阶层2 名称
                    mos.menuClassName = !FG.Equals(mos.itemType) || tempObj == null ? string.Empty : tempObj.menuClassName;
                    // 阶层2 排序字段
                    mos.classSort = !FG.Equals(mos.itemType) || tempObj == null ? othClassSort : tempObj.classSort;

                    return mos;
                }).ToList();

            // 排序
            lstMenuOrderHead = lstMenuOrderHead.OrderBy(r => r.productDesc).ThenBy(r => r.classSort).ThenBy(r => r.linkId).ToList();

            // 取得MenuOrderLine数据集
            List<MenuOrderLine> lstMenuOrderLine = GetMenuOrderLine(lstMenuOrderHead, param);

            //lstMenuOrderHead.Join(lstMenuOrderLine,
            //    moh => new { headGuid = moh.headGuid },
            //    mol => new { headGuid = mol.headGuid },
            //    (moh, mol) =>
            //    {
            //        moh.lstMenuOrderLine = moh.lstMenuOrderLine.Add(mol);
            //    }).ToList();
            
            foreach(MenuOrderHead moh in lstMenuOrderHead)
            {
                moh.lstMenuOrderLine = lstMenuOrderLine.Where(r => moh.headGuid.Equals(r.headGuid)).ToList();

                if (moh.lstMenuOrderLine.Any(r => "adj".Equals(r.hasAdj)))
                    moh.hasAdj = "adj";
                moh.minRequiredDate = moh.lstMenuOrderLine.Where(r => !string.IsNullOrWhiteSpace(r.requiredDate)).Min(r => r.requiredDate);
            }
            return lstMenuOrderHead;
        }

        /// <summary>
        /// 取得MenuOrderHead所对应的行项信息
        /// </summary>
        /// <param name="moh"></param>
        /// <returns></returns>
        private List<MenuOrderLine> GetMenuOrderLine(List<MenuOrderHead> lstMenuOrderHead, MenuOrderHead param)
        {
            List<MenuOrderLine> lstMenuOrderLine = new List<MenuOrderLine>();
            List<MenuOrderLine> lstPurchase = new List<MenuOrderLine>();

            if (lstMenuOrderHead == null || lstMenuOrderHead.Count == 0)
                return lstMenuOrderLine;

            string strHeadGuid = string.Join("','", lstMenuOrderHead.Select(r => r.headGuid).Distinct().ToArray());
            strHeadGuid = "('" + strHeadGuid + "')";

            string strSql = " SELECT L.ID " +
                                 " , (CASE WHEN L.SORTID = 0 THEN L.ID ELSE L.SORTID END) AS SORTID " +
                                 " , L.HEADGUID " +
                                 " , L.LINEGUID " +
                                 " , L.ITEMCODE " +
                                 " , L.ITEMNAME " +
                                 " , L.ITEMDESC " +
                                 " , L.ITEMTYPE " +
                                 " , L.BOMQTY " +
                                 " , L.ACTQTY " +
                                 " , L.SUPPLIERCODE " +
                                 " , L.SUPPLIERNAME " +
                                 " , L.CONVERSIONUNIT " +
                                 " , L.CONVERSIONRATE " +
                                 " , L.PURCHASEPRICE " +
                                 " , L.PURCHASEUNIT " +
                                 " , L.PURCHASETAX " +
                                 " , L.PURCHASETAX AS TAX " +
                                 " , L.REQUIREDQTY " +
                                 " , L.CREATEUSER " +
                                 " , CONVERT(VARCHAR(100), L.CREATETIME, 20) AS CREATETIME " +
                                 " , L.DELETEUSER " +
                                 " , CONVERT(VARCHAR(100), L.DELETETIME, 20) AS DELETETIME " +
                                 " , H.PRODUCTGUID " +
                                 " , H.PURCHASEPOLICY " +
                                 " , CONVERT(VARCHAR(100), H.REQUIREDDATE, 20) AS WORKREQUIREDDATE " +
                                 " , CONVERT(VARCHAR(100), L.REQUIREDDATE, 23) AS REQUIREDDATE " +
                              " FROM MENUORDERLINE AS L" +
                             " INNER JOIN MENUORDERHEAD AS H " +
                                " ON L.HEADGUID = H.HEADGUID " +
                               " AND H.DELETEUSER IS NULL " +
                             " WHERE L.HEADGUID IN {0} " +
                               " AND L.DELETEUSER IS NULL " +
                             " ORDER BY L.HEADGUID  " +
                                 " , (CASE WHEN L.SORTID = 0 THEN L.ID ELSE L.SORTID END) ";

            string strSqlSp = " SELECT L.ITEMCODE " +
                                   " , L.REQUIREDQTY " +
                                   " , CONVERT(VARCHAR(100), H.REQUIREDDATE, 20) AS WORKREQUIREDDATE " +
                                " FROM MENUORDERLINE AS L" +
                               " INNER JOIN MENUORDERHEAD AS H " +
                                  " ON L.HEADGUID = H.HEADGUID " +
                               " WHERE H.COSTCENTERCODE = '{0}' " +
                                 " AND H.REQUIREDDATE BETWEEN '{1}' AND '{2}' " +
                                 " AND H.DELETEUSER IS NULL " +
                                 " AND L.DELETEUSER IS NULL " +
                                 " AND H.SOITEMGUID IS NULL " +
                                 " AND L.ADJFLAG = 'adj' " +
                                 " AND L.REQUIREDQTY <> 0 ";
                                 
            lstMenuOrderLine = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), string.Format(strSql
                , strHeadGuid));

            string minRequiredDate = lstMenuOrderLine.Where(r => !string.IsNullOrWhiteSpace(r.requiredDate)).Min(r => r.requiredDate);

            string workStartDate = param.startDate;
            if (!string.IsNullOrWhiteSpace(minRequiredDate) && SalesOrder.Common.convertDateTime(minRequiredDate).CompareTo(SalesOrder.Common.convertDateTime(param.startDate)) < 0)
                workStartDate = minRequiredDate;

            lstPurchase = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), string.Format(strSqlSp
                , param.costCenterCode, workStartDate, param.endDate));

            if (lstPurchase != null && lstPurchase.Count > 0)
            {
                lstMenuOrderLine.GroupJoin(lstPurchase,
                    line => new { ITEMCODE = line.itemCode, DATE = (line.requiredDate ?? line.workRequiredDate) },
                    pur => new { ITEMCODE = pur.itemCode, DATE = pur.workRequiredDate },
                    (line, pur) =>
                    {
                        bool check = pur == null || pur.Count() == 0;
                        // 有采购数量修改记录
                        line.hasAdj = check ? null : "adj";

                        return line;
                    }).ToList();
            }

            return lstMenuOrderLine;
        }
        #endregion

        #region 保存修改供应商、数量画面数据(菜单部分 + 直接采购部分)
        public int SaveMenuOrderPurchase(MenuOrderPurchaseParam param)
        {
            int result_m = 0;
            int result_p = 0;
            string costCenterCode = string.Empty;
            string createUser = string.Empty;
            string requiredDate = string.Empty;

            try { result_m = SaveMenuPart(param.menuPart); }
            catch (Exception e) { }

            try { result_p = SavePurchasePart(param.purchasePart); }
            catch(Exception e) { }

            MenuOrderHead impParam = new MenuOrderHead();

            costCenterCode = !string.IsNullOrWhiteSpace(param.menuPart.costCenterCode) ? param.menuPart.costCenterCode : param.purchasePart.costCenterCode;
            createUser = !string.IsNullOrWhiteSpace(param.menuPart.createUser) ? param.menuPart.createUser : param.purchasePart.createUser;
            requiredDate = !string.IsNullOrWhiteSpace(param.menuPart.requiredDate) ? param.menuPart.requiredDate : param.purchasePart.requiredDate;

            impParam.costCenterCode = costCenterCode;
            impParam.createUser = createUser;
            impParam.requiredDate = requiredDate;
            (new Purchase.PurchaseOrderFactory()).ImportToPurchaseOrder(impParam);

            return result_m + result_p;
        }

        public int SaveMenuPart(MenuOrderHead param)
        {
            if (param.lstMenuOrderLine == null || param.lstMenuOrderLine.Count == 0)
                return 0;

            int result = 0;
            StringBuilder sbSql = new StringBuilder();
            string strDateTime = SalesOrder.Common.convertDateTime(DateTime.Now.ToString());
            List<MenuOrderLine> lstSupplierChange = new List<MenuOrderLine>();
            List<MenuOrderLine> lstAdjRequiredQtyChange = new List<MenuOrderLine>();

            #region sql定义区
            string strSqlSelect = " SELECT L.ID " +
                                       " , (CASE WHEN L.SORTID = 0 THEN L.ID ELSE L.SORTID END) AS SORTID " +
                                       " , L.HEADGUID " +
                                       " , L.LINEGUID " +
                                       " , L.ITEMCODE " +
                                       " , L.ITEMNAME " +
                                       " , L.ITEMDESC " +
                                       " , L.ITEMTYPE " +
                                       " , L.BOMQTY " +
                                       " , L.ACTQTY " +
                                       " , L.SUPPLIERCODE " +
                                       " , L.SUPPLIERNAME " +
                                       " , L.CONVERSIONUNIT " +
                                       " , L.CONVERSIONRATE " +
                                       " , L.PURCHASEPRICE " +
                                       " , L.PURCHASEUNIT " +
                                       " , L.PURCHASETAX " +
                                       " , L.REQUIREDQTY " +
                                       " , L.CREATEUSER " +
                                       " , CONVERT(VARCHAR(100), L.CREATETIME, 20) AS CREATETIME " +
                                       " , L.DELETEUSER " +
                                       " , CONVERT(VARCHAR(100), L.DELETETIME, 20) AS DELETETIME " +
                                       " , ADJFLAG " +
                                    " FROM MENUORDERLINE L" +
                                   " INNER JOIN MENUORDERHEAD H " +
                                      " ON L.HEADGUID = H.HEADGUID " +
                                     " AND H.DELETEUSER IS NULL " +
                                     " AND L.DELETEUSER IS NULL " +
                                   " WHERE H.COSTCENTERCODE = '{0}' " +
                                     " AND H.REQUIREDDATE = '{1}' " +
                                     " AND L.ITEMCODE IN {2} ";
            string strSqlAdj = "       AND L.ADJFLAG = 'adj' ";

            string strSqlDelete = " UPDATE MENUORDERLINE " +
                                     " SET DELETEUSER = '{0}' " +
                                       " , DELETETIME = '{1}' " +
                                   " WHERE ID = {2} " +
                                     " AND DELETEUSER IS NULL ";

            string strSqlInsert = " INSERT INTO MENUORDERLINE " +
                                       " ( HEADGUID " +
                                       " , LINEGUID " +
                                       " , ITEMCODE " +
                                       " , ITEMNAME " +
                                       " , ITEMDESC " +
                                       " , REMARK " +
                                       " , ITEMTYPE " +
                                       " , BOMQTY " +
                                       " , ACTQTY " +
                                       " , SUPPLIERCODE " +
                                       " , SUPPLIERNAME " +
                                       " , CONVERSIONUNIT " +
                                       " , CONVERSIONRATE " +
                                       " , PURCHASEPRICE " +
                                       " , PURCHASEUNIT " +
                                       " , PURCHASETAX " +
                                       " , REQUIREDQTY " +
                                       " , ADJFLAG " +
                                       " , CREATEUSER " +
                                       " , CREATETIME " +
                                       " , SORTID ) " +
                                  " VALUES  " +
                                       " ( '{0}' " +
                                       " , '{1}' " +
                                       " , '{2}' " +
                                       " , '{3}' " +
                                       " , '{4}' " +
                                       " , '{5}' " +
                                       " , '{6}' " +
                                       " , {7} " +
                                       " , {8} " +
                                       " , '{9}' " +
                                       " , '{10}' " +
                                       " , '{11}' " +
                                       " , {12} " +
                                       " , {13} " +
                                       " , '{14}' " +
                                       " , {15} " +
                                       " , {16} " +
                                       " , '{17}' " +
                                       " , '{18}' " +
                                       " , '{19}' " +
                                       " , {20} ) ";
            #endregion

            /*** Step1: 供应商更新 ***/
            // 拼接需要更新SupplierCode的ItemCode集合 
            string strItemCode = string.Join("','", param.lstMenuOrderLine.Where(r => !r.supplierCode.Equals(r.supplierCode_bak) ||
                r.remark != r.remark_bak).Select(r => r.itemCode).Distinct().ToArray());

            if (!string.IsNullOrWhiteSpace(strItemCode))
            { 
                strItemCode = "('" + strItemCode + "')";

                // 取得需要更新的MenuOrderLine基本数据(菜单部分)
                lstSupplierChange = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(),
                    string.Format(strSqlSelect, param.costCenterCode, param.requiredDate, strItemCode));

                // 匹配新的供应商Code和名称
                lstSupplierChange = lstSupplierChange.Join(param.lstMenuOrderLine,
                    spc => new { ITEMCODE = spc.itemCode },
                    tmp => new { ITEMCODE = tmp.itemCode },
                    (spc, tmp) =>
                    {
                        /***新的备注/说明***/
                        spc.remark_new = tmp.remark;
                        /***新的供应商相关部分***/
                        // 新的供应商代码
                        spc.supplierCode_new = tmp.supplierCode;
                        // 新的供应商名称
                        spc.supplierName_new = tmp.supplierName;
                        // 新的采购单价
                        spc.purchasePrice_new = tmp.purchasePrice;
                        // 新的采购单位
                        spc.purchaseUnit_new = tmp.purchaseUnit;
                        // 新的采购税率
                        spc.purchaseTax_new = tmp.purchaseTax;
                        // 新的转换系数
                        spc.conversionRate_new = tmp.conversionRate;
                        // 新的转换单位
                        spc.conversionUnit_new = tmp.conversionUnit;

                        return spc;
                    }).Where(r => !string.IsNullOrWhiteSpace(r.supplierCode_new)).ToList();
            }

            // 拼接需要更新调整数量的ItemCode集合
            lstAdjRequiredQtyChange = param.lstMenuOrderLine.Where(r => !r.adjRequiredQty.Equals(r.adjRequiredQty_bak)).ToList();

            strItemCode = string.Join("','", lstAdjRequiredQtyChange.Select(r => r.itemCode).Distinct().ToArray());

            if (!string.IsNullOrWhiteSpace(strItemCode))
            {
                strItemCode = "('" + strItemCode + "')";

                // 取得需要更新的MenuOrderLine基本数据(含菜单部分和数量调整的所有ItemCode相关记录)
                List<MenuOrderLine> lstTemp = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), string.Format(
                    strSqlSelect + strSqlAdj, param.costCenterCode, param.requiredDate, strItemCode));

                if (lstTemp != null && lstTemp.Count > 0)
                    // 匹配LineGuid
                    lstAdjRequiredQtyChange = lstAdjRequiredQtyChange.GroupJoin(lstTemp,
                        adj => new { ITEMCODE = adj.itemCode },
                        tmp => new { ITEMCODE = tmp.itemCode },
                        (adj, tmp) =>
                        {
                            bool check = tmp == null || tmp.Count() == 0;
                            // LineGuid
                            adj.lineGuid = check ? null : tmp.FirstOrDefault().lineGuid;
                            // id
                            adj.id = check ? adj.id : tmp.FirstOrDefault().id;

                            return adj;
                        }).ToList();
            }

            // 先做供应商更新
            foreach (MenuOrderLine spc in lstSupplierChange)
            {
                // 如果当前项次为数量调整记录，且本次也有数量调整操作时先跳过，留到处理数量调整时一并操作
                if ("adj".Equals(spc.adjFlag) && lstAdjRequiredQtyChange.Any(r => r.itemCode.Equals(spc.itemCode)))
                    continue;
                
                // 先做删除
                sbSql.Append(string.Format(strSqlDelete, param.createUser, strDateTime, spc.id));

                // 再做新增
                sbSql.Append(string.Format(strSqlInsert, spc.headGuid, spc.lineGuid, spc.itemCode, spc.itemName, spc.itemDesc
                    , spc.remark_new, spc.itemType, spc.bomQty, spc.actQty, spc.supplierCode_new, spc.supplierName_new, spc.conversionUnit_new
                    , spc.conversionRate_new, spc.purchasePrice_new, spc.purchaseUnit_new, spc.purchaseTax_new, spc.requiredQty
                    , spc.adjFlag, param.createUser, strDateTime, spc.sortId));
            }

            // 再处理调整数量
            string strHEADGUID = string.Empty;

            if (lstAdjRequiredQtyChange.Count > 0)
            {
                strHEADGUID = CheckHeadExist(param);

                // Head不存在时新建Head
                if (string.IsNullOrWhiteSpace(strHEADGUID))
                {
                    strHEADGUID = Guid.NewGuid().ToString().ToUpper();
                    sbSql.Append(CreatePurchaseMenuOrderHead(strHEADGUID, param, strDateTime));
                }

                foreach (MenuOrderLine adj in lstAdjRequiredQtyChange)
                {
                    if (!string.IsNullOrWhiteSpace(adj.lineGuid))
                        // 先做删除
                        sbSql.Append(string.Format(strSqlDelete, param.createUser, strDateTime, adj.id));
                    else
                        adj.lineGuid = Guid.NewGuid().ToString().ToUpper();

                    sbSql.Append(GetMenuOrderLineSql(adj, param, strHEADGUID, strDateTime, "adj"));
                }
            }

            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSql.ToString());

            return result;
        }

        /// <summary>
        /// 保存直接采购部分的数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int SavePurchasePart(MenuOrderHead param)
        {
            if (param.lstMenuOrderLine == null || param.lstMenuOrderLine.Count == 0)
                return 0;

            int result = 0;
            StringBuilder sbSql = new StringBuilder();
            string strDateTime = SalesOrder.Common.convertDateTime(DateTime.Now.ToString());
            string strHEADGUID = string.Empty;

            // Delete line Sql
            string strSqlDelete = " UPDATE MENUORDERLINE " +
                                     " SET DELETETIME = '{0}' " +
                                       " , DELETEUSER = '{1}' " +
                                   " WHERE LINEGUID = '{2}' " +
                                     " AND DELETEUSER IS NULL ";

            strHEADGUID = CheckHeadExist(param);

            // Head不存在时新建Head
            if (string.IsNullOrWhiteSpace(strHEADGUID))
            {
                strHEADGUID = Guid.NewGuid().ToString().ToUpper();
                sbSql.Append(CreatePurchaseMenuOrderHead(strHEADGUID, param, strDateTime));
            }

            foreach (MenuOrderLine mol in param.lstMenuOrderLine)
            {
                // 如果LineGuid不为空时先做删除
                if (!string.IsNullOrWhiteSpace(mol.lineGuid))
                    sbSql.Append(string.Format(strSqlDelete, strDateTime, param.createUser, mol.lineGuid));

                // 新增Sql
                sbSql.Append(GetMenuOrderLineSql(mol, param, strHEADGUID, strDateTime));
            }
            // 执行Sql文
            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSql.ToString());

            return result;
        }

        /// <summary>
        /// 取得MenuOrderLine Insert Sql
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="strHeadGuid"></param>
        /// <param name="strCostCenterCode"></param>
        /// <returns></returns>
        private string GetMenuOrderLineSql(MenuOrderLine mol, MenuOrderHead param, string strHeadGuid, string strDateTime
            , string adj = "")
        {
            // Insert Line Sql
            string strSqlLine = " INSERT INTO MENUORDERLINE " +
                                     " ( HEADGUID " +
                                     " , LINEGUID " +
                                     " , ITEMCODE " +
                                     " , ITEMNAME " +
                                     " , ITEMTYPE " +
                                     " , REMARK " +
                                     " , REQUIREDQTY " +
                                     " , SUPPLIERCODE " +
                                     " , SUPPLIERNAME " +
                                     " , CONVERSIONUNIT " +
                                     " , CONVERSIONRATE " +
                                     " , PURCHASEPRICE " +
                                     " , PURCHASEUNIT " +
                                     " , PURCHASETAX " +
                                     " , CREATEUSER " +
                                     " , CREATETIME " +
                                     " , SORTID ) " +
                                " VALUES  " +
                                     " ( '{0}' " +
                                     " , '{1}' " +
                                     " , '{2}' " +
                                     " , '{3}' " +
                                     " , '{4}' " +
                                     " , '{5}' " +
                                     " , {6} " +
                                     " , '{7}' " +
                                     " , '{8}' " +
                                     " , '{9}' " +
                                     " , '{10}' " +
                                     " , {11} " +
                                     " , '{12}' " +
                                     " , '{13}' " +
                                     " , '{14}' " +
                                     " , '{15}' " +
                                     " , {16} ) ";

            // Insert Line Sql
            string strSqlLineAdj = " INSERT INTO MENUORDERLINE " +
                                        " ( HEADGUID " +
                                        " , LINEGUID " +
                                        " , ITEMCODE " +
                                        " , ITEMNAME " +
                                        " , ITEMDESC " +
                                        " , ITEMTYPE " +
                                        " , REMARK " +
                                        " , BOMQTY " +
                                        " , ACTQTY " +
                                        " , REQUIREDQTY " +
                                        " , SUPPLIERCODE " +
                                        " , SUPPLIERNAME " +
                                        " , CONVERSIONUNIT " +
                                        " , CONVERSIONRATE " +
                                        " , PURCHASEPRICE " +
                                        " , PURCHASEUNIT " +
                                        " , PURCHASETAX " +
                                        " , ADJFLAG " +
                                        " , CREATEUSER " +
                                        " , CREATETIME " +
                                        " , SORTID ) " +
                                   " VALUES  " +
                                        " ( '{0}' " +
                                        " , '{1}' " +
                                        " , '{2}' " +
                                        " , '{3}' " +
                                        " , '{4}' " +
                                        " , '{5}' " +
                                        " , '{6}' " +
                                        " , {7} " +
                                        " , {8} " +
                                        " , {9} " +
                                        " , '{10}' " +
                                        " , '{11}' " +
                                        " , '{12}' " +
                                        " , '{13}' " +
                                        " , {14} " +
                                        " , '{15}' " +
                                        " , '{16}' " +
                                        " , '{17}' " +
                                        " , '{18}' " +
                                        " , '{19}' " +
                                        " , {20} ) ";

            // LineGuid
            string strLineGuid = string.IsNullOrWhiteSpace(mol.lineGuid) ? Guid.NewGuid().ToString().ToUpper() : mol.lineGuid;

            if (string.IsNullOrWhiteSpace(adj))
                strSqlLine = string.Format(strSqlLine, strHeadGuid, strLineGuid, mol.itemCode, mol.itemName, mol.itemType, mol.remark
                    , mol.requiredQty, mol.supplierCode, mol.supplierName, mol.conversionUnit , mol.conversionRate, mol.purchasePrice
                    , mol.purchaseUnit, mol.purchaseTax, param.createUser, strDateTime, mol.sortId);
            else
                strSqlLine = string.Format(strSqlLineAdj, strHeadGuid, strLineGuid, mol.itemCode, mol.itemName, mol.itemDesc
                    , mol.itemType, mol.remark, mol.bomQty, mol.actQty, mol.adjRequiredQty - mol.requiredQty, mol.supplierCode
                    , mol.supplierName, mol.conversionUnit, mol.conversionRate, mol.purchasePrice, mol.purchaseUnit, mol.purchaseTax
                    , "adj", param.createUser, strDateTime, mol.sortId);

            return strSqlLine;
        }

        /// <summary>
        /// 检查直接采购Head是否存在
        /// </summary>
        /// <returns></returns>
        private string CheckHeadExist(MenuOrderHead param)
        {
            // Check Sql(SOITEMGUID为空)
            string strCheck = " SELECT HEADGUID " +
                                " FROM MENUORDERHEAD " +
                               " WHERE SOITEMGUID IS NULL " +
                                 " AND COSTCENTERCODE = '{0}' " +
                                 " AND REQUIREDDATE = '{1}' " +
                                 " AND DELETEUSER IS NULL ";

            MenuOrderHead headObj = SqlServerHelper.GetEntity<MenuOrderHead>(SqlServerHelper.salesorderConn(), string.Format(strCheck
                , param.costCenterCode, param.requiredDate));

            if (headObj != null)
                return headObj.headGuid;

            return string.Empty;
        }

        /// <summary>
        /// 创建直接采购Head记录
        /// </summary>
        /// <param name="strHEADGUID"></param>
        /// <param name="param"></param>
        /// <param name="strDateTime"></param>
        /// <returns></returns>
        private string CreatePurchaseMenuOrderHead(string strHEADGUID, MenuOrderHead param, string strDateTime)
        {
            // Insert Sql
            string strSqlInsert = " INSERT INTO MENUORDERHEAD " +
                                       " ( HEADGUID " +
                                       " , COSTCENTERCODE " +
                                       " , REQUIREDDATE " +
                                       " , CREATEUSER " +
                                       " , CREATETIME ) " +
                                  " VALUES " +
                                       " ( '{0}' " +
                                       " , '{1}' " +
                                       " , '{2}' " +
                                       " , '{3}' " +
                                       " , '{4}' ) ";

            string strSql = string.Format(strSqlInsert, strHEADGUID, param.costCenterCode, param.requiredDate, param.createUser,
                   strDateTime);
            return strSql;
        }

        #endregion

        #region 读取修改供应商、数量画面数据(菜单部分 + 直接采购部分)
        /// <summary>
        /// 读取修改供应商画面数据(菜单部分 + 直接采购部分)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public MenuOrderPurchaseParam GetMenuOrderPurchase(MenuOrderHead param)
        {
            MenuOrderPurchaseParam mopp = new MenuOrderPurchaseParam();
            // 菜单部分
            try { mopp.lstMenuPart = GetMenuPart(param); }
            catch (Exception e) { }

            // 直接采购部分
            try { mopp.lstPurchasePart = GetPurchasePart(param); }
            catch (Exception e) { }

            return mopp;
        }

        /// <summary>
        /// 取得直接采购部分数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private List<MenuOrderLine> GetPurchasePart(MenuOrderHead param)
        {
            // 取得Head部分(直接采购的情况SOITEMGUID为空且最多只有一条)
            string strSqlHead = " SELECT HEADGUID " +
                                  " FROM MENUORDERHEAD " +
                                 " WHERE SOITEMGUID IS NULL " +
                                   " AND COSTCENTERCODE = '{0}' " +
                                   " AND REQUIREDDATE = '{1}' " +
                                   " AND DELETEUSER IS NULL ";

            MenuOrderHead moh = SqlServerHelper.GetEntity<MenuOrderHead>(SqlServerHelper.salesorderConn(), string.Format(strSqlHead
                , param.costCenterCode, param.requiredDate));

            if (moh == null)
                return null;

            // 取得Line部分
            string strSqlLine = " SELECT L.ID " +
                                     " , (CASE WHEN L.SORTID = 0 THEN L.ID ELSE L.SORTID END) AS SORTID " +
                                     " , L.HEADGUID " +
                                     " , L.LINEGUID " +
                                     " , L.ITEMCODE " +
                                     " , L.ITEMNAME " +
                                     " , L.ITEMDESC " +
                                     " , L.ITEMTYPE " +
                                     " , L.BOMQTY " +
                                     " , L.ACTQTY " +
                                     " , L.REMARK " +
                                     " , L.REMARK AS REMARK_BAK " +
                                     " , L.REQUIREDQTY " +
                                     " , L.REQUIREDQTY AS REQUIREDQTY_BAK " +
                                     " , L.SUPPLIERCODE " +
                                     " , L.SUPPLIERCODE AS SUPPLIERCODE_BAK " +
                                     " , L.SUPPLIERNAME " +
                                     " , L.CONVERSIONUNIT " +
                                     " , L.CONVERSIONRATE " +
                                     " , L.PURCHASEPRICE " +
                                     " , L.PURCHASEUNIT " +
                                     " , L.PURCHASETAX " +
                                     " , L.CREATEUSER " +
                                     " , CONVERT(VARCHAR(100), L.CREATETIME, 20) AS CREATETIME " +
                                  " FROM MENUORDERLINE AS L" +
                                 " INNER JOIN MENUORDERHEAD AS H " +
                                    " ON L.HEADGUID = H.HEADGUID " +
                                 " WHERE L.HEADGUID = '{0}' " +
                                   " AND L.DELETEUSER IS NULL " +
                                   " AND L.ADJFLAG IS NULL " +
                                 " ORDER BY L.HEADGUID  " +
                                     " , (CASE WHEN L.SORTID = 0 THEN L.ID ELSE L.SORTID END) ";

            List<MenuOrderLine> lstMol = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), string.Format(
                strSqlLine, moh.headGuid));

            if (lstMol == null || lstMol.Count == 0)
                return null;

            return lstMol;
        }

        /// <summary>
        /// 取得菜单部分数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private List<MenuOrderLine> GetMenuPart(MenuOrderHead param)
        {
            string strSqlLine = " SELECT L.ITEMCODE " +
                                     " , L.ITEMNAME " +
                                     " , L.ITEMDESC " +
                                     " , L.ITEMTYPE " +
                                     " , L.BOMQTY " +
                                     " , L.ACTQTY " +
                                     " , ISNULL(L.REMARK, '') AS REMARK " +
                                     " , ISNULL(L.REMARK, '') AS REMARK_BAK " +
                                     " , L.REQUIREDQTY " +
                                     " , L.SUPPLIERCODE " +
                                     " , L.SUPPLIERCODE AS SUPPLIERCODE_BAK " +
                                     " , L.SUPPLIERNAME " +
                                     " , L.CONVERSIONUNIT " +
                                     " , L.CONVERSIONRATE " +
                                     " , L.PURCHASEPRICE " +
                                     " , L.PURCHASEUNIT " +
                                     " , L.PURCHASETAX " +
                                     " , CONVERT(VARCHAR(100), H.REQUIREDDATE, 23) AS REQUIREDDATE " +
                                  " FROM MENUORDERLINE AS L" +
                                 " INNER JOIN MENUORDERHEAD AS H " +
                                    " ON L.HEADGUID = H.HEADGUID " +
                                   " AND H.DELETEUSER IS NULL " +
                                   " AND L.DELETEUSER IS NULL  " +
                                 " WHERE H.SOITEMGUID IS NOT NULL" +
                                   " AND H.COSTCENTERCODE = '{0}' " +
                                   " AND ISNULL(L.REQUIREDDATE, H.REQUIREDDATE) = '{1}' ";

            string strSqlAdj = " SELECT L.ITEMCODE " +
                                    " , L.REQUIREDQTY " +
                                 " FROM MENUORDERLINE AS L" +
                                " INNER JOIN MENUORDERHEAD AS H " +
                                   " ON L.HEADGUID = H.HEADGUID " +
                                  " AND H.DELETEUSER IS NULL " +
                                  " AND L.DELETEUSER IS NULL  " +
                                " WHERE H.SOITEMGUID IS NULL" +
                                  " AND L.ADJFLAG = 'adj' " +
                                  " AND H.COSTCENTERCODE = '{0}' " +
                                  " AND H.REQUIREDDATE = '{1}' ";
            // 菜单部分
            List<MenuOrderLine> lstMol = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), string.Format(
                strSqlLine, param.costCenterCode, param.requiredDate));

            // 数量调整部分
            List<MenuOrderLine> lstAdj = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), string.Format(
                strSqlAdj, param.costCenterCode, param.requiredDate));

            if (lstMol.Count == 0)
                return null;

            lstMol = lstMol.OrderBy(r => r.itemCode).OrderBy(r => r.requiredDate).ToList();

            lstMol = (from l in lstMol
                      group l by new { l.itemCode } into lq
                      let firstMol = lq.FirstOrDefault()
                      let daylyRequiredQty = lq.Sum(r => r.requiredQty)
                      select new MenuOrderLine
                      {
                          itemCode = firstMol.itemCode,
                          itemName = firstMol.itemName,
                          itemDesc = firstMol.itemDesc,
                          itemType = firstMol.itemType,
                          bomQty = firstMol.bomQty,
                          actQty = firstMol.actQty,
                          remark = firstMol.remark,
                          remark_bak = firstMol.remark_bak,
                          requiredQty = daylyRequiredQty,
                          adjRequiredQty = daylyRequiredQty,
                          // TODO
                          adjRequiredQty_bak = daylyRequiredQty,
                          supplierCode = firstMol.supplierCode,
                          supplierCode_bak = firstMol.supplierCode_bak,
                          supplierName = firstMol.supplierName,
                          conversionUnit = firstMol.conversionUnit,
                          conversionRate = firstMol.conversionRate,
                          purchasePrice = firstMol.purchasePrice,
                          purchaseUnit = firstMol.purchaseUnit,
                          purchaseTax = firstMol.purchaseTax
                      } into lines
                      select lines).ToList();

            if (lstAdj != null && lstAdj.Count > 0)
            {
                lstMol = lstMol.GroupJoin(lstAdj,
                    mol => new { ITEMCODE = mol.itemCode },
                    adj => new { ITEMCODE = adj.itemCode },
                    (mol, adj) =>
                    {
                        bool check = adj == null || adj.Count() == 0;
                        mol.adjRequiredQty = check ? mol.adjRequiredQty : mol.adjRequiredQty + adj.FirstOrDefault().requiredQty;
                        mol.adjRequiredQty_bak = mol.adjRequiredQty;

                        return mol;
                    }).ToList();
            }

            lstMol = lstMol.OrderBy(r => r.itemCode).ToList();

            return lstMol;
        }
        #endregion

        #region 根据CostCenterCode + 日期取得已存在的ItemCode和供应商的对应关系(Angel调用)
        public List<MenuOrderLine> GetItemSupplierMapping(string costCenterCode, string requiredDate)
        {
            if (string.IsNullOrWhiteSpace(costCenterCode) || string.IsNullOrWhiteSpace(requiredDate))
                return null;

            string strSql = " SELECT L.ITEMCODE " +
                                 " , MAX(L.SUPPLIERCODE) AS SUPPLIERCODE " +
                                 " , MAX(L.SUPPLIERNAME) AS SUPPLIERNAME " +
                                 " , MAX(L.CONVERSIONUNIT) AS CONVERSIONUNIT " +
                                 " , MAX(L.CONVERSIONRATE) AS CONVERSIONRATE " +
                                 " , MAX(L.PURCHASEPRICE) AS PURCHASEPRICE " +
                                 " , MAX(L.PURCHASETAX) AS PURCHASETAX " +
                                 " , MAX(L.PURCHASEUNIT) AS PURCHASEUNIT " +
                              " FROM MENUORDERLINE AS L " +
                             " INNER JOIN MENUORDERHEAD AS H " +
                                " ON L.HEADGUID = H.HEADGUID " +
                             " WHERE H.REQUIREDDATE = '{0}' " +
                               " AND H.COSTCENTERCODE = '{1}' " +
                               " AND H.DELETEUSER IS NULL " +
                               " AND L.DELETEUSER IS NULL " +
                             " GROUP BY L.ITEMCODE ";

            List<MenuOrderLine> lstMenuOrderLine = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), string.Format(strSql
                , requiredDate, costCenterCode));

            return lstMenuOrderLine;
        }
        #endregion

        #region [废弃] - 供应商修改共用画面数据获取
        /// <summary>
        /// 取得供应商修改共用画面数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<POItem> GetPOItems(POItemParam param)
        {
            MenuData.MenuDataFactory mdf = new MenuData.MenuDataFactory();
            // 原料->供应商对应关系信息取得
            List<Item> purchaseItem = mdf.itemSource(param.dbName, param.costCenterCode, param.startDate);
            // PO基础数据取得
            List<POItem> lstPOItems = GetPOItems(param.costCenterCode, param.startDate, param.endDate);
            // 集计每日的原料需求数
            SummaryRequiredQty(ref lstPOItems);

            var list = lstPOItems.Join(purchaseItem, po => po.itemCode, pi => pi.itemCode, (po, pi) => new { po, pi });
            if ("po".Equals(param.mode.ToLower())) {
                lstPOItems = list//lstPOItems.GroupJoin(purchaseItem, po => po.itemCode, pi => pi.itemCode, (po, pi) => new { po, pi }).Where(q => q.pi != null)
                    .Select(q =>
                    {
                        // 可选供应商列表
                        q.po.lstSuppliers = q.pi.suppliers;//.ToList();

                        return q.po;
                    }).ToList();
            } else {
                lstPOItems = list.Where(q=>q.pi.suppliers.Count() > 1)//lstPOItems.GroupJoin(purchaseItem, po => po.itemCode, pi => pi.itemCode, (po, pi) => new { po, pi }).Where(q => q.pi != null && q.pi.Count() > 1)
                    .Select(q =>
                    {
                        // 可选供应商列表
                        q.po.lstSuppliers = q.pi.suppliers; //.ToList();

                        return q.po;
                    }).ToList();
            }

            lstPOItems = lstPOItems.OrderBy(r=>r.itemCode).ToList();

            return lstPOItems;
        }

        /// <summary>
        /// PO基础数据取得
        /// </summary>
        /// <param name="costCenterCode"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private List<POItem> GetPOItems(string costCenterCode, string startDate, string endDate)
        {
            List<POItem> lstPOItem = new List<POItem>();

            string strSql = " SELECT L.ITEMCODE " +
                                 " , L.ITEMNAME " +
                                 " , L.ITEMDESC " +
                                 " , L.REQUIREDQTY " +
                                 " , CONVERT(VARCHAR(100), H.REQUIREDDATE, 20) AS REQUIREDDATE " +
                                 " , L.PURCHASEPRICE " +
                                 " , L.PURCHASEUNIT " +
                                 " , L.PURCHASEPRICE AS PURCHASEPRICE_BAK " +
                                 " , L.PURCHASEUNIT AS PURCHASEUNIT_BAK " +
                                 " , L.SUPPLIERCODE " +
                                 " , L.SUPPLIERNAME " +
                                 " , L.SUPPLIERCODE AS SUPPLIERCODE_BAK " +
                                 " , L.SUPPLIERNAME AS SUPPLIERNAME_BAK " +
                                 " , L.CONVERSIONRATE " +
                                 " , L.CONVERSIONUNIT " +
                                 " , L.CONVERSIONRATE AS CONVERSIONRATE_BAK " +
                                 " , L.CONVERSIONUNIT AS CONVERSIONUNIT_BAK " +
                            " FROM MENUORDERLINE AS L " +
                           " INNER JOIN MENUORDERHEAD AS H " +
                              " ON L.HEADGUID = H.HEADGUID " +
                             " AND H.DELETEUSER IS NULL " +
                             " AND L.DELETEUSER IS NULL  " +
                           " WHERE H.COSTCENTERCODE = '{0}' " +
                             " AND H.REQUIREDDATE BETWEEN '{1}' AND '{2}' ";

            lstPOItem = SqlServerHelper.GetEntityList<POItem>(SqlServerHelper.salesorderConn(), string.Format(strSql, costCenterCode
                ,  startDate, endDate));

            return lstPOItem;
        }
        /// <summary>
        /// 根据日期和原料集计原料数量
        /// </summary>
        private void SummaryRequiredQty(ref List<POItem> lstPOItem)
        {
            lstPOItem = (from l in lstPOItem
                         group l by new { l.itemCode, l.supplierCode } into lq
                         let firstPOItem = lq.FirstOrDefault()
                         let daylyQty = lq.Sum(r => r.requiredQty)
                         select new POItem
                         {
                             itemCode = firstPOItem.itemCode,
                             itemName = firstPOItem.itemName,
                             itemDesc = firstPOItem.itemDesc,
                             supplierCode = firstPOItem.supplierCode,
                             supplierName = firstPOItem.supplierName,
                             supplierCode_bak = firstPOItem.supplierCode_bak,
                             supplierName_bak = firstPOItem.supplierName_bak,
                             purchasePrice = firstPOItem.purchasePrice,
                             purchaseUnit = firstPOItem.purchaseUnit,
                             daylyQty = daylyQty,
                             monQty = lq.Where(r => (1 == (int)Convert.ToDateTime(r.requiredDate).DayOfWeek)).Sum(r => r.requiredQty),
                             tueQty = lq.Where(r => (2 == (int)Convert.ToDateTime(r.requiredDate).DayOfWeek)).Sum(r => r.requiredQty),
                             wedQty = lq.Where(r => (3 == (int)Convert.ToDateTime(r.requiredDate).DayOfWeek)).Sum(r => r.requiredQty),
                             thuQty = lq.Where(r => (4 == (int)Convert.ToDateTime(r.requiredDate).DayOfWeek)).Sum(r => r.requiredQty),
                             friQty = lq.Where(r => (5 == (int)Convert.ToDateTime(r.requiredDate).DayOfWeek)).Sum(r => r.requiredQty),
                             satQty = lq.Where(r => (6 == (int)Convert.ToDateTime(r.requiredDate).DayOfWeek)).Sum(r => r.requiredQty),
                             sunQty = lq.Where(r => (7 == (int)Convert.ToDateTime(r.requiredDate).DayOfWeek)).Sum(r => r.requiredQty),
                         } into lines
                         select lines).ToList();
        }
        #endregion

        #region [废弃] - 供应商更新
        /// <summary>
        /// 更新供应商
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int ChangeSupplier(POItemParam param)
        {
            int result = 0;

            string strSqlSelect = " SELECT L.ID " +
                                       " , (CASE WHEN L.SORTID = 0 THEN L.ID ELSE L.SORTID END) AS SORTID " +
                                       " , L.HEADGUID " +
                                       " , L.LINEGUID " +
                                       " , L.ITEMCODE " +
                                       " , L.ITEMNAME " +
                                       " , L.ITEMDESC " +
                                       " , L.BOMQTY " +
                                       " , L.ACTQTY " +
                                       " , L.SUPPLIERCODE " +
                                       " , L.SUPPLIERNAME " +
                                       " , L.CONVERSIONUNIT " +
                                       " , L.CONVERSIONRATE " +
                                       " , L.PURCHASEPRICE " +
                                       " , L.PURCHASEUNIT " +
                                       " , L.REQUIREDQTY " +
                                       " , L.CREATEUSER " +
                                       " , CONVERT(VARCHAR(100), L.CREATETIME, 20) AS CREATETIME " +
                                       " , L.DELETEUSER " +
                                       " , CONVERT(VARCHAR(100), L.DELETETIME, 20) AS DELETETIME " +
                                    " FROM MENUORDERLINE L" +
                                   " INNER JOIN MENUORDERHEAD H " +
                                      " ON L.HEADGUID = H.HEADGUID " +
                                     " AND H.DELETEUSER IS NULL " +
                                     " AND L.DELETEUSER IS NULL " +
                                   " WHERE H.COSTCENTERCODE = '{0}' " +
                                     " AND H.REQUIREDDATE BETWEEN '{1}' AND '{2}' " +
                                     " AND L.ITEMCODE IN {3} ";

            string strSqlDelete = " UPDATE MENUORDERLINE " +
                                     " SET DELETEUSER = '{0}' " +
                                       " , DELETETIME = '{1}' " +
                                   " WHERE ID = {2} " +
                                     " AND DELETEUSER IS NULL " ;

            string strSqlInsert = " INSERT INTO MENUORDERLINE " +
                                       " ( HEADGUID " +
                                       " , LINEGUID " +
                                       " , ITEMCODE " +
                                       " , ITEMNAME " +
                                       " , ITEMDESC " +
                                       " , BOMQTY " +
                                       " , ACTQTY " +
                                       " , SUPPLIERCODE " +
                                       " , SUPPLIERNAME " +
                                       " , CONVERSIONUNIT " +
                                       " , CONVERSIONRATE " +
                                       " , PURCHASEPRICE " +
                                       " , PURCHASEUNIT " +
                                       " , REQUIREDQTY " +
                                       " , CREATEUSER " +
                                       " , CREATETIME " +
                                       " , SORTID ) " +
                                  " VALUES  " +
                                       " ( '{0}' " +
                                       " , '{1}' " +
                                       " , '{2}' " +
                                       " , '{3}' " +
                                       " , '{4}' " +
                                       " , {5} " +
                                       " , {6} " +
                                       " , '{7}' " +
                                       " , '{8}' " +
                                       " , '{9}' " +
                                       " , {10} " +
                                       " , {11} " +
                                       " , '{12}' " +
                                       " , {13} " +
                                       " , '{14}' " +
                                       " , '{15}' " +
                                       " , {16} ) ";

            string strItemCode = string.Empty;
            StringBuilder sbSql = new StringBuilder();
            List<POItem> lstTemp = new List<POItem>();
            List<MenuOrderLine> lstMenuOrderLine = new List<MenuOrderLine>();

            lstTemp = param.lstChangeSupplier;
            lstTemp = lstTemp.Where(r => "update".Equals(r.changeFlag)).ToList();

            // 拼接需要更新的itemCode条件字符串
            strItemCode = string.Join("','", lstTemp.Select(r => r.itemCode).Distinct().ToArray());
            strItemCode = "('" + strItemCode + "')";

            // 取得需要更新的MenuOrderLine基本数据
            lstMenuOrderLine = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), string.Format(strSqlSelect
                , param.costCenterCode, param.startDate, param.endDate, strItemCode));

            // 匹配新的供应商Code和名称
            lstMenuOrderLine = lstMenuOrderLine.Join(lstTemp,
                mol => new { CODE = mol.itemCode, SUPPLIERCODE = mol.supplierCode },
                tmp => new { CODE = tmp.itemCode, SUPPLIERCODE = tmp.supplierCode_bak },
                (mol, tmp) =>
                {
                    // 新的供应商代码
                    mol.supplierCode_new = tmp.supplierCode;
                    // 新的供应商名称
                    mol.supplierName_new = tmp.supplierName;
                    // 新的采购单价
                    mol.purchasePrice_new = tmp.purchasePrice;
                    // 新的采购单位
                    mol.purchaseUnit_new = tmp.purchaseUnit;
                    // 新的转换系数
                    mol.conversionRate_new = tmp.conversionRate;
                    // 新的转换单位
                    mol.conversionUnit_new = tmp.conversionUnit;

                    return mol;
                }).Where(r => !string.IsNullOrWhiteSpace(r.supplierCode_new)).ToList();

            // 当前时间
            string strDateTime = SalesOrder.Common.convertDateTime(DateTime.Now.ToString());

            // 生成Sql文
            foreach (MenuOrderLine mol in lstMenuOrderLine)
            {
                // Step1: 删除
                sbSql.Append(string.Format(strSqlDelete, param.userId, strDateTime, mol.id));
                // Step2: 新增
                sbSql.Append(string.Format(strSqlInsert, mol.headGuid, mol.lineGuid, mol.itemCode, mol.itemName, mol.itemDesc, mol.bomQty
                    , mol.actQty, mol.supplierCode_new, mol.supplierName_new, mol.conversionUnit_new, mol.conversionRate_new, mol.purchasePrice_new
                    , mol.purchaseUnit_new, mol.requiredQty, mol.createUser, strDateTime, mol.sortId));
            }
            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSql.ToString());

            return result;
        }
        #endregion
        public List<CCMast> ChangeRecipientsofWeeklyMenu(Dictionary<string, dynamic> param)
        {

            try
            {
                if (param["mark"] == "update")
                {
                    var s = param["oldVal"].ToString();
                    string sqladdLog = string.Format("insert into tblDataLog "
                        +"(TableName,FieldName,RecordId,OldVal,NewVal,CreateTime) values('{0}','{1}','{2}','{3}','{4}','{5}') ",
                        param["tableName"].ToString(), param["fieldName"].ToString(), param["recordId"].ToString(), 
                        param["oldVal"].ToString(), param["newVal"].ToString(),DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    sqladdLog += string.Format("update CCMast set recipentsOfMenu = '{0}' where costCenterCode='{1}'", param["newVal"].ToString(), param["costCenterCode"]);

                    int i = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sqladdLog);
                }

                string sql = string.Format("select id,isnull(recipentsOfMenu,'') recipientsofMenu from CCMast where CostCenterCode='{0}' ", param["costCenterCode"]);

                DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
                if (data == null || data.Rows.Count == 0) return null;

                return data.ToEntityList<CCMast>();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<WeeklyMenu> GetMenuOrder_SUZHYC(WeeklyMenu param)
        {         
            string strSite = string.Format(" SELECT top 1 GUID AS SITEGUID,CODE AS COSTCENTERCODE FROM TBLSITE(NOLOCK) where GUID='{0}'",
                param.siteGuid);
            WeeklyMenu cc = SqlServerHelper.GetEntity<WeeklyMenu>(SqlServerHelper.sfeed(), strSite);
            if (cc == null) return null;

            DateTime date = DateTime.Parse(param.startDate);
            List<WeeklyMenu> lstMenuOrderHead = GetMenuOrder_SUZHYC(cc.siteGuid,date);

            if (lstMenuOrderHead == null)
                return new List<WeeklyMenu>() ;

            var tmp = lstMenuOrderHead.GroupBy(q => new
            {
                mealCode = q.mealCode,
                windowType = q.windowType,
                windowType_en = q.windowType_en,
                sort = q.sort
            }).Select(q =>
            {
                WeeklyMenu d = new WeeklyMenu()
                {
                    siteGuid = cc.siteGuid,
                    mealCode = q.Key.mealCode,
                    windowType = q.Key.windowType,
                    windowType_en = q.Key.windowType_en,
                    sort = q.Key.sort,
                    startDate = date.ToString("yyyy-MM-dd")
                };

                for (int i = 0; i < 7; i++)
                {
                    var r = q.FirstOrDefault(p => DateTime.Parse(p.mealDate).Equals(date.AddDays(i)));
                    PropertyInfo property = typeof(WeeklyMenu).GetProperty("foodNames" + (i + 1));
                    property.SetValue(d, r == null ? "" : r.foodNames);

                    property = typeof(WeeklyMenu).GetProperty("foodNames_en" + (i + 1));
                    property.SetValue(d, r == null ? "" : r.foodNames_en);
                }
               
                return d;

            }).ToList();

            return  tmp;        
        }

        private List<WeeklyMenu> GetMenuOrder_SUZHYC(string siteGuid, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(siteGuid)) return null;

            string strSql = " SELECT M.ID " +
                                 " , M.SITEGUID " +
                                 " , CONVERT(VARCHAR(10),M.MEALDATE,23) MEALDATE " +
                                 " , M.MEALCODE" +
                                 " , M.WINDOWTYPE,M.WINDOWTYPE_EN " +
                                 " , ltrim(rtrim(M.FOODNAMES)) FOODNAMES " +
                                 " , ltrim(rtrim(M.FOODNAMES_EN)) FOODNAMES_EN "+
                                 " , M.SORT " +
                              " FROM SUZCATMenu (NOLOCK) AS M " +
                             " WHERE M.SITEGUID = '{0}' AND M.DELETEUSER IS NULL " +
                               " AND CONVERT(VARCHAR(10),M.MEALDATE,23) BETWEEN '{1}' AND '{2}' " +
                               "order by M.MEALCODE,M.Sort,M.MealDate";

            List<WeeklyMenu> lstMenuOrderHead = SqlServerHelper.GetEntityList<WeeklyMenu>(SqlServerHelper.salesorderConn(),
                string.Format(strSql, siteGuid, date.ToString("yyyy-MM-dd"), date.AddDays(6).ToString("yyyy-MM-dd")));

            if (lstMenuOrderHead == null || !lstMenuOrderHead.Any())
                return null;

            return lstMenuOrderHead;
        }

        /// <summary>
        /// 存数据SUZHYC
        /// </summary>
        /// <param name="lstMenuOrderHead"></param>
        /// <returns></returns>
        public int SaveMenuOrder_SUZHYC(WeeklyMenu param)
        {

            if (param == null || param.menuOrderHeadObj == null || !param.menuOrderHeadObj.Any())
                return 0;

            DateTime date = DateTime.Parse(param.startDate);
            string siteGuid = param.siteGuid.Trim();
            string user = param.createUser.Trim();
            DateTime now = DateTime.Now;
            
            //foodNames1, ... foodNames7
            var properties = typeof(WeeklyMenu).GetProperties().Where(q=>q.Name.ToLower().Contains("foodnames") 
                           && !q.Name.ToLower().Contains("foodnames_en")
                           && q.Name.ToLower() != "foodnames").OrderBy(q=>q.Name);
            
            

            int xsort = 0; //记录顺序
            string oldMealCode = "";
            var list = param.menuOrderHeadObj.SelectMany(q =>
               {
                   if (oldMealCode != q.mealCode) //不同餐次
                   {
                       oldMealCode = q.mealCode;
                       xsort = 10000;
                   }
                   xsort++;

                   string Meal = "";
                   //餐次传值
                   switch (q.mealCode.ToStringTrim())
                   {
                       case "41":
                           Meal = "早餐";
                           break;
                       case "42":
                           Meal = "午餐";
                           break;
                       case "43":
                           Meal = "晚餐";
                           break;
                       case "44":
                           Meal = "夜宵";
                           break;
                       case "45":
                           Meal = "深夜餐";
                           break;
                   }

                   int x = 0;
                    
                    return properties.Select(y => new {
                        mealCode = q.mealCode.ToStringTrim(),
                        meal = Meal,
                        windowType = q.windowType.ToStringTrim(),
                        windowType_en = q.windowType_en.ToStringTrim(),
                        sort = xsort.ToString(),
                        startDate = date.AddDays(x++).ToString("yyyy-MM-dd"),
                        foodNames = y.GetValue(q).ToStringTrim(), 
                        foodNames_en = q.GetProperty("foodNames_en"+y.ToStringTrim().Last()).ToStringTrim()
                    });

                }).Where(q=>!string.IsNullOrWhiteSpace(q.foodNames)).ToList();

            //数据库中的记录
            List<WeeklyMenu> savedList = GetMenuOrder_SUZHYC(siteGuid, date);
            StringBuilder sqls = new StringBuilder();

            #region 删除不存在的旧记录
            string sql = "id={0}";

            if (savedList != null && savedList.Any())
            {

                var tmp = savedList.Select(q => q.id);

                if (list != null && list.Any()) //与新记录不匹配的旧记录
                    tmp = savedList.GroupJoin(list,
                        a => new { mealDate = DateTime.Parse(a.mealDate), mealCode = a.mealCode, windowType = a.windowType, foodNames = a.foodNames, sort = a.sort,foodNames_en = a.foodNames_en, windowType_en = a.windowType_en },
                        b => new { mealDate = DateTime.Parse(b.startDate), mealCode = b.mealCode, windowType = b.windowType, foodNames = b.foodNames, sort = b.sort, foodNames_en = b.foodNames_en, windowType_en = b.windowType_en },
                        (a, b) => new { a.id, b }).Where(q => q.b == null || !q.b.Any()).Select(q => q.id);

                if (tmp.Any())
                {
                    sql = string.Join(" or ", tmp.Select(q => string.Format(sql, q)));

                    sqls.AppendFormat("update SUZCATMenu set deleteuser='{0}',deletetime='{1}' " +
                        "where deleteuser is null and mealdate > '{2}' and mealdate < '{3}' and ({4}); ",
                        user, now.ToString("yyyy-M-d H:m:s "), date.AddDays(-1).ToString("yyyy-MM-dd"),
                        date.AddDays(7).ToString("yyyy-MM-dd"), sql);
                }
            }
            #endregion

            #region 添加

            sql = "INSERT INTO SUZCATMENU (SITEGUID,MEALDATE,MEALTYPE,WINDOWTYPE,FOODNAMES,CREATETIME, " +
                   "CREATEUSER,SORT,MEALCODE,WINDOWTYPE_EN,FOODNAMES_EN) " +
                   "select top 1 '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}' " +
                   "where not exists(" +
                    "select top 1 1 from SUZCATMenu " +
                    "where deleteuser is null and siteguid = '{0}' and mealdate = '{1}' " +
                    "and mealcode = '{8}' and WindowType = '{3}' and FoodNames = '{4}' and sort='{7}' and windowType_en='{9}' and FoodNames_en='{10}')";

            var qry = list;
            if (savedList != null && savedList.Any())
                qry = list.GroupJoin(savedList,
                   a => new { mealDate = DateTime.Parse(a.startDate), mealCode = a.mealCode, windowType = a.windowType, foodNames = a.foodNames, sort = a.sort, foodNames_en = a.foodNames_en, windowType_en = a.windowType_en },
                   b => new { mealDate = DateTime.Parse(b.mealDate), mealCode = b.mealCode, windowType = b.windowType, foodNames = b.foodNames, sort = b.sort, foodNames_en = b.foodNames_en, windowType_en = b.windowType_en },
                   (a, b) => new { a, b }).Where(q => q.b == null || !q.b.Any()).Select(q=>q.a).ToList();

            if(qry != null && qry.Any())
                sqls.Append(string.Join("; ",qry.Select(q => string.Format(sql, siteGuid, q.startDate, q.meal,
                    q.windowType,q.foodNames, now.ToString("yyyy-M-d H:m:s "), user,q.sort, q.mealCode,q.windowType_en,q.foodNames_en))));
            #endregion

            int result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sqls.ToString());
            
            //int result = 0;

            return result;
        }

        /// <summary>
        /// 根据用户权限取得CostCenter
        /// </summary>
        /// <param name="action"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        public List<CCWhs> GetCCWhs_SUZHYC(AuthCompany param)
        {
            List<CCWhs> ccList = GetCCWhs(param);

            string strSite = " SELECT GUID AS SITEGUID,CODE AS COSTCENTERCODE FROM TBLSITE(NOLOCK)";
            List<CCWhs> cc = SqlServerHelper.GetEntityList<CCWhs>(SqlServerHelper.sfeed(), strSite);

            ccList.GroupJoin(cc,
                        mos => new { ccNote = mos.costCenterCode },
                        itemObj => new { ccNote = itemObj.costCenterCode },
                        (mos, itemObj) =>
                        {
                            mos.siteGuid = itemObj.FirstOrDefault().siteGuid;
                            return mos;
                        }
                      ).ToList();

            return ccList;

        }

    }
}