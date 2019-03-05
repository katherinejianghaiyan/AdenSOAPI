using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aden.Model.SiteDIY
{
    public class ItemPrice
    {
        public string buGuid { get; set; }
        public string siteGuid { get; set; }
        public string itemGUID { get; set; }
        public string itemCode { get; set; }
        public string itemName_ZH { get; set; }
        public string itemName_EN { get; set; }
        public string container { get; set; }
        public int itemWeight { get; set; }
        public decimal itemPrice { get; set; }
        public string itemImage { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string oldSDate { get; set; }
        public DateTime createTime { get; set; }
    }
}