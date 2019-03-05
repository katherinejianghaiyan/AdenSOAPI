using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.Reflection;

namespace Aden.Util.Common
{
    public static class BasicConverter
    {
        #region 尝试将对象转换为指定的类型,转换失败则返回默认值

        /// <summary>
        /// 尝试将对象转换为指定的类型,转换失败则返回默认值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="obj">待转换对象</param>
        /// <param name="defaultValue">转换失败返回的默认值</param>
        /// <returns>转换的目标对象</returns>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        ///     string str="123";
        ///     int ret=str.ToType<int>(0); //返回int:123
        ///     str="abc";
        ///     ret=str.ToType<int>(0); //返回0
        /// ]]>
        /// </code>
        /// </example>
        public static T ToType<T>(this object obj, T defaultValue)
        {
            if (obj == null || obj == DBNull.Value) return defaultValue;
            bool isIConvertible = obj is IConvertible;
            if (!isIConvertible) return defaultValue;
            var type = typeof(T);
            try
            {
                return (T)Convert.ChangeType(obj, type);
            }
            catch
            {
                if(obj.GetType() != type && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var valType = type.GetGenericArguments()[0];
                    try
                    {
                        return (T)Convert.ChangeType(obj, valType);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 尝试将对象转换为指定的类型,转换失败则返回default(T)
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="obj">待转换对象</param>
        /// <returns>转换的目标对象</returns>
        public static T ToType<T>(this object obj)
        {
            return ToType<T>(obj, default(T));
        }

        #endregion

        #region 将字符串转换为数字
        public static T ToNumber<T>(this object obj)
        {
            return obj.ToType<T>();
        }
        #region 将字符串转换为整数

        /// <summary>
        /// 尝试转换字符串为数字(int),并返回成功或失败
        /// </summary>
        /// <param name="str">待转换字符串</param>
        /// <param name="result">转换结果</param>
        /// <param name="defaultVal">转换失败,返回默认值</param>
        /// <returns>转换是否成功</returns>
        public static bool TryToInt32(this string str, out int result, int defaultVal = 0)
        {
            result = defaultVal;
            if (string.IsNullOrEmpty(str)) return false;
            if (Int32.TryParse(str, out result)) return true;
            return false;
        }

        /// <summary>
        /// 将字符串转换为整数(int),转换失败返回默认值
        /// </summary>
        /// <param name="str">待转换字符串</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns>返回转换后的值</returns>
        public static int ToInt32(this string str, int defaultVal = 0)
        {
            int result;
            if (str.TryToInt32(out result, defaultVal)) return result;
            else return defaultVal;
        }

        #endregion

        #region 将字符串转换为长整数(Long)

        /// <summary>
        /// 将字符串转换为数字(Int64),并返回成功或失败
        /// </summary>
        /// <param name="str">待转换字符</param>
        /// <param name="result">转换结果</param>
        /// <param name="defaultval">转换失败时,返回默认值</param>
        /// <returns>转换是否成功</returns>
        public static bool TryToLong(this string str, out long result, long defaultval = 0)
        {
            result = defaultval;
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            if (Int64.TryParse(str, out result))
            {
                return true;
            }
            result = defaultval;
            return false;
        }

        /// <summary>
        /// 将字符串转换为数字(Long)
        /// </summary>
        /// <param name="str">待转换字符</param>
        /// <param name="defaultval">转换失败时,返回默认值</param>
        /// <returns>返回转换后的值</returns>
        public static long ToLong(this string str, long defaultval = 0)
        {
            long result;
            str.TryToLong(out result, defaultval);
            return result;
        }

        #endregion

        #region 将字符串分割成int数组
        /// <summary>
        /// 将字符串分割成int数组
        /// </summary>
        /// <param name="str">待转换字符</param>
        /// <param name="splitChar">分隔符</param>
        /// <returns>返回整数数组</returns>
        public static int[] ToIntArray(this string str, string splitChar = ",")
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(splitChar)) return new int[0];
            string[] strArray = str.Split(new string[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length == 0) return new int[0];
            return Array.ConvertAll(strArray, s => s.ToInt32());
        }

        #endregion

        #region 将传入数组转换为string类型数组

        /// <summary>
        /// 将传入数组转换为string类型数组
        /// </summary>
        /// <typeparam name="TInput">待转换值类型</typeparam>
        /// <param name="inputArray">待转换数组</param>
        /// <returns>转换后的字符串型数组</returns>
        public static string[] ToStringArray<TInput>(TInput[] inputArray) where TInput:struct
        {
            if (inputArray == null) return null;
            return Array.ConvertAll(inputArray, delegate(TInput input)
            {
                return input.ToString();
            });
        }

        #endregion

        #endregion

        #region 字符串转换成Bool类型

        /// <summary>
        /// 将传入字符串转换为bool类型
        /// </summary>
        /// <param name="str">待转换字符串</param>
        /// <param name="defaultValue">转换错误,或者待转值为空时,返回的默认值</param>
        /// <returns>转换后的值</returns>
        public static bool ToBool(this string str, bool defaultValue = false)
        {
            if (str.IsNullOrEmptyAndTrim()) return defaultValue;
            bool result = defaultValue;
            string input = str.ToLower();
            if (input == "1" || input == "true") return true;
            if (input == "0" || input == "false") return false;
            return result;
        }

        #endregion

        #region 传入值转换成数字

        /// <summary>
        /// 将传入值转换成float类型
        /// </summary>
        /// <param name="input">传入值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>转换后的值</returns>
        public static float ToFloat(this object input, float defaultValue = 0f)
        {
            if (input == null) return defaultValue;
            return input.ToType<float>(defaultValue);
        }
        
        /// <summary>
        /// 将传入值转换成double类型
        /// </summary>
        /// <param name="input">传入值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>转换后的值</returns>
        public static double ToDouble(this object input, double defaultValue = 0.0)
        {
            if (input == null) return defaultValue;
            return input.ToType<double>(defaultValue);
        }

        /// <summary>
        /// 将传入值转换成decimal类型
        /// </summary>
        /// <param name="input">传入值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>转换后的返回值</returns>
        public static decimal ToDecimal(this object input, decimal defaultValue = 0M)
        {
            if (input == null) return defaultValue;
            return input.ToType<decimal>(defaultValue);
        }

        /// <summary>
        /// 将传入值转换成int类型
        /// </summary>
        /// <param name="input">待转换值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>转换后的值</returns>
        public static int ToInt(this object input, int defaultValue = 0)
        {
            if (input == null) return defaultValue;
            return input.ToType<int>(defaultValue);
        }

        #endregion

        #region 日期转换扩展

        #region 字符串转换为日期

        /// <summary>
        /// 将字符串转换为指定格式的日期
        /// </summary>
        /// <param name="str">日期字符串</param>
        /// <param name="result">转换成功的结果,如果失败则是1900-01-01</param>
        /// <param name="format">需要转换的格式</param>
        /// <returns>转换的结果</returns>
        public static bool TryToDate(this string str, out DateTime result, string format = "yyyy-MM-dd")
        {
            result = DateTime.MinValue;
            if (string.IsNullOrEmpty(str)) return false;
            if (DateTime.TryParse(str, out result))
            {
                if (DateTime.TryParseExact(str, format, System.Globalization.DateTimeFormatInfo.InvariantInfo,
                    System.Globalization.DateTimeStyles.None, out result)) return true;
                return false;
            }
            return false;
        }

        /// <summary>
        /// 字符串转换成指定格式的日期
        /// </summary>
        /// <param name="str">日期格式字符串</param>
        /// <param name="format">指定的日期格式</param>
        /// <returns>转换后的日期值</returns>
        public static DateTime ToDateTime(this string str, string format = "yyyy-MM-dd")
        {
            DateTime result;
            if (str.TryToDate(out result, format))
            {
                return result;
            }
            return result;
        }

        #endregion

        #region 日期转整数日期

        /// <summary>
        /// 将日期按照指定日期格式转换成数字
        /// </summary>
        /// <param name="dateTime">待转换的日期</param>
        /// <param name="format">转换格式</param>
        /// <returns>转换结果,转换失败则返回0</returns>
        public static int ToDateTimeInt(this DateTime dateTime, string format = "yyMMddHHmm")
        {
            return dateTime.ToString(format, System.Globalization.DateTimeFormatInfo.InvariantInfo).ToInt32(0);
        }

        /// <summary>
        /// 转换日期部分
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <returns>日期部分整数</returns>
        public static int ToDateInt(this DateTime dateTime)
        {
            return dateTime.ToDateTimeInt("yyyyMMdd");
        }

        /// <summary>
        /// 转换时间部分
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <returns>时间部分整数</returns>
        public static int ToTimeInt(this DateTime dateTime)
        {
            return dateTime.ToDateTimeInt("HHmmss");
        }

        #endregion

        #region 整数转日期

        /// <summary>
        /// 将整数类型的日期部分和时间部分合并成日期类型，
        /// </summary>
        /// <param name="datePart">日期部分，如 20140313</param>
        /// <param name="timePart">如：91255 或101123</param>
        /// <returns>日期</returns>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        /// int date=20140828;
        /// int time=92359;
        /// 
        /// DateTime dt=DateTimeHelper.ToDateTime(date,time)    //返回 2014-08-28 09:23:59
        /// ]]>
        /// </code>
        /// </example>
        public static DateTime ToDateTime(int datePart, int timePart)
        {
            if (datePart < 19000101) return DateTime.MinValue;
            else if (timePart > 99991231) return DateTime.MaxValue;

            string strDateTime = string.Concat(datePart.ToString("####-##-##"), timePart.ToString().PadLeft(6, '0').Insert(2, ":").Insert(5, ":"));
            return Convert.ToDateTime(strDateTime);
        }

        /// <summary>
        /// 将8位整数转换为日期(yyyyMMdd),转换失败返回1900-01-01
        /// </summary>
        /// <param name="intDate">数字</param>
        /// <param name="dateOut">日期</param>
        /// <returns>转换后的日期</returns>
        public static DateTime ToDate(this int intDate)
        {
            if (intDate.ToString().Length != 8) return DateTime.MinValue;
            DateTime outDate;
            if (intDate.ToString("####-##-##").TryToDate(out outDate)) return outDate;
            return DateTime.MinValue;
        }

        #endregion

        #region 其他日期扩展

        /// <summary>
        /// 获取某年的第一天
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfThisYear(this DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }

        /// <summary>
        /// 获得某季度的第一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>某季度的第一天</returns>
        public static DateTime GetFirstDayOfThisQuarter(this DateTime date)
        {
            return new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1);
        }

        /// <summary>
        /// 获得某月的第一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>某月的第一天</returns>
        public static DateTime GetFirstDayOfThisMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// 获得某周的第一天
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>某周的第一天</returns>
        public static DateTime GetFirstDayOfThisWeek(this DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Sunday)
                return date.Date;

            return date.Date.AddDays(-(int)date.DayOfWeek);
        }

        /// <summary>
        /// 获取时间 是一年中的第几个星期
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>获取时间 是一年中的第几个星期</returns>
        public static int WeekOfYear(this DateTime date)
        {
            var firstDay = new DateTime(date.Year, 1, 1);
            var days = (date - firstDay).Days + (int)firstDay.DayOfWeek;
            return days / 7 + 1;
        }

        /// <summary>
        /// 将DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="dt">时间</param>
        /// <param name="milliseconds">是否精确到毫秒</param>
        /// <returns> Unix时间戳</returns>
        public static long ToUnixTime(this DateTime dt, bool milliseconds = false)
        {
            double result;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            if (milliseconds) result = (dt - startTime).TotalMilliseconds;
            else result = (dt - startTime).TotalSeconds;
            return (long)result;
        }

        /// <summary>
        /// 将Unix时间戳转换为DateTime类型时间
        /// </summary>
        /// <param name="unixTime">Unix时间戳</param>
        /// <param name="milliseconds">是否精确到毫秒</param>
        /// <returns>返回日期</returns>
        public static DateTime ConvertTimeByUnix(this long unixTime, bool milliseconds = false)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            if (milliseconds) return startTime.AddMilliseconds(unixTime);
            else return startTime.AddSeconds(unixTime);
        }

