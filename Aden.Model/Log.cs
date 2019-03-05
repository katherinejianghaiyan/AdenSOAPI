using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model
{
    /// <summary>
    /// 日志
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 日志日期
        /// </summary>
        public DateTime logDate
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 日志消息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 日志明细
        /// </summary>
        public IList<LogDetail> details { get; set; }
    }
}
