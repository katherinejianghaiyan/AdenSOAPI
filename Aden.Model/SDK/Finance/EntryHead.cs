using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SDK.Finance
{
    public class EntryHead
    {    
        public double Amount { get; set; }
         
        public string CostUnit { get; set; }
       
        public string CostCenter { get; set; }
       
        public string CreditorNumber { get; set; }
         
        public string CurrencyCode { get; set; }
        
        public double CurrencyRate { get; set; }
        
        public string DebtorNumber { get; set; }
         
        public string Description { get; set; }
        
        public string EntryDate { get; set; }
         
        public string EntryNumber { get; set; }
         
        public string JournalNumber { get; set; }
         
        public string JournalType { get; set; }
         
        public double NumberField1 { get; set; }
         
        public double NumberField2 { get; set; }

        public string OurReference { get; set; }
         
        public string PaymentCondition { get; set; }
         
        public string PaymentReference { get; set; }

         
        public string PaymentMethod { get; set; }

         
        public string ReportingDate { get; set; }
         
        public string TextField1 { get; set; }

         
        public string TextField2 { get; set; }
         
        public string TextField3 { get; set; }

         
        public string YourReference { get; set; }

         
        public string FulfillmentDate { get; set; }

         
        public string TransactionType { get; set; }
    }
}
