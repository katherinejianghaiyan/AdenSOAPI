using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.RightsData
{
    public class Param
    {
      public string costCenterCode { get; set; }
      public string userId { get; set; }
      public List<WindowOptions> windowOptions { get; set; }
      public List<ccWindowMeals> ccwindowMeals { get; set; } 
    }
}
