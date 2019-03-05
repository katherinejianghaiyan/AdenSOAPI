using Aden.Model.Common;
using Aden.Util.Common;
using Aden.Util.Database;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Aden.DAL.Common
{
    public class LanguageFactory
    {
        /// <summary>
        /// 获取语言代码集
        /// </summary>
        /// <returns>语言代码集</returns>
        public List<Language> GetLanguage()
        {
            string sql = "select code,langName from tblLang where status=1 order by orderby";
            return SqlServerHelper.GetEntityList<Language>(SqlServerHelper.baseConn(), sql);
        }

        /// <summary>
        /// 根据语言代码,获取语言数据集合
        /// </summary>
        /// <param name="code">语言代码</param>
        /// <returns></returns>
        public Dictionary<string, string> GetLanguageData(string code)
        {
            string sql = "select langKey,langValue from tblLangMast where langCode='" + code + "'";
            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.baseConn(), sql);
            if (data == null || data.Rows.Count == 0) return null;
            Dictionary<string, string> dics = new Dictionary<string, string>();
            foreach(DataRow dr in data.Rows)
            {
                if (dics.Keys.Contains(dr.Field<string>("langKey").ToStringTrim())) continue;
                dics.Add(dr.Field<string>("langKey").ToStringTrim(), dr.Field<string>("langValue").ToStringTrim());
            }
            return dics;
        }
    }
}
