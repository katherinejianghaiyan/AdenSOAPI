using System;
using System.Collections.Generic;
using Aden.Model.MenuData;

namespace Aden.Model.MenuOrder
{
    public class POItem
    {
        /// <summary>
        /// 排序ID
        /// </summary>
        public int itemsort{ get; set; }
        /// <summary>
        /// 原料Code
        /// </summary>
        public string itemCode { get; set; }
        /// <summary>
        /// 原料名称
        /// </summary>
        public string itemName { get; set; }
        /// <summary>
        /// 原料别名
        /// </summary>
        public string itemDesc { get; set; }
        /// <summary>
        /// 原料需求数
        /// </summary>
        public decimal requiredQty { get; set; }
        /// <summary>
        /// 供应商Code
        /// </summary>
        public string supplierCode { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string supplierName { get; set; }
        /// <summary>
        /// 供应商Code备份
        /// </summary>
        public string supplierCode_bak { get; set; }
        /// <summary>
        /// 供应商名称备份
        /// </summary>
        public string supplierName_bak { get; set; }
        /// <summary>
        /// 采购单价
        /// </summary>
        public decimal purchasePrice { get; set; }
        /// <summary>
        /// 采购单价（单位）
        /// </summary>
        public string purchaseUnit { get; set; }
        /// <summary>
        /// 采购单价备份
        /// </summary>
        public decimal purchasePrice_bak { get; set; }
        /// <summary>
        /// 采购单价（单位）备份
        /// </summary>
        public string purchaseUnit_bak { get; set; }
        /// <summary>
        /// 转换系数
        /// </summary>
        public decimal conversionRate { get; set; }
        /// <summary>
        /// 转换单位
        /// </summary>
        public string conversionUnit { get; set; }
        /// <summary>
        /// 转换系数备份
        /// </summary>
        public decimal conversionRate_bak { get; set; }
        /// <summary>
        /// 转换单位备份
        /// </summary>
        public string conversionUnit_bak { get; set; }
        /// <summary>
        /// 当天数量合计
        /// </summary>
        public decimal daylyQty { get; set; }
        /// <summary>
        /// 周1数量合计
        /// </summary>
        public decimal monQty { get; set; }
        /// <summary>
        /// 周2数量合计
        /// </summary>
        public decimal tueQty { get; set; }
        /// <summary>
        /// 周3数量合计
        /// </summary>
        public decimal wedQty { get; set; }
        /// <summary>
        /// 周4数量合计
        /// </summary>
        public decimal thuQty { get; set; }
        /// <summary>
        /// 周5数量合计
        /// </summary>
        public decimal friQty { get; set; }
        /// <summary>
        /// 周6数量合计
        /// </summary>
        public decimal satQty { get; set; }
        /// <summary>
        /// 周日数量合计
        /// </summary>
        public decimal sunQty { get; set; }
        /// <summary>
        /// 需求日期
        /// </summary>
        public string requiredDate { get; set; }
        /// <summary>
        /// 修改标志
        /// </summary>
        public string changeFlag { get; set; }
        /// <summary>
        /// 可选供应商列表
        /// </summary>
        public List<Item> lstSuppliers { get; set; }
    }
}
