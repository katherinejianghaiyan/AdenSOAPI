using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aden.Model.Purchase
{
    public class PurchaseOrderLine
    {
        #region PO Head部分
        public string headGuid { get; set; }

        public string orderDate { get; set; }

        public string orderDescription { get; set; }

        public string orderMethod { get; set; }

        public string dbCode { get; set; }

        public string costCenterCode { get; set; }

        public string userId { get; set; }

        public string employee { get; set; }

        #endregion

        #region PO Line部分
        public string lineGuid { get; set; }

        public int lineNumber { get; set; }

        public string supplierCode { get; set; }

        public string warehouseCode { get; set; }

        public string itemDescription { get; set; }

        public string itemCode { get; set; }

        public string unit { get; set; }

        public string costCenter { get; set; }

        public string remark { get; set; }

        public string erpNumber { get; set; }

        #endregion

        #region PO LineDetail部分

        public string costUnit { get; set; }

        public decimal qty { get; set; }

        public decimal qty_back { get; set; }

        public decimal price { get; set; }

        public string createUser { get; set; }

        public string createDate { get; set; }

        public string deleteUser { get; set; }

        public string deleteDate { get; set; }

        public decimal totalPrice { get; set; }
        #endregion
        
        /// <summary>
        /// 删除标志
        /// </summary>
        public bool delFlag { get; set; }
    }
}
