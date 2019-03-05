using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Model.Common;
using Aden.Model.MastData;
using Aden.Util.Database;
using Aden.Util.AdenCommon;

namespace Aden.DAL.MastData
{
    public class CalendarFactory
    {
        public DateTime GetNextWorkingDay(DateTime date, string costCenterCode)
        {
            if (string.IsNullOrWhiteSpace(costCenterCode))
                throw new Exception("Identify Cost Center");

            //日历表
            string sql = "select a1.dbname,a1.startdate,a1.enddate,a1.working " +
                "from tblcalendars a1, CCMAST a2 " +
                "where a2.CostCenterCode = '{0}' and isnull(a1.dbname, a2.dbname)= a2.dbname " +
                "and a1.startdate <= '{2}' and a1.enddate > '{1}'";
            sql = string.Format(sql, costCenterCode, date.ToString("yyyy-MM-dd"),date.AddMonths(1).ToString("yyyy-MM-dd"));
            var list = SqlServerHelper.GetDynamicList(SqlServerHelper.salesorderConn(), sql);

            while (true)
            {
                date = date.AddDays(1);

                if (list == null || !list.Where(q => q.startdate <= date && q.enddate >= date).Any())
                {
                    //周一~五工作
                    if (((int)date.DayOfWeek) < 6 && ((int)date.DayOfWeek) > 0) return date;
                    continue;
                }

                //日历表中定义为工作日
                if (list.Where(q => q.startdate <= date && q.enddate >= date).FirstOrDefault().working) 
                    return date;
            }            
        }

        /// <summary>
        /// 取得至下个工作日的所有日期 + weekday中文
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="strCostCenterCode"></param>
        /// <returns></returns>
        public List<SingleField> GetToNextWorkingDay(GetToNextWorkingDayParam param)
        {
            if (string.IsNullOrWhiteSpace(param.date) || string.IsNullOrWhiteSpace(param.costCenterCode))
                return null;

            List<SingleField> lstResult = new List<SingleField>();
            DateTime dateTemp = Convert.ToDateTime(param.date);
            DateTime endDate = (new CalendarFactory()).GetNextWorkingDay(dateTemp, param.costCenterCode);

            dateTemp = dateTemp.AddDays(1);

            while (dateTemp <= endDate)
            {
                SingleField sf = new SingleField();

                sf.date = DateCommon.convertDateTime(dateTemp, "yyyy-MM-dd");
                sf.weekDay = DateCommon.ToWeekDay(dateTemp, param.lang, param.formatString);

                lstResult.Add(sf);

                dateTemp = dateTemp.AddDays(1);
            }
            return lstResult;
        }
    }
}
