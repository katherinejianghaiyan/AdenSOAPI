using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Net
{
    public class Response
    {
        /// <summary>
        /// 处理状态
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 处理消息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 处理内容
        /// </summary>
        public object content { get; set; }
    }
}