        #endregion

        #endregion

        #region 字符串扩展

        #region 转全角

        /// <summary>
        /// 转全角(SBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>全角字符串</returns>
        public static string ToSBC(this string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }
        #endregion

        #region 转半角

        /// <summary>
        /// 转半角(DBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        public static string ToDBC(this string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        #endregion

        #region 移除不可见字符

        /// <summary>
        /// 移除不可见字符
        /// </summary>
        /// <param name="str">待操作字符串</param>
        /// <returns></returns>
        public static string RemoveInvisibleChar(this string str)
        {
            if (str.IsNullOrEmptyAndTrim())
            {
                return str;
            }
            var sb = new System.Text.StringBuilder(131);
            for (int i = 0; i < str.Length; i++)
            {
                int Unicode = str[i];
                if (Unicode >= 16)
                {
                    sb.Append(str[i].ToString());
                }
            }
            return sb.ToString();
        }

        #endregion

        #region 编码与解码 UrlEncode And UrlDecode

        /// <summary>
        /// UrlEncode Url编码
        /// </summary>
        /// <param name="str">待编码字符串</param>
        /// <param name="encoding">编码规则</param>
        /// <returns>编码后的字符串</returns>
        public static string UrlEncode(this string str, Encoding encoding)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return HttpUtility.UrlEncode(str, encoding);
        }
        /// <summary>
        /// UrlEncode Url编码,默认UTF-8编码
        /// </summary>
        /// <param name="str">待编码字符串</param>
        /// <returns>编码后的字符串</returns>
        public static string UrlEncode(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return str.UrlEncode(Encoding.UTF8);
        }

