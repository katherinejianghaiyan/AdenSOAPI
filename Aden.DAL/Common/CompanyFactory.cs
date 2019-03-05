using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Aden.Util.Common;
using Aden.Model.Common;
using Aden.Util.Database;

namespace Aden.DAL.Common
{
    public class CompanyFactory
    {
        /// <summary>
        /// 根据公司的Guid返回唯一一条公司地址对象
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        public CompanyAddress GetCompanyAddress(string companyGuid)
        {
            string sql = "select top 1 erpCode, ip from tblCompany where guid = '" + companyGuid +"'";

            return SqlServerHelper.GetEntity<CompanyAddress>(SqlServerHelper.baseConn(), sql);
        }

        /// <summary>
        /// 获取EndDate之前一个月的日记账有效日期段
        /// </summary>
        /// <param name="erpCode">数据库公司代码</param>
        /// <param name="ip">数据库IP地址</param>
        /// <param name="journalNumber">日记账</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        public DateRange GetMonthlyJournalDateRange(string erpCode, string ip, string journalNumber, DateTime endDate)
        {
            DateTime startDate = endDate.AddMonths(-1);
            string sql = "select bkjrcode,periode from afgper (nolock) where ltrim(dagbknr)='{0}' and bkjrcode>={1} "
                + "and bkjrcode<={2} order by bkjrcode,periode";
            DataTable data = SqlServerHelper.GetDataTable(string.Format(SqlServerHelper.customerConn(), ip, erpCode), 
                string.Format(sql, journalNumber, startDate.Year.ToString(), endDate.Year.ToString()));
            if (data == null || data.Rows.Count == 0) return null;
            DateTime? sDate = null;
            DateTime? eDate = null;
            while (startDate <= endDate)
            {
                var query = data.AsEnumerable().Where(dr => dr.Field<Int16>("bkjrcode").ToString().Equals(startDate.Year.ToString())
                    && dr.Field<string>("periode").ToStringTrim().Equals(startDate.Month.ToString()));
                if (!query.Any()) //未关帐
                {
                    if (startDate < endDate) sDate = startDate;
                    else eDate = startDate;
                }
                else
                {
                    if (startDate == endDate)
                    {
                        if (sDate != null) eDate = DateTime.Parse(((DateTime)sDate).ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1);
                    }
                    else sDate = DateTime.Parse(startDate.ToString("yyyy-MM-01")).AddMonths(1);
                }
                startDate = startDate.AddMonths(1);
            }
            if (sDate == null || eDate == null) return null;
            return new DateRange() { startDate = (DateTime)sDate, endDate = (DateTime)eDate };
        }
    }
}
