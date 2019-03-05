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
using Aden.Model.RightsData;

namespace AdenWebAPI.Controllers
{
    public class RightsDataController : BaseController
    {

        /// <summary>
        /// 成本中心列表 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="item"></param>
        /// <returns></returns>
       [HttpPost]
       public Response GetUserCompany(string token, [FromBody]Param item)
        {
            Response response = new Response();
            if (string.IsNullOrWhiteSpace(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = RightsDataHelper.GetUserCompany(item);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "No data";
                }
                else
                {
                    response.code = "200";
                    response.content = data;
                }
            }
            return response;
        }

       /// <summary>
       /// 档口和产品选项
       /// </summary>
       /// <param name="token"></param>
       /// <param name="item"></param>
       /// <returns></returns>
       [HttpPost]
       public Response GetItems(string token,[FromBody]Param item)
       {
            Response response = new Response();
            if (string.IsNullOrWhiteSpace(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid Token";
            }
            else
            {
                var data = RightsDataHelper.items(item);
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

        /// <summary>
        /// 新增档口和餐次
        /// </summary>
        /// <param name="token"></param>
        /// <param name="menu"></param>
        /// <returns></returns>
       [HttpPost] 
       public Response NewCCWindowMeal(string token,[FromBody] Param menu)
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
                intExCount = RightsDataHelper.NewCCWindowMeal(menu);

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
    }
}