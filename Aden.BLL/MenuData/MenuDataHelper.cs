using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.MenuData;
using Aden.Model;
using Aden.Model.MenuData;

namespace Aden.BLL.MenuData
{
    public class MenuDataHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static MenuDataFactory factory = new MenuDataFactory();

        /// <summary>
        /// 菜单分类
        /// </summary>
        /// <param name="langCode"></param>
        /// <returns></returns>
        public static List<Model.MenuData.Item> menuClass(Item menu)
        {
            try
            {
                List<Item> menuClass = factory.menuClass(menu);
                if (menuClass == null) throw new Exception("DAL.SalesOrder.CompanyFactory.GetCompany()==null");
                return menuClass;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "menuClass");
                return null;
            }
        }

        public static int setMenuItem(Item menu)
        {
            int intExCount = 0;
            try
            {
                if (menu == null)
                {
                    throw new Exception("setMenuItem is null");
                }
                intExCount = factory.setMenuItem(menu);

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "setMenuItem");
            }
            return intExCount;
        }

        public static Item recipe(Item menu)
        {
            try
            {
                Item recipe = factory.recipe(menu.company,menu.itemGuid);
                if (recipe == null) throw new Exception("DAL.recipe is null");
                return recipe;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "recipe");
                return null;
            }
        }

        public static List<Item> searchItem(Dictionary<string,dynamic> menu)
        {
            try
            {
                List<Item> menuClass = factory.searchItem(menu);
                if (menuClass == null) throw new Exception("DAL.SalesOrder.CompanyFactory.GetCompany()==null");
                return menuClass;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "menuClass");
                return null;
            }
        }
        public static List<Item> itemSource(Dictionary<string, dynamic> menu)
        {
            try
            {
                List<Item> itemSource = factory.itemSource( menu);
                if (itemSource == null) throw new Exception("DAL.recipe is null");
                return itemSource;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "recipe");
                return null;
            }
        }



        public static List<itemSequence> itemSequence(itemSequence menu)
        {
            try
            {
                List<itemSequence> itemSequence = factory.itemSequence(menu);
                if (itemSequence == null) throw new Exception("DAL.SupplierList is null");
                return itemSequence;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "recipe");
                return null;
            }
        }


        public static List<Model.MenuData.ItemClass> GetMenu(Dictionary<string,dynamic> menu)
        {
            //,string guid
            try
            {
                //List<ItemClass> GetMenu = factory.GetMenu(menu["company"], menu["langCode"], menu["itemType"],
                //    menu.ContainsKey("costCenterCode") ? menu["costCenterCode"] : null,
                //    menu.ContainsKey("requiredDate") ? menu["requiredDate"] : null);

                List<ItemClass> GetMenu = factory.GetMenu(menu["langCode"],
                    menu.ContainsKey("itemType") ? menu["itemType"] : null,
                    menu.ContainsKey("company") ? menu["company"] : null,
                    menu.ContainsKey("costCenterCode") ? menu["costCenterCode"] : null,
                    menu.ContainsKey("requiredDate") ? menu["requiredDate"] : null);

                if (GetMenu == null) throw new Exception("DAL.SalesOrder.CompanyFactory.GetCompany()==null");
                return GetMenu;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "menuClass");
                return null;
            }
        }

    }
}
