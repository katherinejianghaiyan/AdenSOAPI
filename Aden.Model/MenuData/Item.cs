using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aden.Model.MenuData
{
    
    public class Item
    {
        public string itemGuid { get; set; }
        public string company { get; set; }
        public string itemCode { get; set; }
        public string itemName_ZH { get; set; }
        public string itemName_EN { get; set; }
        public string itemType { get; set; }
        public decimal weight { get; set; }
        public decimal itemCostperDish { get; set; }
        public string itemUnitCode { get; set; }
        public string version { get; set; }
        public string purchasePolicy { get; set; }
        public bool directBuy { get; set; }
        public bool addToMenu { get; set; }
        public string toBuy { get; set; }
        public string toSell { get; set; }
        public string categoriesClassGUID { get; set; }
        public string categoriesClass { get; set; }
        public string cookwayClassGUID { get; set; }
        public string seasonClassGUID { get; set; }
        public string itemColor { get; set; }
        public string itemTaste { get; set; }
        public int sort { get; set; }
        public string itemStatus { get; set; }
        public string createTime { get; set; }
        public string createUserGUID { get; set; }
        public string createUserName { get; set; }
        public string lastUpdateTime { get; set; }
        public string lastUpdateUserGUID { get; set; }
        public string image1 { get; set; }
        public string image2 { get; set; }
        public string image3 { get; set; }
        public string dishSize { get; set; }
        public string container { get; set; }
        public string purUOMGUID { get; set; }
        public string saleUOMGUID { get; set; }
        public string cost { get; set; }
        public decimal otherCost { get; set; }
        public string isDel { get; set; }
        public string langCode { get; set; }
        public string itemShapeGUID { get; set; }
        public string changeFlag { get; set; }
        public string menuClassGUID { get; set; }
        public string menuClassName { get; set; }
        public string menuClassSort { get; set; }
        public string nationGUID { get; set; }
        public string nation { get; set; }
        public string nationSort { get; set; }
        public string classSort { get; set; }
        public string uomGuid { get; set; }
        public string UOMName { get; set; }
        public string specification { get; set; }
        public string unitcode { get; set; }
        public string shareQty { get; set; }
        public string ItemCodeId { get; set; }
        public string validfrom { get; set; }
        public string validto { get; set; }
        public string itemBrand { get; set; }
        public decimal purchasePrice { get; set; }
        public decimal tax { get; set; }
        public string purchaseUnitcode { get; set; }
        public string conversionUnit { get; set; }
        public decimal conversionRate { get; set; }
        public string oldGUID { get; set; }
        public decimal price_low { get; set; }
        public decimal price_high { get; set; }
        public string keyWord { get; set; }
        public string itemClass { get; set; }
        public int supplierIndex { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public bool defaultSupplier { get; set; }
        public string suppliercheck { get; set; }
        public string costCenterCode { get; set; }
        public List<ItemBOM> ItemBOM { get; set; }
        public List<ItemProcess> ItemProcess { get; set; }
        public List<ItemNutrition> nutrition { get; set; }
        public List<ItemClass> nationClass { get; set; }
        public List<ItemClass> menuClass { get; set; }
        public List<ItemClass> rmClass { get; set; }
        public List<ItemClass> cookwayClass { get; set; }
        public List<ItemClass> seasonClass { get; set; }
        public List<ItemClass> ItemShape { get; set; }
        public List<ItemColor> itemColorOption { get; set; }
        public List<Item> item { get; set; }
        public List<Item> suppliers { get; set; }
        public List<paramType> paramType { get; set; }
        public string itemValidity { get; set; }
        public int itemSpicy { get; set; }
    }
}
