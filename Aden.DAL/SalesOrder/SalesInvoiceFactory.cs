using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Aden.Model.SOMastData;
using Aden.Util.Database;
using Aden.Model.SOCommon;
using Aden.Util.Common;

namespace Aden.DAL.SalesOrder
{
    public class SalesInvoiceFactory
    {
        /// <summary>
        /// 生成开票期间
        /// </summary>
        /// <param name="so"></param>
        /// <returns></returns>
        public List<SalesPeriod> SetSalesPeriod(SalesPeriodParam spp)
        {
            // 日期正确性检查
            if (DateTime.Compare(Convert.ToDateTime(spp.startDate), Convert.ToDateTime(spp.endDate)) > 0)
            {
                return null;
            }
            // 需要开票的合同数据
            List<SalesPeriod> lstSalesPeriod = new List<SalesPeriod>();

            // 取得待生成开票期间的SO
            string strSqlOrderHead = " SELECT H.HEADGUID " +
                                          " , H.VERSION " +
                                          " , H.[CONTRACT] " +
                                          " , H.RECEIVED " +
                                          " , H.SIGNCOUNT " +
                                          " , H.OWNERCOMPANYCODE AS COMPANYCODE" +
                                          " , H.OWNERCOMPANYNAME_ZH AS COMPANYNAME_ZH " +
                                          " , H.OWNERCOMPANYNAME_EN AS COMPANYNAME_EN" +
                                          " , H.CMPCODE " +
                                          " , H.CUSTOMCODE " +
                                          " , H.CUSTOMNAME_ZH " +
                                          " , H.CUSTOMNAME_EN " +
                                          " , H.CURRCODE " +
                                          " , H.CURRNAME_ZH " +
                                          " , H.CURRNAME_EN " +
                                          " , H.PAYMENTCODE " +
                                          " , H.PAYMENTNAME_ZH " +
                                          " , H.PAYMENTNAME_EN " +
                                          " , H.DEADLINE " +
                                          " , CONVERT(VARCHAR(100),H.STARTDATE,20) AS STARTDATE " +
                                          " , CONVERT(VARCHAR(100),ISNULL(H.EXPIRYDATE,H.ENDDATE),20) AS ENDDATE " +
                                          " , CONVERT(VARCHAR(100),H.VALIDDATE,20) AS VALIDDATE " +
                                          " , H.REMARK " +
                                          " , CONVERT(VARCHAR(100),H.CREATEDATE,20) AS CREATEDATE " +
                                          " , H.USERGUID " +
                                          " , H.USERID " +
                                          " , CONVERT(VARCHAR(100),H.CHANGEDATE,20) AS CHANGEDATE " +
                                          " , H.CHANGEUSERGUID " +
                                          " , H.CHANGEUSERID " +
                                          " , CONVERT(VARCHAR(100),H.EXPIRYDATE,20) AS EXPIRYDATE " +
                                          " , H.[STATUS] " +
                                          " , M.ENDDATE AS MAXENDDATE " +
                                          " , M.STARTDATE AS MINSTARTDATE " +
                                       " FROM SALESORDERHEAD AS H " +
                                      " INNER JOIN ( SELECT L.HEADGUID " +
                                                        " , MAX(CASE WHEN P.PRODUCTTYPE = 'SalesMeals' " +
                                                                   " THEN 0 ELSE 1 END) AS P_TYPE " +   //不考虑SalesMeals的产品，即餐次收入
                                                     " FROM SALESORDERITEM AS L " +
                                                    " INNER JOIN PRODUCTTYPEDATA AS P " +
                                                       " ON L.PRODUCTCODE = P.ID " +
                                                    " WHERE L.STATUS <> '0' " +
                                                    " GROUP BY L.HEADGUID ) I " +
                                          " ON H.HEADGUID = I.HEADGUID " +
                                         " AND I.P_TYPE = 1 " +
                                       " LEFT JOIN ( SELECT P.HEADGUID " +
                                                        " , MAX(CONVERT(VARCHAR(100),P.ENDDATE,20)) AS ENDDATE " +
                                                        " , MIN(CONVERT(VARCHAR(100),P.STARTDATE,20)) AS STARTDATE " +   //已生成的开票区间
                                                     " FROM SALESPERIOD AS P " +
                                                    " WHERE P.HEADGUID IN ( SELECT HEADGUID " +
                                                                            " FROM SALESORDERHEAD " +
                                                                           " WHERE OWNERCOMPANYCODE = '{0}' " +
                                                                             " AND VALIDDATE <= '{1}' " +
                                                                             " AND ENDDATE >= '{2}' ) " +
                                                      " AND P.STATUS <> '9' " +
                                                    " GROUP BY P.HEADGUID ) M " +
                                         " ON H.HEADGUID = M.HEADGUID " +
                                      " WHERE H.OWNERCOMPANYCODE = '{0}' " +
                                        " AND H.VALIDDATE <= '{1}' " +
                                        " AND ISNULL(H.EXPIRYDATE,H.ENDDATE) >= '{2}' " +
                                        " AND H.STATUS NOT IN ('0', '9') " +
                                        " AND H.HEADGUID NOT IN ( SELECT T.HEADGUID " +
                                                                  " FROM ( SELECT P.HEADGUID " +
                                                                              " , MAX(P.ENDDATE) AS ENDDATE_MAX " +
                                                                              " , MIN(P.STARTDATE) AS STARTDATE_MIN " +
                                                                              " , MAX(ISNULL(H.EXPIRYDATE,H.ENDDATE)) AS ENDDATE " +
                                                                              " , MAX(H.STARTDATE) AS STARTDATE " +
                                                                           " FROM SALESPERIOD AS P " +
                                                                          " INNER JOIN SALESORDERHEAD AS H " +
                                                                             " ON P.HEADGUID = H.HEADGUID " +
                                                                            " AND H.STATUS NOT IN ('0', '9') " +
                                                                          " WHERE H.OWNERCOMPANYCODE = '{0}'" +
                                                                            " AND H.VALIDDATE <= '{1}' " +
                                                                            " AND ISNULL(H.EXPIRYDATE,H.ENDDATE) >= '{2}' " +
                                                                            " AND P.STATUS <> '9' " +
                                                                          " GROUP BY P.HEADGUID ) AS T " +
                                                                 " WHERE T.STARTDATE_MIN <= '{2}' " +
                                                                   " AND T.ENDDATE_MAX >= '{1}' )  ";

            StringBuilder sbSqlOrderHead = new StringBuilder();

            sbSqlOrderHead.AppendFormat(strSqlOrderHead, spp.companyCode, Common.convertDateTime(spp.endDate), Common.convertDateTime(spp.startDate));

            List<SalesOrderPeriod> lstOrderHead = SqlServerHelper.GetEntityList<SalesOrderPeriod>(SqlServerHelper.salesorderConn(), sbSqlOrderHead.ToString());

            if (lstOrderHead != null && lstOrderHead.Count > 0)
            {
                // 上一轮循计算出的开票区间起始日期
                DateTime lastEndDate = new DateTime();
                // Sql文（参数）
                StringBuilder sbSql = new StringBuilder();
                // Sql文（最终结果）
                StringBuilder sbSqlFinal = new StringBuilder();

                // check head update flag
                bool checkHead = false;

                foreach (SalesOrderPeriod order in lstOrderHead)
                {
                    // 初始化Sql文（参数）
                    sbSql = new StringBuilder();
                    // 初始化Head更新标志
                    checkHead = false;
                    // 追加Insert开票区间Sql语句
                    sbSqlFinal.Append(InsertSql(Convert.ToDateTime(spp.startDate), Convert.ToDateTime(spp.endDate), order, lstSalesPeriod,
                        lastEndDate, sbSql, spp.userGuid, spp.userID, Convert.ToDateTime(spp.startDate), Convert.ToDateTime(spp.endDate), ref checkHead));
                }
                // 执行Insert方法
                SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());
            }
            // TODO
            //string strSqlSalesPeriod = " SELECT P.PERIODGUID " +
            //                                " , P.HEADGUID " +
            //                                " , P.COMPANYCODE " +
            //                                " , P.CONTRACT " +
            //                                " , P.CUSTOMCODE " +
            //                                " , CONVERT(VARCHAR(100),P.STARTDATE,20) AS STARTDATE " +
            //                                " , CONVERT(VARCHAR(100),P.ENDDATE,20) AS ENDDATE " +
            //                                " , CONVERT(VARCHAR(100),P.REPORTDATE,20) AS REPORTDATE " +
            //                                " , CONVERT(VARCHAR(100),P.CREATEDATE,20) AS CREATEDATE " +
            //                                " , P.USERGUID " +
            //                                " , P.USERID " +
            //                                " , CONVERT(VARCHAR(100),P.CHANGEDATE,20) AS CHANGEDATE " +
            //                                " , P.CHANGEUSERGUID " +
            //                                " , P.CHANGEUSERID " +
            //                                " , P.STATUS " +
            //                                " , (CASE WHEN I.COUNT > 0 AND I.STATUS <> '1' " +
            //                                        " THEN '1' " +
            //                                        " ELSE '0' " +
            //                                   " END) AS COMPLETE " +
            //                             " FROM SALESPERIOD P " +
            //                             " LEFT JOIN ( SELECT PERIODGUID " +
            //                                              " , COUNT(1) AS COUNT " +
            //                                              " , MIN(CASE WHEN STATUS = 'tobeinvoiced' THEN '0' ELSE STATUS END) AS STATUS " +
            //                                           " FROM SALESINVOICE " +
            //                                          " WHERE STATUS <> '0' " +
            //                                          " GROUP BY PERIODGUID ) I " +
            //                               " ON P.PERIODGUID = I.PERIODGUID " +
            //                            " WHERE P.COMPANYCODE = '{0}' " +
            //                              " AND P.STARTDATE <= '{1}' " +
            //                              " AND P.ENDDATE >= '{2}' ";

            //StringBuilder sbSqlSalesPeriod = new StringBuilder();

