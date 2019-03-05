using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.RightsData
{
    public class ProductType
    {
        public string costCenterCode { get; set; }
        public string itemGuid { get; set; }
        public string productCode { get; set; }
        public string productDesc { get; set; }
        public string windowGuid { get; set; }
        public Boolean status { get; set; }
        public Array soItemGuid { get; set; }
        public Array validwd { get; set; }
        public List<ProductType> validItem { get; set; }
    }
}
