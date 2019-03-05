using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.IO;
using Aden.Util.Common;
using Aden.Util.Database;

namespace Aden.Util.Reports
{
    public class MailHelper
    {
        public void SendMail(string sendType, string recipients, MemoryStream ms)
        {
            try
            {
                string sql = "select * from tblmastdata (nolock) where datatype in ('smtpserver','{0}') and active=1";
                sql = string.Format(sql, sendType);
                DataTable dt = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
                if (dt == null || dt.Rows.Count < 2) throw new Exception("No smtp or setting");

                DataRow dr = dt.Select("datatype='smtpserver'")[0];
                string fromAddr = dr["val3"].ToStringTrim();
                //创建一个身份凭证，即发送邮件的用户名和密码
                NetworkCredential credential = new NetworkCredential(fromAddr, dr["val4"].ToStringTrim());

                //发送邮件的实例，服务器和端口
                SmtpClient client = new SmtpClient(dr["val1"].ToStringTrim(), dr["val2"].ToInt());
                //发送邮件的方式，通过网络发送
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //是否启用 SSL 
                client.EnableSsl = client.Port != 25;
                //指定发送邮件的身份凭证
                client.Credentials = credential;

                dr = dt.Select(string.Format("datatype='{0}'", sendType))[0];
                //发送的邮件信息
                MailMessage mailmsg = new MailMessage();

                // 指定发件人邮箱和显示的发件人名称
                mailmsg.From = new MailAddress(fromAddr, "ADEN");

                // 指定收件人邮箱
                foreach (string s in recipients.Split(';'))
                {
                    MailAddress mailto = new MailAddress(s);
                    mailmsg.To.Add(mailto);
                }
                
                //邮件主题
                mailmsg.Subject = dr["val1"].ToStringTrim();
                mailmsg.SubjectEncoding = Encoding.UTF8;

                //邮件内容
                mailmsg.Body = dr["val2"].ToStringTrim();
                mailmsg.BodyEncoding = Encoding.UTF8;

                //添加附件
                ms.Position = 0;
                mailmsg.Attachments.Add(new Attachment(ms, dr["val3"].ToStringTrim()));
                ms.Flush();

                client.Send(mailmsg);   // 发送邮件
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
