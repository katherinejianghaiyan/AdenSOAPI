using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Data;
using Aspose.Cells;
using System.Configuration;

using Aden.Util.Database;
using Aden.Util.Reports;

namespace Aden.Util.Reports
{
    public class ExcelReport
    {
        public ExcelReport ()
        {
            try { }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void DownloadExcelFile(MemoryStream ms)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.Clear();
            response.Buffer = true;
            response.ContentType = "Application/ms-excel";
            response.AddHeader("Content-Disposition", "attachment; filename=report.xls");
            response.ContentEncoding = Encoding.Default;
            response.BinaryWrite(ms.ToArray());
            response.Flush();
            response.End();
        }
    }
}
