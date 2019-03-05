using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MenuOrder
{
    public class SaveMenuOrderParam
    {
        public string userId { get; set; }

        public List<MenuOrderHead> lstMenuOrderHead { get; set; }
    }
}