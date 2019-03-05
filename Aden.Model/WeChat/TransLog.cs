using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class TransLog
    {
        public string year { get; set; }

        public string month { get; set; }

        public string transDate { get; set; }

        public string transTime { get; set; }

        public string transType { get; set; }

        public decimal amount { get; set; }
    }
}
