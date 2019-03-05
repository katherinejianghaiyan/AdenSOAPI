using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.MenuData;
using Aden.Model;
using Aden.Model.MenuData;
using Aden.Model.RightsData;

namespace Aden.BLL.MenuData
{
    public class RightsDataHelper
    {
        private readonly static RightsDataFactory factory = new RightsDataFactory();
        /// <summary>
        /// 成本中心列表
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<CCOptions> GetUserCompany(Param item)
        {
            try
            {

                List<CCOptions> GetUserCompany = factory.GetUserCompany(item.userId);
                if(GetUserCompany == null) throw new Exception("DAL.GetCostCenter is null");
                return GetUserCompany;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "RightsDataHelper");
                return null;
            }
        }

       


        /// <summary>
        /// 档口选项和产品选项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Items items(Param item)
        { 
            try
            {
                Items GetItems = factory.GetItems(item.costCenterCode);
                if (GetItems == null) throw new Exception("DAL.items is null");
                return GetItems;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                },"items");
                return null;
            }
        }
        /// <summary>
        /// 新增档口和餐次
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public static int NewCCWindowMeal(Param menu)
        {
            int intExCount = 0;
            try
            {
                if (menu == null)
                {
                    throw new Exception("setMenuItem is null");
                }
                intExCount = factory.NewCCWindowMeal(menu);
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "NewCCWindowMeal");
            }
            return intExCount;
        }
    }
}
