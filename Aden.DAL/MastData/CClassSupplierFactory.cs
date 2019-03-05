using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Util.Common;
using Aden.Util.Database;
using Aden.Model.MastData;
using Aden.Model.MenuData;
using Aden.Model.MenuOrder;
using Aden.Model.SOMastData;
using Aden.Model.Common;

using System.Data;

namespace Aden.DAL.MastData
{
    public class CClassSupplierFactory
    {
        #region 根据Company取得CostCenterCode集合
        // 根据Company取得CostCenterCode集合
        public CClassSupplierParam GetCClassSupplierData(string companyCode)
        {
            CClassSupplierParam result = new CClassSupplierParam();
            // 成本中心、各品类供应商对应关系
            List<CClassSupplier> lstCClassSupplier = new List<CClassSupplier>();
            // 临时表(用于类别行转列)
            List<CClassSupplier> lstTemp = new List<CClassSupplier>();
            // 品类主数据
            List<ItemClass> lstItemClass = new List<ItemClass>();
            // 当前时间
            string strDateTime = DateTime.Now.ToString("yyyy-MM-dd");

            // 取得CostCenter集合
            SingleField param = new SingleField();
            param.code = companyCode;
            param.flag = "1";
            List<SingleField> lstCostCenter = (new SalesOrder.CompanyFactory()).GetActCostCenter(param);

            // 从销售订单行项中取得CostCenter
            CClassSupplier ccs = new CClassSupplier();

            foreach (SingleField cc in lstCostCenter)
            {
                ccs = new CClassSupplier();
                ccs.costCenterCode = cc.code;

                lstCClassSupplier.Add(ccs);
            }

            // 当销售订单中存在记录时，追加一空行作为默认项
            if (lstCClassSupplier.Count > 0)
            {
                CClassSupplier emptyLine = new CClassSupplier();
                lstCClassSupplier.Add(emptyLine);
            }

            // Sql文(CClassSupplier)
            string strSqlCClassSupplier = " SELECT C.COSTCENTERCODE " +
                                               " , C.CLASSGUID " +
                                               " , C.SUPPLIERCODE " +
                                               " , CONVERT(VARCHAR(100), C.STARTDATE, 23) AS STARTDATE " +
                                               " , CONVERT(VARCHAR(100), C.ENDDATE, 23) AS ENDDATE " +
                                               " , C.DEFAULTSUPPLIER " +
                                            " FROM CCLASSSUPPLIER AS C " +
                                           " WHERE C.DELETEUSER IS NULL " +
                                             " AND C.DBNAME = '{0}' " +
                                             " AND ( C.ENDDATE >= '{1}'" +
                                                " OR C.ENDDATE IS NULL ) ";
            // Sql文(ItemClass主数据)
            string strSqlItemClass = " SELECT ID" +
                                          " , GUID " +
                                          " , CLASSNAME_ZH AS NAME " +
                                          " , SORT " +
                                       " FROM TBLITEMCLASS " +
                                      " WHERE STATUS = 'active' " +
                                        " AND TYPE = 'RMClass' " +
                                      " ORDER BY SORT ";

            // 取得CClassSupplier中的既有数据
            lstTemp = SqlServerHelper.GetEntityList<CClassSupplier>(SqlServerHelper.salesorderConn(), string.Format(strSqlCClassSupplier
                , companyCode, strDateTime));

            // 取得ItemClass主数据
            lstItemClass = SqlServerHelper.GetEntityList<ItemClass>(SqlServerHelper.salesorderConn(), strSqlItemClass);

            if (lstTemp != null && lstTemp.Count > 0)
            {
                // 第一顺CostCenter,第二顺类别，第三顺StartDate
                lstTemp = lstTemp.OrderBy(r => r.costCenterCode).OrderBy(r => r.classGuid).OrderBy(r => r.startDate).ToList();

                if (lstTemp != null && lstTemp.Count > 0)
                {
                    lstTemp = (from l in lstTemp
                               group l by l.costCenterCode into lq
                               let firstItem = lq.FirstOrDefault()
                               select new CClassSupplier
                               {
                                   // 成本中心
                                   costCenterCode = firstItem.costCenterCode,
                                   // mealTypeCode对应supplierCode行转列
                                   lstSupplierCode = ConvertToSupplierCodeList(lq.ToList(), lstItemClass)
                               } into lines
                               select lines).ToList();

                    lstCClassSupplier.AddRange(lstTemp);
                }
            }

            // 去重复（寄存数据为主）
            lstCClassSupplier = (from cc in lstCClassSupplier
                                 group cc by cc.costCenterCode into lq
                                 // 取第一条
                                 let firstItem = lq.FirstOrDefault()
                                 // 优先取供应商List不为空的
                                 let firstItemSupplierCode = lq.Where(r => r.lstSupplierCode != null && r.lstSupplierCode.Count > 0).FirstOrDefault()
                                 select new CClassSupplier
                                 {
                                     // 成本中心
                                     costCenterCode = firstItem.costCenterCode,
                                     // 供应商列表
                                     lstSupplierCode = firstItemSupplierCode != null ? firstItemSupplierCode.lstSupplierCode : new List<CClassSupplier>()
                                 } into lines
                                 select lines).ToList();

            // 取得供应商列表
            List<dynamic> lstSupplierTemp = new SupplierFactory().getSuppliers(companyCode);
            lstCClassSupplier = lstCClassSupplier.OrderBy(r => r.costCenterCode).ToList();
            // 画面Table内容
            // 成本中心主档
            Model.SOCommon.CompanyAddress addr = new Model.SOCommon.CompanyAddress();
            addr.ip = (new SalesOrder.CompanyFactory()).GetCompanyInfoByCode(param.code).ip;
            addr.erpCode = param.code;
            List<CostCenter> lstCCMastData = (new SalesOrder.MastDataFactory()).GetCostCenter(addr);

            lstCClassSupplier.GroupJoin(lstCCMastData,
                cc => new { code = cc.costCenterCode },
                ma => new { code = ma.costCenterCode },
                (cc, ma) =>
                {
                    var first = ma.FirstOrDefault();
                    bool check = first != null;
                    cc.costCenterName = check ? first.costCenterName_ZH : string.Empty;

                    return cc;
                }).ToList();

            result.lstCClassSupplier = lstCClassSupplier;
            // 供应商下拉框选项
            result.lstSupplierOption = lstSupplierTemp;
            // 类别主数据
            result.lstItemClass = lstItemClass;

            return result;
        }

