using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.Purchase
{
    public class PurchaseOrderLineDetail
    {
        public int id { get; set; }

        public string lineGuid { get; set; }

        public string costUnit { get; set; }

        public decimal qty { get; set; }

        public decimal qty_back { get; set; }

        public decimal price { get; set; }

        public string createUser { get; set; }

        public string createDate { get; set; }

        public string deleteUser { get; set; }

        public string deleteDate { get; set; }

        public decimal totalPrice { get; set; }
    }
}
