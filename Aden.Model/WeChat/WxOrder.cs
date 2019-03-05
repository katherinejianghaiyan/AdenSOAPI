using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.WeChat
{
    public class WxOrder
    {
        public int id { get; set; }

        public string appName { get; set; }

        public string type { get; set; }

        public string cardId { get; set; }

        public string out_trade_no { get; set; }

        public string transaction_id { get; set; }

        public string openid { get; set; }

        public string attach { get; set; }

        public List<Coupon> coupons { get; set; }
        public decimal total_fee { get; set; }

        public int cash_fee { get; set; }

        public string fee_type { get; set; }

        public string bank_type { get; set; }

        public string time_end { get; set; }

        public string time_start { get; set; }

        public string time_expire { get; set; }

        public string result_code { get; set; }

        public string return_code { get; set; }

        public string createTime { get; set; }

        public string notifyTime { get; set; }

        public string transferGuid { get; set; }

        public string transferTime { get; set; }

        public string costCenterCode { get; set; }
    }

    public class Coupon
    {
        public int price { get; set; }
        public int qty { get; set; }
    }
}
