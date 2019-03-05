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
    public class CCMastHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static CCMastFactory ccmf = new CCMastFactory();

        /// <summary>
        /// 取得至下个工作日的所有日期 + weekday中文
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="strCostCenterCode"></param>
        /// <returns></returns>
        public static CCMast GetCCMastInfo(SingleField param)
        {
            try
            {
                CCMast ccm = ccmf.GetCCMastInfo(param.code);
                if (ccm == null) throw new Exception("DAL.MastData.CalendarFactory.GetCCMastInfo()==null");
                return ccm;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetCCMastInfoParam");
                return null;
            }
        }
    }
}
