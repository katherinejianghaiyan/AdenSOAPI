using System.IO;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace Aden.Util.Common
{
    public static class JsonHelper
    {
        /// <summary>
        /// 将对象序列化成JSON格式字符串,错误则为空值
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>序列化后的字符串</returns>
        public static string SerializeJsonString(this object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// 将字符串反序列化成对象
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="jsonString">字符串</param>
        /// <returns>对象</returns>
        public static T DeSerializerJsonString<T>(this string jsonString)
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                return new JsonSerializer().Deserialize<T>(new JsonTextReader(new StringReader(jsonString)));
            }
            catch { return default(T); }
        }

        /// <summary>
        /// 将字符串反序列化成对象
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="jsonString">字符串</param>
        /// <param name="anonymousType">对象类型</param>
        /// <returns>对象</returns>
        public static T DeSerializeAnonymousJsonString<T>(this string jsonString, T anonymousType)
        {
            try
            {
                return JsonConvert.DeserializeAnonymousType(jsonString, anonymousType);
            }
            catch { return default(T); }
        }


        #region steve.weng 2018-8-20 Newtonsoft.Json 4.5不能用

        /// <summary>
        /// Json字符串转内存对象
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="obj"></param>
        /// <returns></returns>         
        public static T JSDeserialize<T>(this string jsonString)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// 内存对象转换为json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJSON(this object obj)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            JavaScriptSerializer json = new JavaScriptSerializer();
            json.Serialize(obj, sb);
            return sb.ToString();
        }
        #endregion

        public static string GetDynamicProp(dynamic d, string fieldName)
        {
            var res = JsonConvert.DeserializeObject<dynamic>(d);
            return res[0].name;
        }
    }
}
