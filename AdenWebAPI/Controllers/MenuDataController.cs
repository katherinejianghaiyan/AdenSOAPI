using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Aden.Util.Common;
using Aden.BLL.MenuData;
using Aden.Model.MenuData;
using Newtonsoft.Json;
using Aden.Model.Net;

namespace AdenWebAPI.Controllers
{
    public class MenuDataController : BaseController
    {
        [HttpPost]
        public Response menuClass(string token, [FromBody]Item menu)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = MenuDataHelper.menuClass(menu);
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
        public Response setMenuItem(string token, [FromBody] Item menu)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                int intExCount = 0;
                intExCount = MenuDataHelper.setMenuItem(menu);

                if (intExCount == 0)
                {
                    response.code = "500";
                    response.message = "process failed";
                }
                else
                {
                    response.code = "200";
                }
            }
            return response;
        }

        [HttpPost]
        public Response recipe(string token, [FromBody]Item menu)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = MenuDataHelper.recipe(menu);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "Invalid token";
                }
                else
                {
                    response.code = "200";
                    response.content = data;//.ToDynamic("8C3F001C-44E5-4CFC-A356-F22812B6728F");
                }
            }
            return response;
        }


        [HttpPost]
        public Response searchItem(string token, [FromBody]string d)
        {
            //int aa = System.Environment.TickCount;
            Dictionary<string, dynamic> menu = d.JSDeserialize<Dictionary<string, dynamic>>();
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = MenuDataHelper.searchItem(menu);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    string s = menu.ContainsKey("DynamicFieldsGUID") ? menu["DynamicFieldsGUID"].ToString() : "";
                    response.content = data.ToDynamicList(s);
                }
            }
            //int bb = System.Environment.TickCount - aa;
            return response;
        }

        [HttpPost]
        //dbName, costCenterCode, costCenterCode, requiredDate, directBuy
        public Response itemSource(string token, [FromBody]string d)
        {
            Dictionary<string, dynamic> menu = d.JSDeserialize<Dictionary<string, dynamic>>();
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            { 
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = MenuDataHelper.itemSource(menu);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "Invalid token";
                }
                else
                {
                    response.code = "200";
                    string s = menu.ContainsKey("DynamicFieldsGUID") ? menu["DynamicFieldsGUID"].ToString() : "";
                    response.content = data.ToDynamicList(s);
                }
            }
            return response;
        }


        [HttpPost]
        public Response itemSequence(string token, [FromBody] itemSequence menu)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = MenuDataHelper.itemSequence(menu);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "Invalid token";
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
        //company, langCode, itemType, costCenterCode, date
        public Response GetMenu(string token, [FromBody]string d)
        {
            Dictionary<string, dynamic> menu = d.JSDeserialize<Dictionary<string, dynamic>>();
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                
                var data = MenuDataHelper.GetMenu(menu);
                
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


    }
}