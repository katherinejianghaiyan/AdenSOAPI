using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.WeChatPay;
using Aden.Model.WeChat;
using Aden.Model;

namespace Aden.BLL.WeChat
{
    public class WxMastDataHelper
    {
        public static WeChatConfig GetWeChatConfig(string appName)
        {
            WxMastDataFactory factory = new WxMastDataFactory();
            try
            {
                if (string.IsNullOrWhiteSpace(appName)) throw new Exception("Param is null");
                WeChatConfig wxc = factory.GetWeChatConfig(appName);
                if (wxc == null) throw new Exception("DAL.WeChat.WeChatPayFactory.GetPaySign()==null");
                return wxc;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetWeChatConfig");
                return null;
            }
        }
    }
}
