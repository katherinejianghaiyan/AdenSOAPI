using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class WeChatInfo
    {
        public string appName { get; set; }

        public string type { get; set; }

        public string code { get; set; }

        public string openId { get; set; }

        public UserInfo userInfo { get; set; }

        public string signature { get; set; }

        public string encryptedData { get; set; }

        public string iv { get; set; }

        public int total_fee { get; set; }
        public List<string[]> coupons { get; set; }

        public string costCenterCode { get; set; }

        public string costCenterName { get; set; }

        public string cardno { get; set; }

        public string cardId { get; set; }

        public int barCodeSize { get; set; }

        public string rechargeSetting { get; set; }
    }
}
