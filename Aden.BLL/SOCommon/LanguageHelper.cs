using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.SOCommon;
using Aden.Model.SOCommon;
using Aden.Model.SOMastData;
using Aden.Model;

namespace Aden.BLL.SOCommon
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
                if (retList == null)
                {
                    throw new Exception("DAL.SOCommon.LanguageFactory.GetLanguage()==null");
                }
                return retList;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "Language");
                return null;
            }
        }

        /// <summary>
        /// 取得语言包最新版本号及其内容
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        public static LangVersion GetLangVersion(string strType)
        {
            try
            {
                LangVersion langVersion = factory.GetLangVersion(strType);

                return langVersion;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetLangVersion");
                return null;
            }
        }

        /// <summary>
        /// 取得语言字典
        /// </summary>
        /// <returns></returns>
        public static List<LangMast> GetLangMast(string strType)
        {
            try
            {
                List<LangMast> lstLang = factory.GetLangMast(strType);

                return lstLang;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetLangMast");
                return null;
            }
        }

        /// <summary>
        /// 保存语言字典
        /// </summary>
        /// <param name="lstLang"></param>
        /// <returns></returns>
        public static int SaveLangMast(List<LangMast> lstLang)
        {
            try
            {
                return factory.SaveLangMast(lstLang);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SaveLangMast");
                return 0;
            }
        }

        /// <summary>
        /// 检查是否需要更新语言包
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool CheckLangVersion(string appName, string guid)
        {
            try
            {
                return factory.CheckLangVersion(appName, guid);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CheckLangVersion");
                return false;
            }
        }
    }
}
