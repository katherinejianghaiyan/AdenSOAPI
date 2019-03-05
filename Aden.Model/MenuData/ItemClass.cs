using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.MenuData
{
    public class ItemClass
    {
        /// <summary>
        /// ID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 子集分类Guid
        /// </summary>
        public string guid { get; set; }
        /// <summary>
        /// 父集分类Guid
        /// </summary>
        public string pguid { get; set; }
        /// <summary>
        /// 子集分类名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 父集分类名称
        /// </summary>
        public string parentName { get; set; }
        /// <summary>
        /// 分类所属Group
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 语言所属Group
        /// </summary>
        public string nationGuid { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string sort { get; set; }

        public string itemCodeId { get; set; }

        public List<ItemClass> children { get; set; }
        public string value { get; set; }
        public string label { get; set; }
        public string disabled { get; set; }
        public string itemType { get; set; }
    }
}