        /// <summary>
        /// UrlDecode Url解码
        /// </summary>
        /// <param name="str">待解码字符串</param>
        /// <param name="encoding">解码规则</param>
        /// <returns>解码后的字符串</returns>
        public static string UrlDecode(this string str, Encoding encoding)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return HttpUtility.UrlDecode(str, encoding);
        }
        /// <summary>
        /// UrlDecode Url解码,默认UTF-8编码
        /// </summary>
        /// <param name="str">待解码字符串</param>
        /// <returns>解码后的字符串</returns>
        public static string UrlDecode(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return str.UrlDecode(Encoding.UTF8);
        }

        /// <summary>
        /// 超级解码(不用关心字符是UTF-8、Unicode或GB2312编码格式)，在分析不同搜索引擎来源的关键词时特别有用
        /// </summary>
        /// <param name="str">待解码字符串</param>
        /// <returns></returns>
        public static string SuperDecode(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            /* Unicode 编码时，由 %uxxx组成
             * UTF-8 时 一个汉字需要三组 %xx组成 符号需要一组%xx;
             * GB2312 时 一个汉字需要两组 %xx组成
             * 首先如果符合Unicode编码要求，则直接使用Unicode解码
             * 其次尝试用UTF-8解码，如果成功，则解码后的字符串长度等于 %xx组数/3+非编码字符串长度
             * 否则使用GB2312进行解码             * 
             * */
            const string flags = "%20%40%23%24%25%5E%26%2B%7B%7D%5B%5D%3B%22%2F%5C%3A%3F%3E%3C%09%2C%7C%27";  //半角符号
            var unicode = "%u([\\dABCDEF]{4})?";
            if (Regex.Match(str, unicode, RegexOptions.IgnoreCase).Success)
            {
                return System.Web.HttpUtility.UrlDecode(str, System.Text.Encoding.Unicode);
            }

            var pattern = "%([\\dABCDEF]{2})?";
            int matchcount = 0;
            int mode = 0;
            string tmp = Regex.Replace(str, pattern, new MatchEvaluator(match =>
            {
                if (flags.IndexOf(match.Groups[0].Value.ToLower(), StringComparison.OrdinalIgnoreCase) > -1)
                {
                    mode++;
                }
                else
                {
                    matchcount++;
                }
                return match.Result("");
            }), RegexOptions.IgnoreCase);

            string w = System.Web.HttpUtility.UrlDecode(str, System.Text.Encoding.UTF8);
            if (w.Length == (matchcount / 3 + mode + tmp.Length))
                return w;
            return System.Web.HttpUtility.UrlDecode(str, System.Text.Encoding.GetEncoding("GB2312"));
        }

        #endregion

        #endregion

        #region 对象扩展

        /// <summary>
        /// 将对象转换成字符串,并去除字符串首尾空格
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>结果</returns>
        public static string ToStringTrim(this object obj)
        {
            if (obj == null) return string.Empty;
            else return obj.ToString().Trim();
        }

        /// <summary>
        /// 判断类型是否为数字型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static bool IsNumber(this Type type)
        {
            if (type == typeof(double) || type == typeof(decimal) || type == typeof(int)) return true;
            return false;
        }

        public static bool IsAggregate(this string str)
        {
            if (string.IsNullOrEmpty(str)) return false;
            if (str.Replace(" ", "").ToLower().StartsWith("sum(")) return true;
            return false;
        }

        public static string[] SplitBy(this string str, params string[] splits)
        {
            if (string.IsNullOrEmpty(str)) return null;

            return str.Split(splits, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitBy(this string str, string split)
        {
            if (string.IsNullOrEmpty(str)) return null;

            return str.SplitBy(new string[] { split });
        }

        public static string SplitBy(this string str, string split, int index)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            if (!str.Contains(split)) return str.ToStringTrim();

            string[] arr = str.SplitBy(split);

            if (arr.Length > index) return arr[index].ToStringTrim();

            return str.ToStringTrim();
        }

        public static string SplitByAs(this string str, string type)
        {
            if (type == "Formula") return str.SplitBy(" as ", 0);
            if (type == "Field") return str.SplitBy(" as ", 1);

            return str;
        }

        #endregion

        #region 对象转换为字典
        /// <summary>
        /// 对象转换为字典
        /// </summary>
        /// <param name="obj">待转化的对象</param>
        /// <param name="isIgnoreNull">是否忽略NULL 这里我不需要转化NULL的值，正常使用可以不穿参数 默认全转换</param>
        /// <returns></returns>
        public static Dictionary<string, object> ObjectToMap(object obj, bool isIgnoreNull = false)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            Type t = obj.GetType(); // 获取对象对应的类， 对应的类型

            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性

            foreach (PropertyInfo p in pi)
            {
                MethodInfo m = p.GetGetMethod();

                if (m != null && m.IsPublic)
                {
                    // 进行判NULL处理 
                    if (m.Invoke(obj, new object[] { }) != null || !isIgnoreNull)
                    {
                        map.Add(p.Name, m.Invoke(obj, new object[] { })); // 向字典添加元素
                    }
                }
            }
            return map;
        }
        #endregion
    }
}