        public List<CClassSupplier> ConvertToSupplierCodeList(List<CClassSupplier> lq, List<ItemClass> lstItemClass)
        {
            List<CClassSupplier> lstSupplierCode = new List<CClassSupplier>();

            var joinResult = from cc in lq
                             join it in lstItemClass
                             on cc.classGuid equals it.guid
                             select new CClassSupplier
                             {
                                 // itemClassGuid
                                 classGuid = it.guid,
                                 // 供应商Code
                                 supplierCode = cc.supplierCode,
                                 // 供应商Code备份
                                 supplierCode_bak = cc.supplierCode,
                                 // 开始日期
                                 startDate = cc.startDate,
                                 // 开始日期
                                 startDate_bak = cc.startDate,
                                 // 结束日期备份
                                 endDate = cc.endDate,
                                 // 结束日期备份
                                 endDate_bak = cc.endDate,
                                 // 默认
                                 defaultSupplier = cc.defaultSupplier,
                                 // 默认备份
                                 defaultSupplier_bak = cc.defaultSupplier
                             };

            lstSupplierCode = joinResult.ToList();

            // 第一顺ItemClassGuid，第二顺StartDate
            lstSupplierCode = lstSupplierCode.OrderBy(r => r.classGuid).OrderBy(r => r.startDate).ToList();

            #region 废弃
            //lstSupplierCode = (from l in lstSupplierCode
            //                   group l by l.classGuid into lg
            //                   let firstItem = lg.FirstOrDefault()
            //                   let SecondItem = lg.ToList().Count > 1 ? lg.ToList()[1] : null
            //                   select new CClassSupplier
            //                   {
            //                       // itemClassGuid
            //                       classGuid = firstItem.classGuid,
            //                       // 供应商Code(当前)
            //                       supplierCode_Cur = firstItem.supplierCode,
            //                       // 开始日期(当前)
            //                       startDate_Cur = firstItem.startDate,
            //                       // 结束日期(当前)
            //                       endDate_Cur = firstItem.endDate,
            //                       // 供应商Code(计划)
            //                       supplierCode_Fut = SecondItem != null ? SecondItem.supplierCode : null,
            //                       // 开始日期(计划)
            //                       startDate_Fut = SecondItem != null ? SecondItem.startDate : null,
            //                       // 结束日期(计划)
            //                       endDate_Fut = SecondItem != null ? SecondItem.endDate : null,
            //                   } into lines
            //                   select lines).ToList();
            #endregion

            return lstSupplierCode;
        }
        #endregion

