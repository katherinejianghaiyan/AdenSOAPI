using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.DAL.MastData;
using Aden.Model;
using Aden.Model.Common;
using Aden.Model.MastData;

namespace Aden.BLL.MastData
{
    public class CalendarHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static CalendarFactory CalendarFactory = new CalendarFactory();

        /// <summary>
        /// 取得至下个工作日的所有日期 + weekday中文
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="strCostCenterCode"></param>
        /// <returns></returns>
        public static List<SingleField> GetToNextWorkingDay(GetToNextWorkingDayParam param)
        {
            try
            {
                List<SingleField> lstResult = CalendarFactory.GetToNextWorkingDay(param);
                if (lstResult.Count == 0) throw new Exception("DAL.MastData.CalendarFactory.GetToNextWorkingDay()==null");
                return lstResult;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetToNextWorkingDayParam");
                return null;
            }
        }
    }
}
