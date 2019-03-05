using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Util.Common;
using Aden.Util.Database;
using Aden.Model.Purchase;
using Aden.Model.MenuOrder;
using Aden.DAL.MenuOrder;
using System.Data;

namespace Aden.DAL.Purchase
{
    public class PurchaseOrderFactory
    {
        #region 转换成需要保存的采购单记录
        /// <summary>
        /// 根据成本中心、日期导入采购单临时表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int ImportToPurchaseOrder(MenuOrderHead param)
        {
            PurchaseOrderHead po = new PurchaseOrderHead();

            // 根据日期取得所有的采购单行项
            List<PurchaseOrderLine> lstAllLines = GetAllPurchaseOrderLine(param);
            PurchaseOrderHead poParam = new PurchaseOrderHead();

            // 取得DBCODE
            string strSqlDBCode = " SELECT TOP 1 " +
                                         " DBNAME " +
                                       " , WAREHOUSECODE " +
                                    " FROM CCMAST (NOLOCK)" +
                                   " WHERE COSTCENTERCODE = '{0}' ";
            CCWhs ccwhs = SqlServerHelper.GetEntity<CCWhs>(SqlServerHelper.salesorderConn(), string.Format(strSqlDBCode, param.costCenterCode));

            // 根据条件取得当天已存在的采购单行项（待比对）
            poParam.orderDate = param.requiredDate;
            poParam.costCenterCode = param.costCenterCode;
            poParam.warehouseCode = ccwhs.warehouseCode;
            poParam.dbCode = ccwhs.dbName;

            List<PurchaseOrderLine> lstCurLines = new List<PurchaseOrderLine>();

            // 取得既有的采购订单数据
            lstCurLines = GetPurchaseOrderLine(poParam);

            // 取得所有修改和新增的记录
            if (lstCurLines != null && lstCurLines.Count > 0)
            {
                // Menu连接PO取得需要新增和修改的部分
                po.lstPurchaseOrderLine = lstAllLines.GroupJoin(lstCurLines,
                    all => new { all.supplierCode, all.orderDate }, //供应商和订单日期匹配
                    cur => new { cur.supplierCode, cur.orderDate },
                    (all, cur) => new { all, cur })
                    .Where(q => q.cur == null || !q.cur.Any() ||
                        !q.cur.Where(p => p.itemCode == q.all.itemCode &&
                        (p.remark == null ? string.Empty : p.remark) == (q.all.remark == null ? string.Empty : q.all.remark) &&
                        p.qty == q.all.qty).Any())
                    .Select(q =>
                    {
                        if (q.cur == null || !q.cur.Any()) return q.all;
                        // headGuid
                        q.all.headGuid = q.cur.FirstOrDefault().headGuid;

                        var tmp = q.cur.Where(p => p.itemCode == q.all.itemCode);
                        if (tmp.Any()) //供应商，订单日，item 匹配
                            // lineGuid
                            q.all.lineGuid = tmp.FirstOrDefault().lineGuid;

                        return q.all;
                    }).ToList();

                // PO连接Menu取得需要删除的部分
                po.lstPurchaseOrderLine = po.lstPurchaseOrderLine.Concat(lstCurLines.GroupJoin(lstAllLines,
                    cur => new { cur.supplierCode, cur.orderDate },
                    all => new { all.supplierCode, all.orderDate },
                    (cur, all) => new { cur, all })
                .Where(q => q.all == null || !q.all.Any() ||
                    !q.all.Where(p => p.itemCode == q.cur.itemCode).Any())
                    .Select(q =>
                    {
                        // Head DelFlag
                        if (q.all == null || !q.all.Any())
                        {
                            q.cur.delFlag = true;
                            return q.cur;
                        }
                        // Line DelFlag
                        var tmp = q.all.Where(r => r.itemCode == q.cur.itemCode);
                        if (!tmp.Any())
                            q.cur.delFlag = true;
                        return q.cur;
                    }).Where(r => r.delFlag)).ToList();
            }
            else
            {
                po.lstPurchaseOrderLine = lstAllLines;
            }
            po.dbCode = poParam.dbCode;
            po.costCenterCode = poParam.costCenterCode;
            po.warehouseCode = poParam.warehouseCode;
            po.userId = param.createUser;
            po.employee = "1";// (new SOAccount.AccountFactory()).getEmployeeId(param.createUser);

            return SavePurchaseOrder(po);
        }
        #endregion

