using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.SOCommon;
using Aden.Model.SOCommon;
using Aden.Model;

namespace Aden.BLL.SOCommon
{
    public class ConfigHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static ConfigFactory config = new ConfigFactory();

        /// <summary>
        /// 获取Json格式的语言对象字符串
        /// </summary>
        /// <returns></returns>
        public static string GetConfigValue(string type)
        {
            try
            {
                string configValue = config.GetConfigValue(type);

                if (string.IsNullOrWhiteSpace(configValue))
                {
                    throw new Exception("DAL.SOCommon.ConfigFactory.GetConfig()==null");
                }
                return configValue;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "Config");
                return null;
            }
        }
    }
}
