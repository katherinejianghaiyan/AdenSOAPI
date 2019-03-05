using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class WxUserCard
    {
        public int id { get; set; }

        public string wechatId { get; set; }

        public string cardId { get; set; }

        public string userCode { get; set; }

        public string userName { get; set; }

        public string createTime { get; set; }

        public string deleteTime { get; set; }
    }
}