            //sbSqlSalesPeriod.AppendFormat(strSqlSalesPeriod, spp.companyCode, Common.convertDateTime(spp.endDate), Common.convertDateTime(spp.startDate));

            //lstSalesPeriod = SqlServerHelper.GetEntityList<SalesPeriod>(SqlServerHelper.salesorderConn, sbSqlSalesPeriod.ToString());

            return lstSalesPeriod;
        }

        /// <summary>
        /// 跟居Order对象生成对应的Insert Sql文
        /// </summary>
        /// <param name="queryStartDate">传入开始时间</param>
        /// <param name="queryEndDate">传入结束时间</param>
        /// <param name="order">合同订单对象</param>
        /// <param name="lstSalesPeriod">开票区间列表</param>
        /// <param name="lastEndDate">上一次递归的结果</param>
        /// <param name="sbSql">Sql文</param>
        /// <param name="userGuid">用户Guid</param>
        /// <param name="userID">用户ID</param>
        /// <param name="checkHead">是否已经检查过是否要更新SO HEAD状态</param>
        /// <returns></returns>
        private string InsertSql(DateTime queryStartDate, DateTime queryEndDate, SalesOrderPeriod order, List<SalesPeriod> lstSalesPeriod,
            DateTime lastEndDate, StringBuilder sbSql, string userGuid, string userID, DateTime queryStartDateBackup, DateTime queryEndDateBackup, ref bool checkHead)
        {
            // 开票区间起始日
            DateTime wkStartDate = new DateTime();
            // 开票区间结束日
            DateTime wkEndDate = new DateTime();
            // 合同结束日期所属月的最后一天
            DateTime lastDayOfMonth = new DateTime();
            // 开票截止日
            int intDeadLine = order.deadline;
            // 合同结束日期所属月的最后一天（号）
            int intDay = 0;
            // 截止日和最后一日比较的结果日期
            DateTime compareDt = new DateTime();
            // 合同有效开始日期（以生效日期为准）
            DateTime startDate = Convert.ToDateTime(order.validDate);
            // 合同结束日期 / 传入的结束日期
            DateTime endDate = Convert.ToDateTime(order.endDate);
            // 取得已设置区间的最大日期
            DateTime maxEndDate = Convert.ToDateTime(order.maxEndDate);
            // 取得已设置区间的最小日期
            DateTime minStartDate = Convert.ToDateTime(order.minStartDate);

            // 判断是否为第一次循环
            if (lastEndDate == null || lastEndDate == DateTime.MinValue)
            {
                /****调整查询日期 START****/
                // 结束日期调整
                // 结束日期所属月的最后一天
                intDay = Common.GetLastDayofMonth(queryEndDate, ref queryEndDate);
                // 当大于截止日时以截止日为准
                if (intDay > intDeadLine) queryEndDate = new DateTime(queryEndDate.Year, queryEndDate.Month, intDeadLine);

                // 开始日期调整
                // 开始日期上一月的最后一天
                intDay = Common.GetLastDayofMonth(queryStartDate.AddMonths(-1), ref queryStartDate);
                // 当大于截止日时以截止日为准
                if (intDay > intDeadLine) queryStartDate = new DateTime(queryStartDate.Year, queryStartDate.Month, intDeadLine);
                // 上一月截止日期加1天
                queryStartDate = queryStartDate.AddDays(1);
                /****调整查询日期 END****/

                // 当历史区间最大值存在并且合同结束日期或传入结束时间小于等于历史区间最大值时
                if ((maxEndDate != null && maxEndDate != DateTime.MinValue) &&
                   (DateTime.Compare(endDate, maxEndDate) <= 0 || DateTime.Compare(queryEndDate, maxEndDate) <= 0))
                {
                    // 传入日期和合同开始日期均比历史最小开票区间小时，说明两者有交集
                    if (DateTime.Compare(queryStartDate, minStartDate) < 0 && DateTime.Compare(startDate, minStartDate) < 0)
                    {
                        // 设置下一轮开票结束区间为历史最小开票区间减1天并做递归
                        InsertSql(queryStartDate, queryEndDate, order, lstSalesPeriod, minStartDate.AddDays(-1), sbSql, userGuid, userID,
                            queryStartDateBackup, queryEndDateBackup, ref checkHead);
                    }
                }
                else
                {
                    // 当合同结束日期大于传入的结束日期时以传入的结束日期为准
                    if (DateTime.Compare(endDate, queryEndDate) > 0) endDate = queryEndDate;

                    // 合同结束日期所属月的最后一天（号）
                    intDay = Common.GetLastDayofMonth(endDate, ref lastDayOfMonth);

                    // 根据截止日期生成起始开票日期
                    if (intDay > intDeadLine)
                    {
                        // 根据截止日期生成DateTime对象
                        compareDt = new DateTime(endDate.Year, endDate.Month, intDeadLine);
                    }
                    else
                    {
                        // 取当月最后一天
                        compareDt = lastDayOfMonth;
                    }

                    // 再比较与合同终止日的大小
                    if (DateTime.Compare(endDate, compareDt) > 0)
                    {
                        // 大于的部分单独设置一个开票区间
                        // 开票区间起始日
                        wkStartDate = compareDt.AddDays(1);
                        // 开票区间结束日
                        wkEndDate = endDate;
                        // 追加Insert Sql文
                        sbSql.Append(GetSalesPeriodInsertSql(wkStartDate, wkEndDate, order, userGuid, userID, queryStartDateBackup, queryEndDateBackup));
                        // 设置计算出的该月开票结束日期
                        lastEndDate = compareDt;
                    }
                    else
                    {
                        // 当小于等于合同终止日时以合同终止日为准
                        lastEndDate = endDate;
                    }
                    // 递归
                    InsertSql(queryStartDate, queryEndDate, order, lstSalesPeriod, lastEndDate, sbSql, userGuid, userID, queryStartDateBackup, queryEndDateBackup, ref checkHead);
                }
            }
            else
            {
                // 设置开票区间结束日期
                wkEndDate = lastEndDate;
                // 用于比较的开始日期
                DateTime compareStartDate = new DateTime();

                /***** 根据传入的日期计算上一个常规开票日 Start *****/
                // 取得上一个月的最后一天
                intDay = Common.GetLastDayofMonth(wkEndDate.AddMonths(-1), ref lastDayOfMonth);
                // 比较上月最后一天与开票截止日的大小
                if (intDay > intDeadLine)
                {
                    // 根据截止日期生成DateTime对象
                    compareDt = new DateTime(lastDayOfMonth.Year, lastDayOfMonth.Month, intDeadLine);
                }
                else
                {
                    // 反之根据当月最后一天生成DateTime对象（上一个开票区间结束日）
                    compareDt = lastDayOfMonth;
                }
                /***** 根据传入的日期计算上一个常规开票日 End *****/
                // 场景FLAG
                string strCase = String.Empty;
                // 如果历史最大开票区间日期和最小日期都为空时
                if ((maxEndDate == null || maxEndDate == DateTime.MinValue) &&
                    (minStartDate == null || minStartDate == DateTime.MinValue))
                {
                    compareStartDate = startDate;
                    strCase = "1";
                }
                else
                {   
                    // 当计算出的开始日期大于等于历史区间最大结束日期，且历史区间最大结束日期+1天小于等于当前区间结束日期时
                    if (DateTime.Compare(compareDt, maxEndDate) >= 0 && 
                        DateTime.Compare(maxEndDate.AddDays(1), wkEndDate) <=0)
                    {
                        // 计算开始日期用变量取历史区间最大日期
                        compareStartDate = maxEndDate;
                        strCase = "2";
                    }
                    else
                    {
                        if (DateTime.Compare(wkEndDate, maxEndDate) > 0)
                        {
                            compareStartDate = maxEndDate;
                            strCase = "2";
                        }
                        else
                        {
                            compareStartDate = startDate;
                            strCase = "3";
                        }
                    }
                }

                // 再与传入开始日期做比较
                if (DateTime.Compare(compareStartDate, queryStartDate) < 0)
                {
                    compareStartDate = queryStartDate;
                }

                // 再与合同起始日期比较
                int intCompare = DateTime.Compare(compareDt.AddDays(1), compareStartDate);

                if (intCompare >= 0)
                {
                    // 上一个开票区间起始日期+1天
                    wkStartDate = compareDt.AddDays(1);

                    // 排除和历史区间重复生成的情况
                    if (DateTime.Compare(wkEndDate, maxEndDate) != 0)
                    {
                        // 追加Insert Sql文
                        sbSql.Append(GetSalesPeriodInsertSql(wkStartDate, wkEndDate, order, userGuid, userID, queryStartDateBackup, queryEndDateBackup));
                    }

                    // 大于起始日期的情况下做递归
                    if (intCompare > 0)
                    {
                        // 没有摸到合同有效起始日期时做递归
                        InsertSql(queryStartDate, queryEndDate, order, lstSalesPeriod, compareDt, sbSql, userGuid, userID, queryStartDateBackup, queryEndDateBackup, ref checkHead);
                    }
                    // 等于起始日期的情况
                    if (intCompare == 0)
                    {
                        if ("2".Equals(strCase))
                        {
                            // 传入日期和合同开始日期均比历史最小开票区间小时，说明两者有交集
                            if (DateTime.Compare(queryStartDate, minStartDate) < 0 && DateTime.Compare(startDate, minStartDate) < 0)
                            {
                                // 设置下一轮开票结束区间为历史最小开票区间减1天并做递归
                                InsertSql(queryStartDate, queryEndDate, order, lstSalesPeriod, minStartDate.AddDays(-1), sbSql, userGuid, userID, queryStartDateBackup, queryEndDateBackup, ref checkHead);
                            }
                        }
                    }
                }
                else
                {
                    // 当越过开始日期时
                    if ("2".Equals(strCase))
                    {
                        wkStartDate = maxEndDate.AddDays(1);
                    }
                    else
                    {
                        wkStartDate = startDate;

                        if (DateTime.Compare(queryStartDate, startDate) > 0)
                        {
                            wkStartDate = queryStartDate;
                        }
                    }
                    // 追加Insert Sql文并结束递归
                    sbSql.Append(GetSalesPeriodInsertSql(wkStartDate, wkEndDate, order, userGuid, userID, queryStartDateBackup, queryEndDateBackup));

                    if ("2".Equals(strCase))
                    {
                        // 传入日期和合同开始日期均比历史最小开票区间小时，说明两者有交集
                        if (DateTime.Compare(queryStartDate, minStartDate) < 0 && DateTime.Compare(startDate, minStartDate) < 0)
                        {
                            // 设置下一轮开票结束区间为历史最小开票区间减1天并做递归
                            InsertSql(queryStartDate, queryEndDate, order, lstSalesPeriod, minStartDate.AddDays(-1), sbSql, userGuid, userID, queryStartDateBackup, queryEndDateBackup, ref checkHead);
                        }
                    }
                }
            }
            // 看是否已经检查过Head表
            if ("1".Equals(order.status) && !checkHead)
            {
                // 如果StringBuilder不为空的话，更新SalesOrderHead的状态设置为'2'（开票）
                if (sbSql != null && !String.Empty.Equals(sbSql.ToString()))
                {
                    // 追加Update Sql
                    string retSql = GetSalesOrderHeadUpdateSql(order.headGuid, userGuid, userID);
                    if (!String.Empty.Equals(retSql)) sbSql.Append(retSql);
                }
                checkHead = true;
            }

            return sbSql.ToString();
        }

