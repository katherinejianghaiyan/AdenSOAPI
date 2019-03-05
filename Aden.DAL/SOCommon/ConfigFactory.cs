using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model.SOCommon;
using Aden.Util.Database;

namespace Aden.DAL.SOCommon
{
    public class ConfigFactory
    {   /// <summary>
        /// 获取语言代码集
        /// </summary>
        /// <returns></returns>
        public string GetConfigValue(string type)
        {
            string result = string.Empty;
            string sql = " SELECT VALUE " +
                           " FROM CONFIG " +
                          " WHERE STATUS = 1 " +
                          "   AND TYPE = '{0}' ";
            List<Config> lstConfig = new List<Config>();
            lstConfig = SqlServerHelper.GetEntityList<Config>(SqlServerHelper.salesorderConn(), string.Format(sql, type));

            if (lstConfig.Count > 0)
                result = lstConfig[0].value;

            return result;
        }
    }
}
