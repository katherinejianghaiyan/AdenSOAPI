using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.RightsData
{
    public class Items
    {
        public string costCenterCode { get; set; }
        public List<WindowOptions> windowOptions { get; set; }
        public List<ProductType> productType { get; set; }
        public List<ccWindowMeals> ccwindowMeals { get; set; }
        public Array mealType { get; set; }
    }
}
