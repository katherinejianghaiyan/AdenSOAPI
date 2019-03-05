using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MenuOrder
{
    public class MealTypeParam
    {
        public string costCenterCode { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public List<Common.SingleField> lstItemGuid { get; set; }
    }
}
