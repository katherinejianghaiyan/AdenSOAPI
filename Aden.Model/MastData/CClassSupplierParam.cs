using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MastData
{
    public class CClassSupplierParam
    {
        public string userId { get; set; }

        public string dbName { get; set; }

        public List<dynamic> lstSupplierOption { get; set; }

        public List<MenuData.ItemClass> lstItemClass { get; set; }

        public List<CClassSupplier> lstCClassSupplier { get; set; }
    }
}
