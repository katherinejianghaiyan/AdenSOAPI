using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Util.Common
{
    public static class GuidHelper
    {
        /// <summary>
        /// 返回指定格式的Guid值
        /// </summary>
        /// <param name="format">格式(N,D,B,P,X)</param>
        /// <returns></returns>
        public static string GetGuid(string format = "N")
        {
            return new Guid().ToString(format);
        }
    }
}
