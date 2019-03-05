using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections;
using System.Web;
using System.Web.Http;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using Aden.Model.Net;
using Aden.Model.SiteDIY;
using Aden.Model.SOMastData;
using Aden.Model.Upload;
using Aden.BLL.SiteDIY;
using Aden.Util.Common;
using System.Threading.Tasks;
using Aden.DAL.Models;
using System.Data;


namespace AdenWebAPI.Controllers
{
    public class SiteDIYController: BaseController
    {
        /// <summary>
        /// 读取公司和营运点列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetCompanySite(string token, [FromBody]AuthCompany auth)
        {
            Response response = new Response();
            if (string.IsNullOrWhiteSpace(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid Token";
            }
            else
            {
                var data = SiteDIYHelper.GetCompanySite(auth);
                if (data == null || !data.Any())
                {
                    response.code = "500";
                    response.message = "Get data Failed";
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
        /// 获取营运点的海报图片
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetSitePosts(string token, [FromBody]Site param)
        {
            Response response = new Response();
            if (string.IsNullOrWhiteSpace(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = SiteDIYHelper.GetSitePosts(param.dbName,param.siteGuid);
                if (data == null || !data.Any())
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
        /// 获取公司-营运点价格列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetPriceList(string token, [FromBody]string d)
        {
            Dictionary<string, dynamic> param = d.JSDeserialize<Dictionary<string, dynamic>>();
            Response response = new Response();
            if (string.IsNullOrWhiteSpace(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                var data = SiteDIYHelper.GetPriceList(param);
                if (data == null || !data.Any())
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
        /// 跟新价目表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        [HttpPost]
        public Response UpdateFGItemPrice(string token, [FromBody]string d)
        {
            Dictionary<string, dynamic> param = d.JSDeserialize<Dictionary<string, dynamic>>();
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = SiteDIYHelper.UpdateFGItemPrice(param);
                if (result == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = result;
                }
            }
            return response;
        }

        /// <summary>
        /// 获取问卷调研反馈
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SiteSurvey(string token,[FromBody] string d)
        {
            Dictionary<string, dynamic> param = d.JSDeserialize<Dictionary<string, dynamic>>();
            Response response = new Response();
            if(string.IsNullOrWhiteSpace(token)|| !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid Token";
            }
            else
            {
                var data = SiteDIYHelper.SiteSurveySummary(param);
                if (data == null || !data.Any())
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
        /// 上传海报图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<UploadResponse> UploadSitePicture() 
        {
            UploadResponse resp = new UploadResponse();
            try
            {
                
                if (!Request.Content.IsMimeMultipartContent("form-data"))
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                string filePath = ConfigurationManager.AppSettings["PictureRoot"].ToString();
                

                var urlString = Request.RequestUri.OriginalString;

                if (string.IsNullOrWhiteSpace(urlString))
                {
                    resp.Status = "error";
                    return resp;
                }

               
                int pictureFileIndex = urlString.IndexOf("pictureFile=");

                string pictureFile = urlString.Substring(pictureFileIndex + 12).Replace("/","\\");   
               

                if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(pictureFile))
                    throw new Exception("Image path is null");

                filePath = System.IO.Path.Combine(filePath, pictureFile);

                if (!System.IO.Directory.Exists(filePath)) throw new Exception("Path not found in server.");

                ReadMultipartFormDataStreamProvider
                    provider = new ReadMultipartFormDataStreamProvider(filePath, true);

                await Request.Content.ReadAsMultipartAsync(provider);

                if (provider.FileData.Count>1) throw new HttpResponseException(HttpStatusCode.Forbidden);
                
                //目前一个个提交处理   
                if(provider.FileData.Count>0)
                    resp.FileName = System.IO.Path.GetFileName(provider.FileData[0].LocalFileName);

                int Count = SiteDIYHelper.UploadSitePicture(filePath, provider.FormData["picVal1"], 
                    provider.GetFormDataFileNames()==null? null:provider.GetFormDataFileNames().First(), 
                    provider.FormData["siteGuid"],provider.FormData["id"], provider.FormData["businessType"], 
                    provider.FormData["startDate"], provider.FormData["check"].ToBool(), provider.FormData["imageType"]);

                if (Count <= 0)
                    resp.Status = "error";
                else if(Count>0)
                    resp.Status = "ok";

                return resp;
            }
            catch (Exception e)
            {
                resp.Status = "error";
                resp.Msg = e.Message;
                return resp;
            }
        }


    }
}