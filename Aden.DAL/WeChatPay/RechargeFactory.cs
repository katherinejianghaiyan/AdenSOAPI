using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Data;
using Aden.Model.MastData;
using Aden.Model.WeChat;
using Aden.Util.Database;
using Aden.Util.Common;
using Aden.DAL.MastData;
using Aden.Model.Common;


namespace Aden.DAL.WeChatPay
{
    public class RechargeFactory
    {
        public WxUserMast GetUserInfo(string wechatId)
        {
            WxUserMast userInfo = new WxUserMast();

            string strSql = " SELECT APPNAME " +
                                 " , WECHATID " +
                                 " , COSTCENTERCODE " +
                                 " , NICKNAME " +
                                 " , NICKNAMEENCODE " +
                                 " , GENDER " +
                                 " , LANGUAGE " +
                                 " , CITY " +
                                 " , PROVINCE " +
                                 " , ISTESTUSER " +
                                 " , CONVERT(VARCHAR(100), CREATETIME, 20) AS CREATETIME " +
                              " FROM WXUSERMAST " +
                             " WHERE WECHATID = '{0}' " +
                               " AND DELETETIME IS NULL ";
            userInfo = SqlServerHelper.GetEntity<WxUserMast>(SqlServerHelper.salesorderConn(), string.Format(strSql, wechatId));

            return userInfo;
        }

        public int ModifyWxUser(WxUserMast param)
        {
            int result = 0;
            // 当前时间
            string dateTimeNow = DateTimeHelper.convertDateTime(DateTime.Now.ToString());

            #region Sql文定义
            string strSqlSel = " SELECT COSTCENTERCODE " +
                                    " , ISTESTUSER " +
                                 " FROM WXUSERMAST " +
                                " WHERE WECHATID = '{0}' " +
                                  " AND DELETETIME IS NULL ";

            string strSqlInsert = " INSERT INTO WXUSERMAST " +
                                            " ( APPNAME " +
                                            " , WECHATID " +
                                            " , COSTCENTERCODE " +
                                            " , NICKNAME " +
                                            " , NICKNAMEENCODE " +
                                            " , GENDER " +
                                            " , LANGUAGE " +
                                            " , CITY " +
                                            " , PROVINCE " +
                                            " , ISTESTUSER " +
                                            " , CREATETIME ) " +
                                       " VALUES " +
                                            " ( '{0}' " +
                                            " , '{1}' " +
                                            " , '{2}' " +
                                            " , '{3}' " +
                                            " , '{4}' " +
                                            " , '{5}' " +
                                            " , '{6}' " +
                                            " , '{7}' " +
                                            " , '{8}' " +
                                            " , '{9}' " +
                                            " , '{10}' ) ";

            string strSqlUpdate = " UPDATE WXUSERMAST " +
                                     " SET COSTCENTERCODE = '{0}' " +
                                       " , NICKNAME = '{1}' " +
                                       " , NICKNAMEENCODE = '{2}' " +
                                       " , GENDER = '{3}' " +
                                       " , LANGUAGE = '{4}' " +
                                       " , CITY = '{5}' " +
                                       " , PROVINCE = '{6}' " +
                                   " WHERE WECHATID = '{7}' " +
                                     " AND DELETETIME IS NULL ";

            string strSqlDelete1 = " UPDATE WXUSERMAST " +
                                     " SET DELETETIME = '{0}' " +
                                   " WHERE WECHATID = '{1}' " +
                                     " AND DELETETIME IS NULL ";

            string strSqlDelete2 = " UPDATE WXUSERCARD " +
                                      " SET DELETETIME = '{0}' " +
                                    " WHERE WECHATID = '{1}' " +
                                      " AND DELETETIME IS NULL ";
            #endregion
            // 检查用户是否存在
            WxUserMast user = SqlServerHelper.GetEntity<WxUserMast>(SqlServerHelper.salesorderConn(), string.Format(strSqlSel, param.wechatId));
            // 最终Sql文
            StringBuilder sbSqlFinal = new StringBuilder();
            // 用户昵称Base64编码
            string nickNameEncode = System.Web.HttpUtility.UrlEncode(param.nickName, System.Text.Encoding.UTF8);

            if (user == null)
                // Insert Sql
                sbSqlFinal.Append(string.Format(strSqlInsert, param.appName, param.wechatId, param.costCenterCode, param.nickName, nickNameEncode,
                    param.gender, param.language, param.city, param.province, 0, dateTimeNow));
            else
            {
                /***当CostCenter不相同时删除原来的记录后插入新记录***/
                if (!user.costCenterCode.Equals(param.costCenterCode))
                {
                    // Delte Sql WxUserMast
                    sbSqlFinal.Append(string.Format(strSqlDelete1, dateTimeNow, param.wechatId));
                    // Delte Sql WxUserCard
                    sbSqlFinal.Append(string.Format(strSqlDelete2, dateTimeNow, param.wechatId));
                    // Insert Sql
                    sbSqlFinal.Append(string.Format(strSqlInsert, param.appName, param.wechatId, param.costCenterCode,
                        param.nickName, nickNameEncode, param.gender, param.language, param.city, param.province, user.isTestUser, dateTimeNow));
                }
                else
                {
                    // Update Sql
                    sbSqlFinal.Append(string.Format(strSqlUpdate, param.costCenterCode, param.nickName, nickNameEncode,
                        param.gender, param.language, param.city, param.province, param.wechatId));
                }
            }
            // 执行Sql文
            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sbSqlFinal.ToString());

