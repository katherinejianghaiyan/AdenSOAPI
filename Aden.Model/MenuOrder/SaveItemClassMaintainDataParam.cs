using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MenuOrder
{
    public class SaveItemClassMaintainDataParam
    {
        public string dbCode { get; set; }

        public string userId { get; set; }

        public List<tblDataMapping> lstTalDataMapping { get; set; }
    }
}
