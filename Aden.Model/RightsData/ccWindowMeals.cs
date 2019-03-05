using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.RightsData
{
    public class ccWindowMeals
    {
        public string value { get; set; }
        public string costCenterCode { get; set; }
        public string windowGuid { get; set; }
        public string prewindowGuid { get; set; }
        public string windowName { get; set; }
        public string itemGuid { get; set; }
        public string productCode { get; set; }
        public string productDesc { get; set; }
        public List<ProductType> mealStatus { get; set; }
        public int sort { get; set; }
    }
}
