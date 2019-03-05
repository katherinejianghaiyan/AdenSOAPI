using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aden.Model.WeChat;
using Aden.Util.Database;

namespace Aden.DAL.WeChatPay
{
    public class WxMastDataFactory
    {
        public WeChatConfig GetWeChatConfig(string appName)
        {
            string strSql = " SELECT * " +
                  " FROM TBLWECHATCONFIG " +
                 " WHERE APPNAME = '{0}' ";

            WeChatConfig wxc = SqlServerHelper.GetEntity<WeChatConfig>(SqlServerHelper.salesorderConn(), string.Format(strSql, appName));

            return wxc;
        }
    }
}
