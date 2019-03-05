using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.AssignUserRights
{
    public class UserRights
    {
        public string userGuid { get; set; }
        public string userName { get; set; }
        public string menuGuid { get; set; }
        public string menuName { get; set; }
        public string companyCode { get; set; }
        public string companyName { get; set; }
        public Boolean check {get;set;}
        public List<UserRights> userComp { get; set; }
        public List<UserRights> compSource { get; set; }

    }
}
