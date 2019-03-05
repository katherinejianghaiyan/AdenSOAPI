using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Util.Database;
using Aden.Model.SOMastData;

namespace Aden.DAL.SalesOrder
{
    class Common
    {
        /// <summary>
        /// 日期格式化方法
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string convertDateTime(string strDt, bool end = false)
        {
            DateTime dt = Convert.ToDateTime(strDt);
            //if (end) dt = dt.AddDays(1).AddSeconds(-1);
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static string convertDateTime(DateTime dt, bool end = false)
        {
            //if (end) dt = dt.AddDays(1).AddSeconds(-1);
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// 根据日期取得日期所属月份的最后一天是几号
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int GetLastDayofMonth(DateTime dt, ref DateTime dtLastDayOfMonth)
        {
            // 年
            int intYear = dt.Year;
            // 月
            int intMonth = dt.Month;
            // 日
            int intDay = 0;

            // 取得日期月份的第一天
            dtLastDayOfMonth = new DateTime(intYear, intMonth, 1);
            // 取得日期月份的最后一天
            dtLastDayOfMonth = dtLastDayOfMonth.AddMonths(1).AddDays(-1);
            // 取得具体是几号
            intDay = dtLastDayOfMonth.Day;

            return intDay;
        }

        /// <summary>
        ///  根据CostCenter信息取得对应的ServiceType信息
        /// </summary>
        /// <param name="costCenterCode">成本中心</param>
        /// <param name="serviceType">服务类型</param>
        /// <param name="serviceTypeValue">服务类型Code</param>
        /// <param name="lstServiceType">服务类型主数据</param>
        public static void getServiceType(string costCenterCode, ref string serviceType, ref string serviceTypeValue, 
            List<ServiceType> lstServiceType = null)
        {
            // 检查成本中心正确性
            if (costCenterCode == null || String.Empty.Equals(costCenterCode.Length))
            {
                return;
            }

            // 检查服务类型主数据是否存在
            if (lstServiceType == null || lstServiceType.Count == 0)
            {
                string strSql = " SELECT * " +
                                  " FROM ServiceTypeData ";

                lstServiceType = SqlServerHelper.GetEntityList<ServiceType>(SqlServerHelper.salesorderConn(), strSql);

                if (lstServiceType == null || lstServiceType.Count == 0) return;
            }

            string strVal = costCenterCode.Substring(costCenterCode.Length - 2, 2);
            ServiceType st = lstServiceType.First(r => r.add1 == strVal);

            serviceType = st.name;
            serviceTypeValue = st.add2;
        }
    }
}
