using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Aden.Model;
using Aden.Util.Common;
using System.Configuration;


namespace Aden.BLL
{
    public static class LogHelper
    {
        public static string logPath = ConfigurationManager.AppSettings["logPath"].ToString();
        public static void WriteLog(Log log, string fileName = "")
        {
            try
            {
                var path = Path.Combine(logPath, fileName);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = Path.Combine(path, log.logDate.ToDateTimeInt().ToString());
                StringBuilder str = new StringBuilder(log.message);
                if (log.details != null) str.Append("\n" + log.details.SerializeJsonString());
                FileHelper.Write(path, str.ToString());
            }
            catch { }  
        }
    }
}
