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
    public class MastDataHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static MastDataFactory factory = new MastDataFactory();

        /// <summary>
        /// 根据ERP公司获取客户列表
        /// </summary>
        /// <returns>客户列表</returns>
        public static List<Customer> GetCustom(CompanyAddress compAddr)
        {
            try
            {
                if (compAddr == null) throw new Exception("Company is null");
                List<Customer> lstCustom = factory.GetCustomer(compAddr);
                if (lstCustom == null) throw new Exception("DAL.SalesOrder.MastDataFactory.GetCustomer()==null");
                return lstCustom;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCustom");
                return null;
            }
        }

        /// <summary>
        /// 根据ERP公司获取币别列表
        /// </summary>
        /// <returns>币别列表</returns>
        public static List<Currency> GetCurr(CompanyAddress compAddr)
        {
            try
            {
                if (compAddr == null) throw new Exception("Company is null");
                List<Currency> lstCurr = factory.GetCurr(compAddr);
                if (lstCurr == null) throw new Exception("DAL.SalesOrder.MastDataFactory.GetCurr()==null");
                return lstCurr;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCurr");
                return null;
            }
        }

        /// <summary>
        /// 根据ERP公司获取付款方式列表
        /// </summary>
        /// <returns>付款方式列表</returns>
        public static List<Payment> GetPayment(CompanyAddress compAddr)
        {
            try
            {
                if (compAddr == null) throw new Exception("Company is null");
                List<Payment> lstPayment = factory.GetPayment(compAddr);
                if (lstPayment == null) throw new Exception("DAL.SalesOrder.MastDataFactory.GetPayment()==null");
                return lstPayment;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetPayment");
                return null;
            }
        }

        /// <summary>
        /// 根据ERP公司获取成本中心列表
        /// </summary>
        /// <returns>成本中心列表</returns>
        public static List<CostCenter> GetCostCenter(CompanyAddress compAddr)
        {
            try
            {
                if (compAddr == null) throw new Exception("Company is null");
                List<CostCenter> lstCostCenter = factory.GetCostCenter(compAddr);
                if (lstCostCenter == null) throw new Exception("DAL.SalesOrder.MastDataFactory.GetCostCenter()==null");
                return lstCostCenter;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCostCenter");
                return null;
            }
        }

        /// <summary>
        /// 根据ERP公司获取税列表
        /// </summary>
        /// <returns>税列表</returns>
        public static List<Tax> GetTax(CompanyAddress compAddr)
        {
            try
            {
                if (compAddr == null) throw new Exception("Company is null");
                List<Tax> lstTax = factory.GetTax(compAddr);
                if (lstTax == null) throw new Exception("DAL.SalesOrder.MastDataFactory.GetTax()==null");
                return lstTax;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetTax");
                return null;
            }
        }

        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <returns>税列表</returns>
        public static List<Product> GetProduct()
        {
            try
            {
                List<Product> lstProduct = factory.GetProduct();
                if (lstProduct == null) throw new Exception("DAL.SalesOrder.MastDataFactory.GetProduct()==null");
                return lstProduct;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetProduct");
                return null;
            }
        }

        /// <summary>
        /// 获取价格单位列表
        /// </summary>
        /// <returns>税列表</returns>
        public static List<PriceUnit> GetPriceUnit()
        {
            try
            {
                List<PriceUnit> lstPriceUnit = factory.GetPriceUnit();
                if (lstPriceUnit == null) throw new Exception("DAL.SalesOrder.MastDataFactory.GetPriceUnit()==null");
                return lstPriceUnit;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetPriceUnit");
                return null;
            }
        }

        /// <summary>
        /// 获取SO状态主数据
        /// </summary>
        /// <returns>税列表</returns>
        public static List<Status> Status(string type)
        {
            try
            {
                List<Status> lstStatus = factory.Status(type);
                if (lstStatus == null) throw new Exception("DAL.SalesOrder.MastDataFactory.GetStatus()==null");
                return lstStatus;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GeStatus");
                return null;
            }
        }
    }
}
