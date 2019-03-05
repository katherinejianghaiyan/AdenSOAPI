using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.MenuOrder
{
    public class CCWindows
    {
        public int id { get; set; }

        public string costCenterCode { get; set; }

        public string windowGuid { get; set; }

        public string windowName { get; set; }

        public string value { get; set; }

        public string label { get; set; }

        public List<CCWindowMeals> lstCCWindowMeals { get; set; }
    }
}
