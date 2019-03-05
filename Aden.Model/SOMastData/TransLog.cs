using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SOMastData
{
    public class TransLog
    {
        public int id { get; set; }
        public string tableName { get; set; }
        public string guid { get; set; }
        public string funcName { get; set; }
        public string errorMessage { get; set; }
        public string dateTime { get; set; }
    }
}