        /// <summary>
        /// 生成Sales Period Insert Sql文
        /// </summary>
        /// <param name="startDate">开票区间起始日期</param>
        /// <param name="endDate">开票区间结束日期</param>
        /// <param name="order">合同对象</param>
        /// <param name="userGuid">处理人Guid</param>
        /// <param name="userID">处理人ID</param>
        /// <returns></returns>
        private string GetSalesPeriodInsertSql(DateTime startDate, DateTime endDate, SalesOrderPeriod order, 
            string userGuid, string userID, DateTime queryStartDate, DateTime queryEndDate)
        {
            if (DateTime.Compare(endDate, queryEndDate) > 0 || DateTime.Compare(endDate, queryStartDate) < 0)
                return null;
            if (DateTime.Compare(startDate, endDate) > 0)
                return null;

            // 生成Period Guid
            string periodGuid = Guid.NewGuid().ToString().ToUpper();

            string strSql = " INSERT INTO SALESPERIOD " +
                                      " ( PERIODGUID " +
                                      " , HEADGUID " +
                                      " , OWNERCOMPANYCODE " +
                                      " , OWNERCOMPANYNAME_ZH " +
                                      " , OWNERCOMPANYNAME_EN " +
                                      " , COMPANYCODE " +
                                      " , COMPANYNAME_ZH " +
                                      " , COMPANYNAME_EN " +
                                      " , CONTRACT " +
                                      " , CMPCODE " +
                                      " , CUSTOMCODE " +
                                      " , CUSTOMNAME_ZH " +
                                      " , CUSTOMNAME_EN " +
                                      " , STARTDATE " +
                                      " , ENDDATE " +
                                      " , REPORTDATE " +
                                      " , CREATEDATE " +
                                      " , USERGUID " +
                                      " , USERID" +
                                      " , STATUS ) " +
                                 " SELECT " +
                                      "  '{0}' " +
                                      " , '{1}' " +
                                      " , '{2}' " +
                                      " , '{3}' " +
                                      " , '{4}' " +
                                      " , '{5}' " +
                                      " , '{6}' " +
                                      " , '{7}' " +
                                      " , '{8}' " +
                                      " , '{9}' " +
                                      " , '{10}' " +
                                      " , '{11}' " +
                                      " , '{12}' " +
                                      " , '{13}' " +
                                      " , '{14}' " +
                                      " , '{15}' " +
                                      " , '{16}' " +
                                      " , '{17}' " +
                                      " , '{18}' " +
                                      " , '{19}'  " +
                                      " WHERE NOT EXISTS  ( SELECT TOP 1 1 FROM SALESPERIOD " +
                                                          "  WHERE HEADGUID = '{1}' " +
                                                             " AND STARTDATE <= '{14}' " +
                                                             " AND ENDDATE >= '{13}') ";

            StringBuilder sbSql = new StringBuilder();
            // 描述类字段中的单引号和NULL处理
            string ownerCompanyName_ZH = order.ownerCompanyName_ZH == null ? null : order.companyName_ZH.Trim().Replace("'", "''");
            string ownerCompanyName_EN = order.ownerCompanyName_EN == null ? null : order.companyName_EN.Trim().Replace("'", "''");
            string companyName_ZH = order.companyName_ZH == null ? null : order.companyName_ZH.Trim().Replace("'", "''");
            string companyName_EN = order.companyName_EN == null ? null : order.companyName_EN.Trim().Replace("'", "''");
            string customName_ZH = order.customName_ZH == null ? null : order.customName_ZH.Trim().Replace("'", "''");
            string customName_EN = order.customName_EN == null ? null : order.customName_EN.Trim().Replace("'", "''");

            sbSql.AppendFormat(strSql, periodGuid, order.headGuid, order.ownerCompanyCode, ownerCompanyName_ZH,
                ownerCompanyName_EN, order.companyCode, companyName_ZH, companyName_EN, order.contract, order.cmpCode, order.customCode, 
                customName_ZH, customName_EN, Common.convertDateTime(startDate), Common.convertDateTime(endDate), 
                Common.convertDateTime(endDate), Common.convertDateTime(DateTime.Now.ToString()), userGuid, userID, "1");

            return sbSql.ToString();
        }

        /// <summary>
        /// 回写SalesOrderHead表的状态，设置成'2'（开票）1
        /// </summary>
        /// <param name="order">so head表Guid</param>
        /// <param name="userGuid">用户Guid</param>
        /// <param name="userID">用户ID</param>
        /// <returns>Sql文</returns>
        public string GetSalesOrderHeadUpdateSql(string headGuid, string userGuid, string userID)
        {
            // 检查当前Head的状态是否为'2'
            string strSqlCheck = " SELECT HEADGUID " +
                                   " FROM SALESORDERHEAD " +
                                  " WHERE STATUS = '2' " +
                                    " AND HEADGUID = '" + headGuid + "' ";

            List<Model.SOMastData.SalesOrder> lstSalesOrder = SqlServerHelper.GetEntityList<Model.SOMastData.SalesOrder>(SqlServerHelper.salesorderConn(), strSqlCheck);

            // 状态'2'时不做更新处理
            if (lstSalesOrder != null && lstSalesOrder.Count > 0) return String.Empty;

            SalesOrderFactory sof = new SalesOrderFactory();
            string strSql = String.Empty;

            // 根据Guid取得SO Head;
            Model.SOMastData.SalesOrder orderHead = sof.GetSO(headGuid, false);
            // SO状态
            orderHead.status = "2";
            // 变更标志
            orderHead.changeFlag = "1";
            // 用户Guid
            orderHead.changeUserGuid = userGuid;
            // 用户ID
            orderHead.changeUserID = userID;
            // 执行修改SO方法
            sof.EditSO(orderHead, ref strSql, false);

            return strSql;
        }

        public List<SalesInvoice> sL(string periodGuid)
        {
            string sql = "select periodGuid from SALESINVOICE where periodGuid='" + periodGuid + "'";
            List<SalesInvoice> dtemp = SqlServerHelper.GetEntityList<SalesInvoice>(SqlServerHelper.salesorderConn(), sql.ToString());
            return dtemp;
        }

