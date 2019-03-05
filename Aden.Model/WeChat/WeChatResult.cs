using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class WeChatResult
    {
        public string prepay_id { get; set; }

        public string paySign { get; set; }

        public string nonceStr { get; set; }

        public string timeStamp { get; set; }

        public bool isMantain { get; set; }

        public bool matched { get; set; }
    }
}
