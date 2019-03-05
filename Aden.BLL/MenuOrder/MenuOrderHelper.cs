using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.MenuOrder;
using Aden.Model;
using Aden.Model.MenuOrder;
using Aden.Model.SOMastData;
using Aden.Model.MastData;
using Aden.Model.MenuData;

namespace Aden.BLL.MenuOrder
{
    public class MenuOrderHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static WeeklyMenuFactory weeklyFactory = new WeeklyMenuFactory();

        /// <summary>
        /// 菜单分类
        /// </summary>
        /// <param name="langCode"></param>
        /// <returns></returns>
        public static List<CCWhs> GetCCWhs(AuthCompany param)
        {
            try
            {
                List<CCWhs> lstCCWhs = weeklyFactory.GetCCWhs(param);
                if (lstCCWhs == null) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.GetCCWhs()==null");
                return lstCCWhs;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "lstCCWhs");
                return null;
            }
        }

        /// <summary>
        /// 根据CostCenter和SO ItemGUID集合取得餐次信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<SalesOrderItem> GetMealTypeList(MealTypeParam param)
        {
            try
            {
                List<SalesOrderItem> lstMealType = weeklyFactory.GetMealTypeList(param);
                if (lstMealType == null) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.GetMealTypeList()==null");
                return lstMealType;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "lstMealType");
                return null;
            }
        }

        /// <summary>
        /// 保存MenuOrder
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int SaveMenuOrder(SaveMenuOrderParam param)
        {
            try
            {
                int result = weeklyFactory.SaveMenuOrder(param);
                if (result == 0) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.SaveMenuOrder()==0");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "result");
                return 0;
            }
        }

        /// <summary>
        /// 读取MenuOrder
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<MenuOrderHead> GetMenuOrder(MenuOrderHead param)
        {
            try
            {
                List<MenuOrderHead> lstMenuOrders = weeklyFactory.GetMenuOrder(param);
                if (lstMenuOrders.Count == 0) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.GetMenuOrder()==null");
                return lstMenuOrders;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "lstMenuOrders");
                return null;
            }
        }



        /// <summary>
        /// 保存MenuOrder（直接采购）
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int SaveMenuOrderPurchase(MenuOrderPurchaseParam param)
        {
            try
            {
                int result = weeklyFactory.SaveMenuOrderPurchase(param);
                if (result == 0) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.SaveMenuOrderPurchase()==0");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "result");
                return 0;
            }
        }

        /// <summary>
        /// 读取MenuOrderPurchase
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static MenuOrderPurchaseParam GetMenuOrderPurchase(MenuOrderHead param)
        {
            try
            {
                MenuOrderPurchaseParam mopp = weeklyFactory.GetMenuOrderPurchase(param);
                if (mopp == null) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.GetMenuOrderPurchase()==null");
                return mopp;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetMenuOrderPurchase");
                return null;
            }
        }

        /// <summary>
        /// 取得供应商修改共用画面数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<POItem> GetPOItems(POItemParam param)
        {
            try
            {
                List<POItem> lstPOItem = weeklyFactory.GetPOItems(param);
                if (lstPOItem.Count == 0) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.GetPOItems()==null");
                return lstPOItem;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "lstPOItem");
                return null;
            }
        }

        /// <summary>
        /// 更新供应商
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int ChangeSupplier(POItemParam param)
        {
            try
            {
                int result = weeklyFactory.ChangeSupplier(param);
                if (result == 0) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.ChangeSupplier()==0");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "result");
                return 0;
            }
        }
        public static List<CCMast> ChangeRecipientsofWeeklyMenu(Dictionary<string, dynamic> param)
        {
            try
            {
                List<CCMast> ChangeRecipientsofWeeklyMenu =
                    weeklyFactory.ChangeRecipientsofWeeklyMenu(param);

                if(ChangeRecipientsofWeeklyMenu.Count==0)
                    throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.ChangeRecipientsofWeeklyMenu()==null");
                return ChangeRecipientsofWeeklyMenu;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "result");
                return null;
            }
        }
        /// <summary>
        /// 获取取SUZHYC周单数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<WeeklyMenu> GetMenuOrder_SUZHYC(WeeklyMenu param)
        {
            try
            {
                List<WeeklyMenu> lstMenuOrders = weeklyFactory.GetMenuOrder_SUZHYC(param);
                if (lstMenuOrders.Count == 0) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory..GetMenuOrder_SUZCHY()==null");
                return lstMenuOrders;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "lstMenuOrders");
                return null;
            }
        }
        /// <summary>
        /// 存数据SUZHYC
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int SaveMenuOrder_SUZHYC(WeeklyMenu param)
        {
            try
            {
                int result = weeklyFactory.SaveMenuOrder_SUZHYC(param);
                if (result == 0) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.SaveMenuOrder_SUZHYC()==0");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "result");
                return 0;
            }
        }

        /// <summary>
        /// 菜单分类
        /// </summary>
        /// <param name="langCode"></param>
        /// <returns></returns>
        public static List<CCWhs> GetCCWhs_SUZHYC(AuthCompany param)
        {
            try
            {
                List<CCWhs> lstCCWhs = weeklyFactory.GetCCWhs_SUZHYC(param);
                if (lstCCWhs == null) throw new Exception("DAL.MenuOrder.WeeklyMenuFactory.GetCCWhs_SUZCHY==null");
                return lstCCWhs;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "lstCCWhs");
                return null;
            }
        }
    }
}
