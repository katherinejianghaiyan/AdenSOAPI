using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class UserBalance
    {
        public string userName { get; set; }

        public decimal cardCurrAmt { get; set; }

        public WxOrder lastOrder { get; set; }
    }
}
