using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Aden.Util.Database;
using Aden.Model.Common;
using Aden.Util.Common;


namespace Aden.DAL.MastData
{
    public class SupplierFactory
    {
        #region 得到每个成本中心及所有的产品分类，对应的供应商
        //class_Code,supplierCode,defaultSupplier,directBuy,addToMenu
        public List<dynamic> getSuppliers(string dbName, string costCenterCode, DateTime date)
        {
            try
            {
               
                //string sql = "select distinct a1.crdnr SupplierCode, a4.Supplier_Name SupplierName, "
                //   + "a3.Class02_Code,a3.Class03_Code, case when a1.validto + 1 > getdate() then 1 else 0 end IsAvailable "
                //  + "from DM_D_ERP_PurchaseAgreement a1, d_item a2 ,d_item_class_03 a3, d_supplier a4 "
                //  + "where a1.itemcode like '10%' and a1.itemcode = a2.Items_Code and a2.Class03_Code = a3.Class03_Code "
                //  + "and a1.division = '{0}' and a1.validfrom <= getdate()  " // and a1.validto +1 > getdate()
                //  + "and a1.SupplierCode = a4.Supplier_Code ";
                //sql = string.Format(sql, costCenterCode.Substring(0,3));

                //List<dynamic> list = SqlServerHelper.GetDynamicList(SqlServerHelper.purchasepriceconn, sql);
                //if (list == null || !list.Any()) throw new Exception("No supplier");


                //产品分类
                string filterSql = "select distinct a1.SupplierCode,a1.DefaultSupplier,a1.CostCenterCode,a2.code2 Class_Code,"
                    + "isnull(a3.type1,'') type1,isnull(a3.type2,'') type2 from CClassSupplier a1 "
                    + "join tblDataMapping a2 on a1.ClassGUID = a2.code1 "
                    + "join tblitemclass a3 on a1.classguid = a3.guid "
                    + "where a1.dbname = '{0}' and isnull(a1.costcentercode,'{1}')= '{1}' and a1.DeleteTime is null "
                    + "and a2.type = 'RMClassMapping' and a2.deletetime is null and "
                    + "isnull(a1.startdate,'1900-1-1')<='{2}' and isnull(a1.enddate,'2222-12-12') >= '{2}' "
                    + "and a3.type = 'RMClass' and a3.status='active' order by a1.CostCenterCode,a1.DefaultSupplier desc";
                filterSql = string.Format(filterSql, dbName, costCenterCode,date.ToString("yyyy-MM-dd"));
                List<dynamic> tmpclass = SqlServerHelper.GetDynamicList(SqlServerHelper.salesorderConn(), string.Format(filterSql, dbName, costCenterCode,date));
                if (tmpclass == null || !tmpclass.Any()) throw new Exception("No define supplier by site");
                tmpclass = tmpclass.GroupBy(q => q.Class_Code).Select(q =>
                    {
                        var tmp = q.Where(p => p.CostCenterCode == costCenterCode);  //q.OrderBy(p => p.CostCenterCode == costCenterCode ? 0 : 1).FirstOrDefault();
                        if (!tmp.Any())
                            tmp = q.Where(p => string.IsNullOrWhiteSpace(p.CostCenterCode));
                        return tmp.Select(p => new
                            {
                                class_Code = q.Key,
                                supplierCode = p.SupplierCode,
                                defaultSupplier = p.DefaultSupplier,
                                directBuy = true,//p.type1.ToLower() == "directbuy",  //'DirectBuy'表示可以直接下采购单
                                    addToMenu = p.type2.ToLower() == "addtomenu" //'AddToMenu' 表示可以添加到菜单上
                        });

                    }).SelectMany(q=>q).Distinct().OrderBy(q=>q.class_Code).ToList<dynamic>();

                return tmpclass;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 每个自定义的产品分类，及所有的供应商
        //返回ClassGUID,SupplierCode，SupplierName
        public List<dynamic> getSuppliers(string dbName)
        {
            try
            {
                /*
                string sql = "select distinct a1.crdnr SupplierCode, a4.Supplier_Name SupplierName, "
                    + "a3.Class02_Code,a3.Class03_Code, case when a1.validto + 1 > getdate() then 1 else 0 end IsAvailable "
                   + "from DM_D_ERP_PurchaseAgreement a1, d_item a2 ,d_item_class_03 a3, d_supplier a4 "
                   + "where a1.itemcode like '10%' and a1.Condition_Code = 'A' and a1.division = '{0}'  "
                   + " and a1.itemcode = a2.Items_Code and a2.Class03_Code = a3.Class03_Code "
                   + "and a1.validfrom <= getdate()  "
                   + "and a1.SupplierCode = a4.Supplier_Code ";
                sql = string.Format(sql, dbName);
                List<dynamic> list = SqlServerHelper.GetDynamicList(SqlServerHelper.purchasepriceconn, sql);
                */
                List<dynamic> list = (new ItemFactory()).getPurchaseItemsFromMDM(dbName, null,null).Select(q=> new
                {
                    SupplierCode = q.supplierCode,
                    SupplierName = q.supplierName,
                    Class02_Code = q.nationGUID,
                    Class03_Code = q.menuClassGUID,
                    IsAvailable = DateTime.Parse(q.validto).AddDays(1) > DateTime.Now
                }).Distinct().ToList<dynamic>();
                if (list == null || !list.Any()) throw new Exception("No supplier");

                //产品分类
                string sql = "select code1 ClassGUID,code2 Class_Code from tblDataMapping "
                    + "where type = 'RMClassMapping' and deletetime is null";
                List<dynamic> tmpclass = SqlServerHelper.GetDynamicList(SqlServerHelper.salesorderConn(), sql);
                list = (from a in list
                           from b in tmpclass
                           where a.Class02_Code == b.Class_Code || a.Class03_Code == b.Class_Code
                           group a by new { a.SupplierCode, b.ClassGUID } into grp
                           select new
                           {
                               ClassGUID = grp.Key.ClassGUID,
                               SupplierCode = grp.Key.SupplierCode,
                               SupplierName = grp.FirstOrDefault().SupplierName,
                               Status = grp.OrderByDescending(q => q.IsAvailable).FirstOrDefault().IsAvailable
                           }).OrderBy(q=>q.SupplierCode).ToList<dynamic>();


                return list;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        #endregion



    }
}
