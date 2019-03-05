using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.Account
{
    public class AccountMenu
    {
        /// <summary>
        /// User Guid
        /// </summary>
        public string userGuid { get; set; }
        /// <summary>
        /// 菜单index
        /// </summary>
        public string index { get; set; }

        /// <summary>
        /// 菜单ID
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// 菜单名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 执行动作(路径)
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        public string iconCls { get; set; }

        public string action { get; set; }

        /// <summary>
        /// 子菜单
        /// </summary>
        public List<AccountMenu> childs { get; set; }

    }
}
