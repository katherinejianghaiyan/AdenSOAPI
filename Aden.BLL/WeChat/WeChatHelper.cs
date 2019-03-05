using System;
using System.Collections.Generic;
using System.Linq;
using Aden.Util.Common;
using Aden.DAL.WeChatPay;
using Aden.Model;
using Aden.Model.WeChat;
using Aden.Model.Common;
using System.Data;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Drawing;


namespace Aden.BLL.WeChatPay
{
    public class WeChatPayHelper
    {
        /// <summary>
        /// 创建预支付订单
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static WeChatResult UnifiedOrder(WeChatInfo param, string appName)
        {
            WeChatPayFactory factory = new WeChatPayFactory(appName);
            try
            {
                if (param == null) throw new Exception("WeChatInfo is null");
                WeChatResult wxResult = factory.UnifiedOrder(param);
                //if (wxResult == null) throw new Exception("DAL.WeChat.WeChatPayFactory.UnifiedOrder()==0");
                return wxResult;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "UnifiedOrder");
                throw ex;
            }
        }

        /// <summary>
        /// 取得支付Sign
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string GetPaySign(WeChatResult param, string appName)
        {
            WeChatPayFactory factory = new WeChatPayFactory(appName);
            try
            {
                if (param == null) throw new Exception("Param is null");
                string strResult = factory.GetPaySign(param);
                if (string.IsNullOrWhiteSpace(strResult)) throw new Exception("DAL.WeChat.WeChatPayFactory.GetPaySign()==null");
                return strResult;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetPaySign");
                return string.Empty;
            }
        }

        /// <summary>
        /// 将小程序用户code转换成openid
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string GetOpenId(WeChatInfo param, string appName)
        {
            WeChatPayFactory factory = new WeChatPayFactory(appName);
            try
            {
                if (param == null) throw new Exception("Param is null");
                string strResult = factory.GetOpenId(param);
                if (string.IsNullOrWhiteSpace(strResult)) throw new Exception("DAL.WeChat.WeChatPayFactory.GetOpenId()==null");
                return strResult;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetOpenId");
                return string.Empty;
            }
        }


        /// <summary>
        /// 获取access_token
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static string GetAccessToken(string appName, string isNew = "")
        {
            WeChatPayFactory factory = new WeChatPayFactory(appName);
            try
            {
                if (string.IsNullOrWhiteSpace(appName)) throw new Exception("AppName is null");
                string strResult = factory.GetAccessToken(isNew);
                if (string.IsNullOrWhiteSpace(strResult)) throw new Exception("DAL.WeChat.WeChatPayFactory.GetAccessToken()==null");
                return strResult;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetAccessToken");
                return string.Empty;
            }
        }

        /// <summary>
        /// 取得小程序配置信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string GetWxPayConfig(SingleField param, string appName)
        {
            WeChatPayFactory factory = new WeChatPayFactory(appName);
            try
            {
                if (param == null) throw new Exception("Param is null");
                string value = factory.GetWxPayConfig(param);
                if (string.IsNullOrWhiteSpace(value)) throw new Exception("DAL.WeChat.WeChatPayFactory.GetWxPayConfig()==null");
                return value;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetWxPayConfig");
                return string.Empty;
            }
        }

        /// <summary>
        /// 验证支付回调XML中的签名是否正确
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static bool CheckSign(string xml, string appName)
        {
            WeChatPayFactory factory = new WeChatPayFactory(appName);
            try
            {
                if (string.IsNullOrWhiteSpace(xml)) throw new Exception("xml is null");
                bool result = factory.CheckSign(xml);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CheckSign");
                return false;
            }
        }

        /// <summary>
        /// 检查回执XML是否为SUCCESS
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static bool CheckIsSuccess(string xml, string appName)
        {
            WeChatPayFactory factory = new WeChatPayFactory(appName);
            try
            {
                if (string.IsNullOrEmpty(xml)) throw new Exception("xml is null");
                bool result = factory.CheckIsSuccess(xml);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "CheckIsSuccess");
                return false;
            }
        }

        /// <summary>
        /// 将XML转换成实体类
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static WxPayNotifyData GetWxPayNotifyData(string xml)
        {
            WeChatPayFactory factory = new WeChatPayFactory();
            try
            {
                if (string.IsNullOrEmpty(xml)) throw new Exception("xml is null");
                WxPayNotifyData wxData = factory.GetWxPayNotifyData(xml);
                return wxData;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetWxPayNotifyData");
                return null;
            }
        }

        /// <summary>
        /// 生成带参数的二维码
        /// </summary>
        /// <param name="data"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static byte[] CreateWxCode(Dictionary<string,string> dic)//string appName,string guid, string costCenterCode, string cardno, int barCodeSize)
        {
            try
            {
                WeChatPayFactory factory = new WeChatPayFactory(dic["appName"]);

                byte[] byteArray = factory.CreateWxCode(dic);// guid,costCenterCode, cardno, barCodeSize);
                
                return byteArray;

            }
            catch(Exception ex)
            {
                throw ex;
                
            }
            
        }
    }
}