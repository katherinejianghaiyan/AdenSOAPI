using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model.WeChat;
using Aden.Util.Database;
using Aden.Util.Common;

namespace Aden.DAL.WeChatPay
{
    public class WxOrderFactory
    {
        /// <summary>
        /// 创建支付订单
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int CreatePayOrder(WxOrder param)
        {
            int result = 0;
            StringBuilder sb = new StringBuilder();

            #region 微信支付
            if (param.total_fee > 0) 
            {
                // 当前时间
                string dateTimeNow = DateTimeHelper.convertDateTime(DateTime.Now.ToString());

                #region Sql定义
                string strSql = " INSERT INTO WXORDER " +
                                     " ( APPNAME " +
                                     " , TYPE " +
                                     " , CARDID " +
                                     " , OUT_TRADE_NO " +
                                     " , OPENID " +
                                     " , ATTACH " +
                                     " , TOTAL_FEE " +
                                     " , TIME_START " +
                                     " , TIME_EXPIRE " +
                                     " , CREATETIME ) " +
                                " VALUES " +
                                     " ( '{0}' " +
                                     " , '{1}' " +
                                     " , '{2}' " +
                                     " , '{3}' " +
                                     " , '{4}' " +
                                     " , '{5}' " +
                                     " , {6} " +
                                     " , '{7}' " +
                                     " , '{8}' " +
                                     " , '{9}' ); ";
                #endregion

                sb.AppendFormat(strSql, param.appName, param.type, param.cardId, param.out_trade_no, param.openid, param.attach, param.total_fee,
                    param.time_start, param.time_expire, dateTimeNow);
            }
            #endregion

            sb.Append(CreateCouponOrder(param));
            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), sb.ToString());

            return result;
        }


        private string CreateCouponOrder(WxOrder param)
        {
            if (param.coupons == null) return "";

            // 当前时间
            string dateTimeNow = DateTimeHelper.convertDateTime(DateTime.Now.ToString());

            #region Sql定义
            string strSql = " INSERT INTO WXORDER " +
                                 " ( APPNAME " +
                                 " , TYPE " +
                                 " , CARDID " +
                                 " , OUT_TRADE_NO " +
                                 " , OPENID " +
                                 " , ATTACH " +
                                 " , COUPONPRICE,COUPONQTY " +
                                 " , TIME_START " +
                                 " , TIME_EXPIRE " +
                                 " , CREATETIME ) " +
                            " VALUES " +
                                 " ( '{0}' " +
                                 " , '{1}' " +
                                 " , '{2}' " +
                                 " , '{3}' " +
                                 " , '{4}' " +
                                 " , '{5}' " +
                                 " , {6} " +
                                 " , {7} " +
                                 " , '{8}' " +
                                 " , '{9}' ); ";
            #endregion

            strSql = string.Format(strSql, param.appName, param.type, param.cardId, param.out_trade_no,
                param.openid, param.attach, "{0},{1}",
                param.time_start, param.time_expire, dateTimeNow);
            StringBuilder sb = new StringBuilder();
            foreach(Coupon coupon in param.coupons)
                sb.AppendFormat(strSql, coupon.price, coupon.qty);

            return sb.ToString();
        }

        /// <summary>
        /// 微信支付通知回写
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int UpdatePayOrder(WxOrder param)
        {
            int result = 0;

            // 当前时间
            string dateTimeNow = DateTimeHelper.convertDateTime(DateTime.Now.ToString());

            string strSql = " UPDATE WXORDER " +
                               " SET TRANSACTION_ID = '{0}' " +
                                 " , CASH_FEE = {1} " +
                                 " , FEE_TYPE = '{2}' " +
                                 " , BANK_TYPE = '{3}' " +
                                 " , TIME_END = '{4}' " +
                                 " , RESULT_CODE = '{5}' " +
                                 " , RETURN_CODE = '{6}' " +
                                 " , NOTIFYTIME = '{7}' " +
                             " WHERE APPNAME = '{8}' " +
                               " AND OUT_TRADE_NO = '{9}' ";

            string strSqlFinal = string.Format(strSql, param.transaction_id, param.cash_fee, param.fee_type, param.bank_type, param.time_end,
                param.result_code, param.return_code, dateTimeNow, param.appName, param.out_trade_no);

            result = SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), strSqlFinal);

            return result;
        }
    }
}
