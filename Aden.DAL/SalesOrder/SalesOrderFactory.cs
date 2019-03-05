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
using Aden.DAL.SalesOrder;


namespace Aden.DAL.SalesOrder
{
    public class SalesOrderFactory
    {
        /// <summary>
        /// 新增采购订单
        /// </summary>
        /// <param name="head">合同订单头</param>
        /// <param name="items">合同订单行</param>

        public string CreateSO(Model.SOMastData.SalesOrder so)
        {
            // Insert Head Sql文
            string strInsertHead = " INSERT SALESORDERHEAD " +
                                        " ( HEADGUID " +
                                        " , VERSION " +
                                        " , [CONTRACT] " +
                                        " , RECEIVED " +
                                        " , SIGNCOUNT " +
                                        " , OWNERCOMPANYCODE " +
                                        " , OWNERCOMPANYNAME_ZH " +
                                        " , OWNERCOMPANYNAME_EN " +
                                        " , COMPANYCODE " +
                                        " , COMPANYNAME_ZH " +
                                        " , COMPANYNAME_EN " +
                                        " , CMPCODE " + 
                                        " , CUSTOMCODE " +
                                        " , CUSTOMNAME_ZH " +
                                        " , CUSTOMNAME_EN " +
                                        " , CURRCODE " +
                                        " , CURRNAME_ZH " +
                                        " , CURRNAME_EN " +
                                        " , PAYMENTCODE " +
                                        " , PAYMENTNAME_ZH " +
                                        " , PAYMENTNAME_EN " +
                                        " , DEADLINE " +
                                        " , STARTDATE " +
                                        " , ENDDATE " +
                                        " , VALIDDATE " +
                                        " , REMARK " +
                                        " , USERGUID " +
                                        " , USERID " +
                                        " , CREATEDATE " +
                                        " , CHANGEDATE " +
                                        " , [STATUS] " +
                                        " , CROSSCOMPINV )" +
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
                                        " , '{20}' " +
                                        " , '{21}' " +
                                        " , '{22}' " +
                                        " , '{23}' " +
                                        " , '{24}' " +
                                        " , '{25}' " +
                                        " , '{26}' " +
                                        " , '{27}' " +
                                        " , '{28}' " +
                                        " , NULL " +
                                        " , '{29}'"+
                                        " , '{30}')";

            // Insert Item Sql文
            string strInsertItem = " INSERT SALESORDERITEM  " +
                                        " ( ITEMGUID " +
                                        " , HEADGUID " +
                                        " , ITEMNO " +
                                        " , VERSION " +
                                        " , [CONTRACT] " +
                                        " , COSTCENTERCODE " +
                                        " , INVOICECOSTCENTERCODE " +
                                        " , SERVICETYPE " +
                                        " , SERVICETYPEVALUE " +
                                        " , PRODUCTCODE " +
                                        " , PRODUCTDESC " +
                                        " , QTY " +
                                        " , PRICE " +
                                        " , PRICEUNITCODE " +
                                        " , TAXCODE " +
                                        " , TAXRATE " +
                                        " , STARTDATE " +
                                        " , ENDDATE " +
                                        " , REMARK " +
                                        " , CREATEDATE " +
                                        " , USERGUID " +
                                        " , USERID " +
                                        " , [STATUS] )" +
                                   " VALUES " +
                                        " ('{0}' " +
                                        " ,'{1}' " +
                                        " ,'{2}' " +
                                        " ,'{3}' " +
                                        " ,'{4}' " +
                                        " ,'{5}' " +
                                        " ,'{6}' " +
                                        " ,'{7}' " +
                                        " ,'{8}' " +
                                        " ,'{9}' " +
                                        " ,'{10}' " +
                                        " ,'{11}' " +
                                        " ,'{12}' " +
                                        " ,'{13}' " +
                                        " ,'{14}' " +
                                        " ,'{15}' " +
                                        " ,'{16}' " +
                                        " ,'{17}' " +
                                        " ,'{18}' " +
                                        " ,'{19}' " +
                                        " ,'{20}' " +
                                        " ,'{21}' " +
                                        " ,'{22}')";

            string strUpdateHead = " UPDATE SALESORDERHEAD " +
                                      " SET REBUILDGUID = '{0}' " +
                                    " WHERE HEADGUID = '{1}' " +
                                      " AND STATUS <> '0' ";

            // 取得Head Guid
            string headGuid = Guid.NewGuid().ToString().ToUpper();
            // Sql文
            StringBuilder strSql = new StringBuilder();

            // 单引号转义
            string ownerCompanyName_ZH = so.ownerCompanyName_ZH == null ? null : so.ownerCompanyName_ZH.Trim().Replace("'", "''");
            string ownerCompanyName_EN = so.ownerCompanyName_EN == null ? null : so.ownerCompanyName_EN.Trim().Replace("'", "''");
            string companyName_ZH = so.companyName_ZH == null ? null : so.companyName_ZH.Trim().Replace("'", "''");
            string companyName_EN = so.companyName_EN == null ? null : so.companyName_EN.Trim().Replace("'", "''");
            string customName_ZH = so.customName_ZH == null ? null : so.customName_ZH.Trim().Replace("'", "''");
            string customName_EN = so.customName_EN == null ? null : so.customName_EN.Trim().Replace("'", "''");

            // Inert Head Sql参数部分
            strSql.AppendFormat(strInsertHead, headGuid, so.version, so.contract, so.received, so.signCount
                , so.ownerCompanyCode, ownerCompanyName_ZH, ownerCompanyName_EN, so.companyCode, companyName_ZH
                , companyName_EN, so.cmpCode, so.customCode, customName_ZH, customName_EN, so.currCode, so.currName_ZH
                , so.currName_EN, so.paymentCode, so.paymentName_ZH, so.paymentName_EN, so.deadline
                , Common.convertDateTime(so.startDate), Common.convertDateTime(so.endDate, true), Common.convertDateTime(so.validDate)
                , so.remark, so.userGuid, so.userID, Common.convertDateTime(so.createDate),so.status,so.crossCompInv);

            // 取得服务类型主数据
            string serviceType = String.Empty;
            string serviceTypeValue = String.Empty;

            string strSqlServiceType = " SELECT * " +
                                         " FROM ServiceTypeData ";

            List<ServiceType> lstServiceType = SqlServerHelper.GetEntityList<ServiceType>(SqlServerHelper.salesorderConn(), strSqlServiceType);

            int itemNo = 0;

            // Insert Item Sql参数部分
            foreach (SalesOrderItem item in so.items)
            {
                // 变量初始化
                serviceType = String.Empty;
                serviceTypeValue = String.Empty;

                // 取得ServiceType和ServiceTypeValue(用于开票)
                Common.getServiceType(item.costCenterCode, ref serviceType, ref serviceTypeValue, lstServiceType);

                // 项次号每次递增100
                itemNo += 100;

                // Insert Item Sql文
                strSql.AppendFormat(strInsertItem, Guid.NewGuid().ToString().ToUpper(), headGuid, itemNo.ToString(), item.version, item.contract, 
                    item.costCenterCode, item.invoiceCostCenterCode, serviceType, serviceTypeValue, item.productCode , item.productDesc, item.qty, item.price, 
                    item.priceUnitCode, item.taxCode, item.taxRate / 100, Common.convertDateTime(item.startDate), Common.convertDateTime(item.endDate, true), item.remark, Common.convertDateTime(so.createDate), item.userGuid, item.userID, so.status);
            }
            // 执行Sql文
            SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), strSql.ToString());

