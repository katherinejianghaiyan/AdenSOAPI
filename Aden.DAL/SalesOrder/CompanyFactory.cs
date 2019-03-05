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
using Aden.Util.AdenCommon;
using System.Text.RegularExpressions;

namespace Aden.DAL.SalesOrder
{
    public class CompanyFactory
    {
        /// <summary>
        /// 根据权限取得Company列表
        /// </summary>
        /// <param name="auth">权限参数</param>
        /// <returns></returns>
        public List<Company> GetCompanyInAuth(AuthCompany auth)
        {
            // 取得Guid集合
            string strGuid = Util.AdenCommon.MastData.formatActionToMenuGuid(auth.action);

            StringBuilder sbSql = new StringBuilder();

            sbSql.Append("  WITH COMPANYCODE(GUID, CODE) AS ");
            sbSql.Append("  ( ");
            sbSql.Append("  SELECT C.GUID ");
            sbSql.Append("       , C.COMPANY CODE ");
            sbSql.Append("    FROM COMPANY C ");
            sbSql.Append("   INNER JOIN TBLUSERMENUDATA AS U ");
            sbSql.Append("      ON C.GUID = U.GUID ");
            sbSql.Append("     AND U.TYPE = 'company' ");
            sbSql.Append("     AND U.USERGUID = '" + auth.userGuid + "' ");
            sbSql.Append("     AND U.MENUGUID IN " + strGuid);
            sbSql.Append("   WHERE C.STATUS = '1' ");
            sbSql.Append("   UNION ALL ");
            sbSql.Append("  SELECT A2.GUID ");
            sbSql.Append("       , A2.COMPANY CODE ");
            sbSql.Append("    FROM COMPANYCODE A1 ");
            sbSql.Append("       , COMPANY A2 ");
            sbSql.Append("   WHERE A1.CODE = ISNULL(A2.SUPERIORCOMPANY, '') ");
            sbSql.Append("     AND A2.COMPANY <> ISNULL(A2.SUPERIORCOMPANY, '') ");
            sbSql.Append("     AND STATUS = '1' ");
            sbSql.Append("  ) ");
            sbSql.Append("  SELECT DISTINCT ");
            sbSql.Append("         C.GUID AS COMPANYGUID ");
            sbSql.Append("       , C.COMPANY AS COMPANYCODE ");
            sbSql.Append("       , C.IP ");
            sbSql.Append("       , C.DBNAME ");
            sbSql.Append("       , C.NAME_ZH AS COMPANYNAME_ZH ");
            sbSql.Append("       , C.NAME_EN AS COMPANYNAME_EN ");
            sbSql.Append("       , C.SUPPLIERCODE ");
            sbSql.Append("       , C.SUPERIORCOMPANY ");
            sbSql.Append("    FROM COMPANY AS C ");
            sbSql.Append("   INNER JOIN COMPANYCODE AS U ");
            sbSql.Append("      ON C.GUID = U.GUID ");
            sbSql.Append("   WHERE 1 = 1 ");
            if (!string.IsNullOrWhiteSpace(auth.isSupplier))
                sbSql.Append(" AND ISNULL(C.SUPPLIERCODE, '') <> '' ");
            sbSql.Append("   ORDER BY C.COMPANY ");

            List<Company> lstCompany = SqlServerHelper.GetEntityList<Company>(SqlServerHelper.salesorderConn(), sbSql.ToString());
            return lstCompany;
        }

        /// <summary>
        /// 取得所有符合条件的Company列表
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public List<Company> GetCompany(AuthCompany auth)
        {
            string strSql = " SELECT DISTINCT " +
                                   " C.GUID AS COMPANYGUID " +
                                 " , C.COMPANY AS COMPANYCODE " +
                                 " , C.IP " +
                                 " , C.DBNAME " +
                                 " , C.NAME_ZH AS COMPANYNAME_ZH " +
                                 " , C.NAME_EN AS COMPANYNAME_EN " +
                                 " , C.SUPPLIERCODE " +
                                 " , C.SUPERIORCOMPANY " +
                              " FROM COMPANY C " +
                             " WHERE C.STATUS = '1' ";
            if (!string.IsNullOrWhiteSpace(auth.srvCompanyCode))
            {
                strSql = strSql + " AND (ISNULL(C.CUSTOMCODE, '') <> '' " +
                                  "   OR COMPANY = '" + auth.srvCompanyCode + "') ";
            }
            strSql = strSql + " ORDER BY C.COMPANY ";

            List<Company> lstCompany = SqlServerHelper.GetEntityList<Company>(SqlServerHelper.salesorderConn(), strSql);
            return lstCompany;
        }

