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

namespace Aden.Util.img
{
    public class ImageExport
    {
        public ImageExport()
        {
            try { }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public void ImageResponse(byte[] byteArray)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.Clear();
            response.Buffer = true;
            response.ContentType = "image/jpg";
            //response.AddHeader("Content-Disposition", "attachment; filename=picture.jpg");
            //response.ContentEncoding = Encoding.Default;
            response.BinaryWrite(byteArray);
            //response.Flush();
            //response.End();
        }

    }
}
