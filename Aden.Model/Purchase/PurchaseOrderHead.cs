using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.Purchase
{
    public class PurchaseOrderHead
    {
        public string headGuid { get; set; }

        public string orderDate { get; set; }

        public string orderDescription { get; set; }

        public string orderMethod { get; set; }

        public string dbCode { get; set; }

        public string costCenterCode { get; set; }

        public string createUser { get; set; }

        public string createDate { get; set; }

        public string userId { get; set; }

        public string employee { get; set; }

        public string warehouseCode { get; set; }

        public List<PurchaseOrderLine> lstPurchaseOrderLine { get; set; }
    }
}
