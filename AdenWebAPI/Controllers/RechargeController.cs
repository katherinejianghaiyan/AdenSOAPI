using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Aden.Model.Net;
using Aden.Model.MastData;
using Aden.Model.WeChat;
using Aden.BLL.WeChatPay;
using Aden.Model.Common;

namespace AdenWebAPI.Controllers
{
    public class RechargeController : BaseController
    {
        public static string _token = "adenservices";

        public Response ModifyWxUser(string token, [FromBody]WxUserMast param)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = RechargeHelper.ModifyWxUser(param);
                if (result == -1)
                {
                    response.code = "500";
                    response.message = "Get data failed";
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
        public Response GetCostCenterCode(string token, [FromBody]WxUserMast param)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                CCMast ccm = RechargeHelper.GetCostCenterCode(param);
                if (ccm == null)
                {
                    response.code = "500";
                    response.message = "Get data failed";
                }
                else
                {
                    response.code = "200";
                    response.content = ccm;
                }
            }

            return response;
        }

        [HttpPost]
        public Response GetCardInfo(string token, [FromBody]CardInfoParam param)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = RechargeHelper.GetCardInfo(param);
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

        /// <summary>
        /// 根据卡号和userGuid取得余额（外部HttpGet方法）
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetBalance(string token, [FromBody]CardInfoParam param)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                try
                {
                    var data = RechargeHelper.GetBalance(param);
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
                catch(Exception ex)
                {
                    response.code = "404";
                    response.message = ex.Message;
                    var data = RechargeHelper.GetLastRecharge(param);
                    response.content = data;
                } 
            }
            return response;
        }

        /// <summary>
        /// 取得交易记录
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetTransLog(string token, [FromBody]TransLogParam param)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = RechargeHelper.GetTransLog(param);
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

        /// <summary>
        /// 根据WechatId取得卡信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetCardList(string token, [FromBody]WxUserMast param)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = RechargeHelper.GetCardList(param);
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

        /// <summary>
        /// 绑定/解绑指定卡号
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response BindCard(string token, [FromBody]CardInfo param)
        {
            Response response = new Response();

            string[] aryStr = token.Split(',');
            string tmpToken = aryStr[0];
            string mode = aryStr[1];

            if (string.IsNullOrEmpty(tmpToken) || !tmpToken.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = RechargeHelper.BindCard(param, mode);
                if (result == 0)
                {
                    response.code = "500";
                    response.message = "Update failed";
                }
                else
                {
                    response.code = "200";
                    response.content = result;
                }
            }
            return response;
        }
    }
}