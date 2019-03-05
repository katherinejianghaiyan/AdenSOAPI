using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Util.Database;
using Aden.Model.SOMastData;
using System.Text.RegularExpressions;

namespace Aden.Util.AdenCommon
{
    public class MastData
    {
        /// <summary>
        /// 根据action取得MenuGuid id Sql查询条件
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static string formatActionToMenuGuid(string action)
        {
            string[] sArray = Regex.Split(action, "-", RegexOptions.IgnoreCase);

            string strResult = string.Empty;

            List<TblMenu> lstTblMenu = new List<TblMenu>();

            string strSqlParent = " SELECT GUID " +
                                    " FROM TBLMENU " +
                                   " WHERE ACTION = '" + action + "' " +
                                   " UNION " +
                                  " SELECT C.GUID " +
                                    " FROM TBLMENU AS C " +
                                   " INNER JOIN TBLMENU AS P " +
                                      " ON C.PGUID = P.GUID " +
                                   " WHERE P.ACTION = '" + action + "' ";
            string strSqlChild = " SELECT * " +
                                   " FROM TBLMENU " +
                                  " WHERE ACTION = '" + action + "' ";

            // 子的情况
            if (sArray.Length > 1)
            {
                lstTblMenu = SqlServerHelper.GetEntityList<TblMenu>(SqlServerHelper.salesorderConn(), strSqlChild);
                strResult = "('" + lstTblMenu[0].guid + "','" + lstTblMenu[0].pguid + "')";
            }
            else
            {
                lstTblMenu = SqlServerHelper.GetEntityList<TblMenu>(SqlServerHelper.salesorderConn(), strSqlParent);
                strResult = string.Join("','", lstTblMenu.Select(r => r.guid).Distinct().ToArray());
                strResult = "('" + strResult + "')";
            }
            return strResult;
        }
    }
}
