using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.DAL.WeChatPay;
using Aden.Model;
using Aden.Model.WeChat;

namespace Aden.BLL.WeChat
{
    public class WxOrderHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static WxOrderFactory wxf = new WxOrderFactory();

        public static int UpdatePayOrder(WxOrder param)
        {
            try
            {
                if (param == null) throw new Exception("WxOrder is null");
                int result = wxf.UpdatePayOrder(param);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "UpdatePayOrder");
                return -1;
            }
        }
    }
}
