using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MenuOrder
{
    public class tblDataMapping
    {
        public int id { get; set; }

        public string type { get; set; }

        public string dbCode { get; set; }

        public string costCenterCode { get; set; }

        public string code1 { get; set; }

        public string name1 { get; set; }

        public string code2 { get; set; }

        public string name2 { get; set; }

        public string sort { get; set; }

        public string createTime { get; set; }

        public string createUser { get; set; }

        public string deleteTime { get; set; }

        public string deleteUser { get; set; }

        public string p_code { get; set; }

        public string delFlag { get; set; }
    }
}
