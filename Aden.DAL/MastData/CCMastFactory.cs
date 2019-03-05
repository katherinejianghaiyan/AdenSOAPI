using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Model.Common;
using Aden.Model.MastData;
using Aden.Util.Database;
using Aden.Util.AdenCommon;

namespace Aden.DAL.MastData
{
    public class CCMastFactory
    {
        // 根据成本中心获取相关信息
        public CCMast GetCCMastInfo(string costCenterCode)
        {
            string strSql = " SELECT DBNAME " +
                                 " , COSTCENTERCODE " +
                                 " , COSTCENTERNAME " +
                                 " , WAREHOUSECODE " +
                                 " , POITEMENDTIME " +
                                 " , RECIPENTSOFMENU " +
                                 " , STATUS " +
                                 " , POSIP " +
                                 " , POSDBNAME " +
                                 " , POSDBUSERNAME " +
                                 " , POSDBPASSWORD " +
                                 " , RECHARGESETTING " +
                              " FROM CCMAST " +
                             " WHERE STATUS = '1' " +
                               " AND COSTCENTERCODE = '{0}' ";

            CCMast ccObj = SqlServerHelper.GetEntity<CCMast>(SqlServerHelper.salesorderConn(), string.Format(strSql, costCenterCode));

            return ccObj;
        }

        // 根据成本中心获取相关信息
        public List<CCMast> GetCCMastInfo(List<SingleField> lstCostCenterCode)
        {
            string strCostCenterCode = string.Join("','", lstCostCenterCode.Select(r => r.code).Distinct().ToArray());
            strCostCenterCode = "('" + strCostCenterCode + "')";

            string strSql = " SELECT DBNAME " +
                                 " , COSTCENTERCODE " +
                                 " , COSTCENTERNAME " +
                                 " , WAREHOUSECODE " +
                                 " , POITEMENDTIME " +
                                 " , RECIPENTSOFMENU " +
                                 " , STATUS " +
                                 " , POSIP " +
                                 " , POSDBNAME " +
                                 " , POSDBUSERNAME " +
                                 " , POSDBPASSWORD " +
                                 " , RECHARGESETTING " +
                              " FROM CCMAST (nolock) " +
                             " WHERE STATUS = '1' " +
                               " AND COSTCENTERCODE IN {0} ";

            List<CCMast> ccObj = SqlServerHelper.GetEntityList<CCMast>(SqlServerHelper.salesorderConn(), string.Format(strSql, strCostCenterCode));

            return ccObj;
        }

        // 根据成本中心取得对应的Sql
        public List<SqlMast> GetSqlMastInfo(List<SingleField> lstCostCenterCode, string sqlType)
        {
            // ProcessGuid条件
            //string strCostCenterCode = string.Join("','", lstCostCenterCode.Select(r => r.code).Distinct().ToArray());
            //strCostCenterCode = "('" + strCostCenterCode + "')";
            string strCostCenterCodes = "{0} " + string.Join(" or {0} ",
                lstCostCenterCode.Select(q => string.Format("'%,{0},%'", q.code)).ToArray());

            strCostCenterCodes = string.Format(strCostCenterCodes, "',' + replace(costcentercodes,' ','') + ',' like");

            string strSql = " SELECT SQLTYPE " +
                                 " , SQLCOMMAND " +
                                 " , SQLParams " +
                                 " , COSTCENTERCODES " +
                              " FROM SQLMAST (nolock) " +
                             " WHERE ({0}) " +
                               " AND SQLTYPE = '{1}' ";

            List<SqlMast> lstSqlObj = SqlServerHelper.GetEntityList<SqlMast>(SqlServerHelper.salesorderConn(),
                string.Format(strSql, strCostCenterCodes, sqlType));

            return lstSqlObj;
        }
    }
}
