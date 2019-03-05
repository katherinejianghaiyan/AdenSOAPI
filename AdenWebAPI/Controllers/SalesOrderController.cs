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
    public class SalesOrderController : BaseController
    {
        /// <summary>
        /// 根据传入的合同订单对象建立SO
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="so">合同订单对象</param>
        /// <returns></returns>
        [HttpPost]
        public Response CreateSO(string token,[FromBody]Aden.Model.SOMastData.SalesOrder so)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                string headGuid = SalesOrderHelper.CreateSO(so);

                if (!string.IsNullOrWhiteSpace(headGuid))
                {
                    response.code = "200";
                    response.content = headGuid;
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
        /// 根据传入的合同订单对象修改SO
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="so">合同订单对象</param>
        /// <returns></returns>
        [HttpPost]
        public Response EditSO(string token, [FromBody]Aden.Model.SOMastData.SalesOrder so)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                bool status = SalesOrderHelper.EditSO(so);

                if (status)
                {
                    response.code = "200";
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
        /// 根据传入的合同订单对象建立SO
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="so">合同订单对象</param>
        /// <returns></returns>
        [HttpGet]
        public Response CheckContract(string token, string contract)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                List<Aden.Model.SOMastData.SalesOrder> lstSO = SalesOrderHelper.CheckContract(contract);
                response.code = "200";

                if (lstSO != null)
                {
                    response.content = false;
                }
                else
                {
                    response.content = true;
                }
            }
            return response;
        }

        /// <summary>
        /// 根据传入的Guid取得合同SO
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="guid">合同订单的Guid</param>
        /// <returns></returns>
        [HttpGet]
        public Response GetSO(string token, string guid)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                Aden.Model.SOMastData.SalesOrder so = SalesOrderHelper.GetSO(guid);
                response.code = "200";

                if (so != null)
                {
                    response.code = "200";
                    response.content = so;
                }
                else
                {
                    response.code = "500";
                }
            }
            return response;
        }

        /// <summary>
        /// 根据条件查询SO列表
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="so">合同订单对象</param>
        /// <returns></returns>
        [HttpGet]
        public Response QuerySO(string token)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                List<Aden.Model.SOMastData.SalesOrder> so = SalesOrderHelper.QuerySO();
                response.code = "200";

                if (so != null)
                {
                    response.code = "200";
                    response.content = so;
                }
                else
                {
                    response.code = "500";
                }
            }
            return response;
        }

        /// <summary>
        /// SODetail by Angel
        /// </summary>
        /// <param name="token"></param>
        /// <param name="headguid"></param>
        /// <returns></returns>
        [HttpGet]
        public Response SODetail(string token, string company)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                List<Aden.Model.SOMastData.SalesOrderItem> SODetail = SalesOrderHelper.SODetail(company);
                if (SODetail != null)
                {
                    response.code = "200";
                    response.content = SODetail;
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
        /// SOSearch by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        [HttpGet]
        public Response SearchSO(string token, string db, string sDate, string eDate)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                List<Aden.Model.SOMastData.SalesOrder> solist = SalesOrderHelper.SearchSO(db, sDate, eDate);

                if (solist != null)
                {
                    response.code = "200";
                    response.content = solist;
                }
                else
                {
                    response.code = "500";
                    response.message = "process failed";
                }
            }
            return response;
        }

        [HttpGet]
        public Response MenuAction(string token, string userGuid, string Company, string action)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                MenuAction menuAction = SalesOrderHelper.MenuAction(userGuid, Company, action);
                if (menuAction != null)
                {
                    response.code = "200";
                    response.content = menuAction;
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
        /// SOcc by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SOcc(string token, [FromBody]Aden.Model.SOMastData.SalesOrder so)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                List<Aden.Model.SOMastData.SalesOrderItem> SOcc = SalesOrderHelper.SOcc(so.companyCode, so.ownerCompanyCode, so.contract);
                if (SOcc != null)
                {
                    response.code = "200";
                    response.content = SOcc;
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
        /// SearchCostCenterMatch by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SearchCostCenterMatch(string token, [FromBody]Aden.Model.SOMastData.SalesOrderItem line)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                List<Aden.Model.SOMastData.SalesOrderItem> SearchCostCenterMatch = SalesOrderHelper.SearchCostCenterMatch(line);
                if (SearchCostCenterMatch != null)
                {
                    response.code = "200";
                    response.content = SearchCostCenterMatch;
                }
                else
                {
                    response.code = "500";
                    response.content = "process failed";
                }
            }
            return response;
        }


        /// <summary>
        /// 多公司成本中心比配 - by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [HttpPost]
        public Response CostCenterMatch(string token, [FromBody]Aden.Model.SOMastData.SalesOrderItem line)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                int intExCount = 0;
                intExCount = SalesOrderHelper.CostCenterMatch(line);

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
        /// dtCompany by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public Response dtCompany(string token)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid Token";
            }
            else
            {
                List<Aden.Model.SOMastData.Company> dtCompany = CompanyHelper.dtCompany();
                if (dtCompany != null)
                {
                    response.code = "200";
                    response.content = dtCompany;
                }
                else
                {
                    response.code = "500";
                }
            }
            return response;
        }
        /// <summary>
        /// updateCompany by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        [HttpPost]
        public Response updateCompany(string token, [FromBody]Aden.Model.Common.Company lines)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                int intExCount = 0;
                intExCount = CompanyHelper.updateCompany(lines);

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
        /// HD by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <param name="db"></param>
        /// <param name="invcompCode"></param>
        /// <param name="customCode"></param>
        /// <param name="contract"></param>
        /// <param name="PaymentCode"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="vDate"></param>
        /// <returns></returns>
        [HttpPost]
        public Response HD(string token, [FromBody]SalesOrder so)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                List<SalesOrder> HD = SalesOrderHelper.HD(so);
                if (HD != null)
                {
                    response.code = "200";
                    response.content = HD;
                }
                else
                {
                    response.code = "500";
                    response.content = "process failed";
                }
            }
            return response;
        }

        /// <summary>
        /// 检查服务公司的供应商代码在开票公司的供应商列表中是否存在
        /// </summary>
        /// <param name="companyInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public Response CheckSupplierCode(string token, [FromBody]Company companyInfo)
        {
            Response response = new Response();

            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid token";
            }
            else
            {
                try
                {
                    bool result = CompanyHelper.CheckSupplierCode(companyInfo);

                    response.code = "200";
                    response.content = result;
                }
                catch
                {
                    response.code = "500";
                    response.content = "process failed";
                }
            }
            return response;
        }

        /// <summary>
        /// dtCompany by Angel Jiang
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetActCostCenter(string token, [FromBody]Aden.Model.Common.SingleField param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invalid Token";
            }
            else
            {
                List<Aden.Model.Common.SingleField> lstCostCenter = CompanyHelper.GetActCostCenter(param);
                if (lstCostCenter != null && lstCostCenter.Count > 0)
                {
                    response.code = "200";
                    response.content = lstCostCenter;
                }
                else
                {
                    response.code = "500";
                }
            }
            return response;
        }

    }
}