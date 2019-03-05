using System;
using System.Collections.Generic;
using System.Linq;
using Aden.Util.Common;
using Aden.DAL.Order;
using Aden.Model;
using Aden.Model.Order.Warehouse;
using Aden.Model.Order.Purchase;
using Aden.Model.SDK.Finance;
using Aden.Model.Common;
using Aden.BLL.Account;

namespace Aden.BLL.Order
{
    public class WarehouseHelper
    {
        private readonly static WarehouseFactory factory = new WarehouseFactory();

        /// <summary>
        /// 获取消耗日期
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static List<CountsDate> GetCountsDate(PurchaseRequest request)
        {
            try
            {
                CompanyAddress company = Common.CompanyHelper.GetCompanyAddress(request.companyGuid);
                if (company == null)
                    throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + request.companyGuid);
                //获取当前公司账期信息
                DateTime eDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                DateRange range = Common.CompanyHelper.GetMonthlyJournalDateRange(company.erpCode, company.ip, "41", eDate);
                if (range == null) throw new Exception("Journal Closed.dbCode:" + company.erpCode);
                List<CountsDate> dates = factory.GetCountsDate(company.erpCode, company.ip, request.warehouseCode, eDate);
                for (int i = 0; i < dates.Count; i++)
                {
                    DateTime tmpDate = DateTime.Parse(dates[i].date);
                    if (tmpDate < (DateTime)range.startDate || tmpDate > (DateTime)range.endDate) dates[i].statusCode = 2;
                }
                return dates;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCountsDate");
                return null;
            }
        }

        /// <summary>
        /// 获取消耗数据
        /// </summary>
        /// <param name="request">参数</param>
        /// <returns></returns>
        public static CountsData GetCountsData(PurchaseRequest request)
        {
            try
            {
                CompanyAddress company = Common.CompanyHelper.GetCompanyAddress(request.companyGuid);
                if (company == null)
                    throw new Exception("Aden.BLL.Common.CompanyHelper.GetCompanyAddress doesn't exist companyGuid:" + request.companyGuid);
                CountsData data = new CountsData();
                data.lines = factory.GetCountsLines(company.erpCode, company.ip, request.warehouseCode,
                    DateTime.Parse(request.orderDate), request.langCode);
                data.costUnits = AccountHelper.GetUserCostUnits(request.userGuid);
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCountsData");
                return null;
            }
        }

        /// <summary>
        /// 保存消耗数据,直接调用SDK处理
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>处理结果</returns>
        public static bool Save(CountsData data)
        {

            return true;
        }
    }
}
