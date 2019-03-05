using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.MenuOrder
{
    public class MenuOrderHead
    {
        public int id { get; set; }

        public string costCenterCode { get; set; }

        public string windowGuid { get; set; }

        public string requiredDate { get; set; }

        public string supplierCodes { get; set; }

        public string dayOfWeek { get; set; }

        public string soItemGuid { get; set; }

        public string productDesc { get; set; }

        public string itemGuid { get; set; }

        public string itemCode { get; set; }

        public string itemType { get; set; }

        public string itemName_ZH { get; set; }

        public string itemName_EN { get; set; }

        public decimal requiredQty { get; set; }

        public decimal itemCost { get; set; }

        public string itemUnitCode { get; set; }

        public decimal otherCost { get; set; }

        public string headGuid { get; set; }

        public string createTime { get; set; }

        public string createUser { get; set; }

        public string deleteTime { get; set; }

        public string deleteUser { get; set; }

        public string changeFlag { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public string dbName { get; set; }

        public string nationGuid { get; set; }

        public string nation { get; set; }

        public string menuClassGuid { get; set; }

        public string menuClassName { get; set; }

        public string classSort { get; set; }

        public int linkId { get; set; }

        public string productGuid { get; set; }

        public string purchasePolicy { get; set; }

        public string remark { get; set; }

        public string remark_bak { get; set; }

        public string hasAdj { get; set; }

        public string itemColor { get; set; }

        public string minRequiredDate { get; set; }

        public List<MenuOrderLine> lstMenuOrderLine { get; set; }

        public List<string> lstRequiredDate { get; set; }

    }
}