            return result;
        }

        /// <summary>
        /// 根据WeChatId取得成本中心
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public CCMast GetCostCenterCode(WxUserMast param)
        {
            string strSql = " SELECT COSTCENTERCODE " +
                              " FROM WXUSERMAST " +
                             " WHERE APPNAME = '{0}' " +
                               " AND WECHATID = '{1}' " +
                               " AND DELETETIME IS NULL ";
            WxUserMast user = SqlServerHelper.GetEntity<WxUserMast>(SqlServerHelper.salesorderConn(), string.Format(strSql, param.appName, param.wechatId));

            if (user != null)
            {
                CCMast cc = (new CCMastFactory()).GetCCMastInfo(user.costCenterCode);
                string name = (new SalesOrder.MastDataFactory()).GetCostCenterInfo(cc.costCenterCode).costCenterName_ZH;
                cc.costCenterName = string.IsNullOrWhiteSpace(name) ? cc.costCenterName : name;
                return cc;
            }

            return null;
        }

        private List<T> GetList<T>(string costCenterCode, string cardid, string sqltype)
        {
            if(string.IsNullOrWhiteSpace(cardid)) return null;

            CCMastFactory ccmf = new CCMastFactory();
            // Table CCMast
            CCMast ccm = ccmf.GetCCMastInfo(costCenterCode);
            // Table SqlMast
            SingleField ccObj = new SingleField();
            ccObj.code = costCenterCode;
            List<SingleField> lstCcObj = new List<SingleField>();
            lstCcObj.Add(ccObj);
            SqlMast sm = ccmf.GetSqlMastInfo(lstCcObj, sqltype)[0];
            // Sql文
            string strSql = string.Format(sm.sqlCommand, cardid);

            #region TCP 
            if (string.IsNullOrWhiteSpace(ccm.posDBUserName))
                return NetTcpHelper.GetEntityList<T>(ccm.posIp, ccm.posDBName, strSql);
            #endregion

            #region 连数据

            string strConn = string.Format(string.Format(SqlServerHelper.customerAllConn(), 
                ccm.posIp, ccm.posDBName, ccm.posDBUserName, ccm.posDBPassword));
                
            //strConn = "Data Source=192.168.0.97,1433;Initial Catalog=DWPOS;User ID=sa;Password=gladis0083;Persist Security Info=True;Connection Timeout=10";
            // 取得卡信息
            return SqlServerHelper.GetEntityList<T>(strConn, strSql);
            #endregion                   
        }

