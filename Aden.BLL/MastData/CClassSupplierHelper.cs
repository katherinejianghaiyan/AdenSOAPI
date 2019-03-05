using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.MastData;
using Aden.Model;
using Aden.Model.MastData;
using Aden.Model.MenuOrder;
using Aden.Model.Common;

namespace Aden.BLL.MastData
{
    public class CClassSupplierHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static CClassSupplierFactory cClassSupplierFactory = new CClassSupplierFactory();

        /// <summary>
        /// 根据Company取得CostCenterCode对应关系集合
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static CClassSupplierParam GetCClassSupplierData(string companyCode)
        {
            try
            {
                CClassSupplierParam ccsp = cClassSupplierFactory.GetCClassSupplierData(companyCode);
                if (ccsp.lstCClassSupplier.Count == 0) throw new Exception("DAL.MastData.cClassSupplierFactory.GetCClassSupplierData()==null");
                return ccsp;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CClassSupplierParam");
                return null;
            }
        }

        /// <summary>
        /// 保存成本中心对应供应商主数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int SaveCClassSupplier(CClassSupplierParam param)
        {
            try
            {
                int result = cClassSupplierFactory.SaveCClassSupplier(param);
                if (result == 0) throw new Exception("DAL.MastData.cClassSupplierFactory.SaveCClassSupplierFactory()==0");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "result");
                return 0;
            }
        }

        /// <summary>
        /// 取得ItemClass维护界面的集合
        /// </summary>
        /// <param name="param">公司代码</param>
        /// <returns></returns>
        public static ItemClassMaintain GetItemClassMaintainData(SingleField param) 
        {
            try
            {
                ItemClassMaintain result = cClassSupplierFactory.GetItemClassMaintainData(param);
                if ((result.lstItemClass_x == null || result.lstItemClass_x.Count == 0) ||
                    (result.lstItemClass_y == null || result.lstItemClass_y.Count == 0))
                    throw new Exception("DAL.MastData.cClassSupplierFactory.GetItemClassMantainData()==null");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetItemClassMantainData");
                return null;
            }
        }

        /// <summary>
        /// 保存修改的ItemClassMapping数据
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int SaveItemClassMaintainData(SaveItemClassMaintainDataParam param)
        {
            try
            {
                int result = cClassSupplierFactory.SaveItemClassMaintainData(param);
                if (result == 0) throw new Exception("DAL.MastData.cClassSupplierFactory.SaveItemClassMaintainData()==0");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "result");
                return 0;
            }
        }
        
    }
}
