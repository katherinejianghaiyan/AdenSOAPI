using Aden.DAL.Common;
using Aden.Model.Common;
using System;

namespace Aden.BLL.Common
{
    public class CompanyHelper
    {
        private readonly static CompanyFactory factory = new CompanyFactory();

        /// <summary>
        /// 获取公司地址
        /// </summary>
        /// <param name="companyGuid">公司Guid</param>
        /// <returns></returns>
        public static CompanyAddress GetCompanyAddress(string companyGuid)
        {
            try
            {
                CompanyAddress cmpAddress = factory.GetCompanyAddress(companyGuid);
                if (cmpAddress == null)
                {
                    throw new Exception("DAL.Common.CompanyFactory.GetCompanyAddress()==null");
                }
                return cmpAddress;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Model.Log()
                {
                    message = ex.Message
                }, "GetCompanyAddress");

                return null;
            }
        }

        /// <summary>
        /// 获取EndDate之前一个月的有效日记账期间
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库IP地址</param>
        /// <param name="journalNumber">日记账</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>日期段</returns>
        public static DateRange GetMonthlyJournalDateRange(string erpCode, string ip, string journalNumber, DateTime endDate)
        {
            try
            {
                return factory.GetMonthlyJournalDateRange(erpCode, ip, journalNumber, endDate);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Model.Log()
                {
                    message = ex.Message
                }, "GetMonthlyJournalDateRange");

                return null;
            }
        }
    }
}
