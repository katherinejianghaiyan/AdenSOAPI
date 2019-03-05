using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class WxAccessToken
    {
        public string appName { get; set; }

        public string access_token { get; set; }

        public string expires_in { get; set; }

        public DateTime updateTime { get; set; }
    }
}
