using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.SalesOrder;
using Aden.Model;
using Aden.Model.SOCommon;
using Aden.Model.SOMastData;

namespace Aden.BLL.SalesOrder
{
    public class SalesOrderHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static SalesOrderFactory factory = new SalesOrderFactory();

        /// <summary>
        /// 根据传入Sales Order对象建立SO
        /// </summary>
        public static string CreateSO(Model.SOMastData.SalesOrder so)
        {
            try
            {
                if (so == null)
                {
                    throw new Exception("SalesOrder is null");
                }
                // Now Time
                //string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                // 设置SO建立时间
                so.createDate = DateTime.Now.ToString();

                // 执行建立SO
                return factory.CreateSO(so);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CreateSO");
                return null;
            }
        }

        /// <summary>
        /// 根据传入Sales Order对象修改SO
        /// </summary>
        public static bool EditSO(Model.SOMastData.SalesOrder so)
        {
            try
            {
                if (so == null)
                {
                    throw new Exception("SalesOrder is null");
                }

                // 执行建立SO
                string retSql = String.Empty;

                factory.EditSO(so, ref retSql);

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "EditSO");
                return false;
            }
        }

        /// <summary>
        /// 根据传入Guid取得对应的SO信息
        /// </summary>
        public static Model.SOMastData.SalesOrder GetSO(string guid)
        {
            try
            {
                if (guid == null)
                {
                    throw new Exception("Guid is null");
                }

                // 执行建立SO
                Model.SOMastData.SalesOrder so = factory.GetSO(guid);

                return so;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetSO");
                return null;
            }
        }

        /// <summary>
        /// 根据查询条件取得SO列表
        /// </summary>
        public static List<Model.SOMastData.SalesOrder> QuerySO()
        {
            try
            {
                // 执行建立SO
                List<Model.SOMastData.SalesOrder> lstSO = factory.QuerySO();

                return lstSO;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "QuerySO");
                return null;
            }
        }

        /// <summary>
        /// 合同号存在性检查
        /// </summary>
        public static List<Aden.Model.SOMastData.SalesOrder> CheckContract(string contract)
        {
            try
            {
                if (contract == null)
                {
                    throw new Exception("contract is null");
                }
                // 执行check方法
                List<Aden.Model.SOMastData.SalesOrder> lstSO = factory.CheckContract(contract);

                return lstSO;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CheckContract");
                return null;
            }
        }

        /// <summary>
        /// SODetail by Angel
        /// </summary>
        /// <param name="headguid"></param>
        /// <returns></returns>
        public static List<SalesOrderItem> SODetail(string company)
        {
            try
            {
                if (company == null)
                {
                    throw new Exception("headguid is null");
                }
                List<SalesOrderItem> SODetail = factory.SODetail(company);
                return SODetail;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SODetail");
                return null;
            }
        }
        

        /// <summary>
        /// SOSearch by Angel Jiang
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static List<Model.SOMastData.SalesOrder> SearchSO(string db, string sDate, string eDate)
        {
            try
            {
                if (db == null)
                {
                    throw new Exception("DB is null");
                }
                List<Model.SOMastData.SalesOrder> SOList = factory.SearchSO(db, sDate, eDate);
                return SOList;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SearchSO");
                return null;
            }
        }
        /// <summary>
        /// MenuAction by Angel Jiang
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="Company"></param>
        /// <param name="guid"></param>
        /// <param name="pguid"></param>
        /// <returns></returns>
        public static MenuAction MenuAction(string userGuid, string Company, string action)
        {
            try
            {
                if (userGuid == null || Company == null || action == null)
                {
                    throw new Exception("Action is null");
                }
                MenuAction menuAction = factory.MenuAction(userGuid, Company, action);
                return menuAction;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "MenuAction");
                return null;
            }
        }

        /// <summary>
        /// SOcc by Angel Jiang
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public static List<SalesOrderItem> SOcc(string companyCode, string ownerCompanyCode, string contract)
        {
            try
            {
                if (companyCode == null && ownerCompanyCode == null)
                {
                    throw new Exception("company is null");
                }
                List<SalesOrderItem> SOcc = factory.SOcc(companyCode, ownerCompanyCode, contract);
                return SOcc;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SOcc");
                return null;
            }
        }

        /// <summary>
        /// SearchCostCenterMatch by Angel Jiang
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static List<Model.SOMastData.SalesOrderItem> SearchCostCenterMatch(SalesOrderItem line)
        {
            try
            {
                if (line == null)
                {
                    throw new Exception("CostCenterMatch is null");
                }
                List<SalesOrderItem> SearchCostCenterMatch = factory.SearchCostCenterMatch(line);
                return SearchCostCenterMatch;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "SearchCostCenterMatch");
                return null;
            }
        }

        /// <summary>
        /// 多公司成本中心匹配 - by Angel Jiang
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static int CostCenterMatch(SalesOrderItem line)
        {
            int intExCount = 0;
            try
            {
                if (line == null)
                {
                    throw new Exception("CostCenterMatch is null");
                }
                intExCount = factory.CostCenterMatch(line);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CostCenterMatch");
            }
            return intExCount;
        }


        /// <summary>
        /// HD by Angel Jiang
        /// </summary>
        /// <param name="db"></param>
        /// <param name="invcompCode"></param>
        /// <param name="customCode"></param>
        /// <param name="contract"></param>
        /// <param name="PaymentCode"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="vDate"></param>
        /// <returns></returns>
        public static List<Model.SOMastData.SalesOrder> HD(Aden.Model.SOMastData.SalesOrder so)
        {
            try
            {
                if (so == null)
                {
                    throw new Exception("parameter contains null");
                }
                else
                {
                    List<Model.SOMastData.SalesOrder> HD = factory.HD(so);
                    return HD;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "HD");
                return null;
            }
        }

    }
}
