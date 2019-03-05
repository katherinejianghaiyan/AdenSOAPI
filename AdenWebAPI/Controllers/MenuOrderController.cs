using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Aden.Util.Common;
using Aden.BLL.MenuOrder;
using Aden.Model.SOMastData;
using Newtonsoft.Json;
using Aden.Model.Net;
using Aden.Model.MenuOrder;
using Aden.Model.Common;
using Aden.Model.MenuData;

namespace AdenWebAPI.Controllers
{
    public class MenuOrderController : BaseController
    {
        [HttpPost]
        public Response GetCCWhs(string token, [FromBody]AuthCompany param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = MenuOrderHelper.GetCCWhs(param);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = data;
                }
            }
            return response;
        }

        [HttpPost]
        public Response GetMealTypeList(string token, [FromBody]MealTypeParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = MenuOrderHelper.GetMealTypeList(param);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = data;
                }
            }
            return response;
        }

        [HttpPost]
        public Response SaveMenuOrder(string token, [FromBody]SaveMenuOrderParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = MenuOrderHelper.SaveMenuOrder(param);
                if (result == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = result;
                }
            }
            return response;
        }

        [HttpPost]
        public Response GetMenuOrder(string token, [FromBody]MenuOrderHead param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                List<MenuOrderHead> lstMenuOrders = MenuOrderHelper.GetMenuOrder(param);

                if (lstMenuOrders == null || lstMenuOrders.Count == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = lstMenuOrders;
                }
            }
            return response;
        }

        /// <summary>
        /// 保存MenuOrder(直接采购)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SaveMenuOrderPurchase(string token, [FromBody]MenuOrderPurchaseParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = MenuOrderHelper.SaveMenuOrderPurchase(param);
                if (result == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = result;
                }
            }
            return response;
        }

        [HttpPost]
        public Response GetMenuOrderPurchase(string token, [FromBody]MenuOrderHead param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                MenuOrderPurchaseParam mopp = MenuOrderHelper.GetMenuOrderPurchase(param);

                if (mopp.lstMenuPart == null && mopp.lstPurchasePart == null)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = mopp;
                }
            }
            return response;
        }

        [HttpPost]
        public Response GetPOItems(string token, [FromBody]POItemParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                List<POItem> lstPOItem = MenuOrderHelper.GetPOItems(param);

                if (lstPOItem == null || lstPOItem.Count == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = lstPOItem;
                }
            }
            return response;
        }

        [HttpPost]
        public Response ChangeSupplier(string token, [FromBody]POItemParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = MenuOrderHelper.ChangeSupplier(param);
                if (result == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = result;
                }
            }
            return response;
        }

        [HttpPost]
        public Response ChangeRecipientsofWeeklyMenu(string token, [FromBody]string d)
        {
            Dictionary<string, dynamic> param = d.JSDeserialize<Dictionary<string, dynamic>>();
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = MenuOrderHelper.ChangeRecipientsofWeeklyMenu(param);

                if (data == null)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = data;
                    //string s = null; //menu.ContainsKey("DynamicFieldsGUID") ? menu["DynamicFieldsGUID"].ToString() : "";
                    //response.content = data.ToDynamicList(s);
                }
            }
            return response;
        }

        /// <summary>
        /// 获取SUZHYC的周单数据
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetMenuOrder_SUZHYC(string token, [FromBody]WeeklyMenu param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var lstMenuOrders = MenuOrderHelper.GetMenuOrder_SUZHYC(param);

                if (lstMenuOrders == null || lstMenuOrders.Count == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = lstMenuOrders.ToDynamicList("330F333F-E651-48D4-A8B6-535890ABF336");
                }
            }
            return response;
        }

        /// <summary>
        /// 存数据SUZHYC
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SaveMenuOrder_SUZHYC(string token, [FromBody]WeeklyMenu param)
        {
          ;
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result =  MenuOrderHelper.SaveMenuOrder_SUZHYC(param);
                if (result == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = result;
                }
            }
            return response;
        }

        /// <summary>
        /// 获取成本中心
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetCCWhs_SUZHYC(string token, [FromBody]AuthCompany param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = MenuOrderHelper.GetCCWhs_SUZHYC(param);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = data;
                }
            }
            return response;
        }

        [HttpPost]
        public Response tryRes(string token,string s)
        {
            string ss = "a;b;c;d";
            List<string> list = new List<string>(ss.Split(new char[] {';' }));

            return null;
        }

    }
}