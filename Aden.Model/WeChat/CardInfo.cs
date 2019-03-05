using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Model.Common;

namespace Aden.Model.WeChat
{
    public class CardInfo
    {
        public string wechatId { get; set; }

        public string cardId { get; set; }

        public string cardCode { get; set; }

        public string oldCardId { get; set; }
        public string userCode { get; set; }

        public string userName { get; set; }

        // 用于标记当前卡是否已绑定
        public bool isBind { get; set; }
    }
}