using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Order.Warehouse
{
    public class ItemTrans
    {
        public int id { get; set; }
        public double sumInQty { get; set; }
        public double qty { get; set; }
        public double cost { get; set; }
    }
}
