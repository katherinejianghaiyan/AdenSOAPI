
using System.Web.Http;
using Aden.BLL.MastData;
using Aden.Model.Net;
using System.Collections.Generic;
using Aden.Model.MastData;
using Aden.Model.MenuOrder;
using Aden.Model.Common;

namespace AdenWebAPI.Controllers
{
    public class MastDataController : BaseController
    {
        /// <summary>
        /// 根据Company取得CostCenterCode对应关系集合
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetCClassSupplierData(string token, [FromBody]SingleField param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                CClassSupplierParam ccsp = CClassSupplierHelper.GetCClassSupplierData(param.code);

                if (ccsp.lstCClassSupplier == null || ccsp.lstCClassSupplier.Count == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = ccsp;
                }
            }
            return response;
        }

        /// <summary>
        /// 保存成本中心对应供应商主数据
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SaveCClassSupplier(string token, [FromBody]CClassSupplierParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = CClassSupplierHelper.SaveCClassSupplier(param);
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
        /// 取得ItemClass维护界面的集合
        /// </summary>
        /// <param name="param">公司代码</param>
        /// <returns></returns>
        [HttpPost]
        public Response GetItemClassMaintainData(string token, [FromBody]SingleField param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                ItemClassMaintain result = CClassSupplierHelper.GetItemClassMaintainData(param);

                if ((result.lstItemClass_x == null || result.lstItemClass_x.Count == 0) ||
                    (result.lstItemClass_y == null || result.lstItemClass_y.Count == 0))
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
        /// 保存修改的ItemClassMapping数据
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response SaveItemClassMaintainData(string token, [FromBody]SaveItemClassMaintainDataParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                int result = CClassSupplierHelper.SaveItemClassMaintainData(param);
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
        /// 取得至下个工作日的所有日期 + weekday中文
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public Response GetToNextWorkingDay(string token, [FromBody]GetToNextWorkingDayParam param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                List<SingleField> lstResult = CalendarHelper.GetToNextWorkingDay(param);
                if (lstResult.Count == 0)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = lstResult;
                }
            }
            return response;
        }

        /// <returns></returns>
        [HttpPost]
        public Response GetCCMastInfo(string token, [FromBody]SingleField param)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(token) || !token.Equals(_token))
            {
                response.code = "404";
                response.message = "Invild token";
            }
            else
            {
                CCMast ccm = CCMastHelper.GetCCMastInfo(param);
                if (ccm == null)
                {
                    response.code = "500";
                    response.message = "No Data";
                }
                else
                {
                    response.code = "200";
                    response.content = ccm;
                }
            }
            return response;
        }
    }
}