using System;
using System.Collections.Generic;
using System.Linq;
using Aden.Util.Common;
using Aden.DAL.Order;
using Aden.Model;
using Aden.Model.Order.Purchase;
using Aden.Model.MastData;
using Aden.Model.Common;

namespace Aden.BLL.Order
{
    public class PurchaseHelper
    {
        private readonly static PurchaseFactory factory = new PurchaseFactory();

        /// <summary>
        /// 获取用户允许操作采购订单的日期范围
        /// </summary>
        /// <param name="type">WeeklyPO,DailyPO</param>
        /// <param name="userGuid">用户唯一标识</param>
        /// <returns>日期范围对象</returns>
        public static Aden.Model.Common.DateRange GetOrderDateRange(string type, string userGuid)
        {
            try
            {
                int days = factory.GetOrderDateRange(type, userGuid);
                DateTime baseDate = new DateTime(1970, 1, 1);
                DateTime now = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                if (type.Equals("WeeklyPO"))
                {
                    if (days < 7) days = 7;
                    int i = (int)now.DayOfWeek;
                    if (i == 0) i = 7;
                    days += 7 - i;
                }
                if (type.Equals("DailyPO") && days < 3) days = 3;
                return new Aden.Model.Common.DateRange()
                {
                    startDate = (long)(new TimeSpan(now.AddDays(1).ToUniversalTime().Ticks - baseDate.Ticks).TotalMilliseconds),
                    endDate = (long)(new TimeSpan(now.AddDays(days).ToUniversalTime().Ticks - baseDate.Ticks).TotalMilliseconds),
                };
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetOrderDateRange");
                return null;
            }
        }

