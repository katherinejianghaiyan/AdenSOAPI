using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Aden.Model.Net;
using Aden.Model.Purchase;
using Aden.Model.MenuOrder;
using Aden.BLL.Purchase;
using Aden.Model.SOMastData;
using Newtonsoft.Json;

namespace AdenWebAPI.Controllers
{
    public class PurchaseOrderController : BaseController
    {
        /// <summary>
        /// 保存采购订单
        /// </summary>
        /// <param name="token"></param>
        /// <param name="po"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SavePurchaseOrder(string token, [FromBody]PurchaseOrderHead po)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = PurchaseOrderHelper.SavePurchaseOrder(po);
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
        /// 读取采购订单
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetPurchaseOrderLine(string token, [FromBody]PurchaseOrderHead param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                List<PurchaseOrderLine> lstPOL = PurchaseOrderHelper.GetPurchaseOrderLine(param);
                if (lstPOL == null || lstPOL.Count == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = lstPOL;
                }
            }
            return response;
        }

        /// <summary>
        /// 根据成本中心、日期导入采购单临时表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response ImportToPurchaseOrder(string token, [FromBody]MenuOrderHead param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = PurchaseOrderHelper.ImportToPurchaseOrder(param);
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
    }
}