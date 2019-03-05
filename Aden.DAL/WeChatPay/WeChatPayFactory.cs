using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Model.WeChat;
using Aden.Model.MastData;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using WxPayAPI;
using Aden.Util.Database;
using System.Net;
using Aden.Util.Common;
using Aden.Model.Common;
using Aden.DAL.MastData;
using System.Data;

namespace Aden.DAL.WeChatPay
{
    public class WeChatPayFactory
    {
        private WeChatConfig wcf = new WeChatConfig();
        public WeChatPayFactory(string appName = null)
        {
            if (!string.IsNullOrWhiteSpace(appName))
            {
                //string strSql = " SELECT * " +
                //                  " FROM TBLWECHATCONFIG " +
                //                 " WHERE APPNAME = '{0}' ";

                //wcf = SqlServerHelper.GetEntity<WeChatConfig>(SqlServerHelper.salesorderConn, string.Format(strSql, appName));
                wcf = (new WxMastDataFactory()).GetWeChatConfig(appName);
            }
        }

        /// <summary>
        /// 创建预支付订单
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public WeChatResult UnifiedOrder(WeChatInfo param)
        {
            WeChatResult wxResult = new WeChatResult();
            // 回调地址
            string notifyUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["notifyUrl"].ToString();
            try
            {
                // 检查服务端是否处于调整中
                if (CheckServerMantain(param.costCenterCode))
                {
                    wxResult.isMantain = true;
                    return wxResult;
                }
                
                // 检查充值金额和设置是否符合
                wxResult.matched = true;
                if(!CheckRechargeMatched(param))
                {
                    wxResult.matched = false;
                    return wxResult;
                }   

                string openid = param.openId;
                string ordertime = SalesOrder.Common.convertDateTime(DateTime.Now.ToString());
                bool isTestUser = (new RechargeFactory()).GetUserInfo(openid).isTestUser;
                // 本地交易号前三位（用于在商户平台上区分支付场景，回调时手动去除不存数据库）
                string fcode = wcf.paymentCode;
                int len = fcode.Length;
                /***统一下单1***/
                WxPayData data = new WxPayData(wcf);
                data.SetValue("body", param.costCenterCode + "-餐卡充值");
                data.SetValue("attach", param.appName);
                data.SetValue("out_trade_no", fcode + WxPayApi.GenerateOutTradeNo(param.costCenterCode));
                //data.SetValue("total_fee", param.total_fee);
                data.SetValue("total_fee", isTestUser ? 1 : param.total_fee);
                data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
                data.SetValue("time_expire", DateTime.Now.AddMinutes(5).ToString("yyyyMMddHHmmss"));
                //data.SetValue("goods_tag", "test");
                data.SetValue("trade_type", "JSAPI");
                data.SetValue("openid", openid);
                data.SetValue("notify_url", notifyUrl);

                WriteLog(data.ToJson().ToString());

                if (param.total_fee > 0)
                {
                    WxPayData result = WxPayApi.UnifiedOrder(wcf, data);

                    WriteLog(result.ToJson().ToString());

                    if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
                    {
                        Log.Error(this.GetType().ToString(), "UnifiedOrder response error!");
                        throw new WxPayException("UnifiedOrder response error!");
                    }

                    wxResult.prepay_id = result.GetValue("prepay_id").ToString();
                    wxResult.paySign = result.GetValue("sign").ToString();
                    wxResult.nonceStr = result.GetValue("nonce_str").ToString();
                }
                /***订单写入本地***/
                string st = data.GetValue("out_trade_no").ToString();
                WxOrder order = new WxOrder
                {
                    appName = param.appName,
                    type = param.type,
                    cardId = param.cardId,
                    out_trade_no = st.Substring(len, st.Length - len),
                    openid = data.GetValue("openid").ToString(),
                    attach = data.GetValue("attach").ToString(),
                    coupons = param.coupons == null || !param.coupons.Any() ? null :
                        param.coupons.SelectMany(q=>q).Where(q=>!string.IsNullOrWhiteSpace(q)).GroupBy(q=>q)
                        .Select(q=>new Coupon
                        {
                            price = int.Parse(q.Key),
                            qty = q.Count()
                        }).ToList(),
                    total_fee = int.Parse(data.GetValue("total_fee").ToString()),
                    time_start = data.GetValue("time_start").ToString(),
                    time_expire = data.GetValue("time_expire").ToString()
                };

                WxOrderFactory wof = new WxOrderFactory();

                /***本地没有写成功的话直接返回NULL***/
                if (wof.CreatePayOrder(order) <= 0)
                    return null;

                return wxResult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 检查服务器维护
        /// </summary>
        /// <param name="costCenterCode"></param>
        /// <returns></returns>
        private bool CheckServerMantain(string costCenterCode)
        {
            CCMast cm = (new CCMastFactory()).GetCCMastInfo(costCenterCode);

            return string.IsNullOrWhiteSpace(cm.rechargeSetting);
        }

        /// <summary>
        /// 检查充值金额是否符合设定
        /// </summary>
        /// <param name="total_fee"></param>
        /// <param name="rechargeSetting"></param>
        /// <returns></returns>
        private bool CheckRechargeMatched(WeChatInfo param)
        {
            string strTotal_fee = (param.total_fee * 1.0 / 100).ToString();

            string[] aryRecharge = param.rechargeSetting.Split(',');

            if (string.Empty.Equals(aryRecharge[aryRecharge.Length - 1]))
                return true;
            
            foreach(string rc in aryRecharge)
            {
                if (rc.Equals(strTotal_fee))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 客制化方法：生成支付Sign
        /// </summary>
        /// <param name="inputObj"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public string GetPaySign(WeChatResult param)
        {
            WxPayData tempObj = new WxPayData(wcf);
            tempObj.SetValue("appId", wcf.appId);//小程序账号ID
            tempObj.SetValue("nonceStr", param.nonceStr);//随机字符串
            tempObj.SetValue("package", param.prepay_id);//预生成订单ID
            tempObj.SetValue("signType", "MD5");//加密类型
            tempObj.SetValue("timeStamp", param.timeStamp);//用户支付时间	 

            string strSign = tempObj.MakeSign();//签名

            WriteLog("nonceStr=" + param.nonceStr + "&package=" + param.prepay_id + "&timeStamp=" + param.timeStamp + "&sign=" + strSign);

            return strSign;
        }

        /// <summary>
        /// 将小程序用户code转换成openid
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public string GetOpenId(WeChatInfo param)
        {
            string openid = WxPayApi.GetOpenId(wcf, param.code);
            return openid;
        }

        /// <summary>
        /// 获取access_token
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public string GetAccessToken(string isNew = "")
        {
            
            string access_token = WxPayApi.GetAccessToken(wcf, isNew);
            return access_token;
        }

        private void WriteLog(string Log)
        {
            string strDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string strSql = " INSERT WECHATDEVLOG " +
                                 " ( MESSAGE" +
                                 " , CREATETIME ) " +
                            " VALUES " +
                                 " ( '{0}' " +
                                 " , '{1}' ) ";

            SqlServerHelper.Execute(SqlServerHelper.salesorderConn(), string.Format(strSql, Log, strDateTime));
        }

        /// <summary>
        /// 取得小程序配置信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public string GetWxPayConfig(SingleField param)
        {
            string value = string.Empty;

            switch (param.code.ToLower())
            {
                case "appid":
                    value = wcf.appId;
                    break;
                case "mchid":
                    value = wcf.mchId;
                    break;
                case "key":
                    value = wcf.key;
                    break;
                case "appsecret":
                    value = wcf.appSecret;
                    break;
            }
            return value;
        }

        public bool CheckSign(string xml)
        {
            bool result = false;

            WxPayData tempObj = new WxPayData(wcf);
            tempObj.FromXml(xml);

            result = tempObj.CheckSign();
            return result;
        }

        /// <summary>
        /// 将XML转换成实体类
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public WxPayNotifyData GetWxPayNotifyData(string xml)
        {
            WxPayData tempObj = new WxPayData(wcf);
            WxPayNotifyData wxObj = new WxPayNotifyData();

            SortedDictionary<string, Object> dic = tempObj.FromXml(xml);

            // 微信开放平台审核通过的应用APPID
            var wx_appid = "";
            // 微信支付的订单名称
            var wx_attach = "";
            // 微信支付分配的商户号  
            var wx_mch_id = "";
            // 随机字符串，不长于32位  
            var wx_nonce_str = "";
            // 签名，详见签名算法  
            var wx_sign = "";
            // 回执：SUCCESS/FAIL  
            var wx_result_code = "";
            // 回执：SUCCESS/FAIL 
            var wx_return_code = "";
            // 用户在商户appid下的唯一标识  
            var wx_openid = "";
            // 用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效  
            var wx_is_subscribe = "";
            // APP  
            var wx_trade_type = "";
            // 银行类型，采用字符串类型的银行标识，银行类型见银行列表  
            var wx_bank_type = "";
            // 货币类型，符合ISO4217标准的三位字母代码，默认人民币：CNY，其他值列表详见货币类型  
            var wx_fee_type = "";
            // 微信支付订单号  
            var wx_transaction_id = "";
            // 商户系统的订单号，与请求一致 
            var wx_out_trade_no = "";
            // 支付完成时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010。其他详见时间规则
            var wx_time_end = "";
            // 订单总金额，单位为分  
            var wx_total_fee = 1;
            // 现金支付金额订单现金支付金额，详见支付金额  
            var wx_cash_fee = 1;

            var result_code = dic.Where(r => "result_code".Equals(r.Key));
            var return_code = dic.Where(r => "returen_code".Equals(r.Key));

            //wx_appid
            if (dic.ContainsKey("appid"))
            {
                wx_appid = dic["appid"].ToString();
                if (!string.IsNullOrEmpty(wx_appid))
                    wxObj.appid = wx_appid;
            }
            //wx_attach
            if (dic.ContainsKey("attach"))
            {
                wx_attach = dic["attach"].ToString();
                if (!string.IsNullOrEmpty(wx_attach))
                    wxObj.attch = wx_attach;
            }
            //wx_bank_type  
            if (dic.ContainsKey("bank_type"))
            {
                wx_bank_type = dic["bank_type"].ToString();
                if (!string.IsNullOrEmpty(wx_bank_type))
                    wxObj.bank_type = wx_bank_type;
            }
            //wx_cash_fee  
            if (dic.ContainsKey("cash_fee"))
            {
                wx_cash_fee = Convert.ToInt32(dic["cash_fee"].ToString());
                wxObj.cash_fee = wx_cash_fee;
            }

            //wx_fee_type  
            if (dic.ContainsKey("fee_type"))
            {
                wx_fee_type = dic["fee_type"].ToString();
                if (!string.IsNullOrEmpty(wx_fee_type))
                    wxObj.fee_type = wx_fee_type;
            }

            //wx_is_subscribe  
            if (dic.ContainsKey("is_subscribe"))
            {
                wx_is_subscribe = dic["is_subscribe"].ToString();
                if (!string.IsNullOrEmpty(wx_is_subscribe))
                    wxObj.is_subscribe = wx_is_subscribe;
            }

            //wx_mch_id  
            if (dic.ContainsKey("mch_id"))
            {
                wx_mch_id = dic["mch_id"].ToString();
                if (!string.IsNullOrEmpty(wx_mch_id))
                    wxObj.mch_id = wx_mch_id;
            }

            //wx_nonce_str  
            if (dic.ContainsKey("nonce_str"))
            {
                wx_nonce_str = dic["nonce_str"].ToString();
                if (!string.IsNullOrEmpty(wx_nonce_str))
                    wxObj.nonce_str = wx_nonce_str;
            }

            //wx_openid  
            if (dic.ContainsKey("openid"))
            {
                wx_openid = dic["openid"].ToString();
                if (!string.IsNullOrEmpty(wx_openid))
                    wxObj.openid = wx_openid;
            }

            //wx_out_trade_no  
            if (dic.ContainsKey("out_trade_no"))
            {
                wx_out_trade_no = dic["out_trade_no"].ToString();
                if (!string.IsNullOrEmpty(wx_out_trade_no))
                    wxObj.out_trade_no = wx_out_trade_no;
            }

            //wx_result_code   
            if (dic.ContainsKey("result_code"))
            {
                wx_result_code = dic["result_code"].ToString();
                if (!string.IsNullOrEmpty(wx_result_code))
                    wxObj.result_code = wx_result_code;
            }

            //wx_result_code   
            if (dic.ContainsKey("return_code"))
            {
                wx_return_code = dic["return_code"].ToString();
                if (!string.IsNullOrEmpty(wx_return_code))
                    wxObj.return_code = wx_return_code;
            }

            //wx_sign   
            if (dic.ContainsKey("sign"))
            {
                wx_sign = dic["sign"].ToString();
                wxObj.sign = wx_sign;
            }

            //wx_time_end  
            if (dic.ContainsKey("time_end"))
            {
                wx_time_end = dic["time_end"].ToString();
                if (!string.IsNullOrEmpty(wx_time_end))
                    wxObj.time_end = wx_time_end;
            }

            //wx_total_fee  
            if (dic.ContainsKey("total_fee"))
            {
                wx_total_fee = Convert.ToInt32(dic["total_fee"].ToString());
                wxObj.total_fee = wx_total_fee;
            }

            //wx_trade_type  
            if (dic.ContainsKey("trade_type"))
            {
                wx_trade_type = dic["trade_type"].ToString();
                if (!string.IsNullOrEmpty(wx_trade_type))
                    wxObj.trade_type = wx_trade_type;
            }

            //wx_transaction_id  
            if (dic.ContainsKey("transaction_id"))
            {
                wx_transaction_id = dic["transaction_id"].ToString();
                if (!string.IsNullOrEmpty(wx_transaction_id))
                    wxObj.transaction_id = wx_transaction_id;
            }
            return wxObj;
        }

        /// <summary>
        /// 将xml转换成Dictionary对象
        /// </summary>
        /// <param name="xml"></param>
        public bool CheckIsSuccess(string xml)
        {
            bool result = false;

            WxPayData tempObj = new WxPayData(wcf);
            SortedDictionary<string, Object> sd = tempObj.FromXml(xml);

            var result_code = sd.Where(r => "result_code".Equals(r.Key));
            var return_code = sd.Where(r => "returen_code".Equals(r.Key));

            if (result_code != null && "success".Equals(result_code.ToString().ToLower()) &&
               return_code != null && "success".Equals(return_code.ToString().ToLower()))
                result = true;

            return result;
        }


        /// <summary>
        /// 生成带参数的二维码
        /// </summary>
        /// <param name="guid">Web socket唯一标识</param>
        /// <param name=""></param>
        /// <returns></returns>
        public byte[] CreateWxCode(Dictionary<string,string> dic)//string guid,string costCenterCode, string cardno, int barCodeSize)
        {
            try
            {
                string token = GetAccessToken();
                byte[] d = WxPayApi.GetWxAppQRCode(token, string.Format("{0},{1},{2}", 
                    dic.ContainsKey("costCenterCode")? dic["costCenterCode"]:"",
                    dic.ContainsKey("cardno") ? dic["cardno"] : "",
                    dic.ContainsKey("guid") ? dic["guid"] : ""));
                if (d.Length < 1000)
                {
                    token = GetAccessToken("isNew");
                    d = WxPayApi.GetWxAppQRCode(token, string.Format("{0},{1},{2}",
                        dic.ContainsKey("costCenterCode") ? dic["cosCenterCode"] : "",
                        dic.ContainsKey("cardno") ? dic["cardno"] : "",
                        dic.ContainsKey("guid") ? dic["guid"] : ""));
                    if (d.Length < 1000) throw new Exception("生成二维码失败");
                }
                return d;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
