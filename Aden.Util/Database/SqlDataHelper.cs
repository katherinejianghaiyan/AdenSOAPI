using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Data;

namespace Aden.Util.Database
{
    public static class SqlDataHelper
    {
        /// <summary>
        /// 将SqlDataReader对象装载到实体对象
        /// </summary>
        /// <typeparam name="T">实体对象类型</typeparam>
        /// <param name="dr">SqlDataReader对象</param>
        /// <param name="action">自定义操作(实体,SqlDataReader对象)</param>
        /// <returns>实体对象</returns>
        public static T ToEntity<T>(this SqlDataReader dr, Action<T, SqlDataReader> action = null)
        {
            if (dr == null || !dr.HasRows) return default(T);
            IDictionary<string, Func<T, object, object>> dic = GetMap<T>();
            if (dr.Read())
            {
                T model = Activator.CreateInstance<T>();
                LoadEntity(dr, dic, ref model);
                action?.Invoke(model, dr);
                return model;
            }
            return default(T);
        }

        public static DataSet GetDataSet(string salesorderConn, object sqls)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 将SqlDataReader对象装载到实体对象
        /// </summary>
        /// <typeparam name="T">实体对象类型</typeparam>
        /// <param name="dr">SqlDataReader对象</param>
        /// <param name="action">自定义操作(实体,SqlDataReader对象)</param>
        /// <param name="model">实体对象</param>      
        public static void ToEntity<T>(this SqlDataReader dr, ref T model, Action<T, SqlDataReader> action = null)
        {
            if (dr == null || !dr.HasRows) return;
            IDictionary<string, Func<T, object, object>> dic = GetMap<T>();
            if (dr.Read())
            {
                if (model.Equals(null)) model = Activator.CreateInstance<T>();
                LoadEntity(dr, dic, ref model);
                action?.Invoke(model, dr);
            }
        }

        /// <summary>
        /// 将SqlDataReader对象装载到实体对象集合
        /// </summary>
        /// <typeparam name="T">实体对象类型</typeparam>
        /// <param name="dr">SqlDataReader对象</param>
        /// <param name="action">自定义操作(实体,SqlDataReader对象)</param>
        /// <returns>实体对象集合</returns>
        public static List<T> ToEntityList<T>(this SqlDataReader dr, Action<T, SqlDataReader> action = null)
        {
            if (dr == null || !dr.HasRows)
            {
                return null;
            }
            List<T> list = new List<T>();
            IDictionary<string, Func<T, object, object>> dic = GetMap<T>();
            while (dr.Read())
            {
                T model = Activator.CreateInstance<T>();
                LoadEntity(dr, dic, ref model);
                action?.Invoke(model, dr);
                list.Add(model);

            }
            return list;
        }

        /// <summary>
        /// 将SqlDataReader对象装载到dynamic对象集合
        /// </summary>
        /// <param name="dr">SqlDataReader对象</param>
        /// <returns>实体对象集合</returns>
        public static List<dynamic> ToDynamicList(this SqlDataReader dr)
        {
            if (dr == null || !dr.HasRows)
            {
                return null;
            }
            List<dynamic> list = new List<dynamic>();
            while (dr.Read())
            {
                dynamic model = new System.Dynamic.ExpandoObject();
                var dic = (IDictionary<string, object>)model;
                
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string fieldName = dr.GetName(i);
                    object val = dr[fieldName];
                    if (val == null || val == DBNull.Value) val = "";
                    dic[fieldName] = val;
                }

                list.Add(model);
            }
            return list;
        }

        

