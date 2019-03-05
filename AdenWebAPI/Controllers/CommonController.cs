using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Aden.Model.Net;
using Aden.BLL.Common;

namespace AdenWebAPI.Controllers
{
    public class CommonController : BaseController
    {
        [HttpGet]
        public Response Language(string token)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = LanguageHelper.GetLanguage();
                if (data == null)
                {
                    response.code = "500";
                    response.message = "Get data failed";
                } 
                else
                {
                    response.code = "200";
                    response.content = data;
                }
            }
            return response;
        }

        [HttpGet]
        public Response LanguageData(string token, string code)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = LanguageHelper.GetLanguageData(code);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "Get data failed";
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
