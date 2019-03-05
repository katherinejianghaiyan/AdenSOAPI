using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MastData
{
    public class SqlMast
    {
        public string sqlType { get; set; }

        public string sqlCommand { get; set; }

        public string sqlParams { get; set; }

        public string costCenterCodes { get; set; }
    }
}