        private T GetData<T>(string costCenterCode, string cardid, string sqltype)
        {
            if (string.IsNullOrWhiteSpace(cardid)) return default(T);

            CCMastFactory ccmf = new CCMastFactory();
            // Table CCMast
            CCMast ccm = ccmf.GetCCMastInfo(costCenterCode);
            // Table SqlMast
            SingleField ccObj = new SingleField();
            ccObj.code = costCenterCode;
            List<SingleField> lstCcObj = new List<SingleField>();
            lstCcObj.Add(ccObj);
            SqlMast sm = ccmf.GetSqlMastInfo(lstCcObj, sqltype)[0];
            // Sql文
            string strSql = string.Format(sm.sqlCommand, cardid);

            #region TCP 
            if (string.IsNullOrWhiteSpace(ccm.posDBUserName))
                return NetTcpHelper.GetEntity<T>(ccm.posIp, ccm.posDBName, strSql);
            #endregion

            #region 连数据

            string strConn = string.Format(string.Format(SqlServerHelper.customerAllConn(),
                ccm.posIp, ccm.posDBName, ccm.posDBUserName, ccm.posDBPassword));

            //strConn = "Data Source=192.168.0.97,1433;Initial Catalog=DWPOS;User ID=sa;Password=gladis0083;Persist Security Info=True;Connection Timeout=10";
            // 取得卡信息
            return SqlServerHelper.GetEntity<T>(strConn, strSql);
            #endregion                   
        }

        /// <summary>
        /// 根据CardId取得Card相关信息
        /// </summary>
        /// <param name="CardInfoParam"></param>
        /// <param name="sqlType"></param>
        /// <returns></returns>
        public CardInfo GetCardInfo(CardInfoParam param)
        {
            //if (string.IsNullOrWhiteSpace(param.cardkey)) return null;

            //CCMastFactory ccmf = new CCMastFactory();
            //// Table CCMast
            //CCMast ccm = ccmf.GetCCMastInfo(param.costCenterCode);
            //// Table SqlMast
            //SingleField ccObj = new SingleField();
            //ccObj.code = param.costCenterCode;
            //List<SingleField> lstCcObj = new List<SingleField>();
            //lstCcObj.Add(ccObj);
            //SqlMast sm = ccmf.GetSqlMastInfo(lstCcObj, "GetPOSUsers")[0];

            //// Sql Connection
            //string strConn = string.Format(string.Format(SqlServerHelper.customerAllConn, ccm.posIp, ccm.posDBName, ccm.posDBUserName
            //    , ccm.posDBPassword));
            //// Sql文
            //string strSql = string.Format(sm.sqlCommand, param.cardkey);
            ////strConn = "Data Source=192.168.0.97,1433;Initial Catalog=DWPOS;User ID=sa;Password=gladis0083;Persist Security Info=True;Connection Timeout=10";
            //// 取得卡信息
            //List<CardInfo> cardObjs = SqlServerHelper.GetEntityList<CardInfo>(strConn, strSql);
            List<CardInfo> cardObjs = GetList<CardInfo>(param.costCenterCode,param.cardkey, "GetPOSUsers");

            if (!cardObjs.Any()) return null;

            //检查用户卡是否已绑定
            CardInfo cardObj = cardObjs.FirstOrDefault();
            WxUserCard card = CheckBind(param.wechatId, string.Join("','", cardObjs.Select(q => q.oldCardId)));
            cardObj.userCode = string.IsNullOrWhiteSpace(cardObj.userCode)? cardObj.cardCode: cardObj.userCode;
            if (card == null) return cardObj;

            //更改已绑定的cardId
            cardObj.cardId = card.cardId;
            cardObj.isBind = true;

            return cardObj;
        }

