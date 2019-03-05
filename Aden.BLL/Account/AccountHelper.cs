using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model;
using Aden.Model.Account;
using Aden.DAL.Account;

namespace Aden.BLL.Account
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
                if (accountUser == null) throw new Exception("DAL.Account.AccountFactory.GetUser()==null");
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

        /// <summary>
        /// 根据AccountUser获取用户对应菜单
        /// </summary>
        /// <param name="user">AccountUser</param>
        /// <returns>菜单集合</returns>
        public static List<AccountMenu> GetUserMenus(AccountUser user)
        {
            try
            {
                if (user == null) throw new Exception("LoginUser is null");
                return factory.GetUserRoleMenus(user.userGuid, user.langCode);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetUserMenu");
                return null;
            }
        }

        public static List<AccountData> GetUserDatas(AccountUserMenu userMenu)
        {
            try
            {
                if (userMenu == null) throw new Exception("UserMenu is null");
                return factory.GetUserDatas(userMenu.userGuid, userMenu.menuGuid, userMenu.langCode);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetUserDatas");
                return null;
            }
        }

        public static List<AccountCostUnit> GetUserCostUnits(string userGuid)
        {
            try
            {
                return factory.GetUserCostUnits(userGuid);
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetUserCostUnits");
                return null;
            }
        }
    }
}
