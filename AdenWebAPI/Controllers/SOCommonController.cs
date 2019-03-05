using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Aden.Model.Net;
using Aden.Model.SOMastData;
using Aden.Model.Common;
using Aden.BLL.SOCommon;

namespace AdenWebAPI.Controllers
{
    public class SOCommonController : BaseController
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
        public Response GetConfigValue(string token, string type)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = ConfigHelper.GetConfigValue(type);
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
        /// 取得语言包最新版本号及其内容
        /// </summary>
        /// <param name="token"></param>
        /// <param name="strType">分类</param>
        /// <returns></returns>
        [HttpPost]
        public Response GetLangVersion(string token, [FromBody]SingleField param)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = LanguageHelper.GetLangVersion(param.key);
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
        /// 取得语言字典
        /// </summary>
        /// <param name="token"></param>
        /// <param name="strType">分类</param>
        /// <returns></returns>
        [HttpGet]
        public Response GetLangMast(string token, string type)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = LanguageHelper.GetLangMast(type);
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
        /// 取得语言字典
        /// </summary>
        /// <param name="token"></param>
        /// <param name="strType">分类</param>
        /// <returns></returns>
        [HttpPost]
        public Response SaveLangMast(string token, [FromBody]LangMastList lstLangMast)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int i = LanguageHelper.SaveLangMast(lstLangMast.lstLangMast);

                if (i == 0)
                {
                    response.code = "500";
                    response.message = "No data saved";
                }
                else
                {
                    response.code = "200";
                    response.content = i;
                }
            }

            return response;
        }

        /// <summary>
        /// 检查是否需要语言包
        /// </summary>
        /// <param name="token"></param>
        /// <param name="strType">分类</param>
        /// <returns></returns>
        [HttpPost]
        public Response CheckLangVersion(string token, [FromBody]SingleField param)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                bool res = LanguageHelper.CheckLangVersion(param.key, param.guid);

                response.content = res;
            }

            return response;
        }
    }
}