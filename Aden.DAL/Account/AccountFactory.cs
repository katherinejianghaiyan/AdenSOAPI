using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Aden.Model.Account;
using Aden.Util.Common;
using Aden.Util.Database;

namespace Aden.DAL.Account
{
    public class AccountFactory
    {
        /// <summary>
        /// 根据登录用户信息检查并返回用户对象
        /// </summary>
        /// <param name="user">登录用户对象</param>
        /// <returns>通过检查后的用户对象</returns>
        public AccountUser GetUser(LoginUser user)
        {
            string sql = "select a2.userGuid,a2.userName from tblUser a1 join tblUserMast a2 on a1.guid = a2.userGuid "
                + "and a1.status=1 where a1.loginName=@user and a1.password=@password";
            return SqlServerHelper.GetEntity<AccountUser>(SqlServerHelper.baseConn(), sql, System.Data.CommandType.Text,
                new SqlParameter[]
                {
                    new SqlParameter("@user", SqlDbType.NVarChar, 30){ Value = user.userName},
                    new SqlParameter("@password", SqlDbType.NVarChar,50){ Value = user.password}
                });
        }

        /// <summary>
        /// 根据用户权限获取角色菜单
        /// </summary>
        /// <param name="userGuid">用户Key</param>
        /// <param name="langCode">语言代码</param>
        /// <returns>菜单集合</returns>
        public List<AccountMenu> GetUserRoleMenus(string userGuid, string langCode)
        {
            string sql = "select distinct a2.menuGuid from tblUserRole a1 join tblRoleMenu a2 on a1.roleGuid=a2.roleGuid "
              + "and a2.status=1 where a1.status=1 and a1.userGuid='" + userGuid + "'";
            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.baseConn(), sql);
            if (data == null || data.Rows.Count == 0) return null;
            List<string> menuGuids = data.AsEnumerable().Select(dr => dr.Field<string>("menuGuid")).ToList();
            string filter = string.Empty;
            if (!menuGuids.Where(m => m.ToStringTrim().ToLower().Equals("all")).Any())
                filter = string.Join(",", menuGuids.Select(m => "'" + m + "'").ToArray());
            return GetUserMenus(filter, langCode);
        }

