using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class SalesInvoice
    {
        public string processGuid { get; set; }

        public string processType { get; set; }

        public string periodGuid { get; set; }

        public string headGuid { get; set; }

        public string itemGuid { get; set; }

        public string groupGuid { get; set; }

        public string contract { get; set; }
        
        public string costCenterCode { get; set; }

        public string productCode { get; set; }

        public string productDesc { get; set; }

        public decimal qty { get; set; }

        public decimal price { get; set; }

        public decimal adjAmt { get; set; }

        public decimal finAdjAmt { get; set; }

        public string currCode { get; set; }

        public string taxCode { get; set; }

        public string priceUnitCode { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public string reportDate { get; set; }

        public string estimatedReportDate { get; set; }

        public string invoiceGuid { get; set; }

        public string invoiceNumber { get; set; }

        public string estimationGuid { get; set; }

        public string estimationNumber { get; set; }

        public string reverseEstimationGuid { get; set; }

        public string reverseEstimationNumber { get; set; }

        public string interSalesARGuid { get; set; }

        public string interSalesARNumber { get; set; }

        public string interSalesAPGuid { get; set; }

        public string interSalesAPNumber { get; set; }

        public bool toBeReversed { get; set; }

        public string AdjustEntryNumber { get; set; }

        public string createDate { get; set; }

        public string userGuid { get; set; }

        public string userID { get; set; }

        public string changeDate { get; set; }

        public string changeUserGuid { get; set; }

        public string changeUserID { get; set; }

        public int noTax { get; set; }

        public string crossCompInv { get; set; }

        public string status { get; set; }

        public string status_p { get; set; }

        public string complete { get; set; }

        public bool tobeInvoiced { get; set; }
    }
}
