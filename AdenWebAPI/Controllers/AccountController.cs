using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Aden.Model.Net;
using Aden.Model.Account;
using Aden.BLL.Account;

namespace AdenWebAPI.Controllers
{
    public class AccountController : BaseController
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
                if(data == null)
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
        public Response RoleMenu(string token, [FromBody]AccountUser user)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = AccountHelper.GetUserMenus(user);
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
        public Response UserData(string token, [FromBody]AccountUserMenu userMenu)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = AccountHelper.GetUserDatas(userMenu);
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