        /// <summary>
        /// 将DataTable对象装载到实体对象集合
        /// </summary>
        /// <typeparam name="T">实体对象类型</typeparam>
        /// <param name="dt">DataTable对象</param>
        /// <param name="action">自定义操作(实体,SqlDataReader对象)</param>
        /// <returns>实体对象集合</returns>
        public static List<T> ToEntityList<T>(this DataTable dt, Action<T, DataRow> action = null)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }
            List<T> list = new List<T>();
            IDictionary<string, Func<T, object, object>> dic = GetMap<T>();
            foreach (DataRow dr in dt.Rows)
            {
                T model = Activator.CreateInstance<T>();
                LoadEntity(dr, dic, ref model);
                action?.Invoke(model, dr);
                list.Add(model);
            }
            return list;
        }

        /// <summary>
        /// 将DataRow集合装载到实体对象集合
        /// </summary>
        /// <typeparam name="T">实体对象类型</typeparam>
        /// <param name="rowList">DataRow集合</param>
        /// <param name="action">自定义操作(实体,SqlDataReader对象)</param>
        /// <returns>实体对象集合</returns>
        public static List<T> ToEntityList<T>(this List<DataRow> rowList, Action<T, DataRow> action = null)
        {
            if (rowList == null || rowList.Count == 0)
            {
                return null;
            }
            List<T> list = new List<T>();
            IDictionary<string, Func<T, object, object>> dic = GetMap<T>();
            foreach (DataRow dr in rowList)
            {
                T model = Activator.CreateInstance<T>();
                LoadEntity(dr, dic, ref model);
                action?.Invoke(model, dr);
                list.Add(model);
            }
            return list;
        }

        #region 装载实体

        /// <summary>
        /// 装载实体
        /// </summary>
        /// <typeparam name="T">实体对象类型</typeparam>
        /// <param name="dr">SqlDataReader对象</param>
        /// <param name="dic">缓存字典</param>
        /// <param name="model">返回实体</param>
        private static void LoadEntity<T>(SqlDataReader dr, IDictionary<string, Func<T, object, object>> dic, ref T model)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                string fieldName = dr.GetName(i);
                if (dic.ContainsKey(fieldName))
                {
                    object val = dr[fieldName];
                    if (val != null && val != DBNull.Value)
                    {
                        Func<T, object, object> fc = dic[fieldName];
                        fc(model, dr[fieldName]);
                    }
                }
            }
        }

       

        /// <summary>
        /// 装载实体
        /// </summary>
        /// <typeparam name="T">实体对象类型</typeparam>
        /// <param name="dr">DataRow行</param>
        /// <param name="dic">缓存字典</param>
        /// <param name="model">返回实体</param>
        private static void LoadEntity<T>(DataRow dr, IDictionary<string, Func<T, object, object>> dic, ref T model)
        {
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string fieldName = dr.Table.Columns[i].ColumnName;
                if (dic.ContainsKey(fieldName))
                {
                    object val = dr[fieldName];
                    if (val != null && val != DBNull.Value)
                    {
                        Func<T, object, object> fc = dic[fieldName];
                        fc(model, dr[fieldName]);
                    }
                }
            }
        }

        #endregion

        #region 映射并创建实体
        static Func<T, object, object> SetDelegate<T>(MethodInfo m, Type type)
        {
            ParameterExpression paramObj = Expression.Parameter(typeof(T), "obj");
            ParameterExpression paramVal = Expression.Parameter(typeof(object), "val");
            UnaryExpression bodyVal = Expression.Convert(paramVal, type);
            MethodCallExpression body = Expression.Call(paramObj, m, bodyVal);
            Action<T, object> set = Expression.Lambda<Action<T, object>>(body, paramObj, paramVal).Compile();

            return (instance, v) =>
            {
                set(instance, v);
                return null;
            };
        }

        static IDictionary<Type, object> dicCache = new Dictionary<Type, object>();

        private static object lockobj = new object();
        static IDictionary<string, Func<T, object, object>> GetMap<T>()
        {
            object dic;
            Type type = typeof(T);

            if (!dicCache.TryGetValue(type, out dic))
            {
                lock (lockobj)
                {
                    if (!dicCache.TryGetValue(type, out dic))
                    {
                        return InitMap<T>(type);
                    }
                }
            }
            return (IDictionary<string, Func<T, object, object>>)dic;
        }

        /// <summary>
        /// 初始化对象映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IDictionary<string, Func<T, object, object>> InitMap<T>(Type type)
        {
            var _dic = new Dictionary<string, Func<T, object, object>>(StringComparer.OrdinalIgnoreCase);
            PropertyInfo[] ps =
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.IgnoreCase);
            foreach (PropertyInfo pi in ps)
            {
                MethodInfo setMethodInfo = pi.GetSetMethod(true);
                if (setMethodInfo == null)
                    continue;
                Func<T, object, object> func = SetDelegate<T>(setMethodInfo, pi.PropertyType);
                _dic.Add(pi.Name, func);
            }
            dicCache[type] = _dic;
            return _dic;
        }

        #endregion
    }
}
