using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MastData
{
    public class DataLog
    {
        public string tableName { get; set; }
        public string fieldName { get; set; }
        public string recordId { get; set; }
        public string oldVal { get; set; }
        public string newVal { get; set; }
        public DateTime createTime { get; set; }

    }
}
