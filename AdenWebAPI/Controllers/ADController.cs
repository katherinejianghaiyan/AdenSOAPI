using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Aden.Model.Net;
using Aden.Model.SOAccount;
using Aden.BLL.SOAccount;


namespace AdenWebAPI.Controllers
{
    public class ADController : BaseController
    {
        /// <summary>
        /// 根据用户名和密码获取AD域属性
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <returns></returns>
        [HttpGet]
        public Response ADCheck(string token, string userName,string passWord)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = AccountHelper.CheckADProp(userName, passWord);
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