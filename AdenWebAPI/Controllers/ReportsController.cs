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
    public class ReportsController : BaseController
    {
        [HttpGet]
        public void ShowPDF(string token, string ReportName, string data)
        {
            try
            {
                Aden.BLL.Reports.ReportHelper.ShowPDF(ReportName, data);

            }
            catch (Exception e)
            {
                System.Web.HttpContext.Current.Response.Write(e.Message);
                System.Web.HttpContext.Current.Response.End();
            }
        }

        [HttpGet]
        public Response SendExcel(string token, string data)
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
                    Aden.BLL.Reports.ReportHelper.ShowExcel(data);
                    response.code = "200";
                    response.message = "邮件发送成功";
                }
                catch(Exception e)
                {
                    response.code = "500";
                    response.message = e.Message;
                }
            }
            return response;
        }

        [HttpGet]
        public Response ExcelReport(string token, string data)
        {
            Response response = new Response();
            try
            {
                Aden.BLL.Reports.ReportHelper.ShowExcel(data);

                Dictionary<string, string> keyDic =
                    Aden.Util.Common.JsonHelper.JSDeserialize<Dictionary<string, string>>(data);

                if(keyDic["reportType"]== "mailweeklyMenu")
                {
                    response.message = "邮件发送成功";
                }
                
                return response;   
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return response;
            }
        }

    }
}