        /// <summary>
        /// 根据卡号取得卡信息及余额
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public UserBalance GetBalance(CardInfoParam param)
        {
            #region 当前余额信息
            //CCMastFactory ccmf = new CCMastFactory();
            //// Table CCMast
            //CCMast ccm = ccmf.GetCCMastInfo(param.costCenterCode);
            //// Table SqlMast
            //SingleField ccObj = new SingleField();
            //ccObj.code = param.costCenterCode;
            //List<SingleField> lstCcObj = new List<SingleField>();
            //lstCcObj.Add(ccObj);
            //SqlMast sm = ccmf.GetSqlMastInfo(lstCcObj, "GetPOSUserBal")[0];
            //// Sql Connection
            //string strConn = string.Format(string.Format(SqlServerHelper.customerAllConn, ccm.posIp, ccm.posDBName, ccm.posDBUserName
            //    , ccm.posDBPassword));
            ////strConn = "Data Source=222.92.143.28,11233;Initial Catalog=YCCY;User ID=120dw201;Password=gladis0083;Persist Security Info=True;Connection Timeout=10";
            //// Sql文
            //string strSql = string.Format(sm.sqlCommand, param.cardkey);
            //// 取得余额信息
            ////List<dynamic> lst = SqlServerHelper.GetDynamicList(strConn, strSql);

            ////return (lst[0] as IDictionary<string, object>).Values.FirstOrDefault().ToDecimal().ToString("0.##");
            //UserBalance userBalance = SqlServerHelper.GetEntity<UserBalance>(strConn, strSql);
            #endregion

            return GetData<UserBalance>(param.costCenterCode,param.cardkey, "GetPOSUserBal");//userBalance;
        }

        /// <summary>
        /// 取得最后一笔充值成功记录
        /// </summary>
        /// <param name="wechatId"></param>
        /// <returns></returns>
        public WxOrder GetLastRecharge(CardInfoParam param)
        {
            // 最后一笔成功充值信息
            string strSql = " SELECT TOP 1 " +
                                 "   CAST(TOTAL_FEE * 1.0 / 100 AS DEC(10, 2)) AS TOTAL_FEE " +
                                 " , TIME_END " +
                              " FROM WXORDER " +
                             " WHERE OPENID = '{0}' " +
                               " AND TRANSFERTIME IS NOT NULL " +
                             " ORDER BY ID DESC ";

            WxOrder lastOrder = SqlServerHelper.GetEntity<WxOrder>(SqlServerHelper.salesorderConn(), string.Format(strSql, param.wechatId));
            if(lastOrder != null) 
                lastOrder.time_end = DateTimeHelper.convertDateTime(lastOrder.time_end, "yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");

            return lastOrder;
        }

        /// <summary>
        /// 取得交易历史记录
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TransLog> GetTransLog(TransLogParam param)
        {
            //List<TransLog> lstTransLog = new List<TransLog>();

            //CCMastFactory ccmf = new CCMastFactory();
            //// Table CCMast
            //CCMast ccm = ccmf.GetCCMastInfo(param.costCenterCode);
            //// Table SqlMast
            //SingleField ccObj = new SingleField();
            //ccObj.code = param.costCenterCode;
            //List<SingleField> lstCcObj = new List<SingleField>();
            //lstCcObj.Add(ccObj);
            //SqlMast sm = ccmf.GetSqlMastInfo(lstCcObj, "POSTransaction")[0];
            //// Sql Connection
            //string strConn = string.Format(string.Format(SqlServerHelper.customerAllConn, ccm.posIp, ccm.posDBName, ccm.posDBUserName
            //    , ccm.posDBPassword));
            //// Sql文
            //string strSql = string.Format(sm.sqlCommand, param.cardId);
            //// 取得交易信息
            //lstTransLog = SqlServerHelper.GetEntityList<TransLog>(strConn, strSql);
            List<TransLog> lstTransLog = GetList<TransLog>(param.costCenterCode, param.cardId, "POSTransaction");

            foreach (TransLog t in lstTransLog)
            {
                string[] sArray = t.transDate.Split(' ');
                t.year = sArray[0].Substring(0, 4);
                t.month = t.year + "." + sArray[0].Substring(5, 2);
                t.transDate = sArray[0];
                t.transTime = sArray[1];
            }

            lstTransLog = lstTransLog.OrderByDescending(r => r.transDate).ToList();

            return lstTransLog;
        }

