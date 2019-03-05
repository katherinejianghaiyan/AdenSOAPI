using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class SalesInvoiceParam
    {
        public string companyCode { get; set; }
        public string periodGuid { get; set; }

        public string userGuid { get; set; }

        public string userID { get; set; }

        public string status { get; set; }

        public string groupGuid { get; set; }
    }
}
