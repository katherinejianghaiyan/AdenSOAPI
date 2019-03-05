using System;
using CSSDKConnect;

namespace Aden.SDK
{
    public class ConnectHelper
    {
       // private SDKConnect _connect;
        public SDKConnection connection;
        private string _errMsg;

        public string errMsg
        {
            get { return _errMsg; }
        }
        
        /// <summary>
        /// 打开SDK连接,程序自动使用当前调用程序的用户名判断登录权限
        /// </summary>
        /// <param name="ip">连接的IP地址</param>
        /// <param name="dataBaseName">连接的数据库</param>
        /// <returns></returns>
        public bool Open(string ip, string dataBaseName)
        {
            Close();
            _connect = new SDKConnect();
            SDKConnectionErrors sdkErrs = _connect.ConnectDirect(ip, dataBaseName, ref connection);
            if (sdkErrs.Equals(SDKConnectionErrors.ConnNoError)) return true;
            _errMsg = sdkErrs.ToString();
            return false;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (_connect != null)
            {
                try { _connect.Disconnect(ref connection); }
                catch { }
                _connect = null;
            }
        }
    }
}
