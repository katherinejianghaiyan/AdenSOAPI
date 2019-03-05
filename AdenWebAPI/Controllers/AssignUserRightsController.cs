using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Aden.Util.Common;
using Aden.BLL.MenuData;
using Aden.Model.AssignUserRights;
using Newtonsoft.Json;
using Aden.Model.Net;
using Aden.BLL.AssignUserRights;

namespace AdenWebAPI.Controllers
{
    public class AssignUserRightsController : BaseController
    {
       [HttpPost]
       public Response AssignUserRights(string token,UserRights user)
        {
            Response response = new Response();
            if(string.IsNullOrEmpty(token)||!token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = AssignUserRightsHelper.GetUserAuthority(user.userGuid);
                if (data == null)
                {
                    response.code = "500";
                    response.message = "no content";
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