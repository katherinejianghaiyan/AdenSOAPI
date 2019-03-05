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


namespace Aden.DAL.SalesOrder
{
    public class MastDataFactory
    {
        /// <summary>
        /// 取得客户主数据
        /// </summary>
        /// <returns></returns>
        public List<Customer> GetCustomer(CompanyAddress compAddr)
        {
            string strSql = " SELECT REPLACE(T.DEBNR, ' ', '') AS CUSTOMCODE " +
                                 " , REPLACE(T.CMP_CODE, ' ', '') AS CMPCODE " +
                                 " , REPLACE(T.CMP_NAME, '\t', '') AS CUSTOMNAME_EN " +
                                 " , REPLACE(T.CMP_FADD1, '\t', '') AS CUSTOMNAME_ZH " +
                                 " , REPLACE(T.TAXCODE, ' ', '') AS TAXCODE" +
                                 " , T.PAYMENTCONDITION AS PAYMENTCODE " +
                                 " , (CASE " +
                                       " WHEN T.CURRENCY IS NULL THEN " +
                                              " (SELECT TOP 1 VALCODE " +
                                                 " FROM BEDRYF)" +
                                       " ELSE T.CURRENCY " +
                                    " END) AS CURRCODE" +
                                 " , T.ADMINISTRATION AS DIVISION " +
                              " FROM CICMPY AS T " +
                             " WHERE T.CMP_TYPE IN ('B', 'C') order by T.CMP_CODE";

            return SqlServerHelper.GetEntityList<Customer>(string.Format(SqlServerHelper.customerConn(), compAddr.ip, compAddr.erpCode), strSql.ToString());
        }

        /// <summary>
        /// 取得币别主数据
        /// </summary>
        /// <returns></returns>
        public List<Currency> GetCurr(CompanyAddress compAddr)
        {
            string strSql = " SELECT VALCODE AS CURRCODE " +
                                 " , OMS30_0 AS CURRNAME_EN " +
                                 " , OMS30_1 AS CURRNAME_ZH " +
                              " FROM VALUTA " +
                             " WHERE ACTIVE = 1 ";

            return SqlServerHelper.GetEntityList<Currency>(string.Format(SqlServerHelper.customerConn(), compAddr.ip, compAddr.erpCode), strSql.ToString());
        }

        /// <summary>
        /// 取得付款方式主数据
        /// </summary>
        /// <returns></returns>
        public List<Payment> GetPayment(CompanyAddress compAddr)
        {
            string strSql = " SELECT BETCOND AS PAYMENTCODE " +
                                 " , OMS30_0 AS PAYMENTNAME_EN " +
                                 " , OMS30_1 AS PAYMENTNAME_ZH " +
                              " FROM BETCD ";

            return SqlServerHelper.GetEntityList<Payment>(string.Format(SqlServerHelper.customerConn(), compAddr.ip, compAddr.erpCode), strSql.ToString());
        }

        public string ServiceCode()
        {
            StringBuilder filter = new StringBuilder();

            string sql = "SELECT distinct add1 FROM ServiceTypeData where ISNULL(Add1,'')<>''";

            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);

