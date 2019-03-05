using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class SalesPeriodParam
    {
        public string companyCode { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public string userGuid { get; set; }

        public string userID { get; set; }

        public int addMonth { get; set; }
    }
}
