using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Aden.Util.Database
{
    public class SqlDic
    {
        public string TableName { get; set; }
        public string Sql { get; set; }
        public SqlParameter[] Parameters { get; set; }
    }
}
