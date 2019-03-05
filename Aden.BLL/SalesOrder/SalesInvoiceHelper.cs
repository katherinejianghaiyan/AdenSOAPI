using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.SalesOrder;
using Aden.Model;
using Aden.Model.SOCommon;
using Aden.Model.SOMastData;
using Aden.BLL;

namespace Aden.BLL.SalesOrder
{
    public class SalesInvoiceHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static SalesInvoiceFactory factory = new SalesInvoiceFactory();

        /// <summary>
        /// 根据传入SO相关信息设置开票区间
        /// </summary>
        public static List<SalesPeriod> SetSalesPeriod(SalesPeriodParam spp)
        {
            try
            {
                if (spp == null)
                {
                    throw new Exception("SalesPeriodParam is null");
                }
                // 执行设置开票区间
                List<SalesPeriod> lstSalesPeriod = factory.SetSalesPeriod(spp);

                return lstSalesPeriod;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SetSalesPeriod");
                return null;
            }
        }

        /// <summary>
        /// 根据传入SO相关信息设置开票区间
        /// </summary>
        public static List<SalesInvoice> SetSalesInvoice(SalesInvoiceParam sip, ref int insertCount)
        {
            try
            {
                if (sip == null)
                {
                    throw new Exception("SalesInvoiceParam is null");
                }
                // 执行设置开票区间

                List<SalesInvoice> lstSalesInvoice = factory.SetSalesInvoice(sip, ref insertCount);

                return lstSalesInvoice;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SetSalesInvoice");
                return null;
            }
        }

        /// <summary>
        /// 根据Company取得开票区间
        /// </summary>
        public static List<SalesPeriod> SearchPeriod(string company, string sDate, string eDate)
        {
            try
            {
                if (company == null)
                {
                    throw new Exception("Company is null");
                }
                // 执行设置开票区间
                List<SalesPeriod> lstSalesPeriod = factory.SearchPeriod(company, sDate, eDate);

                return lstSalesPeriod;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SearchPeriod");
                return null;
            }
        }

        /// <summary>
        /// 更新开票区间表的状态
        /// </summary>
        public static int UpdatePeriodStatus(SalesPeriodUpdateParam spup)
        {
            int intExCount = 0;

            try
            {
                if (spup == null)
                {
                    throw new Exception("UpdatePeriodStatus is null");
                }
                // 执行设置开票区间
                intExCount = factory.UpdatePeriodStatus(spup);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "UpdatePeriodStatus");
            }
            return intExCount;
        }

        /// <summary>
        /// 根据Company取得开票区间
        /// </summary>
        public static int SaveSalesInvoice(SalesInvoiceSaveParam sisp)
        {
            int intExCount = 0;

            try
            {
                if (sisp == null)
                {
                    throw new Exception("SalesInvoiceSaveParam is null");
                }
                // 执行设置开票区间
                intExCount = factory.SaveSalesInvoice(sisp);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SaveSalesInvoice");
            }
            return intExCount;
        }

        /// <summary>
        /// 设置\清楚分组开票Guid
        /// </summary>
        /// <param name="sisp"></param>
        /// <returns></returns>
        public static int SplitInvoice(SalesInvoiceSaveParam sisp)
        {
            int intExCount = 0;

            try
            {
                if (sisp == null)
                {
                    throw new Exception("SalesInvoiceSaveParam is null");
                }
                // 执行设置开票区间
                intExCount = factory.SplitInvoice(sisp);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SplitInvoice");
            }
            return intExCount;
        }

        /// <summary>
        /// 导入ERP前将状态置为ing状态
        /// </summary>
        public static int ReadyToImportErp(ReadyToImportErp rtie)
        {
            int intExCount = 0;

            try
            {
                if (rtie == null)
                {
                    throw new Exception("ReadyToImportErp is null");
                }
                // 执行设置开票区间
                intExCount = factory.ReadyToImportErp(rtie);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "ReadyToImportErp");
            }
            return intExCount;
        }

        /// <summary>
        /// 删除开票区间 by AJ
        /// </summary>
        public static int CloseSalesInvoice(SalesInvoiceSaveParam sisp)
        {
            int intExCount = 0;

            try
            {
                if (sisp == null)
                {
                    throw new Exception("CloseInvoiceSaveParam is null");
                }
                // 执行设置开票区间
                intExCount = factory.CloseSalesInvoice(sisp);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CloseSalesInvoice");
            }
            return intExCount;
        }

        /// <summary>
        /// 回退 by Angel Jiang
        /// </summary>
        /// <param name="sisp"></param>
        /// <returns></returns>
        public static int DelSalesInvoice(SalesInvoiceSaveParam sisp)
        {
            int intExCount = 0;

            try
            {
                if (sisp == null)
                {
                    throw new Exception("CloseInvoiceSaveParam is null");
                }
                // 执行设置开票区间
                intExCount = factory.DelSalesInvoice(sisp);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CloseSalesInvoice");
            }
            return intExCount;
        }

        /// <summary>
        /// 根据传入的PeriodGuid取得SDK交易错误日志
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<TransLog> GetTransLog(SalesPeriod param)
        {
            try
            {
                if (param == null)
                {
                    throw new Exception("SalesPeriod is null");
                }
                // 执行设置开票区间

                List<TransLog> lstTransLog = factory.GetTransLog(param);

                return lstTransLog;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetTransLog");
                return null;
            }
        }

        /// <summary>
        /// 根据传入的ItemGuid取得生成SalesInvoice的最大EndDate
        /// </summary>
        /// <param name="itemGuid">ItemGuid</param>
        /// <returns></returns>
        public static string GetSalesInvoiceMaxDate(string itemGuid)
        {
            try
            {
                if (itemGuid == null)
                {
                    throw new Exception("itemGuid is null");
                }
                // 取得Max EndDate
                string strEndDate= factory.GetSalesInvoiceMaxDate(itemGuid);

                return strEndDate;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetSalesInvoiceMaxDate");
                return null;
            }
        }

        /// <summary>
        /// 根据传入的HeadGuid取得生成SalesPeriod的最大EndDate
        /// </summary>
        /// <param name="headGuid">headGuid</param>
        /// <returns></returns>
        public static string GetSalesPeriodMaxDate(string headGuid)
        {
            try
            {
                if (headGuid == null)
                {
                    throw new Exception("headGuid is null");
                }
                // 取得Max EndDate
                string strEndDate = factory.GetSalesPeriodMaxDate(headGuid);

                return strEndDate;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetSalesPeriodMaxDate");
                return null;
            }
        }
    }
}
