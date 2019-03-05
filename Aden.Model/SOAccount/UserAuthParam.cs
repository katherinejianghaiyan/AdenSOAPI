using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.SOAccount
{
    public class UserAuthParam
    {
        public string userGuid { get; set; }

        public string action { get; set; }

        public string fieldName { get; set; }

        public bool recursion { get; set; }
    }
}