        /// <summary>
        /// dtCompany by Angel
        /// </summary>
        /// <returns></returns>
        public List<Company> dtCompany()
        {
            string strSql = "select distinct Company,Name_EN,Name_ZH,InterBranchOP,InterBranchOR,InterCompanyOP, "
                + "InterCompanyOR,CompanyGroup, "
                + "supplierCode,customCode as CustomerCode from company where status = 1 order by Company";
            DataTable dtemp = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), strSql);
            if (dtemp == null || dtemp.Rows.Count == 0) return null;
            return dtemp.AsEnumerable().Select(dr => new Company()
            {
                companyCode = dr.Field<string>("Company").ToString().Trim(),
                companyName_EN = dr.Field<string>("Name_EN").ToString().Trim(),
                companyName_ZH = dr.Field<string>("Name_ZH").ToString().Trim(),
                interBranchOP = dr.Field<string>("InterBranchOP") == null ? "" : dr.Field<string>("InterBranchOP").ToString().Trim(),
                interBranchOR = dr.Field<string>("InterBranchOR") == null ? "" : dr.Field<string>("InterBranchOR").ToString().Trim(),
                interCompanyOP = dr.Field<string>("InterCompanyOP") == null ? "" : dr.Field<string>("InterCompanyOP").ToString().Trim(),
                interCompanyOR = dr.Field<string>("InterCompanyOR") == null ? "" : dr.Field<string>("InterCompanyOR").ToString().Trim(),
                companyGroup = dr.Field<string>("CompanyGroup") == null ? "" : dr.Field<string>("CompanyGroup").ToString().Trim(),
                supplierCode = dr.Field<string>("SupplierCode") == null ? "" : dr.Field<string>("SupplierCode").ToString().Trim(),
                customerCode = dr.Field<string>("CustomerCode") == null ? "" : dr.Field<string>("CustomerCode").ToString().Trim()
            }).ToList();

        }

        /// <summary>
        /// updateCompany by Angel Jiang
        /// </summary>
        /// <returns></returns>
        public int updateCompany(Aden.Model.Common.Company lines)
        {
            string sql = "update company set interBranchOP='{0}',interBranchOR='{1}',interCompanyOP='{2}',interCompanyOR='{3}', "
                + "companyGroup='{4}',supplierCode='{5}', "
                + "customCode='{6}' where Company='{7}' and status=1";

            StringBuilder sbSqlFinal = new StringBuilder();
            StringBuilder sbSqlwork = new StringBuilder();

            foreach (Company item in lines.lines)
            {
                sbSqlwork = new StringBuilder();
                sbSqlwork.AppendFormat(sql, item.interBranchOP, item.interBranchOR, item.interCompanyOP, item.interCompanyOR,
                    item.companyGroup, item.supplierCode,item.customerCode, item.companyCode);
                sbSqlFinal.Append(sbSqlwork.ToString());
            }
            return SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());
        }

        /// <summary>
        /// 检查服务公司的供应商代码在开票公司的供应商列表中是否存在
        /// </summary>
        /// <param name="companyInfo"></param>
        /// <returns></returns>
        public bool CheckSupplierCode(Company companyInfo)
        {
            List<Company> lstSupplier = new List<Company>();

            string strSql = " SELECT TOP 1 Y.CRDCODE " +
                              " FROM CICMPY AS Y " +
                             " INNER JOIN CICNTP AS P " +
                                " ON P.CNT_ID = Y.CNT_ID " +
                             " WHERE Y.CRDCODE IS NOT NULL " +
                               " AND Y.CRDCODE = {0} " +
                               " AND Y.CMP_TYPE IN ('A', 'S', 'E', 'B', 'D', 'P') " +
                               " AND Y.CMP_STATUS IN ('A', 'N', 'R', 'P', 'S') " +
                             " ORDER BY Y.CRDCODE ";

            lstSupplier = SqlServerHelper.GetEntityList<Company>(string.Format(SqlServerHelper.customerConn(), 
                companyInfo.ip, companyInfo.dbName), string.Format(strSql, companyInfo.supplierCode));

            if (lstSupplier.Count == 1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 根据公司Code取得CostCenter集合
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public List<Model.Common.SingleField> GetActCostCenter(Model.Common.SingleField param)
        {
            List<Model.Common.SingleField> lstCostCenter = new List<Model.Common.SingleField>();
            List<Model.Common.SingleField> lstCCOrder = new List<Model.Common.SingleField>();
            List<Model.Common.SingleField> lstCCWhs = new List<Model.Common.SingleField>();

            // 当前时间
            string strDateTime = DateTime.Now.ToString("yyyy-MM-dd");
            // Sql文(SalesOrderLine)
            string strSqlOrder = " SELECT DISTINCT L.COSTCENTERCODE AS CODE " +
                                   " FROM SALESORDERITEM (nolock) AS L " +
                                  " INNER JOIN SALESORDERHEAD (nolock) AS H " +
                                     " ON L.HEADGUID = H.HEADGUID " +
                                    " AND H.OWNERCOMPANYCODE = '{0}' " +
                                    " AND H.STATUS <> '0' " +
                                    " AND L.STATUS <> '0' " +
                                  " INNER JOIN PRODUCTTYPEDATA AS P " +
                                     " ON L.PRODUCTCODE = P.ID " +
                                    " AND P.PRODUCTTYPE = 'SalesMeals' " +
                                  " WHERE H.VALIDDATE <= '{1}' AND H.STARTDATE <= '{1}' " +
                                    " AND ISNULL(H.EXPIRYDATE, H.ENDDATE) >= '{1}' " +
                                    " AND L.STARTDATE <= '{1}' " +
                                    " AND L.ENDDATE >= '{1}' " +
                                    " AND ISNULL(L.EXPIRYDATE,'2222-12-12') >= '{1}' ";

            lstCCOrder = SqlServerHelper.GetEntityList<Model.Common.SingleField>(SqlServerHelper.salesorderConn(), string.Format(strSqlOrder,
                param.code, strDateTime));

            // Sql文(CCWhs)
            string strSqlCCWhs = " SELECT COSTCENTERCODE AS CODE " +
                                   " FROM CCMast " +
                                  " WHERE DBNAME = '{0}' ";

            lstCCWhs = SqlServerHelper.GetEntityList<Model.Common.SingleField>(SqlServerHelper.salesorderConn(), string.Format(strSqlCCWhs,
                param.code));

            //lstCostCenter = lstCCOrder.Concat(lstCCWhs).ToList();
            //lstCostCenter = lstCostCenter.Distinct().ToList();

            lstCostCenter = (from so in lstCCOrder
                             join wh in lstCCWhs
                             on so.code equals wh.code into cc
                             select new Model.Common.SingleField() {
                                 code = so.code
                             }).ToList();

            if (lstCostCenter.Count == 0)
                return lstCostCenter;

            // 如果Flag不为空则继续获取中/英文名称
            if (string.IsNullOrWhiteSpace(param.flag))
            {
                // 成本中心主档
                CompanyAddress addr = new CompanyAddress();
                addr.ip = GetCompanyInfoByCode(param.code).ip;
                addr.erpCode = param.code;
                List<CostCenter> lstCCMastData = (new MastDataFactory()).GetCostCenter(addr);

                lstCostCenter.GroupJoin(lstCCMastData,
                    cc => new { code = cc.code },
                    ma => new { code = ma.costCenterCode },
                    (cc, ma) =>
                    {
                        var first = ma.FirstOrDefault();
                        bool check = first != null;
                        cc.name1 = check ? first.costCenterName_ZH : string.Empty;
                        cc.name2 = check ? first.costCenterName_EN : string.Empty;

                        return cc;
                    }).ToList();
            }

            lstCostCenter = lstCostCenter.OrderBy(r => r.code).ToList();

            return lstCostCenter;
        }

        public Company GetCompanyInfoByCode(string companyCode)
        {
            Company compInfo = new Company();

            string strSql = " SELECT TOP 1 * " +
                              " FROM COMPANY " +
                             " WHERE COMPANY = '{0}' " +
                               " AND STATUS = '1' ";

            compInfo = SqlServerHelper.GetEntity<Company>(SqlServerHelper.salesorderConn(), string.Format(strSql, companyCode));

            return compInfo;
        }
    }
}
