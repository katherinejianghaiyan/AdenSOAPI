using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Aden.Model.SOAccount;
using Aden.Util.Database;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using Aden.Model.Common;
using Aden.Model.SOMastData;
using Aden.DAL.SalesOrder;
using System.DirectoryServices;

namespace Aden.DAL.SOAccount
{
    public class AccountFactory
    {
        #region Declare
        private const string domain = "choaden.com";
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private static Dictionary<string, string> ADList = new Dictionary<string, string>()
        {
            { "sn","LastName" },
            { "givenName","FirstName" },
            { "displayname","Displayname"},
            { "physicalDeliveryOfficeName","Office" },
            { "mail","Email" },
            { "telephoneNumber","TelephoneNumber" },
            { "co","CountryRegion" },
            { "mobile" , "Mobile" },
            { "title", "JobTitle" },
            { "department", "Department" },
            { "manager","ManagerName" }
        };
        #endregion

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        public static bool CheckLogin(LoginUser user)
        {
            // 按/切割UserID
            string[] strArray = stringToArray(user.userName.Trim());
            string strUserName = String.Empty;
            strUserName = strArray[0].Trim();
            user.password = user.password.Trim();
            if (strUserName == "" || user.password == "") return false;

            IntPtr token = IntPtr.Zero;

            try
            {
                return LogonUser(strUserName, domain, user.password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, ref token);
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }
        }
        public Dictionary<string,string> GetPropertyValue(string userName, string password)
        {
            return FindPropertyValue(userName, password);
        }

        public static Dictionary<string,string> FindPropertyValue(string userName, string password)
        {
            try
            {
                if (!CheckLogin(new LoginUser { userName = userName, password = password }))
                    throw new Exception("Error in username or password");

                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain);

                DirectorySearcher deSearch = new DirectorySearcher(entry);
                
                deSearch.SearchScope = SearchScope.Subtree;
                deSearch.Filter = string.Format("(&(&(objectCategory=person)(objectClass=user))(sAMAccountName={0}))",
                    userName.Split('@')[0]);
            
                SearchResult result = deSearch.FindOne();
                DirectoryEntry deResult = new DirectoryEntry();
                deResult = result.GetDirectoryEntry();

                Dictionary<string, string> dic = new Dictionary<string, string>();
                
                foreach (KeyValuePair<string, string> kv in ADList)
                {
                    string s = "";
                    if (deResult.Properties.Contains(kv.Key))
                        s =  deResult.Properties[kv.Key].Value.ToString();

                    dic.Add(kv.Value, s);
                }

                return dic;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        /// 根据登录用户名和密码进行存在性检查并返回用户对象
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public AccountUser GetUser(LoginUser user)
        {
            string userNameTemp = String.Empty;
            bool checkDomain = CheckLogin(user);
            StringBuilder sbSqlFin = new StringBuilder();

            string strSql_A = " SELECT [GUID] AS USERGUID " +
                                   " , USERNAME AS ACCOUNT " +
                                   " , FIRSTNAME " +
                                   " , LASTNAME " +
                                   " , (CASE " +
                                   "         WHEN LASTNAME = '' THEN FIRSTNAME " +
                                   "         ELSE FIRSTNAME + '.' + LASTNAME " +
                                   "    END) AS FULLNAME " +
                                   " , EMPLOYEEID " +
                                   " , MAIL " +
                                   " , MOBILE " +
                                " FROM USERMAST " +
                               " WHERE STATUS = '1' " +
                               "   AND USERNAME = '{0}' ";
            string strSql_B = "    AND PASSWORD = '{0}' ";

            // 按/切割UserID
            string[] strArray = stringToArray(user.userName.Trim());

            sbSqlFin.AppendFormat(strSql_A, strArray[0]);

            if (!checkDomain)
                sbSqlFin.AppendFormat(strSql_B, user.password);

            AccountUser userInfo = SqlServerHelper.GetEntity<AccountUser>(SqlServerHelper.salesorderConn(), sbSqlFin.ToString());

            // 当Admin账号且是ref账号的情况时，取得ref账号的权限
            // && "admin".Equals(strArray[0].ToLower())
            if (strArray.Length == 2 && "admin".Equals(strArray[0].ToLower()))
            {
                // 取得ref账号的权限
                userInfo.menus = UserMenus(strArray[1]);
                // 修改Account
                userInfo.account = strArray[1];
                userInfo.userGuid = userInfo.menus[0].userGuid;
                // 附加工号
                userInfo.employeeID = getEmployeeId(userInfo.account);
            }
            else
                userInfo.menus = UserMenus(strArray[0]);

            return userInfo;
        }

        public List<Aden.Model.Account.AccountMenu> UserMenus(string userName)
        {
            string sql = "select distinct a3.guid as userGuid, a2.action from tblUserMenuData a1,tblMenu a2,usermast a3 "
                + "where  a3.username='{0}' and a1.MenuGuid = a2.guid and a1.userguid=a3.guid and a1.status = '1'";
            sql = string.Format(sql, userName);
            List<Aden.Model.Account.AccountMenu> list =
                SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql).ToEntityList<Aden.Model.Account.AccountMenu>();
            if (list == null || !list.Any()) return null;
            return list;
        }

