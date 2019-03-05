using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Aden.Model.Net;
using Aden.Model.SOCommon;
using Aden.Model.SOMastData;
using Aden.BLL.SalesOrder;

namespace AdenWebAPI.Controllers
{
    public class SalesInvoiceController : BaseController
    {
        /// <summary>
        /// 根据传入的SO相关参数设置开票区间
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="spp">HeadGuid + 结束日期 + UserGuid</param>
        /// <returns></returns>
        [HttpPost]
        public Response SetSalesPeriod(string token,[FromBody]SalesPeriodParam spp)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = SalesInvoiceHelper.SetSalesPeriod(spp);

                if (data == null)
                {
                    response.code = "500";
                    response.message = "process failed";
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
        public Response SetSalesInvoice(string token, [FromBody]SalesInvoiceParam sip)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int insertCount = 0;
                var data = SalesInvoiceHelper.SetSalesInvoice(sip, ref insertCount);

                if (data == null)
                {
                    response.code = "500";
                    response.message = "process failed";
                }
                else
                {
                    response.code = "200";
                    response.content = data;
                    response.message = insertCount > 0 ? "new record created" : "no record created"; 
                }
            }
            return response;
        }

        /// <summary>
        /// 根据传入的合同订单对象建立SO
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="so">合同订单对象</param>
        /// <returns></returns>
        [HttpGet]
        public Response SearchPeriod(string token, string company, string sDate, string eDate)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                List<SalesPeriod> lstSalesPeriod = SalesInvoiceHelper.SearchPeriod(company, sDate, eDate);
                response.code = "200";

                if (lstSalesPeriod != null)
                {
                    response.code = "200";
                    response.content = lstSalesPeriod;
                }
                else
                {
                    response.code = "500";
                    response.message = "process failed";
                }
            }
            return response;
        }

        /// <summary>
        /// 更新开票区间表的状态
        /// </summary>
        /// <param name="token"></param>
        /// <param name="sisp"></param>
        /// <returns></returns>
        [HttpPost]
        public Response UpdatePeriodStatus(string token, [FromBody]SalesPeriodUpdateParam spup)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int intExCount = 0;
                intExCount = SalesInvoiceHelper.UpdatePeriodStatus(spup);

                if (intExCount == 0)
                {
                    response.code = "500";
                    response.message = "process failed";
                }
                else
                {
                    response.code = "200";
                }
            }
            return response;
        }

        /// <summary>
        /// 保存/修改开票行
        /// </summary>
        /// <param name="token"></param>
        /// <param name="sisp"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SaveSalesInvoice(string token, [FromBody]SalesInvoiceSaveParam sisp)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int intExCount = 0;
                intExCount = SalesInvoiceHelper.SaveSalesInvoice(sisp);

                if (intExCount == 0)
                {
                    response.code = "500";
                    response.message = "process failed";
                }
                else
                {
                    response.code = "200";
                }
            }
            return response;
        }

        /// <summary>
        /// 设置\清楚分组开票Guid
        /// </summary>
        /// <param name="sisp"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SplitInvoice(string token, [FromBody]SalesInvoiceSaveParam sisp)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int intExCount = 0;
                intExCount = SalesInvoiceHelper.SplitInvoice(sisp);

                if (intExCount == 0)
                {
                    response.code = "500";
                    response.message = "process failed";
                }
                else
                {
                    response.code = "200";
                }
            }
            return response;
        }

        /// <summary>
        ///  导入ERP前将状态置为ing状态
        /// </summary>
        /// <param name="token"></param>
        /// <param name="sisp"></param>
        /// <returns></returns>
        [HttpPost]
        public Response ReadyToImportErp(string token, [FromBody]ReadyToImportErp rtie)
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
                    int intExCount = 0;
                    intExCount = SalesInvoiceHelper.ReadyToImportErp(rtie);

                    response.code = "200";
                    response.content = intExCount;
                }
                catch(Exception e)
                {
                    response.code = "500";
                    response.message = e.Message;
                }
            }
            return response;
        }

        /// <summary>
        /// 保存/修改开票行
        /// </summary>
        /// <param name="token"></param>
        /// <param name="sisp"></param>
        /// <returns></returns>
        [HttpPost]
        public Response CloseSalesInvoice(string token, [FromBody]SalesInvoiceSaveParam sisp)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int intExCount = 0;
                intExCount = SalesInvoiceHelper.CloseSalesInvoice(sisp);

                if (intExCount == 0)
                {
                    response.code = "500";
                    response.message = "process failed";
                }
                else
                {
                    response.code = "200";
                }
            }
            return response;
        }

        /// <summary>
        /// 回退 by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <param name="sisp"></param>
        /// <returns></returns>
        [HttpPost]
        public Response DelSalesInvoice(string token, [FromBody]SalesInvoiceSaveParam sisp)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int intExCount = 0;
                intExCount = SalesInvoiceHelper.DelSalesInvoice(sisp);

                if (intExCount == 0)
                {
                    response.code = "500";
                    response.message = "process failed";
                }
                else
                {
                    response.code = "200";
                }
            }
            return response;
        }

        /// <summary>
        /// 根据传入的PeriodGuid取得SDK交易错误日志
        /// </summary>
        /// <param name="token"></param>
        /// <param name="sisp"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetTransLog(string token, [FromBody]SalesPeriod param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = SalesInvoiceHelper.GetTransLog(param);

                if (data == null)
                {
                    response.code = "500";
                    response.message = "process failed";
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
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="itemGuid"></param>
        /// <returns></returns>
        [HttpGet]
        public Response GetSalesInvoiceMaxDate(string token, string itemGuid)
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
                    var data = SalesInvoiceHelper.GetSalesInvoiceMaxDate(itemGuid);
                    response.code = "200";
                    response.content = data;
                }
                catch(Exception e)
                {
                    response.code = "500";
                    response.message = e.Message;
                }
            }
            return response;
        }

        /// <summary>
        /// 根据传入的HeadGuid取得生成SalesPeriod的最大EndDate
        /// </summary>
        /// <param name="token"></param>
        /// <param name="headGuid"></param>
        /// <returns></returns>
        [HttpGet]
        public Response GetSalesPeriodMaxDate(string token, string headGuid)
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
                    var data = SalesInvoiceHelper.GetSalesPeriodMaxDate(headGuid);
                    response.code = "200";
                    response.content = data;
                }
                catch (Exception e)
                {
                    response.code = "500";
                    response.message = e.Message;
                }
            }
            return response;
        }
    }
}