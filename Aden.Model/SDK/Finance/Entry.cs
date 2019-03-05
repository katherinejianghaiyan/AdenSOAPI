using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.SDK.Finance
{
    public class Entry
    {
        public EntryHead Head { get; set; }
        public List<EntryLine> Lines { get; set; }
    }
}
