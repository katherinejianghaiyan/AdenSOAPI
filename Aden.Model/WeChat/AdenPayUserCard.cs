using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class AdenPayUserCard
    {
        public string userGuid { get; set; }

        public string cardNumber { get; set; }

        public string cardUserName { get; set; }

        public string clientSiteName { get; set; }

        public string clientAreaName { get; set; }

        public bool bit { get; set; }

        public bool canCharge { get; set; }

        public string wechatId { get; set; }
    }
}
