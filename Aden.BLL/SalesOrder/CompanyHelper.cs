using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.SalesOrder;
using Aden.Model.SOMastData;
using Aden.Model;
using Aden.Model.SOCommon;

namespace Aden.BLL.SalesOrder
{
    public class CompanyHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static CompanyFactory factory = new CompanyFactory();

        /// <summary>
        /// 取得所有符合条件的Company列表
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public static List<Company> GetCompany(AuthCompany auth)
        {
            try
            {
                List<Company> lstCompany = factory.GetCompany(auth);
                if (lstCompany == null) throw new Exception("DAL.SalesOrder.CompanyFactory.GetCompany()==null");
                return lstCompany;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCompany");
                return null;
            }
        }

        /// <summary>
        /// 根据权限取得Company列表
        /// </summary>
        /// <returns></returns>
        public static List<Company> GetCompanyInAuth(AuthCompany auth)
        {
            try
            {
                List<Company> lstCompany = factory.GetCompanyInAuth(auth);
                if (lstCompany == null) throw new Exception("DAL.SalesOrder.CompanyFactory.GetCompanyInAuth()==null");
                return lstCompany;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCompanyInAuth");
                return null;
            }
        }

        /// <summary>
        /// dtCompany by Angel Jiang
        /// </summary>
        /// <returns></returns>
        public static List<Company> dtCompany()
        {
            try
            {
                List<Company> dtCompany = factory.dtCompany();
                if (dtCompany == null) throw new Exception("DAL.SalesOrder.CompanyFactory.dtCompany()==null");
                return dtCompany;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "dtCompany");
                return null;
            }
        }

        /// <summary>
        /// updateCompany by Angel Jiang
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static int updateCompany(Aden.Model.Common.Company lines)
        {
            int intExCount = 0;
            try
            {
                if (lines == null)
                {
                    throw new Exception("updateCompany is null");
                }
                intExCount = factory.updateCompany(lines);
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "updateCompany");
            }
            return intExCount;
        }

        public static int updateCompany(Company lines)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 检查服务公司的供应商代码在开票公司的供应商列表中是否存在
        /// </summary>
        /// <param name="companyInfo"></param>
        /// <returns></returns>
        public static bool CheckSupplierCode(Company companyInfo)
        {
            bool result = false;
            try
            {
                if (companyInfo == null)
                {
                    throw new Exception("CheckSupplierCode is null");
                }
                result = factory.CheckSupplierCode(companyInfo);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CheckSupplierCode");
            }
            return result;
        }

        /// <summary>
        /// 根据公司Code取得CostCenter集合
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static List<Model.Common.SingleField> GetActCostCenter(Model.Common.SingleField param)
        {
            try
            {
                List<Model.Common.SingleField> lstCostCenter = factory.GetActCostCenter(param);
                if (lstCostCenter == null || lstCostCenter.Count == 0) throw new Exception("DAL.SalesOrder.CompanyFactory.GetActCostCenter()==null");
                return lstCostCenter;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetActCostCenter");
                return null;
            }
        }
    }
}
