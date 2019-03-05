using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.SOMastData
{
    public class LangVersion
    {
        public string appName { get; set; }

        public string versionGuid { get; set; }

        public List<LangMast> lstLangMast { get; set; }
    }
}