        #region 保存采购订单数据
        /// <summary>
        /// 保存采购订单
        /// </summary>
        /// <param name="param">Purchase Order</param>
        /// <returns></returns>
        public int SavePurchaseOrder(PurchaseOrderHead po)
        {
            int result = 0;

            StringBuilder sbSqlFinal = new StringBuilder();

            string strDateTimeNow = DateTime.Now.ToString();

            string strSqlUptRemark = " UPDATE PURCHASEORDERLINE " +
                                         " SET REMARK = '{0}' " +
                                       " WHERE LINEGUID = '{1}' " +
                                         " AND ISNULL(REMARK, '') <> '{0}' ";

            string strSqlDelDetail = " UPDATE PURCHASEORDERLINEDETAIL " +
                                           " SET DELETEUSER = '{0}' " +
                                             " , DELETEDATE = '{1}' " +
                                         " WHERE LINEGUID = '{2}' " +
                                           " AND DELETEUSER IS NULL ";

            string strSqlIstDetail = " INSERT INTO PURCHASEORDERLINEDETAIL " +
                                                  " ( LINEGUID " +
                                                  " , COSTUNIT " +
                                                  " , QTY " +
                                                  " , PRICE " +
                                                  " , CREATEUSER " +
                                                  " , CREATEDATE " +
                                                  " , TOTALPRICE ) " +
                                             " VALUES " +
                                                  " ( '{0}' " +
                                                  " , '{1}' " +
                                                  " , {2} " +
                                                  " , {3} " +
                                                  " , '{4}' " +
                                                  " , '{5}' " +
                                                  " , '{6}' ) ";

            if (po.lstPurchaseOrderLine == null || po.lstPurchaseOrderLine.Count == 0)
                return 0;

            // 按日期、公司、成本中心、供应商分组
            var lqPO = (from p in po.lstPurchaseOrderLine
                        group p by new
                        {
                            // 日期
                            orderDate = p.orderDate,
                            // 成本中心
                            costCenter = p.costCenter,
                            // 供应商
                            supplierCode = p.supplierCode,
                        });
            // PO表头Guid
            string headGuid = string.Empty;

            foreach (var gPO in lqPO)
            {
                // 检查Head是否存在
                headGuid = gPO.OrderByDescending(r => r.headGuid).FirstOrDefault().headGuid;
                // 检查HeadGuid是否已经存在，没有则新建Head
                if (string.IsNullOrWhiteSpace(headGuid))
                    sbSqlFinal.Append(CreatePurchaseOrderHead(po, strDateTimeNow, gPO.Key.orderDate, ref headGuid));

                foreach (PurchaseOrderLine pol in gPO.ToList())
                {
                    // 1.LineGuid不为空或删除标志为true时删除LineDetail
                    if (!string.IsNullOrWhiteSpace(pol.lineGuid) || pol.delFlag)
                    {
                        sbSqlFinal.Append(string.Format(strSqlDelDetail, po.userId, strDateTimeNow, pol.lineGuid));
                        // 删除标志为true时进入下一次循环
                        if (pol.delFlag) continue;
                    }
                    // LineGuid不为空
                    if (!string.IsNullOrWhiteSpace(pol.lineGuid))
                        // 更新Remark(Sql中判断是否更新)
                        sbSqlFinal.Append(string.Format(strSqlUptRemark, pol.remark, pol.lineGuid));
                    else
                        // 创建Line记录
                        sbSqlFinal.Append(CreatePurchaseOrderLine(po.costCenterCode, po.warehouseCode, pol, strDateTimeNow, headGuid, 0));

                    // 新增LineDetail记录
                    sbSqlFinal.Append(string.Format(strSqlIstDetail, pol.lineGuid, pol.costUnit,
                        pol.qty, pol.price, po.userId, strDateTimeNow,
                        pol.totalPrice));
                }
            }
            result = SqlServerHelper.Execute(SqlServerHelper.purchaseorderconn(), sbSqlFinal.ToString());

            return result;
        }

