using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class ReadyToImportErp
    {
        public string processType { get; set; }

        public string userGuid { get; set; }

        public string userID { get; set; }

        public List<SalesPeriod> periodLine { get; set; }
    }
}