        /// <summary>
        /// 根据筛选器,获取用户菜单
        /// </summary>
        /// <param name="filter">筛选字符串</param>
        /// <param name="langCode">语言代码</param>
        /// <returns>菜单集合</returns>
        public List<AccountMenu> GetUserMenus(string filter, string langCode)
        {
            string sqlFilter = string.Empty;
            if (!string.IsNullOrEmpty(filter)) sqlFilter = "and guid in (" + filter + ") ";
            StringBuilder sql = new StringBuilder("with tb1(guid,pguid,menuName,action,icon,sort) as("
                + "select guid, pguid, menuName, action, icon, sort from tblMenu where status=1 " + sqlFilter
                + "union all select a1.guid,a1.pguid,a1.menuName,a1.action,a1.icon,a1.sort from "
                + "tblMenu a1 join tb1 a2 on a1.guid = a2.pguid where a1.status = 1),"
                + "tb2(guid, pguid, menuName, action, icon, sort) as (select guid, pguid, menuName, action, icon, sort "
                + "from tblMenu where status=1 " + sqlFilter
                + "union all select a1.guid,a1.pguid,a1.menuName,a1.action,a1.icon,a1.sort from "
                + "tblMenu a1 join tb2 a2 on a1.pguid = a2.guid where a1.status = 1) "
                + "select guid, pguid,case when isnull(u2.menuName,'')= '' then u1.menuName else u2.menuName end as name,"
                + "action,icon,sort from (select guid,pguid,menuName,action,icon,sort from tb1 "
                + "union select guid, pguid, menuName, action, icon, sort from tb2) u1 left join tblMenu_Lang u2 "
                + "on u1.guid = u2.menuGuid and u2.langCode='" + langCode + "' order by sort");
            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.baseConn(), sql.ToString());
            if (data == null || data.Rows.Count == 0) return null;
            //获取最外层Menu, 即所有父GUID为空
            var query = data.AsEnumerable().Where(dr => dr.Field<string>("pguid").ToStringTrim().Equals(string.Empty));
            if (!query.Any()) throw new Exception("root menu not found");
            int index = 0;
            return query.Select(dr => new AccountMenu()
            {
                index = index++.ToString(),
                guid = dr.Field<string>("guid").ToStringTrim(),
                name = dr.Field<string>("name").ToStringTrim(),
                path = dr.Field<string>("action").ToStringTrim(),
                iconCls = dr.Field<string>("icon").ToStringTrim(),
                childs = GetChildMenus(dr.Field<string>("guid"), data)
            }).ToList();
        }

        public List<AccountData> GetUserDatas(string userGuid, string menuGuid, string langCode)
        {
            string sql = "select companyGuid,siteGuid from tblUserMenuData where status=1 and userGuid='" + userGuid 
                + "' and menuGuid='" + menuGuid + "' and (isnull(companyGuid,'')<>'' or isnull(siteGuid,'')<>'')";
            DataTable roleData = SqlServerHelper.GetDataTable(SqlServerHelper.baseConn(), sql);
            if (roleData == null || roleData.Rows.Count == 0) return null;
            StringBuilder filter = new StringBuilder();
            var q1 = roleData.AsEnumerable().Where(dr => !string.IsNullOrEmpty(dr.Field<string>("companyGuid")));
            if (q1.Any()) filter.Append("a1.companyGuid in (" + string.Join(",", q1.Select(dr => "'" + dr.Field<string>("companyGuid").ToStringTrim() + "'").ToArray()) + ")");
            var q2 = roleData.AsEnumerable().Where(dr => !string.IsNullOrEmpty(dr.Field<string>("siteGuid")));
            if (q2.Any())
            {
                if (filter.Length > 0) filter.Append(" or ");
                filter.Append("a1.guid in (" + string.Join(",", q2.Select(dr => "'" + dr.Field<string>("siteGuid").ToStringTrim() + "'").ToArray()) + ")");
            }
            StringBuilder dataSql = new StringBuilder("select a1.costcenterCode,a1.warehouseCode,a2.code,a2.guid,case when isnull(a3.description,'')='' "
                + "then a1.description else a3.description end as siteName,case when isnull(a4.companyName,'')= '' then a2.companyName else a4.companyName end companyName "
                + "from tblSite a1 join tblCompany a2 on a1.companyGuid = a2.guid and a2.status = 1 left join tblSite_Lang a3 on a1.guid = a3.siteGuid "
                + "and a3.langCode = '" + langCode + "' left join tblCompany_Lang a4 on a2.guid = a4.companyGuid and a4.langCode = '" + langCode
                + "' where a1.status = 1 and (");
            dataSql.Append(filter).Append(")");
            DataTable data = SqlServerHelper.GetDataTable(SqlServerHelper.baseConn(), dataSql.ToString());
            if (data == null || data.Rows.Count == 0) return null;
            return data.AsEnumerable().GroupBy(dr => new
            {
                CompanyCode = dr.Field<string>("code").ToStringTrim(),
                Guid = dr.Field<string>("guid").ToStringTrim(),
                CompanyName = dr.Field<string>("companyName").ToStringTrim()
            }).Select(dg => new AccountData()
            {
                company = new Model.Common.Company()
                {
                    code = dg.Key.CompanyCode,
                    guid = dg.Key.Guid,
                    companyName = dg.Key.CompanyName
                },
                sites = dg.Select(g=>new Model.Common.Site()
                {
                    costCenterCode = g.Field<string>("costcenterCode").ToStringTrim(),
                    warehouseCode = g.Field<string>("warehouseCode").ToStringTrim(),
                    description = g.Field<string>("siteName").ToStringTrim()
                }).ToList()

            }).ToList();
        }

        /// <summary>
        /// 递归查找子菜单
        /// </summary>
        /// <param name="parentGuid">父菜单GUID</param>
        /// <param name="data">数据集</param>
        /// <returns>子菜单集合</returns>
        private List<AccountMenu> GetChildMenus(string parentGuid, DataTable data)
        {
            var query = data.AsEnumerable().Where(dr => dr.Field<string>("pguid").ToStringTrim().Equals(parentGuid));
            if (query.Any())
            {
                int index = 0;
                return query.Select(dr => new AccountMenu()
                {
                    index = index++.ToString(),
                    guid = dr.Field<string>("guid").ToStringTrim(),
                    name = dr.Field<string>("name").ToStringTrim(),
                    path = dr.Field<string>("action").ToStringTrim(),
                    iconCls = dr.Field<string>("icon").ToStringTrim(),
                    childs = GetChildMenus(dr.Field<string>("guid"), data) //递归获取
                }).ToList();
            }
            else return null;
        }


        public List<AccountCostUnit> GetUserCostUnits(string userGuid)
        {
            string sql = "select unitCode,unitName from tblUserCostUnit where userGuid='{0}' and status=1";

            return SqlServerHelper.GetEntityList<AccountCostUnit>(SqlServerHelper.baseConn(), string.Format(sql, userGuid));
        }

    }
}
