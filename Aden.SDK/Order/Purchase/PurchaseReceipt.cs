using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model;
using CSSDKConnect;
using CSSDKFuncProcess;

namespace Aden.SDK.Order.Purchase
{
    public static class PurchaseReceipt
    {
        ///// <summary>
        ///// 采购订单收货
        ///// </summary>
        ///// <param name="receiptOrders">收货的采购订单集合</param>
        ///// <param name="ip">数据库IP</param>
        ///// <param name="db">数据库名</param>
        ///// <param name="action">操作完成后执行的方法</param>
        //public static void Receipt(IList<Model.Order.Purchase.PurchaseReceipt> receiptOrders, string ip, string db, Action<Log> action = null)
        //{
        //    //创建SDK连接
        //    ConnectHelper conHelper = new ConnectHelper();

        //    //创建日志对象
        //    Log log = new Log();

        //    try
        //    {
        //        if (conHelper.Open(ip, db))
        //        {
        //            SDKPurchaseOrder sdkOrder = new SDKPurchaseOrder();
        //            try
        //            {
        //                if (!sdkOrder.Initialize(conHelper.connection))
        //                    log.message = GetLogMessage(sdkOrder.ErrorMessages, "initialize sdk failed.");
        //                else
        //                {
        //                    oReceipt sdkReceipt = sdkOrder.Receipts;
        //                    log.details = new List<LogDetail>();
        //                    foreach (var order in receiptOrders)
        //                    {
        //                        LogDetail detail = new LogDetail();
        //                        detail.key = order.headGuid;
        //                        if (!sdkReceipt.AddPurchaseOrder(order.orderNumber))
        //                            detail.message = GetLogMessage(sdkOrder.ErrorMessages, "add purchase order failed.");
        //                        else
        //                        {
        //                            sdkReceipt.ProcessDate = DateTime.Parse(order.receiptDate);
        //                            if (!sdkReceipt.Prepare(0))
        //                                detail.message = GetLogMessage(sdkOrder.ErrorMessages, "add purchase order failed.");
        //                            else
        //                            {
        //                                sdkReceipt.YourReference = order.orderNumber.ToString();
        //                                SetZero(sdkReceipt, "");
        //                                SetLines(sdkReceipt, order.receiptLines, "");
        //                                if (!sdkReceipt.Process())
        //                                    detail.message = GetLogMessage(sdkOrder.ErrorMessages, "process failed.");
        //                                else detail.code = "finished";
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                log.message = e.Message;
        //            }
        //            finally
        //            {
        //                try { sdkOrder.Terminate(); } catch { }
        //            }
        //        }
        //        else log.message = conHelper.errMsg;
        //    }
        //    catch(Exception ex)
        //    {
        //        log.message = ex.Message;
        //    }
        //    finally
        //    {
        //        conHelper.Close();
        //        action?.Invoke(log);
        //    }
        //}

        //private static string GetLogMessage(Array errorMessages, string message)
        //{
        //    if (errorMessages.Length > 0) return errorMessages.GetValue(0).ToString();
        //    return message;
        //}

        //private static void SetZero(oReceipt sdkReceipt, string locTo)
        //{
        //    for (int i = 1; i <= sdkReceipt.LineCount; i++)
        //        sdkReceipt.get_Lines(i, ref locTo).Quantity = 0;
        //}

        //private static void SetLines(oReceipt sdkReceipt, IList<Model.Order.Purchase.PurchaseReceiptLine> lines, string locTo)
        //{
        //    foreach(var line in lines)
        //    {
        //        for(int i = 1; i <= sdkReceipt.LineCount; i++)
        //        {
        //            oReceiptLine sdkLine = sdkReceipt.get_Lines(i, ref locTo);
        //            if (sdkLine.OrderLineID.Equals(line.lineNumber)
        //                && sdkLine.ItemCode.Trim().Equals(line.itemCode))
        //            {
        //                sdkLine.Quantity = line.quantity;
        //                sdkLine.UnitType = eUnitType.iPurchaseUnit;
        //                break;
        //            }
        //        }
        //    }
        //}
    }
}
