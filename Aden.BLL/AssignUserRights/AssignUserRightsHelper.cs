using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.MenuData;
using Aden.Model;
using Aden.Model.MenuData;
using Aden.DAL.AssignUserRights;
using Aden.Model.AssignUserRights;

namespace Aden.BLL.AssignUserRights
{
    public class AssignUserRightsHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static AssignUserRightsFactory factory = new AssignUserRightsFactory();

        /// <summary>
        /// 获取用户菜单权限
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static List<UserRights> GetUserAuthority(string userName)
        {
            try
            {
                List<UserRights> GetUserAuthority = factory.UserRightsSource(userName);
                if (GetUserAuthority == null) throw new Exception("DAL.MenuData.MenuDataFactory.GetUserAuthority()==null");
                return GetUserAuthority;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetUserAuthority");
                return null;
            }

        }
    }
}
