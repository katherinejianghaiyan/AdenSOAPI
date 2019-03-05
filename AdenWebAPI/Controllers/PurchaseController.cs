using Aden.BLL.Order;
using Aden.Model.Net;
using Aden.Model.Order.Purchase;
using System.Web.Http;

namespace AdenWebAPI.Controllers
{
    public class PurchaseController : BaseController
    {
        [HttpGet]
        public Response WeeklyOrderDateRange(string token, string user)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = PurchaseHelper.GetOrderDateRange("WeeklyPO", user);
                if (data == null)
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

        [HttpGet]
        public Response DailyOrderDateRange(string token, string user)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = PurchaseHelper.GetOrderDateRange("DailyPO", user);
                if (data == null)
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

        [HttpPost]
        public Response OrderData(string token, [FromBody] PurchaseRequest request)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = PurchaseHelper.GetPurchaseOrderData(request);
                if (data == null)
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

        [HttpPost]
        public Response WeeklySupplier(string token, [FromBody] PurchaseRequest request)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = PurchaseHelper.GetSupplierList(request.companyGuid, request.langCode, request.orderDate, request.warehouseCode, request.costCenterCode, "pricelist");
                if (data == null)
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

        [HttpPost]
        public Response DailySupplier(string token, [FromBody] PurchaseRequest request)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = PurchaseHelper.GetSupplierList(request.companyGuid, request.langCode, request.orderDate, request.warehouseCode, request.costCenterCode, "order");
                if (data == null)
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

        [HttpPost]
        public Response Item(string token, [FromBody] PurchaseRequest request)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = PurchaseHelper.GetItemListFromPriceList(request.companyGuid, request.orderDate, request.warehouseCode, request.langCode, request.supplierCode);

                if (data == null)
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

        [HttpPost]
        public Response SaveOrder(string token, [FromBody] PurchaseOrderData orderData)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                bool status = PurchaseHelper.SavePurchaseOrder(orderData);
                if (status) response.code = "200";
                else
                {
                    response.code = "500";
                    response.message = "process failed";
                }
            }
            return response;
        }

        [HttpPost]
        public Response ReceiptSupplierDate(string token, [FromBody] PurchaseRequest request)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = PurchaseHelper.GetReceiptSupplierDate(request);
                if (data == null)
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

        [HttpPost]
        public Response ReceiptData(string token, [FromBody] PurchaseRequest request)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = PurchaseHelper.GetPurchaseReceiptData(request);
                if (data == null)
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

        [HttpPost]
        public Response SaveReceipt(string token, [FromBody] PurchaseReceiptData receiptData)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                bool status = PurchaseHelper.SavePurchaseReceipt(receiptData);
                if (status) response.code = "200";
                else
                {
                    response.code = "500";
                    response.message = "process failed";
                }
            }
            return response;
        }

        [HttpPost]
        public Response CountsDate(string token, [FromBody] PurchaseRequest request)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = WarehouseHelper.GetCountsDate(request);
                if (data == null)
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

        [HttpPost]
        public Response CountsData(string token, [FromBody] PurchaseRequest request)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                var data = WarehouseHelper.GetCountsData(request);
                if (data == null)
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
    }
}
