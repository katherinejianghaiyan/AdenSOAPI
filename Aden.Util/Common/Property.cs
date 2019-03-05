using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Aden.Util.Common
{
    public static class Property
    {
        /// <summary>
        /// 当为Null时返回空
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string GetString(this string obj)
        {
            string strEmpty = string.Empty;
            return obj == null ? strEmpty : obj.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="strPropertyName"></param>
        /// <returns></returns>
        public static string GetProperty(this Object obj, string strPropertyName)
        {
            return obj.GetProperties(new string[] { strPropertyName}).FirstOrDefault();
        }
        public static string[] GetProperties(this Object obj, string[] strPropertyNames)
        {
            // 取得对象类型
            Type t = obj.GetType();
            // 取得属性对象
            List<string> list = new List<string>();
            foreach(string s in strPropertyNames)
            {
                PropertyInfo propertyInfo = t.GetProperty(s,BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (propertyInfo.GetValue(obj, null) == null) continue;

                list.Add(propertyInfo.GetValue(obj, null).ToString());
            }
            
            return list.ToArray();
        }
    }
}
