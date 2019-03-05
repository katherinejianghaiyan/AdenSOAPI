using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class WxUserMast
    {
        public int id { get; set; }

        public string appName { get; set; }

        public string wechatId { get; set; }

        public string costCenterCode { get; set; }

        public string nickName { get; set; }

        public string nickNameEncode { get; set; }

        public string gender { get; set; }

        public string language { get; set; }

        public string city { get; set; }

        public string province { get; set; }

        public bool isTestUser { get; set; }

        public string createTime { get; set; }

        public string deleteTime { get; set; }
    }
}