            if (data == null || data.Rows.Count == 0)
            {
                return null;
            }
            else if (data.Rows.Count > 0)
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    filter.Append("," + "'" + data.Rows[i][0] + "'");
                }
            }

            return filter.ToString();
        }

        /// <summary>
        /// 取得成本中心主数据
        /// </summary>
        /// <returns></returns>
        public List<CostCenter> GetCostCenter(CompanyAddress compAddr)
        {
            string filter = "";

            filter = ServiceCode();

            string filterFinal = "";

            if (filter != null && filter != "")
                filterFinal = " and right(KSTPLCODE,2) in (" + filter.Substring(1) + ")";
            else if (filter == null || filter == "")
                filterFinal = "";

            string strSql = " SELECT KSTPLCODE AS COSTCENTERCODE " +
                                 " , OMS25_0 AS COSTCENTERNAME_EN " +
                                 " , OMS25_1 AS COSTCENTERNAME_ZH " +
                              " FROM KSTPL (nolock) " +
                             " WHERE ISNULL(CLASS_01,'') <> '' " + filterFinal +
                               //" AND LEN(KSTPLCODE) = 8 " + filterFinal +
                             " {0} ORDER BY KSTPLCODE ";
            string tmpsql = "";
            if (compAddr.filter != null)
            {
                List<string> tmp = GetSOCostCenter(compAddr.filter["StartDate"], compAddr.filter["EndDate"],compAddr.erpCode,
                    compAddr.filter.ContainsKey("SOHeadGUID") ? compAddr.filter["SOHeadGUID"] : "");

                if (tmp != null)
                    tmpsql = string.Format(" and kstplcode not in ('{0}')", string.Join("','", tmp));
            }

            strSql = string.Format(strSql, tmpsql);

            return SqlServerHelper.GetEntityList<CostCenter>(string.Format(SqlServerHelper.customerConn(), compAddr.ip, compAddr.erpCode), strSql.ToString());
        }

        /// <summary>
        /// 根据CostCenterCode取得相关信息
        /// </summary>
        /// <param name="costCenterCode"></param>
        /// <returns></returns>
        public CostCenter GetCostCenterInfo(string costCenterCode)
        {
            string strSqlCompany = " SELECT DISTINCT C.IP " +
                                        " , C.DBNAME " +
                                     " FROM COMPANY AS C " +
                                    " INNER JOIN CCMAST M " +
                                       " ON C.DBNAME = M.DBNAME " +
                                    " WHERE M.COSTCENTERCODE = '{0}' ";

            Company companyObj = SqlServerHelper.GetEntity<Company>(SqlServerHelper.salesorderConn(), string.Format(strSqlCompany, costCenterCode));

            if (companyObj == null)
                return null;

            string strSql = " SELECT KSTPLCODE AS COSTCENTERCODE " +
                                 " , OMS25_0 AS COSTCENTERNAME_EN " +
                                 " , OMS25_1 AS COSTCENTERNAME_ZH " +
                              " FROM KSTPL (NOLOCK) " +
                             " WHERE ISNULL(CLASS_01,'') <> '' " +
                               " AND KSTPLCODE = '{0}' ";

            strSql = string.Format(strSql, costCenterCode);

            return SqlServerHelper.GetEntity<CostCenter>(string.Format(SqlServerHelper.customerConn(), companyObj.ip, companyObj.dbName), strSql.ToString());
        }

        private List<string> GetSOCostCenter(string startDate, string endDate,string companyCode, string SOHGuid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate)) return null;

                //检查日期的有效性
                DateTime.Parse(startDate);
                DateTime.Parse(endDate);
                string sql = "select　distinct a2.costcentercode "
                        + "from salesorderhead (nolock) a1 join SalesOrderItem (nolock) a2 on a1.headguid = a2.headguid "
                        + " where a1.ownercompanycode='{3}' and a1.status <> '0' and a2.status in ('1','2') and ltrim(rtrim(a2.costcentercode)) <> '' "
                        //+ "and (case when a1.startdate >a1.validdate then a1.startdate else a1.validdate end <= '{1}' "
                        + "and (a1.startdate <= '{1}' and a1.validdate <= '{1}') "
                        + "and isnull(a1.expirydate, a1.enddate) >= '{0}') "
                        + "and (a2.startdate <= '{1}' and isnull(a2.expirydate, a2.enddate) >= '{0}') "
                        + "{2}";

                string tmp = "";
                if (!string.IsNullOrWhiteSpace(SOHGuid))
                {
                    tmp = string.Format("and a1.headguid <> '{0}'", SOHGuid);
                }
                sql = string.Format(sql, startDate, endDate, tmp,companyCode);

                DataTable dt = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
                if (dt == null || dt.Rows.Count == 0) return null;

                return dt.AsEnumerable().Select(q => q.Field<string>("costcentercode")).ToList();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 取得税主数据
        /// </summary>
        /// <returns></returns>
        public List<Tax> GetTax(CompanyAddress compAddr)
        {
            string strSql = " SELECT REPLACE(BTWTRANS, ' ', '') AS TAXCODE " +
                                 " , CAST(BTWPER AS NVARCHAR(50)) AS TAXVALUE " +
                                 " , ISNULL(OMS30_0, OMS30_1) AS TAXNAME " +
                              " FROM BTWTRS order by BTWPER ";

            return SqlServerHelper.GetEntityList<Tax>(string.Format(SqlServerHelper.customerConn(), compAddr.ip, compAddr.erpCode), strSql.ToString());
        }

        /// <summary>
        /// 取得产品主数据
        /// </summary>
        /// <returns></returns>
        public List<Product> GetProduct()
        {
            string strSql = " SELECT CONVERT(VARCHAR(50), ID) AS PRODUCTCODE " +
                                 " , PRODUCTTYPE " +
                                 " , NAME_ZH AS PRODUCTNAME_ZH " +
                                 " , NAME_EN AS PRODUCTNAME_EN " +
                                 " , SORT,STATUS " +
                              " FROM PRODUCTTYPEDATA " +
                              "ORDER BY STATUS DESC,SORT";
                             //" WHERE STATUS = '1' ";

            return SqlServerHelper.GetEntityList<Product>(string.Format(SqlServerHelper.salesorderConn()), strSql.ToString());
        }

        /// <summary>
        /// 取得价格单位主数据
        /// </summary>
        /// <returns></returns>
        public List<PriceUnit> GetPriceUnit()
        {
            string strSql = " SELECT CONVERT(VARCHAR(50), ID) AS PRICEUNITCODE " +
                                 " , NAME_ZH AS PRICEUNITNAME_ZH " +
                                 " , NAME_EN AS PRICEUNITNAME_EN " +
                                 " , SORT " +
                                 " , PRODUCTTYPE " +
                                 " , UNIT " +
                              " FROM PRICEUNITDATA " +
                             " WHERE STATUS = '1' ";

            return SqlServerHelper.GetEntityList<PriceUnit>(string.Format(SqlServerHelper.salesorderConn()), strSql.ToString());
        }

        /// <summary>
        /// 取得SO状态主数据
        /// </summary>
        /// <returns></returns>
        public List<Status> Status(string type)
        {
            string strSql = " SELECT * " +
                              " FROM STATUS_LANG " +
                             " WHERE TYPE = '" + type + "' ";

            return SqlServerHelper.GetEntityList<Status>(string.Format(SqlServerHelper.salesorderConn()), strSql.ToString());
        }
    }
}
