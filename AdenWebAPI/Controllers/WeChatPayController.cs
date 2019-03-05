using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Aden.Model.Net;
using Aden.Model.WeChat;
using Aden.BLL.WeChatPay;
using Aden.Model.Common;

namespace AdenWebAPI.Controllers
{
    public class WeChatPayController : BaseController
    {
        public static string _token = "adenservices";

        [HttpPost]
        public Response UnifiedOrder(string token, [FromBody]WeChatInfo param)
        {
            Response response = new Response();
            try
            {
                string[] strAry = token.Split(',');
                string tmpToken = strAry[0];
                string appName = strAry[1];

                if (string.IsNullOrEmpty(tmpToken) || !tmpToken.Equals(_token))
                {
                    response.code = "404";
                    response.message = "Invild token";
                }
                else
                {
                    var data = WeChatPayHelper.UnifiedOrder(param, appName);
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
            }
            catch (Exception e)
            {
                response.code = "501";
                response.message = e.Message;
            }

            return response;
        }

        [HttpPost]
        public Response GetPaySign(string token, [FromBody]WeChatResult param)
        {
            Response response = new Response();
            string[] strAry = token.Split(',');
            string tmpToken = strAry[0];
            string appName = strAry[1];

            if (string.IsNullOrEmpty(tmpToken) || !tmpToken.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = WeChatPayHelper.GetPaySign(param, appName);
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

        [HttpPost]
        public Response GetOpenId(string token, [FromBody]WeChatInfo param)
        {
            Response response = new Response();
            string[] strAry = token.Split(',');
            string tmpToken = strAry[0];
            string appName = strAry[1];

            if (string.IsNullOrEmpty(tmpToken) || !tmpToken.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = WeChatPayHelper.GetOpenId(param, appName);
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

        [HttpPost]
        public Response GetAccessToken(string token)
        {
            Response response = new Response();
            string[] strAry = token.Split(',');
            string tmpToken = strAry[0];
            string appName = strAry[1];
            string isNew = strAry.Length > 2 ? strAry[2] : string.Empty;

            if (string.IsNullOrEmpty(tmpToken) || !tmpToken.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = WeChatPayHelper.GetAccessToken(appName, isNew);
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
        public Response GetWxPayConfig(string token, [FromBody]SingleField param)
        {
            Response response = new Response();
            string[] strAry = token.Split(',');
            string tmpToken = strAry[0];
            string appName = strAry[1];

            if (string.IsNullOrEmpty(tmpToken) || !tmpToken.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                string value = WeChatPayHelper.GetWxPayConfig(param, appName);
                if (string.IsNullOrEmpty(value))
                {
                    response.code = "500";
                    response.message = "Update failed";
                }
                else
                {
                    response.code = "200";
                    response.content = value;
                }
            }
            return response;
        }

        ////生成微信二维码
        //[HttpGet]
        //public HttpResponseMessage WxAppQRCode(string token)
        //{
        //    HttpResponseMessage response = new HttpResponseMessage();

        //    byte[] data = WeChatPayHelper.CreateWxCode("xxxxyyy","Shippment", "120DW201", "123456", 423);

        //    response.Content = new ByteArrayContent(data);

        //    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");

        //    return response;
        //}


        //生成微信二维码
        [HttpGet]
        public HttpResponseMessage WechatBarCode(string token, string data) 
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                Dictionary<string, string> keyDic =
                    Aden.Util.Common.JsonHelper.JSDeserialize<Dictionary<string, string>>(data);

                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(_token))
                    throw new Exception("Invalid Token");

                byte[] barcode = WeChatPayHelper.CreateWxCode(keyDic);
                    //appName,keyDic["guid"], keyDic["costCenterCode"],
                    //eyDic["cardno"], int.Parse(keyDic["barCodeSize"]));

                response.Content = new ByteArrayContent(barcode);

                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
                    
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }
    }
}