        ///// <summary>
        ///// 取得交易记录
        ///// </summary>
        ///// <param name="param"></param>
        ///// <returns></returns>
        //public List<TransLog> GetTransLog_bak2(WxUserCard param)
        //{
        //    string strSql = " SELECT APPNAME " +
        //                         " , TYPE " +
        //                         " , CARDID " +
        //                         " , OUT_TRADE_NO " +
        //                         " , TRANSACTION_ID " +
        //                         " , OPENID " +
        //                         " , ATTACH " +
        //                         " , CAST(TOTAL_FEE AS DECIMAL) AS TOTAL_FEE " +
        //                         " , CASH_FEE " +
        //                         " , FEE_TYPE " +
        //                         " , BANK_TYPE " +
        //                         " , TIME_END " +
        //                         " , TIME_START " +
        //                         " , TIME_EXPIRE " +
        //                         " , RESULT_CODE " +
        //                         " , RETURN_CODE " +
        //                         " , CONVERT(VARCHAR(100), CREATETIME, 120) AS CREATETIME " +
        //                         " , CONVERT(VARCHAR(100), NOTIFYTIME, 120) AS NOTIFYTIME " +
        //                      " FROM WXORDER " +
        //                     " WHERE OPENID = '{0}' " +
        //                       " AND CARDID = '{1}' " +
        //                       " AND RETURN_CODE = 'SUCCESS' " +
        //                       " AND NOTIFYTIME IS NOT NULL ";

        //    List<WxOrder> lstWxOrder = SqlServerHelper.GetEntityList<WxOrder>(SqlServerHelper.salesorderConn, string.Format(strSql, param.wechatId,
        //        param.cardId));

        //    List<TransLog> lstTlog = new List<TransLog>();
        //    TransLog tLog = new TransLog();

        //    foreach(WxOrder o in lstWxOrder)
        //    {
        //        tLog = new TransLog();

        //        tLog.amount = (decimal)o.total_fee / 100;
        //        tLog.type = "success".Equals(o.result_code.ToLower()) ? "s" : "x";
        //        tLog.operateDate = o.time_end;
        //        tLog.date = DateTimeHelper.convertDateTime(o.time_end, "yyyyMMddHHmmss", "MM-dd");
        //        tLog.time = DateTimeHelper.convertDateTime(o.time_end, "yyyyMMddHHmmss", "HH:mm");
        //        tLog.typeCSS = "#3cc51f";

        //        lstTlog.Add(tLog);
        //    }

        //    return lstTlog;
        //}

        ///// <summary>
        ///// 取得交易记录
        ///// </summary>
        ///// <param name="param"></param>
        ///// <returns></returns>
        //public List<TransLog> GetTransLog_bak(AdenPayUserCard param)
        //{
        //    string strSql = " SELECT TOP 30 AMOUNT " +
        //                         " , TYPE " +
        //                         " , OPERATEDATE " +
        //                      " FROM " +
        //                       " ( SELECT AMOUNT " +
        //                              " , 'A' AS TYPE " +
        //                              " , CONVERT(VARCHAR(100), CREATEDATE, 120) AS OPERATEDATE " +
        //                              " , '' AS FLOWNO " +
        //                           " FROM ADENPAIEDRECORD " +
        //                          " WHERE CARDNUMBER = '{0}' " +
        //                          " UNION " +
        //                         " SELECT REALAMT AS AMOUNT " +
        //                              " , TYPE " +
        //                              " , CONVERT(VARCHAR(100), OPERATEDATE, 120) AS OPERATEDATE " +
        //                              " , FLOWNO " +
        //                           " FROM ADENPOSRECORD " +
        //                          " WHERE CARDNUMBER = '{0}' ) AS T " +
        //                          " ORDER BY OPERATEDATE DESC ";

