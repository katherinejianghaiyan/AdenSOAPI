using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Util.AdenCommon
{
    public class DateCommon
    {
        /// 日期格式化方法
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string convertDateTime(string strDt, string formatStr = "")
        {
            if (string.IsNullOrWhiteSpace(formatStr))
                formatStr = "yyyy-MM-dd HH:mm:ss.fff";
            DateTime dt = Convert.ToDateTime(strDt);
            return dt.ToString(formatStr);
        }

        public static string convertDateTime(DateTime dt, string formatStr = "" )
        {
            if (string.IsNullOrWhiteSpace(formatStr))
                formatStr = "yyyy-MM-dd HH:mm:ss.fff";
            return dt.ToString(formatStr);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="date"></param>
        /// <param name="lang"></param>
        /// <param name="formatString"></param>
        /// <returns></returns>
        public static string ToWeekDay(DateTime date, string lang = "cn", string formatString = "dddd")
        {
            string result = string.Empty;
            if ("cn".Equals(lang))
                result = date.ToString(formatString, new System.Globalization.CultureInfo("zh-cn"));
            else if ("en".Equals(lang))
                result = date.ToString(formatString);

            return result;
        }
    }
}
