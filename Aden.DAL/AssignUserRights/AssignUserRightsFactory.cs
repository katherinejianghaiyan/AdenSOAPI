using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Util.Common;
using Aden.Util.Database;
using Aden.Model.MenuOrder;
using Aden.Model.MenuData;
using Aden.Model.SOMastData;
using Aden.Model.Common;
using Aden.Util.AdenCommon;
using System.Data;
using Aden.Model.AssignUserRights;

namespace Aden.DAL.AssignUserRights
{
    public class AssignUserRightsFactory
    {

        /// <summary>
        /// 获取用户菜单权限
        /// </summary>
        public List<UserRights> UserRightsSource (string user)
        {
            try
            {
                if (string.IsNullOrEmpty(user)) return null;
                
                //UserMenu
                string sql = "select distinct a1.userGuid,a2.userName,a1.menuGuid,a3.menuName from tblUserMenuData (nolock) a1 "
                    + "left join UserMast(nolock) a2 on a1.UserGuid = a2.Guid "
                    + "left join tblMenu(nolock) a3 on a3.guid = a1.MenuGuid "
                    +"where a1.userGuid in (select Guid from UserMast(nolock) where guid='{0}')";
                
                //UserComp
                string sqlusercomp = "select distinct a1.menuGuid,a4.Company as companyCode,a4.Name_ZH as companyName from tblUserMenuData (nolock) a1 "
                    + "left join UserMast(nolock) a2 on a1.UserGuid = a2.Guid "
                    + "left join tblMenu(nolock) a3 on a3.guid = a1.MenuGuid "
                    + "left join Company(nolock) a4 on a4.Guid = a1.Guid "
                    + "where a1.userGuid in (select Guid from UserMast(nolock) where  guid='{0}')";

               
                var query = SqlServerHelper.GetEntityList<UserRights>
                    (SqlServerHelper.salesorderConn, string.Format(sql, user));

                var queryusercomp = SqlServerHelper.GetEntityList<UserRights>
                    (SqlServerHelper.salesorderConn, string.Format(sqlusercomp, user));

               
                query = query.Select(dr => new UserRights()
                {
                    userName = dr.userName,
                    userGuid = dr.userGuid,
                    menuGuid = dr.menuGuid,
                    menuName = dr.menuName,
                    userComp = queryusercomp.Where(g => g.menuGuid == dr.menuGuid).ToList(),
                    compSource= queryusercomp
                }).ToList();
                
                return query;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
