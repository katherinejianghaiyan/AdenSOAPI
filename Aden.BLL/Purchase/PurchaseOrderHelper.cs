using System;
using System.Collections.Generic;
using System.Linq;
using Aden.Util.Common;
using Aden.DAL.Purchase;
using Aden.Model;
using Aden.Model.Purchase;
using Aden.Model.MenuOrder;
using Aden.Model.MastData;
using Aden.Model.Common;

namespace Aden.BLL.Purchase
{
    public class PurchaseOrderHelper
    {
        private readonly static PurchaseOrderFactory factory = new PurchaseOrderFactory();

        /// <summary>
        /// 保存采购订单
        /// </summary>
        /// <param name="param">Purchase Order</param>
        /// <returns></returns>
        public static int SavePurchaseOrder(PurchaseOrderHead po)
        {
            try
            {
                if (po == null) throw new Exception("PurchaseOrder is null");
                int result = factory.SavePurchaseOrder(po);
                if (result == 0) throw new Exception("DAL.Purchase.PurchaseOrderFactory.SavePurchaseOrder()==0");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SavePurchaseOrder");
                return 0;
            }
        }

        /// <summary>
        /// 读取采购订单
        /// </summary>
        /// <param name="param">Purchase Order</param>
        /// <returns></returns>
        public static List<PurchaseOrderLine> GetPurchaseOrderLine(PurchaseOrderHead param)
        {
            try
            {
                if (param == null) throw new Exception("param is null");
                List<PurchaseOrderLine> lstPOL = factory.GetPurchaseOrderLine(param);
                if (lstPOL == null) throw new Exception("DAL.Purchase.PurchaseOrderFactory.GetPurchaseOrderLine()==null");
                return lstPOL;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetPurchaseOrderLine");
                return null;
            }
        }

        /// <summary>
        /// 根据成本中心、日期导入采购单临时表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int ImportToPurchaseOrder(MenuOrderHead param)
        {
            try
            {
                if (param == null) throw new Exception("param is null");
                int result = factory.ImportToPurchaseOrder(param);
                if (result == 0) throw new Exception("DAL.Purchase.PurchaseOrderFactory.ImportToPurchaseOrder()==null");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "ImportToPurchaseOrder");
                return 0;
            }
        }
    }
}