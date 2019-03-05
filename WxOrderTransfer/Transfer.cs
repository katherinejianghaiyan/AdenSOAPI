using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model.WeChat;
using Aden.Util.Database;
using Aden.Model.Common;
using Aden.Model.MastData;
using Aden.DAL.MastData;
using Aden.Util.Common;
using System.Data;
using System.Data.SqlClient;

namespace WxOrderTransfer
{
    public class Transfer
    {

        /// <summary>
        /// 入口
        /// </summary>
        /// <returns></returns>
        public static string DoTransfer(string transferGuid)
        {
            string sresult = "";
            try
            {
                
                // 取得需要导入的基本数据
                List<WxOrder> lstOrder = GetTransferData(transferGuid);

                if (lstOrder == null || !lstOrder.Any())
                    throw new Exception("没有需要导入的记录");
            
                /***按成本中心分组***/
                var lqOrder = (from order in lstOrder
                               group order by new
                               {
                                   costCenterCode = order.costCenterCode
                               }).ToList();

                #region 取得导入Sql文集合 & 成本中心主数据集合
                List<SingleField>  lstCC = lqOrder.Select(q => new SingleField { code = q.Key.costCenterCode }).ToList();

                CCMastFactory ccm = new CCMastFactory();
                // Sql文集合
                List<SqlMast> lstSqlTransfer = ccm.GetSqlMastInfo(lstCC, "InsertPOSRecharge");
                // 成本中心主数据集合
                List<CCMast>  lstCCMast = ccm.GetCCMastInfo(lstCC);
                #endregion

                foreach (var gOrder in lqOrder)
                {
                    sresult += string.Format("成本中心：{0}\r\n",gOrder.Key.costCenterCode);
                    // Sql文
                    SqlMast sqlObj = lstSqlTransfer.FirstOrDefault(r =>string.Format(",{0},",r.costCenterCodes)
                        .Contains(string.Format(",{0},", gOrder.Key.costCenterCode)));
                    // Sql连接相关信息
                    CCMast ccObj = lstCCMast.FirstOrDefault(r => gOrder.Key.costCenterCode.Equals(r.costCenterCode));
                    // Data Source={0};Initial Catalog={1};User ID={2};Password={3};Persist Security Info=True;Connection Timeout=10" providerName="System.Data.SqlClient" />
                    string strConn = string.Format(SqlServerHelper.customerAllConn(), ccObj.posIp, ccObj.posDBName, ccObj.posDBUserName, ccObj.posDBPassword);
                    // 初始化最终Sql文
                    StringBuilder sbSqlFinal = new StringBuilder();

                    /***参数设置***/
                    string[] sArray = sqlObj.sqlParams.Split(',');
                    //int x = 0;
                    foreach (WxOrder wxo in gOrder.ToList())
                    {
                        sbSqlFinal.AppendFormat(sqlObj.sqlCommand, wxo.GetProperties(sArray));
                        sbSqlFinal.AppendLine(";");
                    }

                    if (string.IsNullOrWhiteSpace(sbSqlFinal.ToString())) continue;

                    //new System.Threading.Thread(() =>
                    //{
                        int rst = 0;
                        if (string.IsNullOrWhiteSpace(ccObj.posDBUserName))
                            rst = NetTcpHelper.Execute(ccObj.posIp, ccObj.posDBName, sbSqlFinal.ToString());
                        else
                            rst = SqlServerHelper.Execute(strConn, sbSqlFinal.ToString());
                        //if (rst == 0) return ;

                        Console.WriteLine("成本中心：" + gOrder.Key.costCenterCode + " 记录数：" + rst.ToString() + "条导入成功");
                    sresult +=" 记录数：" + rst.ToString() + "条导入成功\r\n";
                        rst = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), GetUpdateSql(transferGuid,gOrder.Key.costCenterCode));
                        Console.WriteLine("成本中心：" + gOrder.Key.costCenterCode + " 记录数：" + rst.ToString() + "条回写成功");
                        Console.WriteLine("");
                    sresult += " 记录数：" + rst.ToString() + "条回写成功\r\n\r\n";

                    //}).Start();
                }
                return sresult;
            }
            catch(Exception e)
            {
                //throw e;
                sresult += e.Message;
            }
            finally {
                /***删除数据导入标志位***/
                SetUpdateGuid(transferGuid,false);
            }
            return sresult;
            //return result;
        }

        /// <summary>
        /// 设置/删除更新标记位
        /// </summary>
        /// <param name="flag">true:设置 | false:删除</param>
        /// <returns></returns>
        private static void SetUpdateGuid(string guid, bool flag)
        {

            //string strGuid = string.Empty;
            //if (flag)
            //    strGuid = "'" + Guid.NewGuid().ToString() + "'";
            //else
            //    strGuid = "NULL";
            try
            {
                if (string.IsNullOrWhiteSpace(guid)) throw new Exception("No transfer GUID");

                string strSqlUpdate = " UPDATE WXORDER " +
                                         " SET TRANSFERGUID = {0} " +
                                       " WHERE NOTIFYTIME IS NOT NULL " +
                                         " AND TRANSFERTIME IS NULL " +
                                         " AND CARDID IS NOT NULL " +
                                         " AND APPNAME = 'Recharge' " +
                                         " AND TYPE = 'Recharge' " +
                                         " AND isnull(TRANSFERGUID,'')='{1}'";
                                         //+ " and id = '1261'";
                strSqlUpdate = string.Format(strSqlUpdate, 
                    flag ? string.Format("'{0}'", guid) : "null",
                    flag ? "" :  guid);

                SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), strSqlUpdate);
            }
            catch(Exception e)
            {
                throw e;
            }
            //return strGuid;
        }

        /// <summary>
        /// 取得要导入的基本数据
        /// </summary>
        /// <returns></returns>
        private static List<WxOrder> GetTransferData(string guid)
        {
            try
            {
                /***设置导入数据标志位***/
                SetUpdateGuid(guid,true);

                List<WxOrder> lstTransferData = new List<WxOrder>();

                string strSql = " SELECT O.APPNAME " +
                                     " , O.TYPE " +
                                     " , O.CARDID " +
                                     " , O.OUT_TRADE_NO " +
                                     " , O.TRANSACTION_ID " +
                                     " , O.OPENID " +
                                     " , O.ATTACH " +
                                     " , CAST((O.TOTAL_FEE * 1.0 / 100) AS DEC(10,2)) AS TOTAL_FEE " +
                                     " , O.CASH_FEE " +
                                     " , O.FEE_TYPE " +
                                     " , O.BANK_TYPE " +
                                     " , O.TIME_END " +
                                     " , O.TIME_EXPIRE " +
                                     " , CONVERT(VARCHAR(100), O.CREATETIME, 120) AS CREATETIME " +
                                     " , M.COSTCENTERCODE " +
                                  " FROM WXORDER AS O " +
                                 " INNER JOIN WXUSERMAST AS M " +
                                    " ON O.OPENID = M.WECHATID " +
                                   " AND M.DELETETIME IS NULL " +
                                 " WHERE TRANSFERGUID='{0}'";
                //"O.NOTIFYTIME IS NOT NULL " +
                //  " AND O.TRANSFERTIME IS NULL " +
                //  " AND O.CARDID IS NOT NULL " +
                //  " AND O.APPNAME = 'Charge' " +
                //  " and o.id in (37, 38) " +
                //  " AND O.TYPE = 'Charge' ";
                strSql = string.Format(strSql, guid);
                lstTransferData = SqlServerHelper.GetEntityList<WxOrder>(SqlServerHelper.salesorderConn(), strSql);

                return lstTransferData;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 取得更新的Sql文
        /// </summary>
        /// <returns></returns>
        private static string GetUpdateSql(string guid,string costCenterCode)
        {
            // 当前时间
            string dateTimeNow = DateTimeHelper.convertDateTime(DateTime.Now.ToString());

            string strSqlUpdate = " UPDATE WXORDER " +
                                     " SET TRANSFERTIME = '{0}' " +
                                     " FROM WXUSERMAST" + 
                                     " WHERE WXORDER.TRANSFERGUID = '{1}' " +
                                     " AND WXORDER.openid=WxUserMast.WechatId " +
                                     " AND WxUserMast.CostcenterCode='{2}' " +
                                     " AND WXUSERMAST.DeleteTime IS NULL ";
            strSqlUpdate = string.Format(strSqlUpdate, dateTimeNow, guid,costCenterCode);

            return strSqlUpdate;
        }
    }
}
