using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class MenuAction
    {
        public string Company { get; set; }
        public string userGuid { get; set; }
        public string menuGuid { get; set; }
        public int Action { get; set; }
        public string description { get; set; }
        public bool CanDelete { get; set; }
        public bool CanChange { get; set; }
    }
}
