using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aden.Model.MenuData
{
    public class WeeklyMenu
    {
        public int id { get; set; }
        public string costCenterCode { get; set; }
        public string siteGuid { get; set; }
        public string mealDate { get; set; }
        public string mealCode { get; set; }
        public string windowType { get; set; }
        public string foodNames { get; set; }
        public string foodNames1 { get; set; }
        public string foodNames2 { get; set; }
        public string foodNames3 { get; set; }
        public string foodNames4 { get; set; }
        public string foodNames5 { get; set; }
        public string foodNames6 { get; set; }
        public string foodNames7 { get; set; }
        public string createTime { get; set; }
        public string createUser { get; set; }
        public string deleteTime { get; set; }
        public string deleteUser { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string dayOfWeek { get; set; }
        public string sort { get; set; }
        public List<dynamic> mealTypeList { get; set; }
        public List<WeeklyMenu> menuOrderHeadObj { get; set; }
    }
}