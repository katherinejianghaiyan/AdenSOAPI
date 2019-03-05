using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Aden.Model.SOCommon
{
    public class CompanyAddress
    {
        /// <summary>
        /// 数据库代码
        /// </summary>
        public string erpCode { get; set; }

        /// <summary>
        /// 数据库IP地址
        /// </summary>
        public string ip { get; set; }

        public string parameters
        {
            set;get;
        }
        public Dictionary<string, string> filter
        {
            get
            {
                try
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    return jss.Deserialize<Dictionary<string, string>>(parameters);
                }
                catch { return null; }

            }

        }



    }
}