        private int InsertSalesInvoice(SalesInvoiceParam sip)
        {
            #region 生成SalesInvoice (旧） deleted by steve 2017-11-21
            //// 生成发票行列表
            //List<SalesInvoice> lstSalesInvoice = new List<SalesInvoice>();

            //string strSqlInvoice = " SELECT distinct I.PROCESSGUID " +
            //                            " , P.PERIODGUID " +
            //                            " , P.HEADGUID " +
            //                            " , L.ITEMGUID " +
            //                            " , L.CONTRACT " +
            //                            " , L.COSTCENTERCODE " +
            //                            " , L.PRODUCTCODE " +
            //                            " , L.PRODUCTDESC " +
            //                            " , L.QTY " +
            //                            " , L.PRICE " +
            //                            " , H.CURRCODE " +
            //                            " , L.TAXCODE " +
            //                            " , L.PRICEUNITCODE " +
            //                            " , CONVERT(VARCHAR(100),P.STARTDATE,20) AS STARTDATE" +
            //                            " , CONVERT(VARCHAR(100),P.ENDDATE,20) AS ENDDATE " +
            //                            " , CONVERT(VARCHAR(100),P.ENDDATE,20) AS REPORTDATE " +
            //                            " , H.CROSSCOMPINV " +
            //                         " FROM SALESPERIOD AS P " +
            //                        " INNER JOIN SALESORDERHEAD AS H " +
            //                           " ON H.HEADGUID = P.HEADGUID " +
            //                          " AND H.STATUS IN ('1','2') " +
            //                        " INNER JOIN SALESORDERITEM AS L " +
            //                           " ON P.HEADGUID = L.HEADGUID " +
            //                          " AND P.STATUS NOT IN ('0', '9') " +
            //                          " AND L.STATUS NOT IN ('0', '9') " +
            //                          " AND L.QTY <> 0 " +
            //                          " AND ( L.STARTDATE <= P.ENDDATE AND " +
            //                                " L.ENDDATE >= P.STARTDATE ) " +
            //                         " LEFT JOIN SALESINVOICE AS I " +
            //                           " ON I.ITEMGUID = L.ITEMGUID " +
            //                          " AND I.STARTDATE = P.STARTDATE" +
            //                          " AND I.ENDDATE = P.ENDDATE " +
            //                        " WHERE P.PERIODGUID = '{0}' " +
            //                          " AND L.ITEMGUID NOT IN ( SELECT ITEMGUID " +
            //                                                    " FROM SALESINVOICE " +
            //                                                   " WHERE PERIODGUID = '{0}' " +
            //                                                     " AND STATUS NOT IN ('0', 'tobeinvoiced') ) ";



            //StringBuilder sbSqlInvoice = new StringBuilder();

            //sbSqlInvoice.AppendFormat(strSqlInvoice, sip.periodGuid);

            //lstSalesInvoice = SqlServerHelper.GetEntityList<SalesInvoice>(SqlServerHelper.salesorderConn, sbSqlInvoice.ToString());

            //// Sql文（最终结果）
            //StringBuilder sbSqlFinal = new StringBuilder();

            //////SL需要单独写，然后在if条件中引用
            ////List<SalesInvoice> SL = sL(sip.periodGuid);

            //if (lstSalesInvoice != null && lstSalesInvoice.Count > 0)
            //{
            //    foreach (SalesInvoice invoice in lstSalesInvoice)
            //    {
            //        sbSqlFinal.Append(GetSalesInvoiceSql(sip, invoice));
            //    }

            //    // 执行Insert方法
            //    SqlServerHelper.Execute(SqlServerHelper.salesorderConn, sbSqlFinal.ToString());
            //}
            #endregion

            #region 生成SalesInvoice created by steve 2017-11-21
            string strSqlInvoice = "insert into salesinvoice("
                + "processguid,periodguid,headguid,itemguid,contract,CostCenterCode,InvoiceCostCenterCode,ProductCode,ProductDesc,"
                + "qty,price,CurrCode,TaxCode,PriceUnitCode,StartDate,EndDate,"
                + "CrossCompInv,createdate,userguid,userid,status,tobeinvoiced) "

                + "select newid(), periodguid,headguid,itemguid,contract,CostCenterCode,InvoiceCostCenterCode,ProductCode,ProductDesc,"
                + "Qty,price,CurrCode,TaxCode,PriceUnitCode,"
                + "(case when startdate1 > startdate2 then startdate1 else startdate2 end),"
                + "(case when enddate1 < enddate2 then enddate1 else enddate2 end),"
                + "CrossCompInv,'{3}','{1}','{2}','0',0 "
                + "from "
                + "(select p.periodguid,it.headguid,it.itemguid,h.contract,it.CostCenterCode,it.InvoiceCostCenterCode,it.ProductCode,it.ProductDesc,"
                + "it.Qty,it.price,h.CurrCode,it.TaxCode,it.PriceUnitCode,"
                + "(case when p.startdate > it.startdate then p.startdate else it.startdate end) startdate1,"
                + "(case when h.validdate > h.startdate then h.validdate else h.startdate end) startdate2,"
                + "(case when p.enddate <isnull(it.expirydate,it.enddate) then p.enddate else isnull(it.expirydate,it.enddate) end) enddate1,"
                + "isnull(h.expirydate,h.enddate) enddate2,"
                + "h.CrossCompInv "
                + "from salesperiod (nolock) p join salesorderhead (nolock)  h on p.headguid=h.headguid "
                + "join (salesorderitem (nolock) it join producttypedata ptd on it.productcode=ptd.id and ptd.ProductType<>'Salesmeals') "
                + "on p.headguid= it.headguid  "
                + "left join salesinvoice (nolock)  inv on it.itemguid=inv.itemguid " //p.periodguid=inv.periodguid and 
                + "and inv.startdate <= p.enddate and inv.enddate >= p.startdate "
                  + " where p.periodguid='{0}' and inv.id is null "
                + "and isnull(it.expirydate,it.enddate)>=p.startdate and it.startdate<=p.enddate "
                + "and (case when h.validdate > h.startdate then h.validdate else h.startdate end) <= p.enddate "
                + "and isnull(h.expirydate,h.enddate) >= p.startdate "
                + "AND H.STATUS IN ('1','2','9') and P.STATUS NOT IN ('0', '9') AND it.STATUS <> '0') a";
            //and isnull(it.qty,0)*isnull(it.price,0) <> 0) a ";

            StringBuilder sbSqlInvoice = new StringBuilder();

            sbSqlInvoice.AppendFormat(strSqlInvoice, sip.periodGuid, sip.userGuid, sip.userID, Common.convertDateTime(DateTime.Now));
            // 回写SalesOrderItem状态
            strSqlInvoice = " UPDATE SALESORDERITEM "
                + "SET STATUS = '2' "
                + "FROM SALESINVOICE "
                + "WHERE SALESINVOICE.PERIODGUID = '{0}' AND SALESINVOICE.STATUS = '0' "
                + "AND SALESORDERITEM.ITEMGUID = SalesInvoice.ItemGuid AND SALESORDERITEM.STATUS = '1'";
            sbSqlInvoice.AppendFormat(strSqlInvoice, sip.periodGuid);

            // 执行Insert方法
            return SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlInvoice.ToString());
            #endregion
        }
        /// <summary>
        /// 根据传入的开票区间信息生成开票行
        /// </summary>
        /// <param name="sip">传入参数结构</param>
        /// <returns></returns>
        public List<SalesInvoice> SetSalesInvoice(SalesInvoiceParam sip, ref int insertCount)
        {
            if ("0".Equals(sip.status) && !string.IsNullOrWhiteSpace(sip.periodGuid) && !string.IsNullOrWhiteSpace(sip.userGuid))
                insertCount = InsertSalesInvoice(sip);

            // 取出执行结果并返回
            string strSqlInvoice = " SELECT PROCESSGUID " +
                                 " , PERIODGUID " +
                                 " , HEADGUID " +
                                 " , ITEMGUID " +
                                 " , GROUPGUID " +
                                 " , CONTRACT " +
                                 " , COSTCENTERCODE " +
                                 " , PRODUCTCODE " +
                                 " , PRODUCTDESC " +
                                 " , QTY " +
                                 " , PRICE " +
                                 " , ADJAMT " +
                                 " , FINADJAMT " +
                                 " , CURRCODE " +
                                 " , TAXCODE " +
                                 " , PRICEUNITCODE " +
                                 " , CONVERT(VARCHAR(10),STARTDATE,120) AS STARTDATE " +
                                 " , CONVERT(VARCHAR(10),ENDDATE,120) AS ENDDATE " +
                                 " , CONVERT(VARCHAR(10),REPORTDATE,120) AS REPORTDATE " +
                                 " , CONVERT(VARCHAR(10),ESTIMATEDREPORTDATE,120) AS ESTIMATEDREPORTDATE " + 
                                 " , INVOICEGUID " +
                                 " , INVOICENUMBER " +
                                 " , ESTIMATIONGUID " +
                                 " , ESTIMATIONNUMBER " +
                                 " , REVERSEESTIMATIONGUID " +
                                 " , REVERSEESTIMATIONNUMBER " +
                                 " , INTERSALESARGUID " +
                                 " , INTERSALESARNUMBER " +
                                 " , INTERSALESAPGUID " +
                                 " , INTERSALESAPNUMBER " +
                                 " , TOBEREVERSED " +
                                 " , ADJUSTENTRYNUMBER " +
                                 " , CONVERT(VARCHAR(100),CREATEDATE,20) AS CREATEDATE " +
                                 " , USERGUID " +
                                 " , USERID " +
                                 " , CONVERT(VARCHAR(100),CHANGEDATE,20) AS CHANGEDATE " +
                                 " , CHANGEUSERGUID " +
                                 " , CHANGEUSERID " +
                                 " , NOTAX " +
                                 " , STATUS " +
                                 " , TOBEINVOICED " +
                                 " , (CASE WHEN STATUS IN ('invoicing', 'reverseestimating') THEN 'invoicing' " +
                                         " WHEN STATUS IN ('invoiced', 'reverseestimated') THEN 'invoiced' " +
                                         " ELSE STATUS END) AS STATUS_P " +
                              " FROM SALESINVOICE " +
                             " WHERE {0}";// PERIODGUID = '{0}' ";

            // Sql Status条件
            string strSqlStatus = String.Empty;

            switch(sip.status)
            {
                // 待处理
                case "0":
                    strSqlStatus = " AND STATUS IN ('0', 'estimated') ";
                    break;
                // 暂估中
                case "estimating":
                    strSqlStatus = " AND STATUS = 'estimating' ";
                    break;
                // 开票中
                case "invoicing":
                    strSqlStatus = " AND STATUS IN ('invoicing', 'estimating', 'reverseestimating') ";
                    break;
                // 已暂估
                case "estimated":
                    strSqlStatus = " AND STATUS = 'estimated' ";
                    break;
                // 已开票
                case "invoiced":
                    strSqlStatus = " AND STATUS IN ('invoiced', 'reverseestimated') ";
                    break;
                // 终止
                case "9":
                    strSqlStatus = " AND STATUS = '9' ";
                    break;
            }

            string strSqlGroupGuid = String.Empty;

            if (!string.IsNullOrWhiteSpace(sip.groupGuid))
            {
                strSqlGroupGuid = " AND GROUPGUID = '" + sip.groupGuid + "' ";
            }
            else
            {
                strSqlGroupGuid = " AND GROUPGUID IS NULL ";
            }

            StringBuilder sbSqlInvoice = new StringBuilder();

            List<SalesInvoice> lstSalesInvoice = new List<SalesInvoice>();

            if (string.IsNullOrWhiteSpace(sip.periodGuid))
            {
                sbSqlInvoice.AppendFormat(strSqlInvoice, string.Format("headguid in (select headguid from salesorderhead where ownercompanycode = '{0}')", sip.companyCode));
            }
            else
            {
                strSqlInvoice = strSqlInvoice  + strSqlStatus + strSqlGroupGuid;
                sbSqlInvoice.AppendFormat(strSqlInvoice, string.Format("PERIODGUID = '{0}'", sip.periodGuid), sip.status);
                //sbSqlInvoice.AppendFormat(strSqlInvoice, string.Format("PERIODGUID = '{0}'", sip.periodGuid));
            }
            sbSqlInvoice.Append(" ORDER BY ESTIMATIONNUMBER ");

            lstSalesInvoice = SqlServerHelper.GetEntityList<SalesInvoice>(SqlServerHelper.salesorderConn(), sbSqlInvoice.ToString());

            return lstSalesInvoice;
        }

        /// <summary>
        /// 生成Sales Invoice Insert Sql文
        /// </summary>
        /// <param name="sip">Sales Invoice参数</param>
        /// <param name="invoice">Invoice行</param>
        /// <returns></returns>
        private string GetSalesInvoiceSql(SalesInvoiceParam sip, SalesInvoice invoice)
        {
            // 生成Period Guid
            string processGuid = Guid.NewGuid().ToString().ToUpper();
            // Insert Sql文
            string strSqlInsert = String.Empty;
            // Update status Sql文
            string strSqlUpdate = String.Empty;
            // Update SalesOrderItem Status Sql文
            string strSqlUpdateStatus = String.Empty;

            string strDateNow = Common.convertDateTime(DateTime.Now);

            strSqlInsert = " INSERT INTO SALESINVOICE " +
                                     " ( PROCESSGUID " +
                                     " , PERIODGUID " +
                                     " , HEADGUID " +
                                     " , ITEMGUID " +
                                     " , CONTRACT " +
                                     " , COSTCENTERCODE " +
                                     " , PRODUCTCODE " +
                                     " , PRODUCTDESC " +
                                     " , QTY " +
                                     " , PRICE " +
                                     " , CURRCODE " +
                                     " , TAXCODE " +
                                     " , PRICEUNITCODE " +
                                     " , STARTDATE " +
                                     " , ENDDATE " +
                                     " , REPORTDATE " +
                                     " , CREATEDATE " +
                                     " , USERGUID " +
                                     " , USERID " +
                                     " , CROSSCOMPINV " +
                                     " , STATUS ) " +
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
                                     " , '{9}' " +
                                     " , '{10}' " +
                                     " , '{11}' " +
                                     " , '{12}' " +
                                     " , '{13}' " +
                                     " , '{14}' " +
                                     " , '{15}' " +
                                     " , '{16}' " +
                                     " , '{17}' " +
                                     " , '{18}' " +
                                     " , '{19}' " +
                                     " , '{20}' ) ";

            strSqlUpdate = " UPDATE SALESINVOICE " +
                              " SET CONTRACT = '{0}' " +
                                " , COSTCENTERCODE = '{1}' " +
                                " , PRODUCTCODE = '{2}' " +
                                " , PRODUCTDESC = '{3}' " +
                                " , QTY = '{4}' " +
                                " , PRICE = '{5}' " +
                                " , CURRCODE = '{6}' " +
                                " , TAXCODE = '{7}' " +
                                " , PRICEUNITCODE = '{8}' " +
                                " , STARTDATE = '{9}' " +
                                " , ENDDATE = '{10}' " +
                                " , ReportDate = '{11}' " +
                                " , CHANGEDATE = '{12}' " +
                                " , CHANGEUSERGUID = '{13}' " +
                                " , CHANGEUSERID = '{14}' " +
                                " , STATUS = '{15}' " +
                            " WHERE PROCESSGUID = '{16}' ";

            strSqlUpdateStatus = " UPDATE SALESORDERITEM " +
                                    " SET STATUS = '2' " +
                                  " WHERE ITEMGUID = '{0}' " +
                                    " AND STATUS = '1' ";

            StringBuilder sbSql = new StringBuilder();
            string endOfStatus = "";

            //if (invoice.processGuid == null || String.Empty.Equals(invoice.processGuid))
            //List<SalesInvoice> SL = sL(sip.periodGuid);

            if (invoice.processGuid == null || String.Empty.Equals(invoice.processGuid))
            {
                sbSql.AppendFormat(strSqlInsert, processGuid, invoice.periodGuid, invoice.headGuid, invoice.itemGuid,
                    invoice.contract, invoice.costCenterCode, invoice.productCode, invoice.productDesc, invoice.qty,
                    invoice.price, invoice.currCode, invoice.taxCode, invoice.priceUnitCode,
                    invoice.startDate, invoice.endDate, invoice.reportDate, strDateNow, sip.userGuid, sip.userID, invoice.crossCompInv ,"0");

                sbSql.AppendFormat(strSqlUpdateStatus, invoice.itemGuid);
            }
            else
            {
                endOfStatus = invoice.status.Substring(invoice.status.Length - 2, 2);
                // 如果状态以ed/ing结尾，并且状态不为estimated时做Update
                if (("ed".Equals(endOfStatus) || "ng".Equals(endOfStatus)) && !"estimated".Equals(invoice.status))
                {
                    sbSql.AppendFormat(strSqlUpdate, invoice.contract, invoice.costCenterCode, invoice.productCode, invoice.productDesc,
                        invoice.qty, invoice.price, invoice.currCode, invoice.taxCode, invoice.priceUnitCode,
                        invoice.startDate, invoice.endDate, invoice.reportDate, strDateNow, sip.userGuid, sip.userID, invoice.status,
                        invoice.processGuid);
                }
            }
            return sbSql.ToString();
        }

        /// <summary>
        /// 根据Company取得开票区间
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<SalesPeriod> SearchPeriod(string company, string sDate, string eDate)
        {
            List<SalesPeriod> lstSalesPeriod = new List<SalesPeriod>();
            List<SalesPeriod> lstTemp = new List<SalesPeriod>();

            // SQL文
            string strSqlSalesPeriod = " SELECT DISTINCT " +
                                              " P.PERIODGUID " +
                                            " , P.HEADGUID " +
                                            " , P.COMPANYCODE " +
                                            " , M.OWNERCOMPANYCODE " +
                                            " , M.COMPANYCODE AS INVCOMPANYCODE " +
                                            " , M.CROSSCOMPINV " +
                                            " , P.COMPANYNAME_ZH " +
                                            " , P.COMPANYNAME_EN " +
                                            " , P.CONTRACT " +
                                            " , P.CMPCODE " +
                                            " , P.CUSTOMCODE " +
                                            " , P.CUSTOMNAME_ZH " +
                                            " , P.CUSTOMNAME_EN " +
                                            " , CONVERT(VARCHAR(10),P.STARTDATE,120) AS STARTDATE " +
                                            " , CONVERT(VARCHAR(10),P.ENDDATE,120) AS ENDDATE " +
                                            " , CONVERT(VARCHAR(10),P.CREATEDATE,120) AS CREATEDATE " +
                                            " , CONVERT(VARCHAR(10),P.REPORTDATE, 120) AS REPORTDATE " +
                                            " , P.USERGUID " +
                                            " , P.USERID " +
                                            " , CONVERT(VARCHAR(10),P.CHANGEDATE,120) AS CHANGEDATE " +
                                            " , P.CHANGEUSERGUID " +
                                            " , P.CHANGEUSERID " +
                                            " , P.STARTDATE AS SORTDATE " +
                                            " , ISNULL(I.STATUS, '0') AS STATUS " +
                                            " , CASE WHEN ISNULL(I.STATUS,'0') IN ('0', 'estimated') THEN '1' ELSE '0' END AS CHANGETYPE " +
                                            " , ISNULL(I.TOBEINVOICED, '0') AS TOBEINVOICED " +
                                            " , I.TOTALAMOUNT " +
                                            " , (CASE WHEN I.GUID IS NULL THEN '0' ELSE '1' END) AS WITHLOG " +
                                            " , I.COSTCENTERCODE " +
                                            " , I.GROUPGUID " +
                                            " , I.ITEMCOUNT " +
                                         " FROM SALESPERIOD P (NOLOCK) " +
                                         " LEFT JOIN ( SELECT S.PERIODGUID " +
                                                          " , (CASE WHEN S.STATUS = 'reverseestimating' THEN 'invoicing'  " +
                                                                  " WHEN S.STATUS = 'reverseestimated' THEN 'invoiced' " +
                                                                  " ELSE S.STATUS END) STATUS " +
                                                          " , MAX(CASE S.TOBEINVOICED WHEN '1' THEN '1' ELSE '0' END) TOBEINVOICED " +
                                                          " , SUM(CASE WHEN S.STATUS='9' OR (S.STATUS = '0' AND S.TOBEINVOICED = 0) " +
                                                                     " THEN 0 ELSE ISNULL(S.QTY, 0) * ISNULL(S.PRICE, 0) + ISNULL(S.FINADJAMT, 0) END) TOTALAMOUNT " +
                                                          " , MAX(L.GUID) AS GUID " +
                                                          " , COSTCENTERCODE " +
                                                          " , GROUPGUID " +
                                                          " , COUNT(1) AS ITEMCOUNT " +
                                                       " FROM SALESINVOICE S (NOLOCK) " +
                                                       " LEFT JOIN ( SELECT DISTINCT GUID" +
                                                                        " , TABLENAME " +
                                                                     " FROM SALESINVOICELOG ) AS L " +
                                                         " ON S.STATUS LIKE '%ing' " +
                                                        " AND L.GUID = S.PROCESSGUID " +
                                                        " AND L.TABLENAME = 'SalesInvoice' " +
                                                      " GROUP BY PERIODGUID " +
                                                             " , CASE " +
                                                                   " WHEN STATUS = 'reverseestimating' THEN 'invoicing' " +
                                                                   " WHEN STATUS = 'reverseestimated' THEN 'invoiced' " +
                                                                   " ELSE STATUS " +
                                                                " END " +
                                                             " , COSTCENTERCODE " +
                                                             " , GROUPGUID ) AS I  " +
                                           " ON P.PERIODGUID = I.PERIODGUID " +
                                         " LEFT JOIN SALESORDERHEAD M " +
                                           " ON M.HEADGUID = P.HEADGUID " +
                                          " AND M.STATUS <> '0' " +
                                        " WHERE M.OWNERCOMPANYCODE = '" + company + "' " +
                                          " AND P.STARTDATE <= '" + eDate + "'" +
                                          " AND P.ENDDATE >= '" + sDate + "' ";
                                        //" ORDER BY P.HEADGUID, P.STARTDATE ";

            lstSalesPeriod = SqlServerHelper.GetEntityList<SalesPeriod>(SqlServerHelper.salesorderConn(), strSqlSalesPeriod);

            lstSalesPeriod = (from salesPeriod in lstSalesPeriod
                              group salesPeriod by new { salesPeriod.periodGuid, salesPeriod.status, salesPeriod.groupGuid } into lq
                              let first = lq.FirstOrDefault()
                              select new SalesPeriod
                              {
                                  periodGuid = first.periodGuid,
                                  headGuid = first.headGuid,
                                  companyCode = first.companyCode,
                                  ownerCompanyCode = first.ownerCompanyCode,
                                  invCompanyCode = first.invCompanyCode,
                                  crossCompInv = first.crossCompInv,
                                  companyName_ZH = first.companyName_ZH,
                                  companyName_EN = first.companyName_EN,
                                  contract = first.contract,
                                  cmpCode = first.cmpCode,
                                  customCode = first.customCode,
                                  customName_ZH = first.customName_ZH,
                                  customName_EN = first.customName_EN,
                                  startDate = first.startDate,
                                  endDate = first.endDate,
                                  createDate = first.createDate,
                                  reportDate = first.reportDate,
                                  userGuid = first.userGuid,
                                  userID = first.userID,
                                  changeDate = first.changeDate,
                                  changeUserGuid = first.changeUserGuid,
                                  changeUserID = first.changeUserID,
                                  status = first.status,
                                  changeType = first.changeType,
                                  tobeInvoiced = lq.Max(r => r.tobeInvoiced),
                                  totalAmount = (decimal)lq.Sum(r => r.totalAmount),
                                  withLog = first.withLog,
                                  costCenterCode = string.Join(",", lq.Select(r => r.costCenterCode).Distinct().ToArray()),
                                  groupGuid = first.groupGuid,
                                  itemCount = lq.Sum(r => r.itemCount)
                              } into lines
                              select lines).OrderBy(r => r.contract).ThenBy(r=>r.startDate).ToList();

            // 用于取得Period的Status的Count数
            lstTemp = (from c in lstSalesPeriod
                      group c by c.periodGuid into g
                      let first = g.FirstOrDefault()
                      select new SalesPeriod
                      {
                          periodGuid = first.periodGuid,
                          itemCount = g.Sum(r => r.itemCount),
                          headGuid = first.headGuid,
                          startDate = first.startDate,
                          endDate = first.endDate,
                          status = g.Min(r => r.status)
                      } into lines
                      select lines).ToList();

            lstTemp = lstTemp.Where(r => !"0".Equals(r.status)).ToList();

            if (lstTemp.Count > 0)
            {
                string strHeadGuid = string.Join("','", lstTemp.Select(r => r.headGuid).Distinct().ToArray());
                strHeadGuid = "('" + strHeadGuid + "')";

                string strSqlCount = " SELECT L.HEADGUID " +
                                          " , CONVERT(VARCHAR(10), (CASE WHEN H.VALIDDATE IS NOT NULL " +
                                                                        " AND H.VALIDDATE > L.STARTDATE " +
                                                                       " THEN H.VALIDDATE " +
                                                                       " ELSE L.STARTDATE END), 120) AS STARTDATE " +
                                          " , CONVERT(VARCHAR(10), (CASE WHEN H.EXPIRYDATE IS NOT NULL " +
                                                                        " AND H.EXPIRYDATE < L.ENDDATE " +
                                                                       " THEN H.EXPIRYDATE " +
                                                                       " ELSE L.ENDDATE END), 120) AS ENDDATE " +
                                          " , CONVERT(VARCHAR(10), L.CREATEDATE, 120) AS CREATEDATE " +
                                       " FROM SALESORDERITEM L " +
                                      " INNER JOIN SALESORDERHEAD H " +
                                         " ON H.HEADGUID = L.HEADGUID " +
                                        " AND H.STATUS <> '0' " +
                                      " WHERE L.HEADGUID IN " + strHeadGuid +
                                        //" AND ISNULL(L.QTY, 0) * ISNULL(L.PRICE, 0) <> 0 " +
                                        " AND L.STATUS <> '0' ";

                List<SalesOrderItem> lstOrderItem = SqlServerHelper.GetEntityList<SalesOrderItem>(SqlServerHelper.salesorderConn(), strSqlCount);
                List<SalesOrderItem> lstWork = new List<SalesOrderItem>();

                int intCount = 0;
                DateTime maxCreateDate = new DateTime();
                SalesPeriod first = new SalesPeriod();
                SalesPeriod periodObj = new SalesPeriod();

                // 新系统上线日期
                DateTime onlineDate = Convert.ToDateTime("2017-12-21");

                foreach (SalesPeriod temp in lstTemp)
                {
                    lstWork = lstOrderItem.Where(r => temp.headGuid.Equals(r.headGuid) &&
                                             DateTime.Compare(Convert.ToDateTime(r.startDate), Convert.ToDateTime(temp.endDate)) <= 0 &&
                                             DateTime.Compare(Convert.ToDateTime(r.endDate), Convert.ToDateTime(temp.startDate)) >= 0).ToList();
                    intCount = lstWork.Count();
                    maxCreateDate = Convert.ToDateTime(lstWork.Max(r => r.createDate));

                    // 只有在新系统上线后追加的SalesOrderItem才考虑做追加处理
                    if (DateTime.Compare(maxCreateDate, onlineDate) < 0)
                        continue;

                    if (intCount > temp.itemCount)
                    {
                        first = lstSalesPeriod.FirstOrDefault(r => temp.periodGuid.Equals(r.periodGuid));

                        if (first != null)
                        {
                            periodObj = new SalesPeriod();

                            periodObj.periodGuid = first.periodGuid;
                            periodObj.headGuid = first.headGuid;
                            periodObj.companyCode = first.companyCode;
                            periodObj.ownerCompanyCode = first.ownerCompanyCode;
                            periodObj.invCompanyCode = first.invCompanyCode;
                            periodObj.crossCompInv = first.crossCompInv;
                            periodObj.companyName_ZH = first.companyName_ZH;
                            periodObj.companyName_EN = first.companyName_EN;
                            periodObj.contract = first.contract;
                            periodObj.cmpCode = first.cmpCode;
                            periodObj.customCode = first.customCode;
                            periodObj.customName_ZH = first.customName_ZH;
                            periodObj.customName_EN = first.customName_EN;
                            periodObj.startDate = first.startDate;
                            periodObj.endDate = first.endDate;
                            periodObj.createDate = first.createDate;
                            periodObj.reportDate = first.reportDate;
                            periodObj.userGuid = first.userGuid;
                            periodObj.userID = first.userID;
                            periodObj.changeDate = first.changeDate;
                            periodObj.changeUserGuid = first.changeUserGuid;
                            periodObj.changeUserID = first.changeUserID;
                            periodObj.changeType = "1";
                            periodObj.status = "0";
                            periodObj.costCenterCode = string.Empty;
                            periodObj.groupGuid = null;
                            periodObj.tobeInvoiced = "0";
                            periodObj.totalAmount = (decimal)0;
                            periodObj.withLog = string.Empty;

                            lstSalesPeriod.Add(periodObj);
                        }
                    }
                }

                lstSalesPeriod = lstSalesPeriod.OrderBy(r => r.contract).ThenBy(r => r.startDate).ToList();
            }

            return lstSalesPeriod;
        }

        /// <summary>
        /// 更新Status
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public int UpdatePeriodStatus(SalesPeriodUpdateParam spup)
        {
            StringBuilder sbSql = new StringBuilder();

            sbSql.Append(" UPDATE SALESPERIOD ");
            sbSql.Append("    SET STATUS = '" + spup.status + "' ");
            if(!string.IsNullOrWhiteSpace(spup.reportDate))
                sbSql.Append("  , REPORTDATE = '" + spup.reportDate + "' ");
            sbSql.Append("  WHERE PERIODGUID = '" + spup.periodGuid + "' ");

            return SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSql.ToString());
        }

        /// <summary>
        /// 保存SalesInvoice
        /// </summary>
        /// <param name="lstSalesInvoice"></param>
        public int SaveSalesInvoice(SalesInvoiceSaveParam sisp)
        {
            //执行结果
            int retCd = 0;
            // 去掉勾选时
            string strSqlCancel = " UPDATE SALESINVOICE " +
                                     " SET ADJAMT = {0} " +
                                       " , FINADJAMT = {1} " +
                                       " , QTY = {2} " +
                                       " , CHANGEDATE = '{3}' " +
                                       " , CHANGEUSERGUID = '{4}' " +
                                       " , CHANGEUSERID = '{5}' " +
                                       " , TOBEINVOICED = '0' " +
                                   " WHERE PROCESSGUID = '{6}' ";

            // 去掉勾选时（暂估）
            string strSqlCancelEstimated = " UPDATE SALESINVOICE " +
                                              " SET FINADJAMT = {0} " +
                                                " , QTY = {1} " +
                                                " , CHANGEDATE = '{2}' " +
                                                " , CHANGEUSERGUID = '{3}' " +
                                                " , CHANGEUSERID = '{4}' " +
                                                " , TOBEINVOICED = '0' " +
                                            " WHERE PROCESSGUID = '{5}' ";

            // 正常勾选时
            string strSqlSelect = " UPDATE SALESINVOICE " +
                                     " SET ADJAMT = {0} " +
                                       " , FINADJAMT = {0} " +
                                       " , QTY = {1} " +
                                       " , ESTIMATEDREPORTDATE = '{2}' " + 
                                       " , REPORTDATE='{2}' " +
                                       " , CHANGEDATE = '{3}' " +
                                       " , CHANGEUSERGUID = '{4}' " +
                                       " , CHANGEUSERID = '{5}' " +
                                       " , TOBEINVOICED = '1' " +
                                   " WHERE PROCESSGUID = '{6}' ";
            // 已经暂估时用
            string strSqlEstimated = " UPDATE SALESINVOICE " +
                                        " SET FINADJAMT = {0} " +
                                          " , QTY = {1} " +
                                          " , REPORTDATE = '{2}' " +
                                          " , CHANGEDATE = '{3}' " +
                                          " , CHANGEUSERGUID = '{4}' " +
                                          " , CHANGEUSERID = '{5}' " +
                                          " , TOBEINVOICED = '1' " +
                                      " WHERE PROCESSGUID = '{6}' ";

            // 终止时用
            string strSqlClose = " UPDATE SALESINVOICE " +
                                    " SET ADJAMT = 0 " +
                                      " , FINADJAMT = 0 " +
                                      " , QTY = {0} " +
                                      " , REPORTDATE = NULL " +
                                      " , ESTIMATEDREPORTDATE = NULL " +
                                      " , CHANGEDATE = '{1}' " +
                                      " , CHANGEUSERGUID = '{2}' " +
                                      " , CHANGEUSERID = '{3}' " +
                                      " , TOBEINVOICED = '0' " +
                                      " , STATUS = '9' " +
                                  " WHERE PROCESSGUID = '{4}' ";

            StringBuilder sbSqlFinal = new StringBuilder();
            StringBuilder sbSqlwork = new StringBuilder();
            string strDateNow = Common.convertDateTime(DateTime.Now);
            string endOfStatus = string.Empty;
            List<SalesInvoice> lstInsertLog = new List<SalesInvoice>();

            foreach (SalesInvoice invoice in sisp.lines)
            {
                // Initial
                sbSqlwork = new StringBuilder();

                // 当非勾选时，还原Status和调整金额
                if (("0".Equals(invoice.status) || "estimated".Equals(invoice.status)) && !invoice.tobeInvoiced)
                {
                    if ("estimated".Equals(invoice.status))
                        sbSqlwork.AppendFormat(strSqlCancelEstimated, invoice.finAdjAmt, invoice.qty, strDateNow, sisp.userGuid,
                                sisp.userID, invoice.processGuid);
                    else
                        sbSqlwork.AppendFormat(strSqlCancel, invoice.adjAmt, invoice.finAdjAmt, invoice.qty, strDateNow, sisp.userGuid,
                                sisp.userID, invoice.processGuid);
                    sbSqlFinal.Append(sbSqlwork.ToString());
                    // 追加至Log追加准备中
                    lstInsertLog.Add(invoice);
                    continue;
                }
                // 新记录勾选时
                if("0".Equals(invoice.status))
                {
                    sbSqlwork.AppendFormat(strSqlSelect, invoice.adjAmt, invoice.qty, invoice.reportDate, strDateNow, sisp.userGuid,
                                                sisp.userID, invoice.processGuid);
                    sbSqlFinal.Append(sbSqlwork.ToString());
                    // 追加至Log追加准备中
                    lstInsertLog.Add(invoice);
                    continue;
                }
                // 取得Status末两位
                endOfStatus = invoice.status.Length > 2 ? invoice.status.Substring(invoice.status.Length - 2, 2): invoice.status;
                // 如果状态以ed/ing结尾，并且状态不为estimated时做Update
                if (("ed".Equals(endOfStatus) || "ng".Equals(endOfStatus)) && !"estimated".Equals(invoice.status))
                    continue;
                else
                {
                    // 已暂估记录勾选时
                    if ("estimated".Equals(invoice.status.ToLower())) 
                        sbSqlwork.AppendFormat(strSqlEstimated, invoice.finAdjAmt, invoice.qty, invoice.reportDate, strDateNow, sisp.userGuid,
                                                sisp.userID, invoice.processGuid);
                    // 状态为终止，且勾选时
                    if ("9".Equals(invoice.status) && invoice.tobeInvoiced)
                        sbSqlwork.AppendFormat(strSqlClose, invoice.qty, strDateNow, sisp.userGuid, sisp.userID, invoice.processGuid);

                    sbSqlFinal.Append(sbSqlwork.ToString());
                    // 追加至Log追加准备中
                    lstInsertLog.Add(invoice);
                }
            }
            if (sbSqlFinal != null && !string.IsNullOrWhiteSpace(sbSqlFinal.ToString()))
            {
                // 插入更新日志
                InsertSalesInvoiceUpdateLog(lstInsertLog, sisp.userID, sisp.userGuid);
                // 执行Insert方法
                retCd = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());
            }
            return retCd;
        }

        /// <summary>
        /// 设置\清楚分组开票Guid
        /// </summary>
        /// <param name="sisp"></param>
        /// <returns></returns>
        public int SplitInvoice(SalesInvoiceSaveParam sisp)
        {
            int result = -1;

            string guid = Guid.NewGuid().ToString().ToUpper();
            string strDateNow = Common.convertDateTime(DateTime.Now);

            string strSqlUpdate = " UPDATE SALESINVOICE " +
                                     " SET GROUPGUID = '{0}' " +
                                       " , CHANGEDATE = '{1}' " +
                                       " , CHANGEUSERGUID = '{2}' " +
                                       " , CHANGEUSERID = '{3}' " +
                                   " WHERE PROCESSGUID = '{4}' ";

            string strSqlClear = " UPDATE SALESINVOICE " +
                                     " SET GROUPGUID = NULL " +
                                       " , CHANGEDATE = '{0}' " +
                                       " , CHANGEUSERGUID = '{1}' " +
                                       " , CHANGEUSERID = '{2}' " +
                                   " WHERE PROCESSGUID = '{3}' ";

            StringBuilder sbSqlFinal = new StringBuilder();

            foreach(SalesInvoice iv in sisp.lines)
            {
                if (sisp.split)
                    sbSqlFinal.Append(string.Format(strSqlUpdate, guid, strDateNow, sisp.userGuid, sisp.userID, iv.processGuid));
                else
                    sbSqlFinal.Append(string.Format(strSqlClear, strDateNow, sisp.userGuid, sisp.userID, iv.processGuid));
            }

            if (string.Empty.Equals(sbSqlFinal.ToStringTrim()))
                return 0;

            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());
            return result;
        }

        /// <summary>
        /// 导入ERP前将状态置为ing状态
        /// </summary>
        /// <param name="rtie"></param>
        /// <returns></returns>
        public int ReadyToImportErp(ReadyToImportErp rtie)
        {
            // 正常开票或暂估状态
            string strStatusNoramal = String.Empty;
            // 反冲状态
            string strStatusSpecial = "reverseestimating";

            switch (rtie.processType)
            {
                case "Invoice":
                    strStatusNoramal = "invoicing";
                    break;
                case "EstimationEntry":
                    strStatusNoramal = "estimating";
                    break;
            }
            // 最终Sql
            StringBuilder sbSqlFinal = new StringBuilder();
            // 临时用
            string strSql = String.Empty;
            // periodGuid集合条件
            string strPeriodGuid = String.Empty;
            // SalesInvoice表状态更新Sql
            string strSqlUpdateStatus = String.Empty;
            // 执行结果条数
            int intResult = 0;

            // 拼接Where条件
            strPeriodGuid = string.Join("','", rtie.periodLine.Select(r => r.periodGuid).Distinct().ToArray());
            strPeriodGuid = "('" + strPeriodGuid + "')";

            List<SalesInvoice> lstSalesInvoice = new List<SalesInvoice>();

            // 根据开票区间Guid取得SalesInvoice集合(只取打勾且状态为'0'或'estimated'的记录)
            strSql = " SELECT PROCESSGUID " +
                          " , STATUS " +
                       " FROM SALESINVOICE " +
                      " WHERE TOBEINVOICED = '1' " +
                        " AND STATUS IN ('0', 'estimated') " + 
                        " AND PERIODGUID IN " + strPeriodGuid;

            lstSalesInvoice = SqlServerHelper.GetEntityList<SalesInvoice>(SqlServerHelper.salesorderConn(), strSql);
            
            if (lstSalesInvoice.Count == 0)
                return 0;

            // 取得当前时间
            string strDateNow = Common.convertDateTime(DateTime.Now);
            // 状态变量
            string strStatus = String.Empty;

            // SalesInvoice的状态更新Sql文
            strSqlUpdateStatus = " UPDATE SALESINVOICE " +
                                    " SET STATUS = '{0}' " +
                                      " , TOBEINVOICED = '0' " +
                                      " , CHANGEDATE = '{1}' " +
                                      " , CHANGEUSERGUID = '{2}' " +
                                      " , CHANGEUSERID = '{3}' " +
                                  " WHERE PROCESSGUID = '{4}' ";

            // 生成SalesInvoice的状态更新Sql文
            foreach (SalesInvoice si in lstSalesInvoice)
            {
                if (!"estimated".Equals(si.status) && !"0".Equals(si.status))
                    continue;
                if ("estimated".Equals(si.status))
                {
                    // 已暂估的记录不能再做暂估处理
                    if ("EstimationEntry".ToLower().Equals(rtie.processType.ToLower()))
                        continue;
                    strStatus = strStatusSpecial;
                }
                else
                    strStatus = strStatusNoramal;
                // 追加Update SQL文
                sbSqlFinal.AppendFormat(strSqlUpdateStatus, strStatus, strDateNow, rtie.userGuid, rtie.userID, si.processGuid);
            }

            // 执行Update SQL文
            if(sbSqlFinal != null && !string.IsNullOrWhiteSpace(sbSqlFinal.ToString()))
                intResult = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());
            return intResult;
        }

        /// <summary>
        /// 删除SalesInvoiceItem
        /// </summary>
        /// <param name="lstSalesInvoice">by Angel Jiang</param>
        public int CloseSalesInvoice(SalesInvoiceSaveParam sisp)
        {
            string strSqlUpdate1 = " UPDATE SALESINVOICE " +
                                        "set STATUS = '{0}' " +
                                        " , CHANGEDATE = '{1}' " +
                                        " , CHANGEUSERGUID = '{2}' " +
                                        " , CHANGEUSERID = '{3}' " +
                                    " WHERE PROCESSGUID = '{4}' ";

            StringBuilder sbSqlFinal = new StringBuilder();
            StringBuilder sbSqlwork = new StringBuilder();
            string strDateNow = Common.convertDateTime(DateTime.Now);

            foreach (SalesInvoice invoice in sisp.lines)
            {
                sbSqlwork = new StringBuilder();

                sbSqlwork.AppendFormat(strSqlUpdate1, '9', strDateNow, sisp.userGuid,
                    sisp.userID, invoice.processGuid);

                sbSqlFinal.Append(sbSqlwork.ToString());
            }

            // 执行Insert方法
            return SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());
        }

        /// <summary>
        /// 回退SalesInvoice
        /// </summary>
        /// <param name="lstSalesInvoice"></param>
        public int DelSalesInvoice(SalesInvoiceSaveParam sisp)
        {
            //string strSqlUpdate1 = " UPDATE SALESINVOICE " +
            //                          " SET STATUS = '{0}' " +
            //                        " WHERE PROCESSGUID = '{1}' ";

            string strSqlUpdate2 = " UPDATE SALESINVOICE " +
                                      " SET STATUS = '{0}' " +
                                        " , CHANGEDATE = '{1}' " +
                                        " , CHANGEUSERGUID = '{2}' " +
                                        " , CHANGEUSERID = '{3}' " +
                                    " WHERE PROCESSGUID = '{4}' ";

            StringBuilder sbSqlFinal = new StringBuilder();
            StringBuilder sbSqlwork = new StringBuilder();
            string strDateNow = Common.convertDateTime(DateTime.Now);

            foreach (SalesInvoice invoice in sisp.lines)
            {
                sbSqlwork = new StringBuilder();

                if (invoice.tobeInvoiced)
                    sbSqlwork.AppendFormat(strSqlUpdate2, "0", strDateNow, sisp.userGuid,
                        sisp.userID, invoice.processGuid);
                sbSqlFinal.Append(sbSqlwork.ToString());
            }
            // 执行Insert方法
            return SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());
        }

        /// <summary>
        /// 更新SalesInvoiceUpdateLog表
        /// </summary>
        /// <param name="lstSalesInvoice"></param>
        /// <returns></returns>
        private int InsertSalesInvoiceUpdateLog(List<SalesInvoice> lstSalesInvoice, string userId, string userGuid)
        {
            // 拼接Where条件
            string strProcessGuid = string.Join("','", lstSalesInvoice.Select(r => r.processGuid).Distinct().ToArray());
            strProcessGuid = "('" + strProcessGuid + "')";

            string strSql = " SELECT PROCESSGUID " +
                                 " , TOBEINVOICED " +
                                 " , ADJAMT " +
                                 " , FINADJAMT " +
                                 " , CONVERT(VARCHAR(10), REPORTDATE, 120) AS REPORTDATE " +
                                 " , STATUS " +
                              " FROM SALESINVOICE " +
                             " WHERE PROCESSGUID IN " + strProcessGuid;
            List<SalesInvoice> lstCompare = new List<SalesInvoice>();
            lstCompare = SqlServerHelper.GetEntityList<SalesInvoice>(SqlServerHelper.salesorderConn(), strSql);

            if (lstCompare.Count == 0)
                return 0;

            string strSqlUpdate = " INSERT SALESINVOICEUPDATELOG " +
                                       " ( GUID " +
                                       " , TABLENAME " +
                                       " , FIELDNAME " +
                                       " , KEYID " +
                                       " , OLDVALUE " +
                                       " , NEWVALUE " +
                                       " , CREATEDATE " +
                                       " , CREATEUSER " +
                                       " , CREATEUSERGUID ) " +
                                  " VALUES " +
                                       " ( '{0}' " +
                                       " , '{1}' " +
                                       " , '{2}' " +
                                       " , '{3}' " +
                                       " , '{4}' " +
                                       " , '{5}' " +
                                       " , '{6}' " +
                                       " , '{7}' " +
                                       " , '{8}' ) ";

            StringBuilder sbSqlUpdate = new StringBuilder();
            SalesInvoice siTemp = new SalesInvoice();
            string strGuid = String.Empty;
            string strDateNow = Common.convertDateTime(DateTime.Now);
            int insertCount = 0;

            foreach (SalesInvoice si in lstSalesInvoice)
            {
                // Guid
                strGuid = Guid.NewGuid().ToString().ToUpper();
                // 取得比较对象
                siTemp = lstCompare.FirstOrDefault(r => si.processGuid.Equals(r.processGuid));

                if (!si.tobeInvoiced.Equals(siTemp.tobeInvoiced))
                    sbSqlUpdate.AppendFormat(strSqlUpdate, strGuid, "SalesInvoice", "TobeInvoiced", si.processGuid, siTemp.tobeInvoiced
                        , si.tobeInvoiced, strDateNow, userId, userGuid);
                if(!si.status.Equals(siTemp.status))
                    sbSqlUpdate.AppendFormat(strSqlUpdate, strGuid, "SalesInvoice", "Status", si.processGuid, siTemp.status
                        , si.status, strDateNow, userId, userGuid);
                if (!si.qty.Equals(siTemp.qty))
                    sbSqlUpdate.AppendFormat(strSqlUpdate, strGuid, "SalesInvoice", "Qty", si.processGuid, siTemp.qty
                        , si.qty, strDateNow, userId, userGuid);
                if (!"estimated".Equals(si.status) && !si.adjAmt.Equals(siTemp.adjAmt))
                    sbSqlUpdate.AppendFormat(strSqlUpdate, strGuid, "SalesInvoice", "AdjAmt", si.processGuid, siTemp.adjAmt
                        , si.adjAmt, strDateNow, userId, userGuid);
                if (!si.finAdjAmt.Equals(siTemp.finAdjAmt))
                    sbSqlUpdate.AppendFormat(strSqlUpdate, strGuid, "SalesInvoice", "FinAdjAmt", si.processGuid, siTemp.finAdjAmt
                        , si.finAdjAmt, strDateNow, userId, userGuid);
                if (!si.reportDate.Equals(siTemp.reportDate))
                    sbSqlUpdate.AppendFormat(strSqlUpdate, strGuid, "SalesInvoice", "ReportDate", si.processGuid, siTemp.reportDate
                        , si.reportDate, strDateNow, userId, userGuid);
            }
            if (sbSqlUpdate != null && !string.IsNullOrWhiteSpace(sbSqlUpdate.ToString()))
                insertCount = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlUpdate.ToString());

            return insertCount;
        }

        /// <summary>
        /// 取得SDK交易报错日志
        /// </summary>
        /// <returns></returns>
        public List<TransLog> GetTransLog(SalesPeriod param)
        {
            string strSql = " SELECT L.TABLENAME " +
                                 " , L.GUID " +
                                 " , L.FUNCNAME " +
                                 " , L.ERRORMESSAGE " +
                                 " , MAX(CONVERT(VARCHAR(20), L.DATETIME, 120)) AS DATETIME " +
                              " FROM SALESINVOICELOG AS L " +
                             " INNER JOIN SALESINVOICE AS I " +
                                " ON I.PROCESSGUID = L.GUID " +
                             " WHERE I.PERIODGUID = '{0}' " +
                               " AND I.STATUS LIKE '%ing' " +
                          " GROUP BY L.GUID " +
                                 " , L.TABLENAME " +
                                 " , L.FUNCNAME " +
                                 " , L.ERRORMESSAGE ";

            List<TransLog> lstTransLog = new List<TransLog>();

            lstTransLog = SqlServerHelper.GetEntityList<TransLog>(SqlServerHelper.salesorderConn(), string.Format(strSql, param.periodGuid));
            lstTransLog = lstTransLog.OrderByDescending(r => r.dateTime).ToList();

            return lstTransLog;
        }

        /// <summary>
        /// 根据传入的ItemGuid取得生成SalesInvoice的最大EndDate
        /// </summary>
        /// <param name="itemGuid">ItemGuid</param>
        /// <returns></returns>
        public string GetSalesInvoiceMaxDate(string itemGuid)
        {
            string strEndDate = string.Empty;
            string strSql = " SELECT MAX(CONVERT(VARCHAR(10),ENDDATE,120)) AS ENDDATE " +
                              " FROM SALESINVOICE " +
                             " WHERE ITEMGUID = '" + itemGuid + "' " +
                             "   AND STATUS <> '9' ";

            List<SalesInvoice> lstSalesInvoice = SqlServerHelper.GetEntityList<SalesInvoice>(SqlServerHelper.salesorderConn(), strSql);

            if (lstSalesInvoice.Count > 0 && !string.IsNullOrWhiteSpace(lstSalesInvoice[0].endDate))
                strEndDate = lstSalesInvoice[0].endDate;

            return strEndDate;
        }

        /// <summary>
        /// 根据传入的ItemGuid取得生成SalesInvoice的最大EndDate
        /// </summary>
        /// <param name="headGuid">headGuid</param>
        /// <returns></returns>
        public string GetSalesPeriodMaxDate(string headGuid)
        {
            string strEndDate = string.Empty;
            string strSql = " SELECT MAX(CONVERT(VARCHAR(10),ENDDATE,120)) AS ENDDATE " +
                              " FROM SALESPERIOD " +
                             " WHERE HEADGUID = '" + headGuid + "' " +
                               " AND STATUS IN ('1', '2') ";

            List<SalesPeriod> lstSalesPeriod = SqlServerHelper.GetEntityList<SalesPeriod>(SqlServerHelper.salesorderConn(), strSql);

            if (lstSalesPeriod.Count > 0 && !string.IsNullOrWhiteSpace(lstSalesPeriod[0].endDate))
                strEndDate = lstSalesPeriod[0].endDate;

            return strEndDate;
        }
    }
}