        //    List<TransLog> lstTLog = new List<TransLog>();

        //    lstTLog = SqlServerHelper.GetEntityList<TransLog>(SqlServerHelper.gladisConn, string.Format(strSql, param.cardNumber));

        //    foreach (TransLog tlog in lstTLog)
        //    {
        //        string[] strs = tlog.operateDate.Split(' ');
        //        tlog.date = strs[0].Replace("-", ".");
        //        tlog.time = strs[1];
        //        tlog.typeCSS = "S".Equals(tlog.type) ? "red" : "#3cc51f";
        //    }

        //    return lstTLog;
        //}

        /// <summary>
        /// 根据WechatId取得绑定的卡信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<WxUserCard> GetCardList(WxUserMast param)
        {
            List<WxUserCard> lstCards = new List<WxUserCard>();

            string strSql = " SELECT CARDID " +
                                 " , USERCODE " +
                                 " , USERNAME " +
                              " FROM WXUSERCARD " +
                             " WHERE WECHATID = '{0}' " +
                               " AND DELETETIME IS NULL " +
                             " ORDER BY CREATETIME DESC ";

            lstCards = SqlServerHelper.GetEntityList<WxUserCard>(SqlServerHelper.salesorderConn(), string.Format(strSql, param.wechatId));

            return lstCards;
        }

        /// <summary>
        /// 检查是否已经绑定过卡
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool CheckBind(CardInfo param)
        {
            return CheckBind(param.wechatId, param.cardId) == null;
        }
        private WxUserCard CheckBind(string wechatid, string cardids)
        {
            string strSql = " SELECT CARDID " +
                              " FROM WXUSERCARD " +
                             " WHERE WECHATID = '{0}' " +
                               " AND CARDID in ('{1}') " +
                               " AND DELETETIME IS NULL ";

            WxUserCard card = SqlServerHelper.GetEntity<WxUserCard>(SqlServerHelper.salesorderConn(),
                string.Format(strSql, wechatid, cardids));

            return card;
        }

        /// <summary>
        /// 将卡号和WechatId绑定/解绑
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int BindCard(CardInfo param, string mode)
        {
            int result = 0;

            #region Sql文定义
            string strSqlInsert = " INSERT INTO WXUSERCARD  " +
                                            " ( WECHATID  " +
                                            " , CARDID  " +
                                            " , USERCODE  " +
                                            " , USERNAME  " +
                                            " , CREATETIME ) " +
                                       " VALUES " +
                                            " ( '{0}' " +
                                            " , '{1}' " +
                                            " , '{2}' " +
                                            " , '{3}' " +
                                            " , '{4}' ) ";

            string strSqlUpdate = " UPDATE WXUSERCARD " +
                                     " SET DELETETIME = '{0}' " +
                                   " WHERE WECHATID = '{1}' " +
                                     " AND USERCODE = '{2}' " +
                                     " AND DELETETIME IS NULL ";
            #endregion
            // 当前时间
            string dateTimeNow = DateTimeHelper.convertDateTime(DateTime.Now.ToString());
            // 最终Sql文
            string strSqlFinal = string.Empty;
            // 绑定卡
            if ("bind".Equals(mode.ToLower()))
            {
                if (CheckBind(param))
                    strSqlFinal = string.Format(strSqlInsert, param.wechatId, param.cardId, param.userCode, param.userName, dateTimeNow);
            }
            // 解除绑定卡
            else if ("unbind".Equals(mode.ToLower()))
                strSqlFinal = string.Format(strSqlUpdate, dateTimeNow, param.wechatId, param.userCode);

            if (!string.IsNullOrWhiteSpace(strSqlFinal))
                result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), strSqlFinal);

            return result;
        }
    }
}
