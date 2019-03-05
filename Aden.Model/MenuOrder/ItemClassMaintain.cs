using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Model.MenuData;

namespace Aden.Model.MenuOrder
{
    public class ItemClassMaintain
    {
        // 寄存ItemClass主数据
        public List<ItemClassHierarchy> lstItemClass_x { get; set; }
        // 后续自定义ItemClass主数据
        public List<ItemClass> lstItemClass_y { get; set; }
        // 已匹配好的数据
        public List<tblDataMapping> lstTblDataMapping { get; set; }
    }
}
