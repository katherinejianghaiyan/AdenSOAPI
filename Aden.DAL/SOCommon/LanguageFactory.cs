using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model.SOCommon;
using Aden.Model.SOMastData;
using Aden.Util.Database;

namespace Aden.DAL.SOCommon
{
    public class LanguageFactory
    {   /// <summary>
        /// 获取语言代码集
        /// </summary>
        /// <returns></returns>
        public List<Language> GetLanguage()
        {
            string sql = " SELECT CODE " +
                               ", LANGNAME " +
                           " FROM LANG " +
                          " WHERE STATUS = 1 " +
                          " ORDER BY SORT";
            return SqlServerHelper.GetEntityList<Language>(SqlServerHelper.salesorderConn(), sql);
        }

        /// <summary>
        /// 取得语言包最新版本号及其内容
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        public LangVersion GetLangVersion(string strType)
        {
            string strSql = " SELECT APPNAME " +
                                 " , VERSIONGUID " +
                              " FROM LANGVERSION " +
                             " WHERE APPNAME = '{0}' ";
            LangVersion langVersion = SqlServerHelper.GetEntity<LangVersion>(SqlServerHelper.salesorderConn(), string.Format(strSql, strType));

            if (langVersion == null)
                return null;

            langVersion.lstLangMast = GetLangMast(strType);

            return langVersion;
        }

        /// <summary>
        /// 取得语言字典
        /// </summary>
        /// <returns></returns>
        public List<LangMast> GetLangMast(string strType)
        {
            string strSql = " SELECT GUID " +
                                 " , TYPE " +
                                 " , NUMBER " +
                                 " , ZH " +
                                 " , EN " +
                                 " , JA " +
                                 " , VI " +
                                 " , STATUS " +
                                 " , '0' AS CHANGEFLAG " +
                              " FROM LANGMAST " +
                             " WHERE STATUS = '1' ";

            if (!string.IsNullOrWhiteSpace(strType))
                strSql = strSql + " AND TYPE = '" + strType + "' ";

            List<LangMast> lstLang = SqlServerHelper.GetEntityList<LangMast>(SqlServerHelper.salesorderConn(), strSql);

            return lstLang;
        }

        /// <summary>
        /// 保存语言字典
        /// </summary>
        /// <returns></returns>
        public int SaveLangMast(List<LangMast> lstLang)
        {
            if (lstLang == null || lstLang.Count == 0)
                return 0;

            string strType = lstLang[0].type;
            int maxNumber = GetMaxNumber(strType);

            string strSqlWork = string.Empty;

            string strSqlUpdate = " UPDATE LANGMAST " +
                                     " SET ZH = '{0}' " +
                                       " , EN = '{1}' " +
                                       " , JA = '{2}' " +
                                       " , VI = '{3}' " +
                                    " FROM LANGMAST " +
                                   " WHERE GUID = '{4}' ";

            string strSqlInsert = " INSERT LANGMAST " +
                                       " ( GUID " +
                                       " , TYPE " +
                                       " , NUMBER " +
                                       " , ZH " +
                                       " , EN " +
                                       " , JA " +
                                       " , VI " +
                                       " , STATUS ) " +
                                  " VALUES " +
                                       " ( '{0}' " +
                                       " , '{1}' " +
                                       " , '{2}' " +
                                       " , '{3}' " +
                                       " , '{4}' " +
                                       " , '{5}' " +
                                       " , '{6}' " +
                                       " , '{7}' ) ";

            StringBuilder sbSql = new StringBuilder();

            foreach (LangMast lm in lstLang)
            {
                if (string.IsNullOrWhiteSpace(lm.guid))
                {
                    maxNumber += 1;
                    strSqlWork = string.Format(strSqlInsert, Guid.NewGuid().ToString().ToUpper(), strType, GetLangNumber(strType, maxNumber),
                        lm.zh, lm.en, lm.ja, lm.vi, "1");
                    sbSql.Append(strSqlWork);
                }
                else
                {
                    if ("1".Equals(lm.changeFlag))
                    {
                        strSqlWork = string.Format(strSqlUpdate, lm.zh, lm.en, lm.ja, lm.vi, lm.guid);
                        sbSql.Append(strSqlWork);
                    }
                }
            }
            int result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSql.ToString());

            if (result > 0)
                // 更新语言包版本GUID
                UpdateVersionGuid(strType);
            return result;
        }

        /// <summary>
        /// 跟新语言包版本GUID
        /// </summary>
        private void UpdateVersionGuid(string appName)
        {
            /***判断是否为需要做版本控制的类型***/
            string[] aryAppName = { "charge" };
            int idx = Array.IndexOf(aryAppName, appName);
            if (idx == -1)
                return;

            string strVersionGuid = Guid.NewGuid().ToString().ToUpper();

            string strSqlUpdate = " UPDATE LANGVERSION " +
                                     " SET VERSIONGUID = '{0}' " +
                                   " WHERE appName = '{1}' ";
            SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), string.Format(strSqlUpdate, strVersionGuid, appName));
        }

        /// <summary>
        /// 检查是否需要更新语言包
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool CheckLangVersion(string appName, string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                return true;

            string strSql = " SELECT VERSIONGUID " +
                              " FROM LANGVERSION " +
                             " WHERE APPNAME = '{0}' ";

            LangVersion langVersion = SqlServerHelper.GetEntity<LangVersion>(SqlServerHelper.salesorderConn(), string.Format(strSql, appName));

            if (langVersion == null)
                return false;

            return !guid.Equals(langVersion.versionGuid);
        }

        // 取得当前类型中的最大编号
        private int GetMaxNumber(string type)
        {
            string strSql = " SELECT TOP 1 * " +
                              " FROM LANGMAST " +
                             " WHERE TYPE = '{0}' " +
                             " ORDER BY NUMBER DESC ";

            List<LangMast> lstLang = SqlServerHelper.GetEntityList<LangMast>(SqlServerHelper.salesorderConn(), string.Format(strSql, type));

            if (lstLang == null || lstLang.Count == 0)
                return 0;

            string strNumber = lstLang[0].number;
            int index = 0;

            switch (type.ToLower())
            {
                case "menu":
                    index = 1;
                    break;
                case "label":
                    index = 1;
                    break;
                case "button":
                    index = 1;
                    break;
                case "text":
                    index = 1;
                    break;
                case "charge":
                    index = 2;
                    break;
            }

            strNumber = strNumber.Substring(index, strNumber.Length - index);

            return int.Parse(strNumber);
        }

        // 根据数字取得编号
        private string GetLangNumber(string strType, int intNumber)
        {
            int size = 6;
            string strLast = intNumber.ToString();
            string strFirst = string.Empty;
            string strMiddle = string.Empty;
            string strNumber = string.Empty;

            // 补0
            for (int i = 0; i < (size - strLast.Length); i++)
            {
                strMiddle += "0";
            }

            switch (strType.ToLower())
            {
                case "menu":
                    strFirst = "M";
                    break;
                case "label":
                    strFirst = "L";
                    break;
                case "button":
                    strFirst = "B";
                    break;
                case "text":
                    strFirst = "T";
                    break;
                case "charge":
                    strFirst = "WC";
                    break;
            }

            strNumber = strFirst + strMiddle + strLast;

            return strNumber;
        }
    }
}
