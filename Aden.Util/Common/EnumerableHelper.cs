using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace Aden.Util.Common
{
    public static class EnumerableHelper
    {
        #region 集合转换成List<dynamic>，删除多余字段
        public static dynamic ToDynamic<T>(this T data, string fieldsGUID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fieldsGUID)) throw new Exception();
                dynamic d = GetParamsForDynamic(fieldsGUID);
                return data.ToDynamic((string[])d.fields, (Dictionary<string,string>)d.listFields);
            }
            catch(Exception e)
            {
                return data;
            }
        }

        public static IEnumerable<dynamic> ToDynamicList<T>(this IEnumerable<T> data, string fieldsGUID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fieldsGUID)) throw new Exception();
                dynamic d = GetParamsForDynamic(fieldsGUID);
                return data.Select(q=> q.ToDynamic((string[])d.fields, (Dictionary<string, string>)d.listFields));
            }
            catch
            {
                return (IEnumerable<dynamic>)data;
            }
        }


        private static dynamic GetParamsForDynamic(string fieldsGUID)
        {
            try
            {
                string sql = "select val1,val2 from tblAPIFunConfigs where guid='{0}'";
                DataTable dt = Database.SqlServerHelper.GetDataTable(Database.SqlServerHelper.salesorderConn(),
                    string.Format(sql, fieldsGUID));
                if (dt == null || dt.Rows.Count == 0) return new Exception("No define");

                string[] fields = dt.Rows[0]["val1"].ToStringTrim().Split(new char[] { ',', ';' });

                Dictionary<string, string> tmp = dt.Rows[0]["val2"].ToStringTrim().JSDeserialize<Dictionary<string, string>>();
                Dictionary<string, string> lstFields = null;
                if (tmp != null) lstFields = tmp.ToDictionary(a => a.Key, b => b.Value);

                return new { fields = fields, listFields = lstFields };
            }
            catch (Exception e)
            {
                throw e;
            }

        }


        /// <summary>
        /// 集合转换成List<dynamic>
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="data">对象</param>
        /// <param name="fields">需要保留的字段</param>
        /// <param name="listFields">需要保留的字段(多行记录)</param>
        /// <returns>List<dynamic></returns>
        private static dynamic ToDynamic<T>(this T data, string[] fields,Dictionary<string, string> listFields = null)
        {
            try
            {
                Type type = typeof(T);
                //匹配需要的Property
                PropertyInfo[] properties = type.GetProperties()
                    .Join(fields, a => a.Name.ToLower(), b => b.ToLower(), (a, b) => a).ToArray();

                PropertyInfo[] lstProperties = null;
                if (listFields != null && listFields.Any())
                    lstProperties = type.GetProperties()
                        .Join(listFields, a => a.Name.ToLower(), b => b.Key.ToLower(), (a, b) => a).ToArray();

                if (!properties.Any() && (lstProperties == null || !lstProperties.Any()))
                    throw new Exception("No exist your defined fields.");

                dynamic model = new System.Dynamic.ExpandoObject();
                var dic = (IDictionary<string, object>)model;

                foreach (PropertyInfo p in properties)
                {
                    object o = p.GetValue(data, null);
                    dic[fields.FirstOrDefault(b => b.ToLower() == p.Name.ToLower())] = o;
                }

                if (lstProperties != null && lstProperties.Any())
                    foreach (PropertyInfo p in lstProperties)
                    {
                        object o = p.GetValue(data, null);

                        dynamic v = null;
                        if (o != null)
                        {
                            string s = listFields.FirstOrDefault(b => b.Key.ToLower() == p.Name.ToLower()).Value;
                            try
                            {
                                v = ((IEnumerable)o).AsQueryable().Select(string.Format("new ({0})", s));
                            }
                            catch
                            {
                                v = o;
                            }

                        }
                        dic[listFields.FirstOrDefault(b => b.Key.ToLower() == p.Name.ToLower()).Key] = v;
                           
                    }

                return model;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static IQueryable ToSimpleList<T>(this IEnumerable<T> data, string fields, Dictionary<string,string> listFields = null)
        {
            if (data == null || !data.Any()) return null;
            try
            {
                if (listFields == null || !listFields.Any()) return data.ToSimpleList(fields);

                fields = string.Format("{0},{1}", fields, string.Join(",", listFields.Select(q => q.Key)));
                if (fields.StartsWith(",")) fields = fields.Substring(1);
                var qry = data.ToSimpleList(fields);

                PropertyInfo[] lstProperties = qry.ElementType.GetProperties()
                        .Join(listFields, a => a.Name.ToLower(), b => b.Key.ToLower(), (a, b) => a).ToArray();
                foreach(var q in qry)
                {
                    foreach (PropertyInfo p in lstProperties)
                    {
                        object o = p.GetValue(q, null);
                        dynamic v = null;

                        if(o != null)                 
                            v = ((IEnumerable<dynamic>)Convert.ChangeType(o, p.PropertyType)).ToSimpleList(listFields.FirstOrDefault(b => b.Key.ToLower() == p.Name.ToLower()).Value);
                        p.SetValue(q, v, null);
                    }
                }

                return null;// qry.ToSimpleList(fields);               
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        private static IQueryable ToSimpleList<T>(this IEnumerable<T> data, string fields)
        {
            if (data == null || !data.Any()) return null;
            try
            {
                if (string.IsNullOrWhiteSpace(fields)) return data.AsQueryable();

                //只能用，分隔
                fields = fields.Replace(';', ',');
                return data.AsQueryable().Select(string.Format("new ({0})", fields));

            }
            catch
            {
                return data.AsQueryable();
            }
        }

        #endregion
        /// <summary>
        /// 集合中是否包含满足条件的某项
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">集合对象</param>
        /// <param name="predicate">判定条件</param>
        /// <returns>是否存在</returns>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        ///     List<string> list={"1","3","5","7"};
        ///     
        ///     bool ret=list.Contains(c=>c=="3");  //True
        /// ]]>
        /// </code>
        /// </example>
        public static bool Contains<T>(this IEnumerable<T> list, Predicate<T> predicate)
        {
            return list.Any(item => predicate(item));
        }

        /// <summary>
        /// 对IEnumerable&lt;T&gt;的每个元素执行指定操作
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">集合对象</param>
        /// <param name="action">要执行的操作</param>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        ///     string[] list={"1","3","5","7"};
        ///     list.ForEach(c=>{
        ///         Response.Write(c);
        ///     });
        /// ]]>
        /// </code>
        /// </example>
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }

        /// <summary>
        /// 对IEnumerable的每个元素执行指定操作
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">集合对象</param>
        /// <param name="action">要执行的操作</param>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        ///     string[] list={"1","3","5","7"};
        ///     list.ForEach(c=>{
        ///         Response.Write(c);
        ///     });
        /// ]]>
        /// </code>
        /// </example>
        public static void ForEach<T>(this IEnumerable list, Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }

        /// <summary>
        ///  在指定  IEnumerable`T 的每个元素之间串联指定的分隔符 System.String，从而产生单个串联的字符串。
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">集合对象</param>
        /// <param name="toText">将T转换为文本</param>
        /// <param name="separator">分隔符</param>
        /// <returns>拼接字符串</returns>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        ///     List<DateTime> list=new List<DateTime>();
        ///     
        ///     string ret=list.Join(d=>d.ToString("yyyyMMdd"),",");
        /// 
        /// ]]>
        /// </code>
        /// </example>
        public static string Join<T>(this IEnumerable<T> list, Func<T, string> toText, string separator)
        {
            if (list == null)
                return null;

            StringBuilder sb = new StringBuilder();
            var enumtor = list.GetEnumerator();
            if (enumtor.MoveNext())
            {
                sb.Append(toText(enumtor.Current));
                while (enumtor.MoveNext())
                {
                    sb.Append(separator);
                    sb.Append(toText(enumtor.Current));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 在指定  IEnumerable`T 的每个元素之间串联指定的分隔符 System.String，从而产生单个串联的字符串。
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">集合对象</param>
        /// <param name="separator">分割字符</param>
        /// <returns>拼接字符串</returns>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        ///     int[] ary={1,2,3,4,5};
        ///     
        ///     string str=ary.Join();  //返回："1,2,3,4,5"
        /// ]]>
        /// </code>
        /// </example>
        public static string Join<T>(this IEnumerable<T> list, string separator = ",")
        {
            if (list == null)
                return null;

            return list.Join((T v) => v.ToString(), separator);
        }

        /// <summary>
        /// 通过使用指定的 System.Collections.Generic.IEqualityComparer&lt;T&gt; 对值进行比较返回序列中的非重复元素。
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="V">比较器类型</typeparam>
        /// <param name="source">集合对象</param>
        /// <param name="keySelector">元素比较方法</param>
        /// <param name="comparer">比较器</param>
        /// <returns>去除重复元素的集合</returns>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        ///     UserInfo[] users=new UserInfo[10];
        ///     ary.Distinct(c => c.Name,StringComparer.OrdinalIgnoreCase);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector, IEqualityComparer<V> comparer)
        {
            return source.Distinct(new CommonEqualityComparer<T, V>(keySelector, comparer));
        }

        /// <summary>
        /// 通过使用指定的 System.Collections.Generic.IEqualityComparer&lt;T&gt; 对值进行比较返回序列中的非重复元素。
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <typeparam name="V">比较器类型</typeparam>
        /// <param name="source">集合对象</param>
        /// <param name="keySelector">比较器</param>
        /// <returns>去重后的集合</returns>
        /// <example>
        /// <code lang="c#">
        /// <![CDATA[
        ///     UserInfo[] users=new UserInfo[10];
        ///     ary.Distinct(c => c.Name);
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector)
        {
            return source.Distinct(keySelector, EqualityComparer<V>.Default);
        }
    }
}
