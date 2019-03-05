using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Aden.Model.Net;
using Aden.Model.SOAccount;
using Aden.BLL.SOAccount;

namespace AdenWebAPI.Controllers
{
    public class SOAccountController : BaseController
    {
        [HttpPost]
        public Response LoginUser(string token, [FromBody]LoginUser user)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = AccountHelper.GetUser(user);
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
        /// 取得权限列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetAuthList(string token, [FromBody]UserAuthParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = AccountHelper.GetAuthList(param);
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