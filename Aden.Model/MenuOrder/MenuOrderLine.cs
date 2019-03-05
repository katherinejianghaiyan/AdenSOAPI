using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Model.MenuData;

namespace Aden.Model.MenuOrder
{
    public class MenuOrderLine
    {
        public int id { get; set; }

        public int sortId { get; set; }

        public string headGuid { get; set; }

        public string lineGuid { get; set; }

        public string itemCode { get; set; }

        public string itemName { get; set; }

        public string itemDesc { get; set; }

        public string itemDesc_bak { get; set; }

        public string itemType { get; set; }

        public decimal bomQty { get; set; }

        public decimal actQty { get; set; }

        public decimal actQty_bak { get; set; }

        public string supplierCode { get; set; }

        public string supplierCode_bak { get; set; }

        public string supplierName { get; set; }

        public string supplierCode_new { get; set; }

        public string supplierName_new { get; set; }

        public List<Item> suppliers { get; set; }

        public string conversionUnit { get; set; }

        public decimal conversionRate { get; set; }

        public string conversionUnit_new { get; set; }

        public decimal conversionRate_new { get; set; }

        public decimal purchasePrice { get; set; }

        public string purchaseUnit { get; set; }

        public decimal purchaseTax { get; set; }

        public decimal tax { get; set; }

        public decimal purchasePrice_new { get; set; }

        public string purchaseUnit_new { get; set; }

        public decimal purchaseTax_new { get; set; }

        public decimal requiredQty { get; set; }

        public decimal requiredQty_bak { get; set; }

        public decimal adjRequiredQty { get; set; }

        public decimal adjRequiredQty_bak { get; set; }

        public string remark { get; set; }

        public string remark_bak { get; set; }

        public string remark_new { get; set; }

        // 仅用于画面传值
        public string productGUID { get; set; }
        // 仅用于画面传值
        public string purchasePolicy { get; set; }
        // 数量调整记录（用于前端判断是否显示）
        public string adjFlag { get; set; }

        public string createUser { get; set; }

        public string createTime { get; set; }

        public string deleteUser { get; set; }

        public string deleteTime { get; set; }

        public string changeFlag { get; set; }

        public string headRequiredDate { get; set; }

        public string workRequiredDate { get; set; }

        public string requiredDate { get; set; }

        public string requiredDate_bak { get; set; }

        public string hasAdj { get; set; }

        public string soItemGuid { get; set; }
    }
}
