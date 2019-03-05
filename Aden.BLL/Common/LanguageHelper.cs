using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model;
using Aden.Model.Common;
using Aden.DAL.Common;
using Aden.Util.Common;

namespace Aden.BLL.Common
{
    public class LanguageHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static LanguageFactory factory = new LanguageFactory();

        /// <summary>
        /// 获取Json格式的语言对象字符串
        /// </summary>
        /// <returns></returns>
        public static List<Language> GetLanguage()
        {
            try
            {
                List<Language> retList = factory.GetLanguage();
                if (retList == null) throw new Exception("DAL.Common.LanguageFactory.GetLanguage()==null");
                return retList;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "Langauge");
                return null;
            }
        }

        public static Dictionary<string, string> GetLanguageData(string code)
        {
            try
            {
                Dictionary<string, string> data = factory.GetLanguageData(code);
                if (data == null || data.Count == 0) throw new Exception("DAL.Common.LanguageFactory.GetLanguageData()==null");
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "LangaugeData");
                return null;
            }
        }

    }
}
