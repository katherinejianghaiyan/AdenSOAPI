using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using Aden.Util.Database;
using Aden.Util.Reports;
using System.IO;

namespace Aden.BLL.Reports
{
    public class ReportHelper
    {
        public static void ShowPDF(string reportName, string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportName))
                    throw new Exception("请明确报表名");

                Dictionary<string,string> keyDic = 
                    Aden.Util.Common.JsonHelper.JSDeserialize<Dictionary<string, string>>(data);                

                string sql = "select * from tblcrystalreports where reportname='{0}'";
                sql = string.Format(sql, reportName);
                DataTable dt = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
                if (dt == null || dt.Rows.Count == 0) throw new Exception(string.Format("报表 ({0}) 的配置信息", reportName));

                (new Aden.Util.Reports.CrystalReports(dt.Rows[0]["ServerIP"].ToString(), dt.Rows[0]["DatabaseName"].ToString(),
                    dt.Rows[0]["UserName"].ToString(), dt.Rows[0]["Password"].ToString()))
                    .ExportPDF(dt.Rows[0]["FileName"].ToString(), keyDic);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static void ShowExcel(string data)
        {
            try
            {
                Dictionary<string, string> keyDic =
                    Aden.Util.Common.JsonHelper.JSDeserialize<Dictionary<string, string>>(data);

                if (keyDic["reportType"] == "weeklyMenu")
                    new Aden.Util.Reports.ExcelReport().DownloadExcelFile
                        (Aden.DAL.ReportLib.DownloadReport.
                        ExportExcelWeeklyMenu(keyDic));
                else if (keyDic["reportType"] == "menuProcess")
                    new Aden.Util.Reports.ExcelReport().DownloadExcelFile
                        (Aden.DAL.ReportLib.DownloadReport.
                        ExportExcelMenuProcess(keyDic));
                else if (keyDic["reportType"] == "wxRechargeReport")
                    new Aden.Util.Reports.ExcelReport().DownloadExcelFile
                        (Aden.DAL.ReportLib.DownloadReport.
                        WechatRechargeReport(keyDic));
                else if (keyDic["reportType"] == "mailweeklyMenu")
                    new Aden.Util.Reports.MailHelper().SendMail(keyDic["sendType"], keyDic["recipients"], (Aden.DAL.ReportLib.DownloadReport.
                    ExportExcelWeeklyMenu(keyDic)));
                else if (keyDic["reportType"] == "SiteSurveyReport")
                    new Aden.Util.Reports.ExcelReport().DownloadExcelFile
                        (Aden.DAL.ReportLib.DownloadReport.
                        SiteSurveyReport(keyDic));
                     
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