        /// <summary>
        /// 根据账号取得工号
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string getEmployeeId(string userName)
        {
            string strSql = " SELECT EMPLOYEEID " +
                              " FROM USERMAST " +
                             " WHERE USERNAME = '{0}' ";

            AccountUser userInfo = SqlServerHelper.GetEntity<AccountUser>(SqlServerHelper.salesorderConn(), string.Format(strSql, userName));

            return userInfo != null ? userInfo.employeeID : string.Empty;
        }

        /// <summary>
        /// 取得权限列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<SingleField> GetAuthList(UserAuthParam param)
        {
            // 取得Guid集合
            string strMenuGuid = Aden.Util.AdenCommon.MastData.formatActionToMenuGuid(param.action);
            // costCenter list
            List<SingleField> lstCC = new List<SingleField>();
            // company list
            List<SingleField> lstCP = new List<SingleField>();
            // company Guid条件拼接
            string strCompanyGuid = string.Empty;
            // company Code条件拼接
            string strCompanyCode = string.Empty;

            string strSqlCC = " SELECT RTRIM(CODE) AS CODE " +
                                " FROM TBLUSERMENUDATA " +
                               " WHERE USERGUID = '{0}' " +
                                 " AND STATUS = '1' " +
                                 " AND TYPE = 'costcenter' " +
                                 " AND MENUGUID IN {1} ";

            string strSqlCP = " SELECT RTRIM(GUID) AS GUID " +
                                " FROM TBLUSERMENUDATA " +
                               " WHERE USERGUID = '{0}' " +
                                 " AND STATUS = '1' " +
                                 " AND TYPE = 'company' " +
                                 " AND MENUGUID IN {1} ";

            if ("costcenter".Equals(param.fieldName.ToLower()))
                lstCC = SqlServerHelper.GetEntityList<SingleField>(SqlServerHelper.salesorderConn(), string.Format(strSqlCC,param.userGuid, strMenuGuid));

            if (param.recursion)
            {
                // 递归取得所有子公司
                AuthCompany auth = new AuthCompany();
                auth.action = param.action;
                auth.userGuid = param.userGuid;
                List<Model.SOMastData.Company> lstCompany = (new CompanyFactory()).GetCompanyInAuth(auth);

                SingleField sf = new SingleField();
                foreach (Model.SOMastData.Company company in lstCompany)
                {
                    sf = new SingleField();
                    sf.guid = company.companyGuid;
                    lstCP.Add(sf);
                }
            }
            else
            {
                lstCP = SqlServerHelper.GetEntityList<SingleField>(SqlServerHelper.salesorderConn(), string.Format(strSqlCP, param.userGuid, strMenuGuid));
            }

            if (lstCP != null && lstCP.Any())
            {
                strCompanyGuid = string.Join("','", lstCP.Select(r => r.guid).Distinct().ToArray());
                strCompanyGuid = "('" + strCompanyGuid + "')";

                // 取得CompanyGuid对应的companyCode
                string strSqlCompany = " SELECT GUID " +
                                            " , COMPANY AS CODE " +
                                            " , NAME_ZH AS NAME1 " +
                                            " , NAME_EN AS NAME2 " +
                                         " FROM COMPANY " +
                                        " WHERE GUID IN {0} " +
                                          " AND STATUS = '1' ";

                lstCP = SqlServerHelper.GetEntityList<SingleField>(SqlServerHelper.salesorderConn(), string.Format(strSqlCompany, strCompanyGuid));
                lstCP = lstCP.OrderBy(r => r.code).ToList();
            }

            if ("company".Equals(param.fieldName.ToLower()))
                return lstCP;

            /***将公司List转换成CCList***/
            if (lstCP != null && lstCP.Any())
            {
                List<SingleField> lstTemp = new List<SingleField>();
                strCompanyCode = string.Join("','", lstCP.Select(r => r.code).Distinct().ToArray());
                strCompanyCode = "('" + strCompanyCode + "')";

                string strSqlCostCenter = " SELECT COSTCENTERCODE AS CODE" +
                                            " FROM CCMast " +
                                           " WHERE STATUS = '1' " +
                                             " AND DBNAME IN {0} ";
                lstTemp = SqlServerHelper.GetEntityList<SingleField>(SqlServerHelper.salesorderConn(), string.Format(strSqlCostCenter, strCompanyCode));
                lstCC = lstCC.Concat(lstTemp).ToList();
                lstCC = lstCC.OrderBy(r => r.code).ToList();
                /***根据Code去重复***/
                lstCC = lstCC.Where((x, i) => lstCC.FindIndex(z => z.code == x.code) == i).ToList();
            }

            return lstCC;
        }

        private static string[] stringToArray(string str)
        {
            string[] sArray = Regex.Split(str, "/", RegexOptions.IgnoreCase);

            return sArray;
        }
    }
}
