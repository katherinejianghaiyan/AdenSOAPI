using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aden.Model.Upload
{
    public class UploadResponse
    {
        
            public string ResultUrl { get; set; }
            public string FileName { get; set; }
            public string Status { get; set; }
            public string Msg { get; set; }
        
    }   
}