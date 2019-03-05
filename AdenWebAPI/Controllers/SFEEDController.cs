using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Aden.Model.Net;

namespace AdenWebAPI.Controllers
{
    public class SFEEDController : BaseController
    {
        [HttpPost]
        public Response GetCostCenter(string token)
        {
            Response response = new Response();
            if(string.IsNullOrWhiteSpace(token)||!token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                
            }

            return null;
        }
    }
}