            // 如果是续签的情况时，将当前HeadGuid写入参照的合同中
            if(!string.IsNullOrWhiteSpace(so.rebuildGuid))
                SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), string.Format(strUpdateHead, headGuid, so.rebuildGuid));

            return headGuid;
        }

        /// <summary>
        /// 更新SO head和SO items
        /// </summary>
        /// <param name="so">Sales Order对象</param>
        /// <param name="retSql">执行的Sql文</param>
        /// <param name="doExecuteFlag">是否需要执行Sql文的标志</param>
        /// <returns></returns>
        public int EditSO(Model.SOMastData.SalesOrder so, ref string retSql, bool doExecuteFlag = true)
        {
            // 设置SO建立时间
            string createDate = DateTime.Now.ToString();

            // Update Head Sql文
            string strUpdateHead = " UPDATE SALESORDERHEAD " +
                                      " SET STATUS = '{0}' " +
                                    " WHERE HEADGUID = '{1}' " +
                                      " AND STATUS <> '0' ";

            // Insert Head Sql文
            string strInsertHead = " INSERT SALESORDERHEAD " +
                                        " ( HEADGUID " +
                                        " , VERSION " +
                                        " , [CONTRACT] " +
                                        " , RECEIVED " +
                                        " , SIGNCOUNT " +
                                        " , OWNERCOMPANYCODE " +
                                        " , OWNERCOMPANYNAME_ZH " +
                                        " , OWNERCOMPANYNAME_EN " +
                                        " , COMPANYCODE " +
                                        " , COMPANYNAME_ZH " +
                                        " , COMPANYNAME_EN " +
                                        " , CMPCODE " +
                                        " , CUSTOMCODE " +
                                        " , CUSTOMNAME_ZH " +
                                        " , CUSTOMNAME_EN " +
                                        " , CURRCODE " +
                                        " , CURRNAME_ZH " +
                                        " , CURRNAME_EN " +
                                        " , PAYMENTCODE " +
                                        " , PAYMENTNAME_ZH " +
                                        " , PAYMENTNAME_EN " +
                                        " , DEADLINE " +
                                        " , STARTDATE " +
                                        " , ENDDATE " +
                                        " , VALIDDATE " +
                                        " , REMARK " +
                                        " , CROSSCOMPINV " +
                                        " , USERGUID " +
                                        " , USERID " +
                                        " , CREATEDATE " +
                                        " , CHANGEDATE " +
                                        " , CHANGEUSERGUID " +
                                        " , CHANGEUSERID " +
                                        " , [STATUS] " +
                                        " , GLADISID " +
                                        " , REBUILDGUID) " +
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
                                        " , '{20}' " +
                                        " , '{21}' " +
                                        " , '{22}' " +
                                        " , '{23}' " +
                                        " , '{24}' " +
                                        " , '{25}' " +
                                        " , '{26}' " +
                                        " , '{27}' " +
                                        " , '{28}' " +
                                        " , '{29}' " +
                                        " , '{30}' " +
                                        " , '{31}' " +
                                        " , '{32}' " +
                                        " , '{33}' " +
                                        " , '{34}' " +
                                        " , '{35}' ) ";

            // Insert Head Sql文
            string strInsertHead2 = " INSERT SALESORDERHEAD " +
                                         " ( HEADGUID " +
                                         " , VERSION " +
                                         " , [CONTRACT] " +
                                         " , RECEIVED " +
                                         " , SIGNCOUNT " +
                                         " , OWNERCOMPANYCODE " +
                                         " , OWNERCOMPANYNAME_ZH " +
                                         " , OWNERCOMPANYNAME_EN " +
                                         " , COMPANYCODE " +
                                         " , COMPANYNAME_ZH " +
                                         " , COMPANYNAME_EN " +
                                         " , CMPCODE " +
                                         " , CUSTOMCODE " +
                                         " , CUSTOMNAME_ZH " +
                                         " , CUSTOMNAME_EN " +
                                         " , CURRCODE " +
                                         " , CURRNAME_ZH " +
                                         " , CURRNAME_EN " +
                                         " , PAYMENTCODE " +
                                         " , PAYMENTNAME_ZH " +
                                         " , PAYMENTNAME_EN " +
                                         " , DEADLINE " +
                                         " , STARTDATE " +
                                         " , ENDDATE " +
                                         " , VALIDDATE " +
                                         " , REMARK " +
                                         " , CROSSCOMPINV " +
                                         " , USERGUID " +
                                         " , USERID " +
                                         " , CREATEDATE " +
                                         " , CHANGEDATE " +
                                         " , CHANGEUSERGUID " +
                                         " , CHANGEUSERID " +
                                         " , EXPIRYDATE" +
                                         " , [STATUS] " +
                                         " , GLADISID " +
                                         " , REBUILDGUID ) " +
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
                                         " , '{20}' " +
                                         " , '{21}' " +
                                         " , '{22}' " +
                                         " , '{23}' " +
                                         " , '{24}' " +
                                         " , '{25}' " +
                                         " , '{26}' " +
                                         " , '{27}' " +
                                         " , '{28}' " +
                                         " , '{29}' " +
                                         " , '{30}' " +
                                         " , '{31}' " +
                                         " , '{32}' " +
                                         " , '{33}' " +
                                         " , '{34}' " +
                                         " , '{35}' " +
                                         " , '{36}' ) ";

            // Update Item Sql文
            string strUpdateItem = " UPDATE SALESORDERITEM " +
                                      " SET STATUS = '{0}' " +
                                    " WHERE ITEMGUID = '{1}' " +
                                      " AND STATUS <> '0' ";

            // Insert Item Sql文
            string strInsertItem = " INSERT SALESORDERITEM  " +
                                        " ( ITEMGUID " +
                                        " , HEADGUID " +
                                        " , ITEMNO " +
                                        " , VERSION " +
                                        " , [CONTRACT] " +
                                        " , COSTCENTERCODE " +
                                        " , INVOICECOSTCENTERCODE " +
                                        " , SERVICETYPE " +
                                        " , SERVICETYPEVALUE " +
                                        " , PRODUCTCODE " +
                                        " , PRODUCTDESC " +
                                        " , QTY " +
                                        " , PRICE " +
                                        " , PRICEUNITCODE " +
                                        " , TAXCODE " +
                                        " , TAXRATE " +
                                        " , STARTDATE " +
                                        " , ENDDATE " +
                                        " , REMARK " +
                                        " , CREATEDATE " +
                                        " , USERGUID " +
                                        " , USERID " +
                                        " , CHANGEDATE " +
                                        " , CHANGEUSERGUID " +
                                        " , CHANGEUSERID " +
                                        " , EXPIRYDATE " +
                                        " , GLADISID" +
                                        " , [STATUS] ) " +
                                   " VALUES " +
                                        " ('{0}' " +
                                        " ,'{1}' " +
                                        " ,'{2}' " +
                                        " ,'{3}' " +
                                        " ,'{4}' " +
                                        " ,'{5}' " +
                                        " ,'{6}' " +
                                        " ,'{7}' " +
                                        " ,'{8}' " +
                                        " ,'{9}' " +
                                        " ,'{10}' " +
                                        " ,'{11}' " +
                                        " ,'{12}' " +
                                        " ,'{13}' " +
                                        " ,'{14}' " +
                                        " ,'{15}' " +
                                        " ,'{16}' " +
                                        " ,'{17}' " +
                                        " ,'{18}' " +
                                        " ,'{19}' " +
                                        " ,'{20}' " +
                                        " ,'{21}' " +
                                        " ,'{22}' " +
                                        " ,'{23}' " +
                                        " ,'{24}' " +
                                        " , NULL " +
                                        " ,'{25}' " +
                                        " ,'{26}') ";

            // Insert Item Sql文2
            string strInsertItem2 = " INSERT SALESORDERITEM  " +
                                        " ( ITEMGUID " +
                                        " , HEADGUID " +
                                        " , ITEMNO " +
                                        " , VERSION " +
                                        " , [CONTRACT] " +
                                        " , COSTCENTERCODE " +
                                        " , INVOICECOSTCENTERCODE " +
                                        " , SERVICETYPE " +
                                        " , SERVICETYPEVALUE " +
                                        " , PRODUCTCODE " +
                                        " , PRODUCTDESC " +
                                        " , QTY " +
                                        " , PRICE " +
                                        " , PRICEUNITCODE " +
                                        " , TAXCODE " +
                                        " , TAXRATE " +
                                        " , STARTDATE " +
                                        " , ENDDATE " +
                                        " , REMARK " +
                                        " , CREATEDATE " +
                                        " , USERGUID " +
                                        " , USERID " +
                                        " , GLADISID " +
                                        " , [STATUS] ) " +
                                   " VALUES " +
                                        " ('{0}' " +
                                        " ,'{1}' " +
                                        " ,'{2}' " +
                                        " ,'{3}' " +
                                        " ,'{4}' " +
                                        " ,'{5}' " +
                                        " ,'{6}' " +
                                        " ,'{7}' " +
                                        " ,'{8}' " +
                                        " ,'{9}' " +
                                        " ,'{10}' " +
                                        " ,'{11}' " +
                                        " ,'{12}' " +
                                        " ,'{13}' " +
                                        " ,'{14}' " +
                                        " ,'{15}' " +
                                        " ,'{16}' " +
                                        " ,'{17}' " +
                                        " ,'{18}' " +
                                        " ,'{19}' " +
                                        " ,'{20}' " +
                                        " ,'{21}' " +
                                        " ,'{22}' " +
                                        " ,'{23}') ";

            // Insert Item Sql文3
            string strInsertItem3 = " INSERT SALESORDERITEM  " +
                                         " ( ITEMGUID " +
                                         " , HEADGUID " +
                                         " , ITEMNO " +
                                         " , VERSION " +
                                         " , [CONTRACT] " +
                                         " , COSTCENTERCODE " +
                                         " , INVOICECOSTCENTERCODE " +
                                         " , SERVICETYPE " +
                                         " , SERVICETYPEVALUE " +
                                         " , PRODUCTCODE " +
                                         " , PRODUCTDESC " +
                                         " , QTY " +
                                         " , PRICE " +
                                         " , PRICEUNITCODE " +
                                         " , TAXCODE " +
                                         " , TAXRATE " +
                                         " , STARTDATE " +
                                         " , ENDDATE " +
                                         " , REMARK " +
                                         " , CREATEDATE " +
                                         " , USERGUID " +
                                         " , USERID " +
                                         " , CHANGEDATE " +
                                         " , CHANGEUSERGUID " +
                                         " , CHANGEUSERID" +
                                         " , EXPIRYDATE " +
                                         " , GLADISID " +
                                         " , [STATUS] ) " +
                                    " VALUES " +
                                         " ('{0}' " +
                                         " ,'{1}' " +
                                         " ,'{2}' " +
                                         " ,'{3}' " +
                                         " ,'{4}' " +
                                         " ,'{5}' " +
                                         " ,'{6}' " +
                                         " ,'{7}' " +
                                         " ,'{8}' " +
                                         " ,'{9}' " +
                                         " ,'{10}' " +
                                         " ,'{11}' " +
                                         " ,'{12}' " +
                                         " ,'{13}' " +
                                         " ,'{14}' " +
                                         " ,'{15}' " +
                                         " ,'{16}' " +
                                         " ,'{17}' " +
                                         " ,'{18}' " +
                                         " ,'{19}' " +
                                         " ,'{20}' " +
                                         " ,'{21}' " +
                                         " ,'{22}' " +
                                         " ,'{23}' " +
                                         " ,'{24}' " +
                                         " ,'{25}' " +
                                         " ,'{26}' " +
                                         " ,'{27}') ";

            // Sql文
            StringBuilder strSql = new StringBuilder();
            String nowDateTime = Common.convertDateTime(DateTime.Now.ToString());

            // 当Head有变化时执行UPDATE + INSERT
            if ("1".Equals(so.changeFlag))
            {
                // Update Head部分
                strSql.AppendFormat(strUpdateHead, "0", so.headGuid);
                // 单引号转义
                string ownerCompanyName_ZH = so.ownerCompanyName_ZH == null ? null : so.ownerCompanyName_ZH.Trim().Replace("'", "''");
                string ownerCompanyName_EN = so.ownerCompanyName_EN == null ? null : so.ownerCompanyName_EN.Trim().Replace("'", "''");
                string companyName_ZH = so.companyName_ZH == null ? null : so.companyName_ZH.Trim().Replace("'", "''");
                string companyName_EN = so.companyName_EN == null ? null : so.companyName_EN.Trim().Replace("'", "''");
                string customName_ZH = so.customName_ZH == null ? null : so.customName_ZH.Trim().Replace("'", "''");
                string customName_EN = so.customName_EN == null ? null : so.customName_EN.Trim().Replace("'", "''");

                // Insert Head 部分
                if ("9".Equals(so.status))
                {
                    // 终止表单
                    strSql.AppendFormat(strInsertHead2, so.headGuid, (int.Parse(so.version) + 1).ToString(), so.contract, 
                    so.received, so.signCount, so.ownerCompanyCode, ownerCompanyName_ZH, ownerCompanyName_EN, 
                    so.companyCode, companyName_ZH, companyName_EN, so.cmpCode, so.customCode, customName_ZH, customName_EN, 
                    so.currCode, so.currName_ZH, so.currName_EN, so.paymentCode, so.paymentName_ZH, so.paymentName_EN, so.deadline, 
                    Common.convertDateTime(so.startDate), Common.convertDateTime(so.endDate, true), Common.convertDateTime(so.validDate), 
                    so.remark, so.crossCompInv, so.userGuid, so.userID, Common.convertDateTime(so.createDate), nowDateTime, so.userGuid, 
                    so.userID, Common.convertDateTime(so.expiryDate), so.status, so.gladisID, so.rebuildGuid);
                }
                else
                {
                    // 非终止表单
                    strSql.AppendFormat(strInsertHead, so.headGuid, (int.Parse(so.version) + 1).ToString(), so.contract, 
                    so.received, so.signCount, so.ownerCompanyCode, ownerCompanyName_ZH, ownerCompanyName_EN, 
                    so.companyCode, companyName_ZH, companyName_EN, so.cmpCode, so.customCode, customName_ZH, customName_EN, 
                    so.currCode, so.currName_ZH, so.currName_EN, so.paymentCode, so.paymentName_ZH, so.paymentName_EN, so.deadline, 
                    Common.convertDateTime(so.startDate), Common.convertDateTime(so.endDate, true), Common.convertDateTime(so.validDate), 
                    so.remark, so.crossCompInv, so.userGuid, so.userID, Common.convertDateTime(so.createDate), nowDateTime, so.changeUserGuid, 
                    so.changeUserID, so.status, so.gladisID, so.rebuildGuid);
                }
            }

            if (so.items.Count > 0)
            {
                // 取得当前itemData中的项次号最大值
                string strTemp = so.items.Max(r => int.Parse(string.IsNullOrWhiteSpace(r.itemNo) ? "0" : r.itemNo)).ToString();
                int maxItemNo = int.Parse(string.IsNullOrWhiteSpace(strTemp) ? "100": strTemp);

                // 取得服务类型主数据
                string serviceType = String.Empty;
                string serviceTypeValue = String.Empty;

                string strSqlServiceType = " SELECT * " +
                                             " FROM ServiceTypeData ";

                List<ServiceType> lstServiceType = SqlServerHelper.GetEntityList<ServiceType>(SqlServerHelper.salesorderConn(), strSqlServiceType);

                // Insert Item Sql参数部分
                foreach (SalesOrderItem item in so.items)
                {
                    if (!"1".Equals(item.changeFlag) && item.itemGuid != null) continue;

                    // 变量初始化
                    serviceType = String.Empty;
                    serviceTypeValue = String.Empty;

                    // 取得ServiceType和ServiceTypeValue(用于开票)
                    Common.getServiceType(item.costCenterCode, ref serviceType, ref serviceTypeValue, lstServiceType);

                    if (!String.IsNullOrWhiteSpace(item.itemGuid))
                    {
                        // Update Item部分
                        strSql.AppendFormat(strUpdateItem, "0", item.itemGuid);

                        // Insert Item部分
                        if ("9".Equals(item.status))
                        {
                            strSql.AppendFormat(strInsertItem3, item.itemGuid.ToString().ToUpper(), item.headGuid.ToString().ToUpper(), 
                                item.itemNo ,(int.Parse(item.version) + 1).ToString(), item.contract, item.costCenterCode, item.invoiceCostCenterCode, 
                                serviceType, serviceTypeValue, item.productCode, item.productDesc, item.qty, item.price, item.priceUnitCode, item.taxCode, 
                                item.taxRate / 100, Common.convertDateTime(item.startDate), Common.convertDateTime(item.endDate, true), item.remark, 
                                Common.convertDateTime(item.createDate), item.userGuid, item.userID, nowDateTime, item.changeUserGuid, item.changeUserID, 
                                Common.convertDateTime(item.expiryDate), item.gladisID, item.status);
                        }
                        else
                        {
                            strSql.AppendFormat(strInsertItem, item.itemGuid.ToString().ToUpper(), item.headGuid.ToString().ToUpper(), item.itemNo, 
                                (int.Parse(item.version) + 1).ToString(), item.contract, item.costCenterCode, item.invoiceCostCenterCode, serviceType, 
                                serviceTypeValue, item.productCode, item.productDesc, item.qty, item.price, item.priceUnitCode, item.taxCode, item.taxRate / 100, 
                                Common.convertDateTime(item.startDate), Common.convertDateTime(item.endDate, true), item.remark, Common.convertDateTime(item.createDate), 
                                item.userGuid, item.userID, nowDateTime, item.changeUserGuid, item.changeUserID, item.gladisID, item.status);
                        }
                    }
                    else
                    {
                        // item项次每次递增100
                        maxItemNo += 100;
                        strSql.AppendFormat(strInsertItem2, Guid.NewGuid().ToString().ToUpper(), so.headGuid, maxItemNo.ToString(), 
                            item.version.ToString(), item.contract, item.costCenterCode, item.invoiceCostCenterCode, serviceType, serviceTypeValue, 
                            item.productCode, item.productDesc, item.qty, item.price, item.priceUnitCode, item.taxCode, item.taxRate / 100, 
                            Common.convertDateTime(item.startDate), Common.convertDateTime(item.endDate, true), item.remark, 
                            Common.convertDateTime(createDate), item.userGuid, item.userID, item.gladisID, item.status);
                    }
                }
            }

            int retCd = 0;

            // 执行Sql文
            if (doExecuteFlag) retCd = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), strSql.ToString());

            // Sql文结果
            retSql = strSql.ToString();

            return retCd;
        }

        /// <summary>
        /// 根据Guid取得Sales Order信息
        /// </summary>
        /// <param name="headGuid">SO head Guid</param>
        /// <param name="withItems">是否需要获取Items信息</param>
        /// <returns></returns>
        public Model.SOMastData.SalesOrder GetSO(string headGuid, bool withItems = true)
        {
            // SO Head部分取得
            string strSql = " SELECT HEADGUID " +
                                 " , VERSION " +
                                 " , [CONTRACT] " +
                                 " , RECEIVED " +
                                 " , SIGNCOUNT " +
                                 " , OWNERCOMPANYCODE " +
                                 " , OWNERCOMPANYNAME_ZH " +
                                 " , OWNERCOMPANYNAME_EN " +
                                 " , COMPANYCODE " +
                                 " , COMPANYNAME_ZH " +
                                 " , COMPANYNAME_EN " +
                                 " , CMPCODE " +
                                 " , CUSTOMCODE " +
                                 " , CUSTOMNAME_ZH " +
                                 " , CUSTOMNAME_EN " +
                                 " , CURRCODE " +
                                 " , CURRNAME_ZH " +
                                 " , CURRNAME_EN " +
                                 " , PAYMENTCODE " +
                                 " , PAYMENTNAME_ZH " +
                                 " , PAYMENTNAME_EN " +
                                 " , DEADLINE " +
                                 " , CONVERT(VARCHAR(100),STARTDATE,20) AS STARTDATE" +
                                 " , CONVERT(VARCHAR(100),ENDDATE,20) AS ENDDATE " +
                                 " , CONVERT(VARCHAR(100),VALIDDATE,20) AS VALIDDATE " +
                                 " , REMARK " +
                                 " , CROSSCOMPINV " +
                                 " , CONVERT(VARCHAR(100),CREATEDATE,20) AS CREATEDATE " +
                                 " , USERGUID " +
                                 " , USERID " +
                                 " , CONVERT(VARCHAR(100),CHANGEDATE,20) AS CHANGEDATE " +
                                 " , CHANGEUSERGUID " +
                                 " , CHANGEUSERID " +
                                 " , CONVERT(VARCHAR(100),EXPIRYDATE,20) AS EXPIRYDATE " +
                                 " , [STATUS] " +
                                 " , REBUILDGUID " +
                                 " , GLADISID " +
                              " FROM SALESORDERHEAD " +
                             " WHERE HEADGUID = @guid " +
                               " AND STATUS <> '0' ";

            List<Model.SOMastData.SalesOrder> lstHead = SqlServerHelper.GetEntityList<Model.SOMastData.SalesOrder>(SqlServerHelper.salesorderConn(), strSql, System.Data.CommandType.Text,
                new SqlParameter[] {
                    new SqlParameter("@guid", SqlDbType.NVarChar, 50) { Value = headGuid },
            });

            if (lstHead.Count == 0) return null;

            List<SalesOrderItem> lstItems = new List<SalesOrderItem>();

            if (withItems)
            {
                // SO item部分取得
                strSql = " SELECT ITEMGUID " +
                              " , HEADGUID " +
                              " , ITEMNO " +
                              " , VERSION " +
                              " , [CONTRACT] " +
                              " , COSTCENTERCODE " +
                              " , INVOICECOSTCENTERCODE " +
                              " , SERVICETYPE " +
                              " , SERVICETYPEVALUE " +
                              " , PRODUCTCODE " +
                              " , PRODUCTDESC " +
                              " , QTY " +
                              " , PRICE " +
                              " , PRICEUNITCODE " +
                              " , TAXCODE " +
                              " , CONVERT(VARCHAR(100),STARTDATE,20) AS STARTDATE " +
                              " , CONVERT(VARCHAR(100),ENDDATE,20) AS ENDDATE " +
                              " , REMARK " +
                              " , CONVERT(VARCHAR(100),CREATEDATE,20) AS CREATEDATE " +
                              " , USERGUID " +
                              " , USERID " +
                              " , CONVERT(VARCHAR(100),CHANGEDATE,20) AS CHANGEDATE " +
                              " , CHANGEUSERGUID " +
                              " , CHANGEUSERID " +
                              " , CONVERT(VARCHAR(100),EXPIRYDATE,20) AS EXPIRYDATE " +
                              " , [STATUS] " +
                              " , GLADISID " +
                           " FROM SALESORDERITEM " +
                          " WHERE HEADGUID = @guid " +
                            " AND STATUS <> '0' " +
                          " ORDER BY CAST(ITEMNO AS INT) ";

                lstItems = SqlServerHelper.GetEntityList<SalesOrderItem>(SqlServerHelper.salesorderConn(), strSql, System.Data.CommandType.Text,
                    new SqlParameter[] {
                    new SqlParameter("@guid", SqlDbType.NVarChar, 50) { Value = headGuid },
                });

                if (lstItems.Count == 0) return null;
            }

            Model.SOMastData.SalesOrder SO = new Model.SOMastData.SalesOrder();

            // Head部分设置
            SO = lstHead[0];
            // Item部分设置
            SO.items = lstItems;

            return SO;
        }

        /// <summary>
        /// 根据Guid取得Sales Order信息
        /// </summary>
        /// <param name="Guid"></param>
        /// <returns></returns>
        public List<Model.SOMastData.SalesOrder> QuerySO()
        {
            // SO Head部分取得
            string strSql = " SELECT HEADGUID " +
                                 " , VERSION " +
                                 " , [CONTRACT] " +
                                 " , RECEIVED " +
                                 " , SIGNCOUNT " +
                                 " , OWNERCOMPANYCODE " +
                                 " , OWNERCOMPANYNAME_ZH " +
                                 " , OWNERCOMPANYNAME_EN " +
                                 " , COMPANYCODE " +
                                 " , COMPANYNAME_ZH " +
                                 " , COMPANYNAME_EN " +
                                 " , CUSTOMCODE " +
                                 " , CUSTOMNAME_ZH " +
                                 " , CUSTOMNAME_EN " +
                                 " , CURRCODE " +
                                 " , CURRNAME_ZH " +
                                 " , CURRNAME_EN " +
                                 " , PAYMENTCODE " +
                                 " , PAYMENTNAME_ZH " +
                                 " , PAYMENTNAME_EN " +
                                 " , DEADLINE " +
                                 " , CONVERT(VARCHAR(100),STARTDATE,20) AS STARTDATE" +
                                 " , CONVERT(VARCHAR(100),ENDDATE,20) AS ENDDATE " +
                                 " , CONVERT(VARCHAR(100),VALIDDATE,20) AS VALIDDATE " +
                                 " , REMARK " +
                                 " , USERGUID " +
                                 " , USERID " +
                                 " , CONVERT(VARCHAR(100),CREATEDATE,20) AS CREATEDATE " +
                                 " , CONVERT(VARCHAR(100),CHANGEDATE,20) AS CHANGEDATE " +
                                 " , CONVERT(VARCHAR(100),EXPIRYDATE,20) AS EXPIRYDATE " +
                                 " , [STATUS] " +
                              " FROM SALESORDERHEAD " +
                             " WHERE STATUS <> '0' " +
                             " ORDER BY CREATEDATE DESC ";

            List<Model.SOMastData.SalesOrder> lstHead = SqlServerHelper.GetEntityList<Model.SOMastData.SalesOrder>(SqlServerHelper.salesorderConn(), strSql);

            return lstHead;
        }

        /// <summary>
        /// 合同号存在性检查(表头)
        /// </summary>
        /// <param name="contract">合同号</param>
        /// <returns></returns>
        public List<Model.SOMastData.SalesOrder> CheckContract(string contract)
        {
            string strSql = " SELECT [CONTRACT] " +
                              " FROM SALESORDERHEAD " +
                             " WHERE [CONTRACT] = @contract " +
                               " AND STATUS <> '0' ";
            return SqlServerHelper.GetEntityList<Model.SOMastData.SalesOrder>(SqlServerHelper.salesorderConn(), strSql, System.Data.CommandType.Text,
               new SqlParameter[]{
                    new SqlParameter("@contract", SqlDbType.NVarChar, 30){ Value = contract },
               });
        }

        /// <summary>
        /// SODetail changed by Angel
        /// </summary>
        /// <param name="headguid"></param>
        /// <returns></returns>
        public List<SalesOrderItem> SODetail(string company)
        {

            string sql = "select distinct a1.headGuid,a1.itemGuid,a1.contract,a1.costCenterCode,a1.productCode,a1.productDesc, "
                + "a1.qty,a1.price,a1.StartDate,a1.EndDate,convert(varchar(100),a1.expiryDate,23) as ExpiryDate,a1.Remark from [dbo].[SalesOrderItem] a1 "
                + "left join [dbo].[SalesOrderHead] a2 on a2.HeadGuid=a1.HeadGuid where a2.CompanyCode='" + company + "' ";
            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
            return data.AsEnumerable().Select(dr => new SalesOrderItem()
            {
                headGuid = dr.Field<string>("headGuid").ToStringTrim(),
                itemGuid = dr.Field<string>("itemGuid").ToStringTrim(),
                contract = dr.Field<string>("contract").ToStringTrim(),
                costCenterCode = dr.Field<string>("costCenterCode").ToStringTrim(),
                productCode = dr.Field<string>("productCode").ToStringTrim(),
                productDesc = dr.Field<string>("productDesc").ToStringTrim(),
                qty = dr.Field<int>("qty"),
                price = dr.Field<decimal>("price"),
                startDate = dr.Field<DateTime>("startDate").ToString("yyyy-MM-dd"),
                endDate = dr.Field<DateTime>("endDate").ToString("yyyy-MM-dd"),
                expiryDate = dr.Field<string>("expiryDate").ToStringTrim(),
                remark = dr.Field<string>("remark").ToStringTrim()
            }).ToList();
        }

        public List<Model.SOMastData.SalesOrder> SearchSO(string db, string sDate, string eDate)
        {
            List<Model.SOMastData.SalesOrder> lstSalesOrderWork = new List<Model.SOMastData.SalesOrder>();
            List<Model.SOMastData.SalesOrder> lstSalesOrder = new List<Model.SOMastData.SalesOrder>();

            string strSql = " SELECT H.HEADGUID " +
                                 " , MAX(H.OWNERCOMPANYCODE) AS COMPANYCODE " +
                                 " , MAX(H.SIGNCOUNT) AS SIGNCOUNT " +
                                 " , MAX(H.CMPCODE) AS CMPCODE" +
                                 " , MAX(H.CUSTOMCODE) AS CUSTOMCODE " +
                                 " , MAX(H.CUSTOMNAME_ZH) AS CUSTOMNAME_ZH " +
                                 " , MAX(H.CUSTOMNAME_EN) AS CUSTOMNAME_EN " +
                                 " , MAX(H.CONTRACT) AS CONTRACT " +
                                 " , MAX(CONVERT(VARCHAR(100),H.STARTDATE, 23)) AS STARTDATE " +
                                 " , MAX(CONVERT(VARCHAR(100),H.ENDDATE, 23)) AS ENDDATE " +
                                 " , MAX(CONVERT(VARCHAR(100), H.EXPIRYDATE, 23)) AS EXPIRYDATE " +
                                 " , MAX(H.CURRCODE) AS CURRCODE " +
                                 " , MAX(H.DEADLINE) AS DEADLINE " +
                                 " , MAX(CONVERT(VARCHAR(100),H.VALIDDATE, 23)) AS VALIDDATE " +
                                 " , MAX(H.PAYMENTNAME_ZH) AS PAYMENTNAME_ZH " +
                                 " , MAX(H.PAYMENTNAME_EN) AS PAYMENTNAME_EN " +
                                 " , MAX(H.REMARK) AS REMARK " +
                                 " , MAX(H.STATUS) AS STATUS " +
                                 " , L.COSTCENTERCODE " +
                                 " , MAX(H.REBUILDGUID) AS REBUILDGUID " +
                             " FROM [DBO].[SALESORDERHEAD] AS H " +
                            " INNER JOIN [DBO].[SALESORDERITEM] AS L " +
                               " ON H.HEADGUID = L.HEADGUID " +
                            " WHERE H.STATUS <> '0' " +
                              " AND H.OWNERCOMPANYCODE = '{0}' " +
                              " AND H.STARTDATE <= '{1}' " +
                              " AND H.ENDDATE >= '{2}' " +
                            " GROUP BY H.HEADGUID" +
                                " , L.COSTCENTERCODE " +
                            " ORDER BY MAX(H.ID) DESC ";
            lstSalesOrderWork = SqlServerHelper.GetEntityList<Model.SOMastData.SalesOrder>(SqlServerHelper.salesorderConn(), 
                string.Format(strSql, db, eDate, sDate));

            lstSalesOrder = (from salesOrder in lstSalesOrderWork
                             group salesOrder by new { salesOrder.headGuid } into lq
                             let first = lq.FirstOrDefault()
                             select new Model.SOMastData.SalesOrder
                             {
                                 headGuid = first.headGuid,
                                 companyCode = first.companyCode,
                                 cmpCode = first.cmpCode,
                                 customCode = first.customCode,
                                 customName_ZH = first.customName_ZH,
                                 customName_EN = first.customName_EN,
                                 contract = first.contract,
                                 startDate = first.startDate,
                                 endDate = first.endDate,
                                 expiryDate = first.expiryDate,
                                 currCode = first.currCode,
                                 deadline = first.deadline,
                                 validDate = first.validDate,
                                 paymentName_ZH = first.paymentName_ZH,
                                 paymentName_EN = first.paymentName_EN,
                                 remark = first.remark,
                                 status = first.status,
                                 costCenterCode = string.Join("," , lq.Select(r=>r.costCenterCode).Distinct().ToArray()),
                                 rebuildGuid = first.rebuildGuid
                             } into lines
                             select lines).ToList();
            return lstSalesOrder;
        }

        /// <summary>
        /// MenuAction by Angel
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="Company"></param>
        /// <param name="guid"></param>
        /// <param name="pguid"></param>
        /// <returns></returns>
        public MenuAction MenuAction(string userGuid, string Company, string action)//, string pguid)
        {
            //string sql = "select distinct a2.Company,a1.userGuid,a1.menuGuid,a1.Action,isnull(a1.CanChange,'') CanChange,isnull(a1.CanDelete,'') CanDelete "
            //    + "from[dbo].[tblUserMenuData] (nolock) a1 join[dbo].[Company] (nolock) a2 on a1.Guid = a2.Guid "
            //    + "where a1.status = 1 and userGuid = '" + userGuid + "' and a2.Company = '" + Company + "' and "
            //    + "(menuGuid = '" + guid + "' or menuGuid = '" + pguid + "')";
            //string sql = "select a3.Company,a1.userGuid,a1.menuGuid,a1.Action,isnull(a1.CanChange,0) CanChange,isnull(a1.CanDelete,0) CanDelete "
            //    + "from tblUserMenuData a1,tblmenu a2, company a3 "
            //    + " where a1.userguid = '{0}' and a3.company in ('{1}', '{2}') "
            //    + "and(a1.menuguid = a2.guid or a1.menuguid = a2.pguid) and a2.action='{3}' "
            //    + "and a1.type = 'company' and a1.guid = a3.guid and a1.status = '1' and a2.status = '1' "
            //    + "order by case a2.action when '{3}' then 0 else 1 end";

            string strSql = " WITH COMPANYCODE(GUID,SUPERIORCOMPANY) AS " +
                            " ( " +
                             " SELECT GUID " +
                                  " , SUPERIORCOMPANY " +
                               " FROM COMPANY " +
                              " WHERE COMPANY = '{0}' " +
                              " UNION ALL " +
                             " SELECT A2.GUID " +
                                  " , A2.SUPERIORCOMPANY " +
                               " FROM COMPANYCODE A1 " +
                                  " , COMPANY A2 " +
                              " WHERE A1.SUPERIORCOMPANY = A2.COMPANY " +
                                " AND A2.COMPANY <> A2.SUPERIORCOMPANY " +
                            " ) " +
                            " SELECT A3.SUPERIORCOMPANY AS COMPANY " +
                                 " , A1.USERGUID " +
                                 " , A1.MENUGUID " +
                                 " , A1.ACTION " +
                                 " , ISNULL(A1.CANCHANGE, 0) CANCHANGE " +
                                 " , ISNULL(A1.CANDELETE, 0) CANDELETE " +
                              " FROM TBLUSERMENUDATA A1 " +
                                 " , TBLMENU A2 " +
                                 " , COMPANYCODE A3 " +
                             " WHERE A1.USERGUID = '{1}' " +
                               " AND (A1.MENUGUID = A2.GUID " +
                                 " OR A1.MENUGUID = A2.PGUID) " +
                               " AND A2.ACTION='{2}' " +
                               " AND A1.TYPE = 'company' " +
                               " AND A1.GUID = A3.GUID " +
                               " AND A1.STATUS = '1' " +
                               " AND A2.STATUS = '1' " +
                             " ORDER BY CASE A2.ACTION " +
                                          " WHEN '{2}' THEN 0 " +
                                          " ELSE 1 " +
                                      " END ";

            strSql = string.Format(strSql, Company, userGuid, action);
            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), strSql);
            //return data.ToEntityList<MenuAction>();
            var list = data.AsEnumerable().Select(dr => new MenuAction()
            {
                CanChange = dr.Field<Boolean>("CanChange"),
                CanDelete = dr.Field<Boolean>("CanDelete")
            }).ToList();
            if (!list.Any()) return null;

            return list[0];
        }

        /// <summary>
        /// ItemDetail by Angel
        /// </summary>
        /// <param name="HeadGuid"></param>
        /// <returns></returns>
        public List<SalesOrderItem> ItemDetail(string HeadGuid)
        {
            string sql = "select distinct a1.headGuid,a1.itemGuid,a1.contract,a1.costCenterCode,a1.productCode,a1.productDesc, "
               + "a1.qty,a1.price,a1.StartDate,a1.EndDate,convert(varchar(100),a1.expiryDate,23) as ExpiryDate,a1.Remark from [dbo].[SalesOrderItem] a1 "
               + "left join [dbo].[SalesOrderHead] a2 on a2.HeadGuid=a1.HeadGuid where a1.headGuid='" + HeadGuid + "' ";
            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
            return data.AsEnumerable().Select(dr => new SalesOrderItem()
            {
                costCenterCode = dr.Field<string>("costCenterCode").ToStringTrim(),
                productCode = dr.Field<string>("productCode").ToStringTrim(),
                productDesc = dr.Field<string>("productDesc").ToStringTrim(),
                qty = dr.Field<int>("qty"),
                price = dr.Field<decimal>("price"),
                startDate = dr.Field<DateTime>("startDate").ToString("yyyy-MM-dd"),
                endDate = dr.Field<DateTime>("endDate").ToString("yyyy-MM-dd"),
                expiryDate = dr.Field<string>("expiryDate").ToStringTrim(),
                remark = dr.Field<string>("remark").ToStringTrim()
            }).ToList();
        }

        /// <summary>
        /// SOcc by Angel Jiang
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public List<SalesOrderItem> SOcc(string companyCode, string ownerCompanyCode, string contract)
        {
            string filter = "";
            string compfilter = "";
            if (ownerCompanyCode != null && companyCode == null)
                compfilter = "where a2.ownerCompanyCode='" + ownerCompanyCode + "'";
            else if (ownerCompanyCode == null && companyCode != null)
                compfilter = "where a2.companyCode='" + companyCode + "'";
            if (contract != "" && contract != null) filter = " and a2.contract='" + contract + "'";
            string sql = "select distinct a1.status as status,a1.headGuid,a2.contract,a1.costCenterCode,a3.Name from [dbo].[SalesOrderItem] a1 "
                + "left join [dbo].[SalesOrderHead] a2 on a2.HeadGuid=a1.HeadGuid left join[dbo].[ServiceTypeData] a3 on a3.Add1=right(a1.CostCenterCode,2) "
                + compfilter + filter;

            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
            return data.AsEnumerable().Select(dr => new SalesOrderItem()
            {
                headGuid = dr.Field<string>("headGuid").ToStringTrim(),
                contract = dr.Field<string>("contract").ToStringTrim(),
                costCenterCode = dr.Field<string>("costCenterCode").ToStringTrim(),
                remark = dr.Field<string>("Name").ToStringTrim(),
                status = dr.Field<string>("status").ToStringTrim()
            }).ToList();
        }

        /// <summary>
        /// SearchCostCenterMatch by Angel Jiang
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public List<SalesOrderItem> SearchCostCenterMatch(SalesOrderItem line)
        {
            string sql = "select distinct headGuid,costCenterCode,InvoiceCostCenterCode from [dbo].[SalesOrderItem] where HeadGuid ='" + line.headGuid + "'";
            DataTable dtemp = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
            if (dtemp == null || dtemp.Rows.Count == 0) return null;
            else
            {
                return dtemp.AsEnumerable().Select(dg => new SalesOrderItem
                {
                    headGuid = dg.Field<string>("headGuid").ToStringTrim(),
                    invoiceCostCenterCode = dg.Field<string>("InvoiceCostCenterCode").ToStringTrim(),
                    costCenterCode=dg.Field<string>("costCenterCode").ToStringTrim()
                }).ToList();
            }
        }
        /// <summary>
        /// CostCenterMatch by Angel Jiang
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int CostCenterMatch(SalesOrderItem line)
        {
            StringBuilder sbSql = new StringBuilder();
            StringBuilder finalsbSql = new StringBuilder();
            string ChangeDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");

            string sqlupdate = "update [dbo].[SalesOrderItem] set InvoiceCostCenterCode='{0}',ChangeUserGuid='{1}',ChangeUserID='{2}',ChangeDate='{3}' where HeadGuid='{4}' and CostCenterCode='{5}'";

            sbSql.AppendFormat(sqlupdate.ToString(), line.invoiceCostCenterCode, line.changeUserGuid, line.changeUserID, ChangeDate, line.headGuid,line.costCenterCode);
            
            finalsbSql.Append(sbSql.ToStringTrim());

            return SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), finalsbSql.ToString());
        }

        /// <summary>
        /// HD by Angel Jiang
        /// </summary>
        /// <param name="db"></param>
        /// <param name="invcompCode"></param>
        /// <param name="customCode"></param>
        /// <param name="contract"></param>
        /// <param name="CurrCode"></param>
        /// <param name="PaymentCode"></param>
        /// <param name="deadline"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="vDate"></param>
        /// <returns></returns>
        public List<Model.SOMastData.SalesOrder> HD(Aden.Model.SOMastData.SalesOrder so)
        {
            string sql = "select top 1 a1.headGuid from [dbo].[SalesOrderHead] a1 where a1.status <> 0 and a1.OwnerCompanyCode = '" + so.ownerCompanyCode + "' "
                + "and a1.CompanyCode = '" + so.companyCode + "' and CustomCode = '" + so.customCode + "' and contract = '" + so.contract + "' and PaymentCode = '" + so.paymentCode + "' "
                + "and convert(varchar(10),a1.StartDate,23)= '" + so.startDate + "' and convert(varchar(10),a1.EndDate,23)= '" + so.endDate + "' "
                + "and convert(varchar(10),a1.ValidDate,23)= '" + so.validDate + "' order by a1.id desc";
            return SqlServerHelper.GetEntityList<Model.SOMastData.SalesOrder>(SqlServerHelper.salesorderConn(), sql);
        }

        /// <summary>
        /// 产品为SalesMeals的记录追加到Gladis中
        /// </summary>
        /// <returns></returns>
        //private string CreateSOGladis(Model.SOMastData.SalesOrder so)
        //{
        //    string strSqlInertHead = " INSERT INTO [DBO].[SALESORDER] " +
        //                                  " ( [COMPANYCODE] " +
        //                                  " , [COMPANYNAMECODE] " +
        //                                  " , [CONTRACTCODE] " +
        //                                  " , [COSTCENTER] " +
        //                                  " , [CONTRACTCUSTCODE] " +
        //                                  " , [CUSTCODE] " +
        //                                  " , [STARTDATE] " +
        //                                  " , [ENDDATE] " +
        //                                  " , [CLOSEDATE] " +
        //                                  " , [PAYMENTTERM] " +
        //                                  " , [ACTUALPAYMENTTERM] " +
        //                                  " , [CURRENCY] " +
        //                                  " , [COMMENT] " +
        //                                  " , [RENEWTIMES] " +
        //                                  " , [FULFILMENTDATE] " +
        //                                  " , [VALIDFULFILMENTDATE] " +
        //                                  " , [DISCOUNT] " +
        //                                  " , [AUTORESIGN] " +
        //                                  " , [SIGNED] " +
        //                                  " , [REMARK] " +
        //                                  " , [CREATEUSER] " +
        //                                  " , [CREATEDATE] " +
        //                                  " , [DELETEREASON] " +
        //                                  " , [DELETEUSER] " +
        //                                  " , [DELETEDATE] " +
        //                                  " , [FIRSTID] " +
        //                                  " , [SYSGUID] " +
        //                                  " , [SALESORDERID] ) " +
        //                              " VALUES " +
        //                                  " ( '{0}'} " +
        //                                  " , '{1}' " +
        //                                  " , '{2}' " +
        //                                  " , '{3}' " +
        //                                  " , '{4}' " +
        //                                  " , '{5}' " +
        //                                  " , '{6}' " +
        //                                  " , '{7}' " +
        //                                  " , '{8}' " +
        //                                  " , '{9}' " +
        //                                  " , '{10}' " +
        //                                  " , '{11}' " +
        //                                  " , '{12}' " +
        //                                  " , '{13}' " +
        //                                  " , '{14}' " +
        //                                  " , '{15}' " +
        //                                  " , '{16}' " +
        //                                  " , '{17}' " +
        //                                  " , '{18}' " +
        //                                  " , '{19}' " +
        //                                  " , '{20}' " +
        //                                  " , '{21}' " +
        //                                  " , '{22}' " +
        //                                  " , '{23}' " +
        //                                  " , '{24}' " +
        //                                  " , '{25}' " +
        //                                  " , '{26}' " +
        //                                  " , '{27}' ) ";

        //    string strSqlInsertItem = " INSERT INTO [DBO].[SALESORDERLINE] " +
        //                                  " ( [ORDERID] " +
        //                                  " , [LINEGUID] " +
        //                                  " , [LINENBR] " +
        //                                  " , [PARENTID] " +
        //                                  " , [CONTRACTCODE] " +
        //                                  " , [COSTCENTER] " +
        //                                  " , [ITEMTYPE] " +
        //                                  " , [ITEMNAME] " +
        //                                  " , [QTY] " +
        //                                  " , [PRICE] " +
        //                                  " , [TAXCODE] " +
        //                                  " , [TAXRATE] " +
        //                                  " , [PRICEUNIT] " +
        //                                  " , [NEEDTODISCOUNT] " +
        //                                  " , [STARTDATE] " +
        //                                  " , [ENDDATE] " +
        //                                  " , [STARTWORKTIME] " +
        //                                  " , [ENDWORKTIME] " +
        //                                  " , [FULFILMENTDATE] " +
        //                                  " , [CLOSEDATE] " +
        //                                  " , [CREATEUSER] " +
        //                                  " , [CREATEDATE] " +
        //                                  " , [DELETEUSER] " +
        //                                  " , [DELETEDATE] " +
        //                                  " , [FIRSTID] " +
        //                                  " , [SYSGUID] " +
        //                                  " , [REMARK] " +
        //                                  " , [SALESORDERID] ) " +
        //                              " VALUES " +
        //                                  " ( '{0}' " +
        //                                  " , '{1}' " +
        //                                  " , '{2}' " +
        //                                  " , '{3}' " +
        //                                  " , '{4}' " +
        //                                  " , '{5}' " +
        //                                  " , '{6}' " +
        //                                  " , '{7}' " +
        //                                  " , '{8}' " +
        //                                  " , '{9}' " +
        //                                  " , '{10}' " +
        //                                  " , '{11}' " +
        //                                  " , '{12}' " +
        //                                  " , '{13}' " +
        //                                  " , '{14}' " +
        //                                  " , '{15}' " +
        //                                  " , '{16}' " +
        //                                  " , '{17}' " +
        //                                  " , '{18}' " +
        //                                  " , '{19}' " +
        //                                  " , '{20}' " +
        //                                  " , '{21}' " +
        //                                  " , '{22}' " +
        //                                  " , '{23}' " +
        //                                  " , '{24}' " +
        //                                  " , '{25}' " +
        //                                  " , '{26}' " +
        //                                  " , '{27}' ";



        //    return string.Empty;
        //}
    }
}