        /// <summary>
        /// 根据PurchaseRequest获取供应商列表
        /// </summary>
        /// <param name="companyGuid">公司Guid</param>
        /// <param name="langCode">语言代码</param>
        /// <param name="orderDate">订单日期</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <returns>供应商列表</returns>
        public static List<Supplier> GetSupplierList(string companyGuid, string langCode, string orderDate, string warehouseCode, string costCenterCode, string type)
        {
            try
            {
                CompanyAddress company = Common.CompanyHelper.GetCompanyAddress(companyGuid);
                if (company == null)
                    throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + companyGuid);
                if (type == "pricelist") return factory.GetSupplierListFromPriceList(company.erpCode, company.ip, langCode, DateTime.Parse(orderDate).ToString("yyyy-MM-dd"), warehouseCode);
                if (type == "order") return factory.GetSupplierListFromOrder(company.erpCode, company.ip, langCode, DateTime.Parse(orderDate).ToString("yyyy-MM-dd"), warehouseCode, costCenterCode);
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "getSuppliersData");
                return null;
            }

        }

        /// <summary>
        /// 根据供应商获取该供应商的产品信息
        /// </summary>
        /// <param name="companyGuid">公司Guid</param>
        /// <param name="date">订单日期</param>
        /// <param name="warehouseCode">仓库代码</param>
        /// <param name="langCode">语言代码</param>
        /// <param name="supplierCode">供应商代码</param>
        /// <returns>供应商的产品信息列表</returns>
        public static List<PurchaseItem> GetItemListFromPriceList(string companyGuid, string date, string warehouseCode, string langCode, string supplierCode)
        {
            try
            {
                CompanyAddress cmpAddress = Aden.BLL.Common.CompanyHelper.GetCompanyAddress(companyGuid);

                if (cmpAddress == null) throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + companyGuid);

                return factory.GetItemListFromPriceList(cmpAddress.erpCode, cmpAddress.ip, date, warehouseCode, langCode, new List<string>() { supplierCode });

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetPriceItemByPurchase");
                return null;
            }
        }

        /// <summary>
        /// 获取采购订单数据
        /// </summary>
        /// <param name="request">前端传递的请求参数</param>
        /// <returns>采购单数据</returns>
        public static PurchaseOrderData GetPurchaseOrderData(PurchaseRequest request)
        {
            try
            {
                CompanyAddress company = Common.CompanyHelper.GetCompanyAddress(request.companyGuid);

                if (company == null) throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + request.companyGuid);
                List<PurchaseOrderLine> lines = factory.GetPurchaseOrderData(company.erpCode, company.ip,
                    DateTime.Parse(request.orderDate).ToString("yyyy-MM-dd"), request.warehouseCode,
                    request.costCenterCode, request.supplierCode, request.langCode);
                return new PurchaseOrderData() { lines = lines };
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetPurchaseOrderData");
                return null;
            }
        }

        /// <summary>
        /// 保存采购订单
        /// </summary>
        /// <param name="data">前端传回的订单数据,如lineGuid存在则更新,不存在则添加</param>
        /// <returns></returns>
        public static bool SavePurchaseOrder(PurchaseOrderData data)
        {
            try
            {
                if (data.lines == null || data.lines.Count == 0) return false;
                CompanyAddress company = Common.CompanyHelper.GetCompanyAddress(data.companyGuid);
                if (company == null) throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + data.companyGuid);
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var query1 = data.lines.Where(l => !string.IsNullOrEmpty(l.lineGuid));
                if (query1.Any()) factory.UpdatePurchaseOrder(query1.ToList(), data.userGuid, now);
                var query2 = data.lines.Where(l => string.IsNullOrEmpty(l.lineGuid) && l.qty.ToDouble() > 0);
                if (query2.Any())
                {
                    string date = DateTime.Parse(data.orderDate).ToString("yyyy-MM-dd");
                    string description = string.Empty;
                    if (data.costCenterCode.Length >= 6) description = "PO-" + data.costCenterCode.Substring(3, 3) + "(" + date + ")";
                    else description = "PO-" + data.costCenterCode + "(" + date + ")";
                    factory.AddPurchaseOrder(query2.ToList(), company.erpCode, date, data.warehouseCode, data.costCenterCode, description, "001", data.userGuid, now);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SavePurchaseOrder");
                return true;
            }
        }

        /// <summary>
        /// 保存采购收货
        /// </summary>
        /// <param name="data">前端采购收货数据</param>
        /// <returns>处理状态响应</returns>
        public static bool SavePurchaseReceipt(PurchaseReceiptData data)
        {
            try
            {
                if (data.lines == null || data.lines.Count == 0) return false;
                CompanyAddress company = Common.CompanyHelper.GetCompanyAddress(data.companyGuid);
                if (company == null) throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + data.companyGuid);
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                return factory.AddPurchaseReceipt(data.lines, company.erpCode, data.receiptDate, data.warehouseCode, data.supplierCode, data.poType,
                    data.userGuid, now);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SavePurchaseReceipt");
                return true;
            }
        }

        /// <summary>
        /// 获取收货供应商日期数据
        /// </summary>
        /// <param name="request">前端请求参数</param>
        /// <returns>数据</returns>
        public static List<PurchaseReceiptSupplierDate> GetReceiptSupplierDate(PurchaseRequest request)
        {
            try
            {
                CompanyAddress company = Common.CompanyHelper.GetCompanyAddress(request.companyGuid);
                if (company == null)
                    throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + request.companyGuid);
                //获取当前公司账期信息
                DateTime eDate = DateTime.Now;
                DateRange range = Common.CompanyHelper.GetMonthlyJournalDateRange(company.erpCode, company.ip, "41", eDate);
                if (range == null) throw new Exception("Journal Closed.dbCode:" + company.erpCode);
                List<PurchaseReceiptSupplierDate> data = factory.GetReceiptSupplierDate(company.erpCode, company.ip,
                    request.langCode, request.warehouseCode, ((DateTime)range.startDate).ToString("yyyy-MM-dd"),
                    ((DateTime)range.endDate).ToString("yyyy-MM-dd"));

                return data;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetReceiptSupplierDate");
                return null;
            }
        }

        /// <summary>
        /// 获取采购收货数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static PurchaseReceiptData GetPurchaseReceiptData(PurchaseRequest request)
        {
            try
            {
                CompanyAddress company = Common.CompanyHelper.GetCompanyAddress(request.companyGuid);
                if (company == null)
                    throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + request.companyGuid);
                List<PurchaseReceiptLine> lines = factory.GetPurchaseReceiptData(company.erpCode, company.ip, request.orderDate,
                    request.poType, request.supplierCode, request.warehouseCode, request.langCode);
                if (lines == null || lines.Count == 0) return null;
                var query = lines.GroupBy(l => l.itemCode).Where(g => g.Select(r => r.price).Distinct().Count() > 1);
                if (query.Any()) throw new Exception("Exist different price in the same supplier.dbCode:" + company.erpCode
                     + ";supplierCode:" + request.supplierCode + ";warehouseCode:" + request.warehouseCode + ";date:" + request.orderDate);
                return new PurchaseReceiptData() { lines = lines };
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetReceiptData");
                return null;
            }
        }
    }
}
