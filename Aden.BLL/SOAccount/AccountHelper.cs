using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model;
using Aden.Model.SOAccount;
using Aden.Model.Common;
using Aden.DAL.SOAccount;


namespace Aden.BLL.SOAccount
{
    public class AccountHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static AccountFactory factory = new AccountFactory();

        /// <summary>
        /// 根据登录用户,检查数据,并返回用户对象
        /// </summary>
        /// <param name="user">登录用户对象</param>
        /// <returns>存在的用户对象</returns>
        public static AccountUser GetUser(LoginUser user)
        {
            try
            {
                if (user == null) throw new Exception("LoginUser is null");
                AccountUser accountUser = factory.GetUser(user);
                if (accountUser == null) throw new Exception("DAL.SOAccount.AccountFactory.GetUser()==null");
                if(!string.IsNullOrEmpty(user.langCode)) accountUser.langCode = user.langCode;
                return accountUser;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetUser");
                return null;
            }
        }
        public static Dictionary<string,string> CheckADProp(string userName, string passWord)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(passWord))
                    throw new Exception("LoginUser is null");
                Dictionary<string,string> AD = factory.GetPropertyValue(userName, passWord);
                if (AD == null) throw new Exception("DAL.SOAccount.AccountFactory.CheckADProp()==null");
                
                return AD;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetUser");
                return null;
            }
        }
        /// <summary>
        /// 取得权限列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<SingleField> GetAuthList(UserAuthParam param)
        {
            try
            {
                if (param == null) throw new Exception("LoginUser is null");
                List<SingleField> lstResult = factory.GetAuthList(param);
                if (lstResult == null || lstResult.Count == 0) throw new Exception("DAL.SOAccount.AccountFactory.GetAuthList()==null");
                return lstResult;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetAuthList");
                return null;
            }
        }
    }
}
