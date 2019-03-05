using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOAccount
{
    public class AccountUser
    {
        public string userGuid { get; set; }

        public string account { get; set; }

        public string firstName { get; set; }

        public string lastName { get; set; }

        public string fullName { get; set; }

        public string employeeID { get; set; }

        public string mail { get; set; }

        public string mobile { get; set; }

        public string langCode { get; set; }

        public List<Aden.Model.Account.AccountMenu> menus { get; set; }
    }
}
