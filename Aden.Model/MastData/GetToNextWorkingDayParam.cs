using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MastData
{
    public class GetToNextWorkingDayParam
    {
        public string date { get; set; }

        public string costCenterCode { get; set; }

        public string lang { get; set; }

        public string formatString { get; set; }
    }
}
