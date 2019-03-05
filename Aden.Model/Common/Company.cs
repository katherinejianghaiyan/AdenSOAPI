using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Common
{
    public class Company
    {
        /// <summary>
        /// 公司代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 唯一识别号GUID
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string companyName { get; set; }

        public List<Aden.Model.SOMastData.Company> lines { get; set; }
    }
}
