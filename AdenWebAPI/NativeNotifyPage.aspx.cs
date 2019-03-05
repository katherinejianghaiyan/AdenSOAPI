using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;
using Aden.BLL.WeChatPay;
using Aden.BLL.WeChat;
using Aden.Model.WeChat;
using System.Diagnostics;

namespace AdenWebAPI
{
    public partial class NativeNotifyPage : System.Web.UI.Page
    {
        public static string wxJsApiParam { get; set; } //前段显示
        public string return_result = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            //LogHelper.WriteLog(typeof(NativeNotifyPage), "可以运行1-1");

            String xmlData = getPostStr();//获取请求数据

            if (!string.IsNullOrWhiteSpace(xmlData))
            {
                #region Step1.先创建回执XML start
                var dic = new Dictionary<string, string>
                {
                  {"return_code", "SUCCESS"},
                  {"return_msg","OK"}
                };
                var sb = new StringBuilder();
                sb.Append("<xml>");

                foreach (var d in dic)
                {
                    sb.Append("<" + d.Key + ">" + d.Value + "</" + d.Key + ">");
                }
                sb.Append("</xml>");
                #endregion

                /***将xml转换成实体类***/
                WxPayNotifyData wxData = WeChatPayHelper.GetWxPayNotifyData(xmlData);

                #region step2.检查XML是否合法
                if (WeChatPayHelper.CheckSign(xmlData, wxData.attch))
                {
                    // 创建回写订单的实体类
                    WxOrder order = new WxOrder();
                    // 取得PaymentCode长度
                    int len = 0;
                    WeChatConfig wxc = WxMastDataHelper.GetWeChatConfig(wxData.attch);
                    if (wxc != null && !string.IsNullOrWhiteSpace(wxc.paymentCode))
                        len = wxc.paymentCode.Length;

                    order.appName = wxData.attch;
                    order.out_trade_no = wxData.out_trade_no.Substring(len, wxData.out_trade_no.Length - len);
                    order.transaction_id = wxData.transaction_id;
                    order.cash_fee = wxData.cash_fee;
                    order.fee_type = wxData.fee_type;
                    order.bank_type = wxData.bank_type;
                    order.time_end = wxData.time_end;
                    order.result_code = wxData.result_code;
                    order.return_code = wxData.return_code;

                    // 执行回写
                    if (WxOrderHelper.UpdatePayOrder(order) > 0)
                    {
                        // 调用数据导入Exe
                        this.DoExe();
                        // 回写成功时返回SUCCESS
                        return_result = sb.ToString();
                    }
                }
                #endregion
            }
            Response.Write(return_result);
            Response.End();
        }

        // 获得Post过来的数据  
        private string getPostStr()
        {
            Int32 intLen = Convert.ToInt32(System.Web.HttpContext.Current.Request.InputStream.Length);
            byte[] b = new byte[intLen];
            System.Web.HttpContext.Current.Request.InputStream.Read(b, 0, intLen);
            return System.Text.Encoding.UTF8.GetString(b);
        }

        // 调用数据导入Exe
        private void DoExe()
        {
            //Process process = new Process();
            //process.StartInfo.FileName = "D:\\MyProject\\VS2017\\AdenWebAPI\\WxOrderTransfer\\bin\\Debug\\WxOrderTransfer.exe";
            string transferAppUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["transferAppUrl"];
            Process.Start(@transferAppUrl);
            //Process.Start("WxOrderTransfer.exe");
        }
    }
}