        /// <summary>
        /// 新建采购单
        /// </summary>
        /// <param name="po"></param>
        /// <returns></returns>
        private string CreatePurchaseOrderHead(PurchaseOrderHead po, string strDateTime, string strDateFormat, ref string headGuid)
        {
            headGuid = Guid.NewGuid().ToString();

            string orderDescription = po.costCenterCode.Substring(4, 3) + "-PR" + "(" + strDateFormat + ")";

            string strSql = " INSERT INTO PURCHASEORDERHEAD " +
                                      " ( HEADGUID " +
                                      " , ORDERDATE " +
                                      " , ORDERDESCRIPTION " +
                                      " , ORDERMETHOD " +
                                      " , DBCODE " +
                                      " , CREATEUSER " +
                                      " , CREATEDATE) " +
                                 " VALUES " +
                                      " ( '{0}' " +
                                      " , '{1}' " +
                                      " , '{2}' " +
                                      " , '{3}' " +
                                      " , '{4}' " +
                                      " , '{5}' " +
                                      " , '{6}' ) ";

            return string.Format(strSql, headGuid, strDateFormat, orderDescription, "001"
                , po.dbCode, po.employee, strDateTime);
        }

        /// <summary>
        /// 新建采购单行项
        /// </summary>
        /// <param name="po"></param>
        /// <returns></returns>
        private string CreatePurchaseOrderLine(string costCenter, string warehouseCode, PurchaseOrderLine pol, string strDate, string headGuid, int maxLineNumber)
        {
            string strSql = " INSERT INTO PURCHASEORDERLINE " +
                                      " ( LINEGUID " +
                                      " , HEADGUID " +
                                      " , LINENUMBER " +
                                      " , SUPPLIERCODE " +
                                      " , WAREHOUSECODE " +
                                      " , ITEMDESCRIPTION " +
                                      " , ITEMCODE " +
                                      " , UNIT " +
                                      " , REMARK " +
                                      " , COSTCENTER ) " +
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
                                      " , '{9}' ) ";

            pol.lineGuid = Guid.NewGuid().ToString();

            return string.Format(strSql, pol.lineGuid, headGuid, maxLineNumber, pol.supplierCode, warehouseCode
                , pol.itemDescription, pol.itemCode, pol.unit, pol.remark, costCenter);
        }

        /// <summary>
        /// 取得最大LineNumber合集
        /// </summary>
        /// <param name="lstPurchaseOrderLine"></param>
        /// <returns></returns>
        private List<PurchaseOrderLine> GetMaxLineNumberList(List<PurchaseOrderLine> lstPurchaseOrderLine)
        {
            List<PurchaseOrderLine> lstMaxLineNumber = new List<PurchaseOrderLine>();

            string strSql = " SELECT HEADGUID " +
                                 " , MAX(LINENUMBER) AS LINENUMBER" +
                              " FROM PURCHASEORDERLINE (NOLOCK) " +
                             " WHERE HEADGUID IN {0} " +
                             " GROUP BY HEADGUID ";

            // 取得HeadGuid集合字符串
            string strHeadGuid = string.Join("','", lstPurchaseOrderLine.Where(r => !string.IsNullOrWhiteSpace(r.headGuid))
                .Select(r => r.headGuid).Distinct().ToArray());

            // 无即有记录时返回
            if (string.IsNullOrWhiteSpace(strHeadGuid))
                return lstMaxLineNumber;

            strHeadGuid = "('" + strHeadGuid + "')";
            lstMaxLineNumber = SqlServerHelper.GetEntityList<PurchaseOrderLine>(SqlServerHelper.purchaseorderconn(),
                string.Format(strSql, strHeadGuid));

            return lstMaxLineNumber;
        }
        #endregion

