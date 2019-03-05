public class SalesPeriod
{
    public string periodGuid { get; set; }

    public string headGuid { get; set; }

    public string ownerCompanyCode { get; set; }

    public string ownerCompanyName_ZH { get; set; }

    public string ownerCompanyName_EN { get; set; }

    public string companyCode { get; set; }

    public string companyName_ZH { get; set; }

    public string companyName_EN { get; set; }

    public string contract { get; set; }

    public string cmpCode { get; set; }

    public string customCode { get; set; }

    public string customName_ZH { get; set; }

    public string customName_EN { get; set; }

    public string startDate { get; set; }

    public string endDate { get; set; }

    public string reportDate { get; set; }

    public string costCenterCode { get; set; }

    public string groupGuid { get; set; }

    public string createDate { get; set; }

    public string userGuid { get; set; }

    public string userID { get; set; }

    public string changeDate { get; set; }

    public string changeUserGuid { get; set; }

    public string changeUserID { get; set; }

    public string status { get; set; }

    public string complete { get; set; }
    // 公司间开票Flag
    public string crossCompInv { get; set; }
    // 决定销售开票画面的区间显示查看还是修改
    public string changeType { get; set; }
    // 决定销售开票画面的区间是否可以勾选
    public string tobeInvoiced { get; set; }
    // 区间开票总金额
    public decimal totalAmount { get; set; } 

    public string invCompanyCode { get; set; }

    public string withLog { get; set; }

    public int itemCount { get; set; }

    /// <summary>
    /// added by Angel.Jiang
    /// </summary>
    public decimal value { get; set; }

    public string remark { get; set; }

}