using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Util.Common;
using Aden.Util.Database;
using Aden.Model.MenuData;
using System.Data;
using Aden.Model.RightsData;

namespace Aden.DAL.MenuData
{

    public class RightsDataFactory
    {
        /// <summary>
        /// 中心列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CCOptions> GetUserCompany(string userId)
        {
            try
            {
                string sql = "select distinct a2.Company,a2.Name_ZH,a2.Name_EN from [dbo].[tblUserMenuData] (nolock) a1 "
                    + "join Company (nolock) a2 on a1.Guid = a2.Guid where UserGuid = '{0}' ";

                var data = SqlServerHelper.GetEntityList<CCOptions>(SqlServerHelper.salesorderConn(), string.Format(sql,userId));

                if (data == null) return null;

                var query = data.Select(q=>new CCOptions()
                {
                    company=q.company,
                    name_ZH=q.name_ZH,
                    name_EN=q.name_EN
                }).ToList();

                return query;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        } 

        /// <summary>
        /// 档口选项和产品选项
        /// </summary>
        /// <returns></returns>
        public Items GetItems(string costcenter)
        {
            try
            {
                List<SqlDic> Sqls = new List<SqlDic>();

                //string sql = "select distinct costCenterCode,windowGuid,windowName,windowName as value,sort from [dbo].[CCWindows] (nolock) "
                //    + "where costCenterCode='{0}'";
                string sql = "select distinct costCenter as costCenterCode,windowGuid,windowName,windowName as value,windowSort as sort from CCWindowMeals (nolock) "
                    + "where costCenter='{0}'";
                Sqls.Add(new SqlDic()
                {
                    TableName = "WindowOptions",
                    Sql = string.Format(sql, costcenter)
                });

                //每个营运点下的产品
                sql = "select distinct a2.productCode,a2.productDesc from [dbo].[SalesOrderItem] (nolock) a2 "
                    + "left join CCWindowMeals (nolock) a1 on a1.SOItemGUID=a2.ItemGuid "
                    + "left join SalesOrderHead (nolock) a3 on a3.HeadGuid=a2.HeadGuid "
                    + "left join [dbo].[ProductTypeData] (nolock) a4 on a4.id= a2.ProductCode where a4.ProductType= 'SalesMeals' "
                    + "and a2.CostCenterCode='{0}' and a2.status not in (9) "
                    + "and a2.StartDate<=GETDATE() and  a2.EndDate>=GETDATE() and isnull(a2.ExpiryDate,'9999-12-30')>=GETDATE() "
                    + "and a3.StartDate<=GETDATE() and  a3.EndDate>=GETDATE() ";

                Sqls.Add(new SqlDic()
                {
                    TableName= "ProductType",
                    Sql=string.Format(sql,costcenter)
                });

                //每个产品对应的合同行
                sql = string.Format("select distinct a2.itemGuid,a2.productCode,a2.productDesc, "
                    + "case when a1.SOItemGUID is not null then convert(bit,0) else convert(bit,1) end 'Status' "
                    + "from [dbo].[SalesOrderItem] (nolock) a2 "
                    + "left join CCWindowMeals (nolock) a1 on a1.SOItemGUID=a2.ItemGuid "
                    + "left join SalesOrderHead (nolock) a3 on a3.HeadGuid=a2.HeadGuid "
                    + "left join [dbo].[ProductTypeData] (nolock) a4 on a4.id= a2.ProductCode where a4.ProductType= 'SalesMeals' "
                    + "and a2.CostCenterCode='{0}' and a2.status not in (9) and a2.StartDate<=GETDATE() "
                    + "and a2.EndDate>=GETDATE() and isnull(a2.ExpiryDate,'9999-12-30')>=GETDATE() and a3.StartDate<=GETDATE() and a3.EndDate>=GETDATE() ", costcenter);

                var soItembyProduct = SqlServerHelper.GetEntityList<ProductType>(SqlServerHelper.salesorderConn(), sql);

                DataSet ds = SqlServerHelper.GetDataSet(SqlServerHelper.salesorderConn(), Sqls);
                Items item = new Items();
                item.costCenterCode = costcenter;
                //档口
                item.windowOptions = ds.Tables["WindowOptions"].ToEntityList<WindowOptions>();
                //该成本中心的有效餐次
                var productType= ds.Tables["ProductType"].ToEntityList<ProductType>();
                item.productType = productType.GroupJoin(soItembyProduct, p => new { productCode=p.productCode,productDesc=p.productDesc}, q => new { productCode=q.productCode, productDesc = q.productDesc }, (p, q) => new { p, q }).
                    Select(pi =>
                      {
                          Array soItemGuid = pi.q.Select(dr =>new { itemGuid=dr.itemGuid}).ToList().ToArray(); //, productDesc=dr.productDesc,status=dr.status
                          var validwd = CheckedMealType(costcenter, pi.p.productCode, pi.p.productDesc)==null?null:CheckedMealType(costcenter, pi.p.productCode, pi.p.productDesc).Select(dr=> new{ windowGuid=dr.windowGuid}).ToList().ToArray();  //CheckedMealType(costcenter, pi.p.productCode, pi.p.productDesc).Select(dr => dr.windowGuid);
                          return new ProductType()
                          {
                              validwd= validwd,//string.IsNullOrWhiteSpace(validwd)?"":validwd,
                              productCode = pi.p.productCode,
                              productDesc = pi.p.productDesc,
                              //备选项的和同行GUID
                              soItemGuid = soItemGuid,
                              //备选项有效性
                              status= validwd==null?false:true
                          };
                      }).ToList();

                //主数据呈现于界面
                List<ccWindowMeals> table = CheckedMealType(costcenter, "", "");
                if (table ==null)
                    item.mealType = null;
                else
                    item.mealType = table.ToList().ToArray();
                
                return item;
                
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        //主数据和餐次列表
        public List<ccWindowMeals> CheckedMealType(string costCenterCode, string productCode, string productDesc)
        {
            try
            {
                string filter = "";
                if (!string.IsNullOrWhiteSpace(costCenterCode)) filter=string.Format(" and a2.costCenterCode='{0}' ", costCenterCode);
                if (!string.IsNullOrWhiteSpace(productCode) && !string.IsNullOrWhiteSpace(productDesc))
                    filter += string.Format(" and a2.productCode='{0}' and a2.productDesc='{1}'", productCode, productDesc);

                string sql = string.Format("select distinct a2.costCenterCode,a1.windowGuid,a1.windowName,a1.WindowSort as sort "
                    + "from CCWindowMeals (nolock) a1 left join [dbo].[SalesOrderItem] (nolock) a2 on a1.SOItemGUID = a2.ItemGuid "
                    + "left join SalesOrderHead (nolock) a3 on a3.HeadGuid=a2.HeadGuid "
                    + "left join[dbo].[ProductTypeData] (nolock) a4 on a4.id=a2.ProductCode " 
                    + "where a4.ProductType='SalesMeals' and a1.deletetime is null {0} "
                    + "and a2.status not in (9) and a2.StartDate<=GETDATE() and  a2.EndDate>=GETDATE() and isnull(a2.ExpiryDate,'9999-12-30')>=GETDATE() "
                    + "and a3.StartDate<=GETDATE() and  a3.EndDate>=GETDATE() "
                    + "order by a1.WindowSort", filter);


                var checkedMealType = SqlServerHelper.GetEntityList<ccWindowMeals>(SqlServerHelper.salesorderConn(), sql);
                if (checkedMealType == null) return null;

                return checkedMealType;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //新增档口和餐次
        public int NewCCWindowMeal(Param menu)    //(string costCenterCode,string windowGuid,string windowName)
        {
            try
            {
                StringBuilder sqladdwindow = new StringBuilder();
                StringBuilder sqleditwindow = new StringBuilder();
                string windowGuid = "";

                //新增餐次
                foreach (ccWindowMeals item in menu.ccwindowMeals)
                {
                    
                    windowGuid = string.IsNullOrWhiteSpace(item.windowGuid) ? Guid.NewGuid().ToString() : item.windowGuid;
                    string windowName = item.windowName;
                    int sort = item.sort;

                    if (!string.IsNullOrWhiteSpace(item.windowGuid))
                    {
                        sqladdwindow.Append(string.Format("update CCWindowMeals set status='inactive',deleteTime='{0}',deleteUser='{1}' where costCenter='{2}' and windowGuid='{3}' ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), menu.userId, menu.costCenterCode, item.windowGuid));
                    }

                    //新增餐次
                    foreach (ProductType row in item.mealStatus)
                    {
                        if (row.validItem.Count>0 && row.status){
                            foreach(ProductType line in row.validItem)
                            {
                                sqladdwindow.Append(string.Format("insert into CCWindowMeals (costCenter,windowGuid,SOItemGUID,Status,CreateUser,CreateTime,WindowName,WindowSort) "
                                + "values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7})", menu.costCenterCode, line.windowGuid == null ? windowGuid : line.windowGuid, line.itemGuid == null ? "" : line.itemGuid, "active", menu.userId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), windowName, sort));
                            }
                        }
                    }
                }
              
                int i = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sqladdwindow.ToString());
                return i;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
