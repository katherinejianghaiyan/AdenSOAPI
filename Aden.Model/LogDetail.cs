using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model
{
    /// <summary>
    /// 处理日志明细
    /// </summary>
    public class LogDetail
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 日志消息
        /// </summary>
        public string message { get; set; }

    }
}
