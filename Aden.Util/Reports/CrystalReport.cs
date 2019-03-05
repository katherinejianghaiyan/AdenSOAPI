using System;
using System.Linq;
using System.IO;
using System.Web;
using System.Data;
using System.Collections.Generic;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace Aden.Util.Reports
{
    public class CrystalReports
    {
        private ConnectionInfo connInfo = null;
        public ReportDocument rpt = null;

        public CrystalReports(string server, string database, string user, string password)
        {
            try
            {
                connInfo = new ConnectionInfo();
                connInfo.ServerName = server;
                connInfo.DatabaseName = database;
                connInfo.UserID = user;
                connInfo.Password = password;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public byte[] GetIMGbyte(string PicturePath)
        {

            //将需要存储的图片读取为数据流
            FileStream fs = new FileStream(Path.Combine(Path.Combine(HttpRuntime.AppDomainAppPath, "rpts"), @PicturePath),
                FileMode.Open, FileAccess.Read);
            Byte[] btye2 = new byte[fs.Length];
            fs.Read(btye2, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return btye2;
        }

        #region 生成PDF
        public void ExportPDF(string rptFile, Dictionary<string, string> keyDic)
        {
            try
            {
                string appPath = HttpRuntime.AppDomainAppPath;
                appPath = Path.Combine(appPath, "ToReports");
                string s = Path.Combine(appPath, "RPTs");
                rptFile = Path.Combine(s, rptFile);
                using (ReportDocument reportDocument = SetReport(rptFile, keyDic, null))
                {
                    //reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, @"e:\aaa.pdf");
                    reportDocument.ExportToHttpResponse(ExportFormatType.PortableDocFormat,
                        System.Web.HttpContext.Current.Response, false, "crReport");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        #endregion

        private ReportDocument SetReport(string rptFile, Dictionary<string, string> keyDic, DataSet ds)
        {

            try
            {
                //ReportDocument 
                rpt = new ReportDocument();

                rpt.Load(rptFile);


                if (ds != null)
                {
                    try
                    {
                        SetTables(rpt, ds);
                    }
                    catch { }

                    return rpt;
                }

                if (connInfo == null) return rpt;
                //动态设置数据库连接信息            
                foreach (Table table in rpt.Database.Tables)
                {
                    TableLogOnInfo logOnInfo = table.LogOnInfo;
                    logOnInfo.ConnectionInfo = connInfo;

                    table.ApplyLogOnInfo(logOnInfo);
                }

                if (keyDic == null) return rpt;

                //传入参数               

                SetParams(keyDic);
                return rpt;

            }
            catch (Exception e)
            {
                throw new Exception(string.Format("{0} ({1})",e.Message ,rptFile)) ;
            }
        }

        private void SetTables(ReportDocument rpt, DataSet ds)
        {
            try
            {
                foreach (ReportDocument subrpt in rpt.Subreports)
                    foreach (Table table in subrpt.Database.Tables)
                        table.SetDataSource(ds.Tables[table.Name.ToLower()]);

                foreach (Table table in rpt.Database.Tables)
                    table.SetDataSource(ds.Tables[table.Name.ToLower()]);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private void SetParams(Dictionary<string, string> keyDic)
        {
            try
            {
                if (keyDic == null || keyDic.Count == 0) throw new Exception("No parameters");
                foreach (ParameterField field in rpt.ParameterFields)
                {
                    if (!string.IsNullOrWhiteSpace(field.ReportName)) continue;
                    ParameterDiscreteValue parameter = new ParameterDiscreteValue();

                    string key = keyDic.Keys.Where(q => q.ToLower() == field.Name.Trim().ToLower()).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(key)) throw new Exception(string.Format("缺参数 ({0})", key));
                    object val = keyDic[key];
                    try
                    {
                        foreach (string s in (List<string>)val)
                        {
                            parameter.Value = s;
                            field.CurrentValues.Add(parameter);

                            if (!field.EnableAllowMultipleValue) break;
                        }

                        continue;
                    }
                    catch { }

                    try
                    {
                        parameter.Value = val;
                        field.CurrentValues.Add(parameter);
                    }
                    catch { }

                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
