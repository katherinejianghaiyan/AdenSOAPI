using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MenuOrder
{
    public class MenuOrderPurchaseParam
    {
        public List<MenuOrderLine> lstMenuPart { get; set; }

        public List<MenuOrderLine> lstPurchasePart { get; set; }

        public MenuOrderHead menuPart { get; set; }

        public MenuOrderHead purchasePart { get; set; }
    }
}
