using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;

namespace WxOrderTransfer
{
    class TransferData
    {
        static void Main(string[] args)
        {
            //Process[] vProcesses = Process.GetProcesses();

            //foreach (Process vProcess in vProcesses)
            //{
            //    if (Process.GetProcessesByName("WxOrderTransfer").Length > 1)
            //    {
            //        Environment.Exit(0);
            //    }
            //}

            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                Environment.Exit(0);

            string logStr = "";
                
            try
            {                
                Console.WriteLine(logStr);
                string s = Guid.NewGuid().ToString();
                logStr = string.Format("===批量导入开始({0})===\r\n",s);
                // 主逻辑
                logStr += Transfer.DoTransfer(s);
            }
            catch (Exception e)
            {
                logStr += "\r\n" + e.Message;
                Console.WriteLine(e.Message);
            }

            using (EventLog log = new EventLog())
            {
                log.Source = Process.GetCurrentProcess().ProcessName;
                log.WriteEntry(logStr);
                log.Close();
            }

        }
    }
}