        #region 保存CClassSupplierFactory
        /// <summary>
        /// 保存成本中心对应供应商主数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int SaveCClassSupplier(CClassSupplierParam param)
        {
            // 最终Sql文
            StringBuilder sbSqlFinal = new StringBuilder();
            // 当前时间
            string strDateTime = SalesOrder.Common.convertDateTime(DateTime.Now.ToString());

            int result = 0;

            #region Sql文定义
            string strSqlUpdate = " UPDATE CCLASSSUPPLIER " +
                                     " SET DELETEUSER = '{0}' " +
                                       " , DELETETIME = '{1}' " +
                                   " WHERE COSTCENTERCODE = '{2}' " +
                                     " AND DBNAME = '{3}' " +
                                     " AND CLASSGUID = '{4}' ";

            string strSqlUpdate2 = " UPDATE CCLASSSUPPLIER " +
                                      " SET DELETEUSER = '{0}' " +
                                        " , DELETETIME = '{1}' " +
                                    " WHERE COSTCENTERCODE IS NULL " +
                                      " AND DBNAME = '{2}' " +
                                      " AND CLASSGUID = '{3}' ";

            string strSqlInsert = " INSERT INTO CCLASSSUPPLIER " +
                                            " ( COSTCENTERCODE " +
                                            " , DBNAME " +
                                            " , CLASSGUID " +
                                            " , SUPPLIERCODE " +
                                            " , DEFAULTSUPPLIER " +
                                            " , STARTDATE " +
                                            " , ENDDATE " +
                                            " , CREATETIME " +
                                            " , CREATEUSER ) " +
                                       " VALUES " +
                                            " ( '{0}' " +
                                            " , '{1}' " +
                                            " , '{2}' " +
                                            " , '{3}' " +
                                            " , {4} " +
                                            " , '{5}' " +
                                            " , '{6}' " +
                                            " , '{7}' " +
                                            " , '{8}' ) ";

            string strSqlInsert2 = " INSERT INTO CCLASSSUPPLIER " +
                                             " ( COSTCENTERCODE " +
                                             " , DBNAME " +
                                             " , CLASSGUID " +
                                             " , SUPPLIERCODE " +
                                             " , DEFAULTSUPPLIER " +
                                             " , STARTDATE " +
                                             " , CREATETIME " +
                                             " , CREATEUSER ) " +
                                        " VALUES " +
                                             " ( '{0}' " +
                                             " , '{1}' " +
                                             " , '{2}' " +
                                             " , '{3}' " +
                                             " , {4} " +
                                             " , '{5}' " +
                                             " , '{6}' " +
                                             " , '{7}' ) ";

            string strSqlDefaultInsert = " INSERT INTO CCLASSSUPPLIER " +
                                                   " ( DBNAME " +
                                                   " , CLASSGUID " +
                                                   " , SUPPLIERCODE " +
                                                   " , DEFAULTSUPPLIER " +
                                                   " , STARTDATE " +
                                                   " , ENDDATE " +
                                                   " , CREATETIME " +
                                                   " , CREATEUSER ) " +
                                              " VALUES " +
                                                   " ( '{0}' " +
                                                   " , '{1}' " +
                                                   " , '{2}' " +
                                                   " , {3} " +
                                                   " , '{4}' " +
                                                   " , '{5}' " +
                                                   " , '{6}' " +
                                                   " , '{7}' ) ";

            string strSqlDefaultInsert2 = " INSERT INTO CCLASSSUPPLIER " +
                                                    " ( DBNAME " +
                                                    " , CLASSGUID " +
                                                    " , SUPPLIERCODE " +
                                                    " , DEFAULTSUPPLIER " +
                                                    " , STARTDATE " +
                                                    " , CREATETIME " +
                                                    " , CREATEUSER ) " +
                                               " VALUES " +
                                                    " ( '{0}' " +
                                                    " , '{1}' " +
                                                    " , '{2}' " +
                                                    " , {3} " +
                                                    " , '{4}' " +
                                                    " , '{5}' " +
                                                    " , '{6}' ) ";

            #endregion

            // 根据成本中心 + ItemClassGuid进行分组
            var lqCClassSupplier = (from cClassSupplier in param.lstCClassSupplier
                                    group cClassSupplier by new
                                    {
                                        // 成本中心
                                        costCenterCode = cClassSupplier.costCenterCode,
                                        // ItemClassGuid
                                        classGuid = cClassSupplier.classGuid,
                                        // 修改标志(一组中的changeFlag应该均相同)
                                        changeFlag = cClassSupplier.changeFlag
                                    }).ToList();

            List<CClassSupplier> lstCCSTemp = new List<CClassSupplier>();
            string strCostCenterCode = string.Empty;
            string strClassGuid = string.Empty;
            string strSql1 = string.Empty;
            string strSql2 = string.Empty;

            // By成本中心 + ItemClass进行保存
            foreach (var gCClassSupplier in lqCClassSupplier)
            {
                if (string.IsNullOrWhiteSpace(gCClassSupplier.Key.changeFlag))
                {
                    continue;
                }
                // By成本中心 + ItemClass删除之前的记录(1~2条)
                if(!string.IsNullOrEmpty(gCClassSupplier.Key.costCenterCode))
                    sbSqlFinal.Append(string.Format(strSqlUpdate, param.userId, strDateTime, gCClassSupplier.Key.costCenterCode, param.dbName,
                        gCClassSupplier.Key.classGuid));
                else
                    sbSqlFinal.Append(string.Format(strSqlUpdate2, param.userId, strDateTime, param.dbName, gCClassSupplier.Key.classGuid));

                lstCCSTemp = gCClassSupplier.ToList();

                // 插入新记录
                foreach (CClassSupplier ccs in lstCCSTemp)
                {
                    if (!string.IsNullOrWhiteSpace(ccs.costCenterCode))
                    {
                        if (string.IsNullOrEmpty(ccs.endDate))
                            sbSqlFinal.Append(string.Format(strSqlInsert2, gCClassSupplier.Key.costCenterCode, param.dbName
                                , gCClassSupplier.Key.classGuid, ccs.supplierCode, ccs.defaultSupplier? 1: 0, ccs.startDate, strDateTime, param.userId));
                        else
                            sbSqlFinal.Append(string.Format(strSqlInsert, gCClassSupplier.Key.costCenterCode, param.dbName
                                , gCClassSupplier.Key.classGuid, ccs.supplierCode, ccs.defaultSupplier? 1: 0, ccs.startDate, ccs.endDate, strDateTime
                                , param.userId));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ccs.endDate))
                            sbSqlFinal.Append(string.Format(strSqlDefaultInsert2, param.dbName, gCClassSupplier.Key.classGuid
                                , ccs.supplierCode, ccs.defaultSupplier? 1: 0, ccs.startDate, strDateTime, param.userId));
                        else
                            sbSqlFinal.Append(string.Format(strSqlDefaultInsert, param.dbName, gCClassSupplier.Key.classGuid
                                , ccs.supplierCode, ccs.defaultSupplier? 1: 0, ccs.startDate, ccs.endDate, strDateTime, param.userId));
                    }
                }
            }
            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());

            return result;
        }
        #endregion

        #region 取得ItemClass维护界面的集合
        /// <summary>
        /// 取得ItemClass维护界面的集合
        /// </summary>
        /// <param name="param">公司代码</param>
        /// <returns></returns>
        public ItemClassMaintain GetItemClassMaintainData(SingleField param)
        {
            ItemClassMaintain result = new ItemClassMaintain();

            string strSql = " SELECT M.ID " +
                                 " , M.TYPE " +
                                 " , M.COSTCENTERCODE " +
                                 " , M.CODE1 " +
                                 " , M.NAME1 " +
                                 " , M.CODE2 " +
                                 " , M.NAME2 " +
                                 " , I.SORT " +
                              " FROM TBLDATAMAPPING AS M " +
                             " INNER JOIN TBLITEMCLASS AS I " +
                                " ON M.CODE1 = I.GUID " +
                             " WHERE M.DBCODE = '{0}' " +
                               " AND M.TYPE = 'RMClassMapping' " +
                               " AND M.COSTCENTERCODE IS NULL " +
                               " AND M.DELETEUSER IS NULL ";

            // Sql文(ItemClass主数据)
            string strSqlItemClass = " SELECT ID" +
                                          " , GUID " +
                                          " , CLASSNAME_ZH AS NAME " +
                                          " , SORT " +
                                       " FROM TBLITEMCLASS " +
                                      " WHERE STATUS = 'active' " +
                                        " AND TYPE = 'RMClass' " +
                                      " ORDER BY SORT ";

            // 已有的ItemClass Mapping数据
            List<tblDataMapping> lstTblDataMapping = SqlServerHelper.GetEntityList<tblDataMapping>(SqlServerHelper.salesorderConn(), string.Format(strSql
                , param.code));
            // X轴数据
            List<ItemClass> lstItemClassData = (new ItemFactory()).getItemClass(param.code);
            // y轴数据
            List<ItemClass> lstItemClass_y = SqlServerHelper.GetEntityList<ItemClass>(SqlServerHelper.salesorderConn(), strSqlItemClass);

            List<ItemClassHierarchy> lstItemClass_x = new List<ItemClassHierarchy>();
            ItemClassHierarchy ich = new ItemClassHierarchy();

            foreach (ItemClass parent in lstItemClassData)
            {
                if(parent.children.Count  > 1)
                {
                    ich = new ItemClassHierarchy();

                    ich.p_code = parent.value;
                    ich.p_name = parent.label;
                    ich.c_code = "all";
                    ich.c_name = "全选";
                    ich.rowSpan = parent.children.Count + 1;

                    lstItemClass_x.Add(ich);
                }

                foreach(ItemClass child in parent.children)
                {
                    ich = new ItemClassHierarchy();

                    ich.p_code = parent.value;
                    ich.p_name = parent.label;
                    ich.c_code = child.value;
                    ich.c_name = child.label;
                    ich.rowSpan = parent.children.Count == 1 ? 1 : 0;

                    lstItemClass_x.Add(ich);
                }
            }

            result.lstTblDataMapping = lstTblDataMapping;

            result.lstItemClass_x = lstItemClass_x;

            result.lstItemClass_y = lstItemClass_y;

            return result;
        }
        #endregion

        #region 保存ItemClassMapping数据
        /// <summary>
        /// 保存修改的ItemClassMapping数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int SaveItemClassMaintainData(SaveItemClassMaintainDataParam param)
        {
            int result = 0;
            string strType = "RMClassMapping";
            StringBuilder sbSqlFinal = new StringBuilder();

            string strSqlUpdate = " UPDATE TBLDATAMAPPING " +
                                     " SET DELETETIME = '{0}' " +
                                       " , DELETEUSER = '{1}' " +
                                   " WHERE DBCODE = '{2}' " +
                                     " AND CODE2 IN {3} " +
                                     " AND DELETEUSER IS NULL "; 

            string strSqlInsert = " INSERT INTO TBLDATAMAPPING " +
                                            " ( TYPE " +
                                            " , DBCODE " +
                                            " , CODE1 " +
                                            " , NAME1 " +
                                            " , CODE2 " +
                                            " , NAME2 " +
                                            " , CREATETIME " +
                                            " , CREATEUSER ) " +
                                       " VALUES " +
                                            " ( '{0}' " +
                                            " , '{1}' " +
                                            " , '{2}' " +
                                            " , '{3}' " +
                                            " , '{4}' " +
                                            " , '{5}' " +
                                            " , '{6}' " +
                                            " , '{7}' ) ";

            // Item数据
            List<ItemClass> lstItemClassData = (new ItemFactory()).getItemClass(param.dbCode);
            // 当前时间
            string strDateTime = SalesOrder.Common.convertDateTime(DateTime.Now.ToString());
            // ItemCode Sql条件
            string strItemCodesString = string.Empty;

            var linq = param.lstTalDataMapping.GroupBy(q => new { q.p_code }).ToList();

            foreach(var lq in linq)
            {
                // 取得ItemCode Sql条件
                strItemCodesString = GetItemCodesString(lq.Key.p_code, lstItemClassData);
                // 父级Item为组进行删除
                sbSqlFinal.Append(string.Format(strSqlUpdate, strDateTime, param.userId, param.dbCode, strItemCodesString));
 
                // 父级为组进行新增
                foreach(tblDataMapping map in lq.ToList())
                {
                    if (!string.IsNullOrWhiteSpace(map.delFlag))
                        continue;
                    sbSqlFinal.Append(string.Format(strSqlInsert, strType, param.dbCode, map.code1, map.name1, map.code2, map.name2, strDateTime
                        , param.userId));
                }
            }

            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());

            return result;
        }

        // 取得ItemCode Sql条件
        private string GetItemCodesString(string p_code, List<ItemClass> lstItemClassData)
        {
            ItemClass itemObj = lstItemClassData.FirstOrDefault(r => p_code.Equals(r.value));
            // 父阶层Code
            string strItemCodes = p_code;

            // 自阶层Code
            if (itemObj.children.Count > 0)
                strItemCodes = strItemCodes + "','" + string.Join("','", itemObj.children.Select(r => r.value).Distinct().ToArray());
            strItemCodes = "('" + strItemCodes + "')";

            return strItemCodes;
        }
        #endregion
    }
}