        #region 取得即有的采购订单数据
        /// <summary>
        /// 根据HeadGuid取得PurchaseOrderLine集合
        /// </summary>
        /// <param name="headGuid"></param>
        /// <returns></returns>
        public List<PurchaseOrderLine> GetPurchaseOrderLine(PurchaseOrderHead param)
        {
            /*** 取得PurchaseOrderLine部分 ***/
            string strSqlLine = " SELECT H.HEADGUID " +
                                     " , L.LINEGUID " +
                                     " , CONVERT(VARCHAR(10), H.ORDERDATE, 23) AS ORDERDATE " +
                                     " , H.ORDERDESCRIPTION " +
                                     " , H.ORDERMETHOD " +
                                     " , H.DBCODE " +
                                     " , L.LINENUMBER " +
                                     " , L.SUPPLIERCODE " +
                                     " , L.WAREHOUSECODE " +
                                     " , L.ITEMDESCRIPTION " +
                                     " , L.ITEMCODE " +
                                     " , L.UNIT " +
                                     " , L.COSTCENTER " +
                                     " , L.REMARK " +
                                     " , L.ERPNUMBER " +
                                     " , D.COSTUNIT " +
                                     " , D.QTY " +
                                     " , D.PRICE " +
                                     " , D.TOTALPRICE " +
                                  " FROM PURCHASEORDERLINE AS L (NOLOCK) " +
                                 " INNER JOIN PURCHASEORDERHEAD AS H (NOLOCK) " +
                                    " ON L.HEADGUID = H.HEADGUID " +
                                 " INNER JOIN PURCHASEORDERLINEDETAIL AS D (NOLOCK) " +
                                    " ON L.LINEGUID = D.LINEGUID " +
                                   " AND D.DELETEUSER IS NULL " +
                                 " WHERE H.ORDERDATE >= '{0}' " +
                                   " AND H.DBCODE = '{1}' " +
                                   " AND L.COSTCENTER = '{2}' ";

            List<PurchaseOrderLine> lstPOL = SqlServerHelper.GetEntityList<PurchaseOrderLine>(SqlServerHelper.purchaseorderconn(),
                string.Format(strSqlLine, param.orderDate, param.dbCode, param.costCenterCode));

            return lstPOL;
        }
        #endregion

