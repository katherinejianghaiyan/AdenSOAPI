using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class SalesInvoiceSaveParam
    {
        public string userGuid { get; set; }

        public string userID { get; set; }

        public bool split { get; set; }

        //public string status { get; set; }

        public List<SalesInvoice> lines { get; set; }
    }
}
