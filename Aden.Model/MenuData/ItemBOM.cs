using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.MenuData
{
    public class ItemBOM
    {
        public string costCenterCode { get; set; }
        public int step { get; set; }
        public string productGUID { get; set; }
        public string itemGuid { get; set; }
        public string itemCode { get; set; }
        public string itemName { get; set; }
        public string bomQty { get; set; }
        public string actQty { get; set; }
        public string effectPercent { get; set; }
        public string itemCost { get; set; }
        public string uomGuid { get; set; }
        public string itemDesc { get; set; }
        public string itemType { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public string purchasePolicy { get; set; }
        public string conversionUnit { get; set; }
        public decimal conversionRate { get; set; }
        public decimal purchasePrice { get; set; }
        public string purchaseUnit { get; set; }
        public decimal tax { get; set; }
        public string itemStatus { get; set; }
    }
}