        #region 取得待分析的采购订单数据
        private List<PurchaseOrderLine> GetAllPurchaseOrderLine(MenuOrderHead param)
        {
            List<PurchaseOrderLine> lstPol = new List<PurchaseOrderLine>();
            string strSql = string.Empty;
            string strSqlLine = " SELECT L.ITEMCODE " +
                                     " , L.ITEMNAME " +
                                     " , L.ITEMDESC " +
                                     " , L.ITEMTYPE " +
                                     " , L.BOMQTY " +
                                     " , L.ACTQTY " +
                                     " , L.REMARK " +
                                     " , L.REQUIREDQTY " +
                                     " , L.SUPPLIERCODE " +
                                     " , L.SUPPLIERNAME " +
                                     " , L.CONVERSIONUNIT " +
                                     " , L.CONVERSIONRATE " +
                                     " , L.PURCHASEPRICE " +
                                     " , L.PURCHASEUNIT " +
                                     " , L.PURCHASETAX " +
                                     " , H.SOITEMGUID " +
                                     " , CONVERT(VARCHAR(100), H.REQUIREDDATE, 23) AS HEADREQUIREDDATE " +
                                     " , CONVERT(VARCHAR(100), ISNULL(L.REQUIREDDATE, H.REQUIREDDATE), 23) AS REQUIREDDATE " +
                                     " , CONVERT(VARCHAR(100), L.REQUIREDDATE, 23) AS WORKREQUIREDDATE " +
                                     " , L.ADJFLAG " +
                                  " FROM MENUORDERLINE AS L (NOLOCK) " +
                                 " INNER JOIN MENUORDERHEAD AS H (NOLOCK) " +
                                    " ON L.HEADGUID = H.HEADGUID " +
                                   " AND H.DELETEUSER IS NULL " +
                                   " AND L.DELETEUSER IS NULL  " +
                                 " WHERE H.COSTCENTERCODE = '{0}' " +
                                   " AND L.REQUIREDQTY <> 0 ";

            string strCond1 = "      AND H.REQUIREDDATE >= '{1}' ";

            string strCond2 = "      AND ( H.REQUIREDDATE >= '{1}' " +
                                      " OR H.REQUIREDDATE IN {2} ) ";

            if (param.lstRequiredDate != null && param.lstRequiredDate.Any())
            {
                // 取得HeadGuid集合字符串
                string strRequiredDate = "('" + string.Join("','", param.lstRequiredDate.ToArray()) + "')";
                strSql = string.Format((strSqlLine + strCond2), param.costCenterCode, param.requiredDate, strRequiredDate);
            }
            else
            {
                strSql = string.Format((strSqlLine + strCond1), param.costCenterCode, param.requiredDate);
            }

            List<MenuOrderLine> lstMenuOrderLine = SqlServerHelper.GetEntityList<MenuOrderLine>(SqlServerHelper.salesorderConn(), strSql);

            if (lstMenuOrderLine == null || !lstMenuOrderLine.Any())
                return lstPol;

            //lstMenuOrderLine.RemoveAll(r => "adj".Equals(r.adjFlag) && r.requiredQty == 0);

            #region RequiredDate、供应商相关信息调整
            MenuOrderLine mol = new MenuOrderLine();

            lstMenuOrderLine = lstMenuOrderLine.GroupBy(q => new { q.itemCode, q.requiredDate })
                .SelectMany(q =>
                {
                    MenuOrderLine firstLine = q.OrderBy(p => p.headRequiredDate).FirstOrDefault();
                    return q.Select(p =>
                    {
                        p.supplierCode = firstLine.supplierCode;
                        p.supplierName = firstLine.supplierName;
                        p.purchasePrice = firstLine.purchasePrice;
                        p.purchaseTax = firstLine.purchaseTax;
                        p.remark = q.OrderByDescending(r => r.remark).FirstOrDefault().remark;
                        return p;
                    });
                }).ToList();

            #endregion

            // 将remark有值的往前放
            //lstMenuOrderLine = lstMenuOrderLine.OrderBy(r => r.requiredDate).OrderBy(r => r.itemCode).OrderByDescending(r => r.remark).ToList();

            // 按itemCodew
            lstPol = (from line in lstMenuOrderLine
                      group line by new { line.requiredDate, line.itemCode } into lqLine
                      let firstLine = lqLine.FirstOrDefault()
                      let requiredQty = (decimal)lqLine.Sum(r => r.requiredQty)
                      select new PurchaseOrderLine
                      {
                          orderDate = firstLine.requiredDate,
                          itemCode = firstLine.itemCode,
                          itemDescription = firstLine.itemName,
                          costCenter = param.costCenterCode,
                          supplierCode = firstLine.supplierCode,
                          unit = firstLine.purchaseUnit,
                          remark = firstLine.remark,
                          price = firstLine.purchasePrice,
                          qty = requiredQty,
                          totalPrice = Math.Round( firstLine.purchasePrice * (1+firstLine.purchaseTax),6)
                      } into lines
                      select lines).ToList();

            return lstPol;
        }
        #endregion
    }
}