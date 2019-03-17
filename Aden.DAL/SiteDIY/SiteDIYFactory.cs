using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Aden.Util.Common;
using Aden.Util.Database;
using Aden.Model.SiteDIY;
using Aden.Model.SOMastData;
using Aden.DAL.SOAccount;
using Aden.DAL.SalesOrder;
using Aden.DAL.Models;
using System.Data;
using System.Configuration;
using Aden.Model.MastData;
using Aden.Model.Upload;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.IO;


namespace Aden.DAL.SiteDIY
{
    public class SiteDIYFactory :ApiController
    {
        private readonly static CompanyFactory compfactory = new CompanyFactory();

        /// <summary>
        /// 获取用户有权限获得的分公司-营运点下拉表
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public List<DBSite> GetCompanySite(AuthCompany auth)
        {
            try
            {

                string strCCList = "SELECT A1.BUGUID AS COMPANYGUID,A2.ERPCODE AS DBNAME,A1.CODE AS COSTCENTERCODE,A1.GUID AS SITEGUID FROM TBLSITE (NOLOCK) A1,TBLBU (NOLOCK) A2 where A1.BUGUID=A2.BUGUID";
                //"SELECT DISTINCT DBNAME,COSTCENTERCODE FROM CCMAST (NOLOCK)";

                //List<CCMast> CCList = SqlServerHelper.GetEntityList<CCMast>(SqlServerHelper.salesorderConn, strCCList);
                List<CCMast> CCList = SqlServerHelper.GetEntityList<CCMast>(SqlServerHelper.sfeed(), strCCList);

                List<Company> GetCompanySite = new List<Company>();

                if (auth.action == "wechatBarCode" && auth.userGuid == "None")
                {
                    GetCompanySite = null;
                }
                else
                {
                    GetCompanySite = compfactory.GetCompanyInAuth(auth);
                }
                List<DBSite> SiteList = GetSite(auth.action, auth.userGuid);
                SiteList = (from a in SiteList
                            join b in CCList
                            on a.costCenterCode equals b.costCenterCode
                            select a).ToList();
                         
                List<DBSite> strList = new List<DBSite>();

                if (GetCompanySite != null && GetCompanySite.Count > 0)
                {
                    strList = GetCompanySite.Join(CCList,
                    mos => new { companyCode = mos.companyCode },
                    nos => new { companyCode = nos.dbName },
                    (mos, nos) =>
                    {
                        var data = new DBSite();
                        data.dbName = mos.dbName;
                        data.companyGuid = nos.companyGuid;
                        data.companyCode = mos.companyCode;
                        data.companyName_ZH = mos.companyName_ZH;
                        data.companyName_EN = mos.companyName_EN;
                        data.costCenterCode = nos.costCenterCode;
                        data.siteGuid = nos.siteGuid;

                        return data;
                    }).ToList();

                    if (SiteList != null && SiteList.Count > 0)
                        strList = strList.Union(SiteList).Distinct(dr => dr.costCenterCode).ToList();
                }
                else
                {
                    if (SiteList != null && SiteList.Count > 0)
                        strList = SiteList;
                    else if (SiteList == null || SiteList.Count == 0)
                        return null;
                }

                List<Site> list = new List<Site>();
                foreach (var item in strList)
                {
                    Site db = new Site();
                    db.dbName = item.dbName;
                    db.companyGuid = item.companyGuid;
                    db.companyCode = item.companyCode;
                    db.companyName_ZH = item.companyName_ZH;
                    db.companyName_EN = item.companyName_EN;
                    db.costCenterCode = item.costCenterCode;
                    db.siteGuid = item.siteGuid;
                    list.Add(db);
                }

                list.OrderBy(dr => dr.dbName).ThenBy(dr => dr.costCenterCode);

                var lg = strList.GroupBy(dr => dr.dbName).Select(dg => new DBSite()
                {
                    dbName = dg.FirstOrDefault().dbName,
                    companyName_ZH = dg.FirstOrDefault().companyName_ZH,
                    companyGuid=dg.FirstOrDefault().companyGuid,
                    siteList = list.Where(ds => ds.dbName.Equals(dg.FirstOrDefault().dbName)).OrderBy(dr=>dr.costCenterCode).ToList()
                }).OrderBy(dg => dg.dbName).ToList().Distinct();

                return lg.ToList();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// 改菜单下的营运点权限
        /// </summary>
        /// <param name="action"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        public List<DBSite> GetSite(string action, string userGuid)
        {
            try
            {

                string strAuthCC = string.Empty;
                if (action=="wechatBarCode" && userGuid== "None")
                {
                    strAuthCC = "SELECT DISTINCT '' DBNAME,C.Guid COMPANYGUID, C.COMPANY AS COMPANYCODE, "
                        + "C.NAME_ZH AS COMPANYNAME_ZH, C.NAME_EN AS COMPANYNAME_EN, "
                        + "A.CostCenterCode,A.SITEGUID from CCMAST A, COMPANY C "
                        + "where A.DBName = C.Company";
                }
                else
                {
                    string strGuid = Util.AdenCommon.MastData.formatActionToMenuGuid(action);


                    strAuthCC = string.Format("SELECT DISTINCT '' DBNAME,C.Guid COMPANYGUID, C.COMPANY AS COMPANYCODE, "
                                + "C.NAME_ZH AS COMPANYNAME_ZH, C.NAME_EN AS COMPANYNAME_EN, "
                                + "A.CostCenterCode,A.SITEGUID from TBLUSERMENUDATA U, CCMAST A,COMPANY C "
                                + "where U.USERGUID = '{0}' "
                                + "AND U.MENUGUID IN " + strGuid
                                + "AND A.CostCenterCode = U.Code AND A.DBName = C.Company", userGuid);

                }

                List<DBSite> GetSite = SqlServerHelper.GetEntityList<DBSite>(SqlServerHelper.salesorderConn(), strAuthCC);
                return GetSite;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<SitePost> GetSitePosts(string dbName,string siteGuid)
        {
            try
            {
                string filterDB = string.IsNullOrWhiteSpace(dbName) ? "" : string.Format(" AND A3.ERPCODE='{0}' ", dbName);

                string pictureRoot = ConfigurationManager.AppSettings["PictureFile"].Replace("\\","/");

                //pictureRoot =pictureRoot.Substring(pictureRoot.IndexOf(":")).Replace("\\","/");
                string sql = string.Format("SELECT A1.ID,A3.ERPCODE AS DBNAME,A1.SITEGUID,ISNULL(A1.SORTNAME,100) SORTNAME,a2.CODE AS COSTCENTERCODE,A1.VAL1 AS POSTNAME, "
                    + "A1.VAL2 AS IMAGETYPE,BUSINESSTYPE, "
                    + "CONVERT(VARCHAR(10),A1.STARTDATE,23) AS STARTDATE,ISNULL(CONVERT(VARCHAR(10),A1.ENDDATE,23),'9999-12-31') AS ENDDATE, "
                    + "A1.ITEMSTATUS FROM TBLDATAS A1,TBLSITE A2,TBLBU A3 WHERE A1.SITEGUID=A2.GUID AND A1.DATATYPE='IMAGE' AND A3.BUGUID=A2.BUGUID {0}"
                    + "ORDER BY A1.ID ",filterDB);
                sql = string.Format(sql, siteGuid);

                List<SitePost> GetSitePosts = SqlServerHelper.GetEntityList<SitePost>(SqlServerHelper.sfeed(), sql);
                foreach(var item in GetSitePosts)
                {
                    item.picAddress = pictureRoot+item.postName;
                }
                if (GetSitePosts == null || !GetSitePosts.Any())
                    return null;
                return GetSitePosts;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemPrice> GetPriceList(Dictionary<string,dynamic> param)
        {
            try
            {
                #region OLD SQL
                //string strSql = "WITH TBL AS "
                //    + "(SELECT DISTINCT RANK() OVER(PARTITION BY A1.ITEMGUID, a1.BUGUID, a1.SITEGUID ORDER BY A1.CREATETIME DESC) ROWINDEX, "
                //    + "CASE WHEN A1.BUGUID IS NULL THEN NULL ELSE A1.BUGUID END AS BUGUID, "
                //    + "CASE WHEN A1.SITEGUID IS NULL THEN NULL ELSE A1.SITEGUID END AS SITEGUID, "
                //    + "A1.ITEMGUID,A2.ITEMCODE, A2.ITEMNAMEZHCN AS ITEMNAME_ZH, A2.ITEMNAMEENUS AS ITEMNAME_EN, A2.CONTAINER, A1.PRICE AS ITEMPRICE, "
                //    + "CONVERT(VARCHAR(10),CAST(CONVERT(VARCHAR(8),A1.STARTDATE) AS DATETIME),23) STARTDATE, CONVERT(VARCHAR(10),CAST(CONVERT(VARCHAR(8),ISNULL(A1.ENDDATE,20991231)) AS DATETIME),23) ENDDATE, "
                //    + "'{0}'+A2.IMAGE1 AS ITEMIMAGE "
                //    + "FROM TBLITEMPRICE (NOLOCK) A1,TBLITEM (NOLOCK) A2, TBLSITE (NOLOCK) a3, TBLBU (NOLOCK) A4 "
                //    + "WHERE A1.PRICETYPE = 'SALES' AND A2.GUID = A1.ITEMGUID AND A2.STATUS='1' AND A2.IsDel='0' AND (A1.SITEGUID = A3.GUID OR A1.BUGUID = A3.BUGUID) AND A4.BUGUID = A3.BUGUID) "
                //    + "SELECT DISTINCT * FROM TBL WHERE ROWINDEX = 1 AND (BUGUID ='{1}' OR SITEGUID='{1}') "
                //    + "AND (ISNULL(ITEMCODE,'')+ISNULL(ItemName_EN,'')+ISNULL(ItemName_ZH,'')) LIKE '%{2}%' ";
                #endregion

                string strSql = "SELECT distinct A2.BUGUID,A2.SITEGUID,A1.GUID AS ITEMGUID,A1.ITEMCODE,A1.SORT,A1.ITEMNAMEZHCN AS ITEMNAME_ZH, "
                    + "A1.ITEMNAMEENUS AS ITEMNAME_EN,A1.CONTAINER,A1.WEIGHT AS ITEMWEIGHT,A2.PRICE AS ITEMPRICE, "
                    + "CONVERT(VARCHAR(10), CAST(CONVERT(VARCHAR(8), A2.STARTDATE) AS DATETIME), 23) STARTDATE, "
                    + "CONVERT(VARCHAR(10), CAST(CONVERT(VARCHAR(8), A2.ENDDATE) AS DATETIME), 23) ENDDATE,'{0}'+A1.IMAGE1 AS ITEMIMAGE, " +
                    "(CASE WHEN A2.PRICE IS NULL THEN 1 ELSE 0 END) "
                    + "FROM TBLITEM(NOLOCK) A1 LEFT JOIN TBLITEMPRICE(NOLOCK) A2 ON A1.GUID = A2.ItemGUID "
                    +"AND A2.PRICETYPE = 'SALES' " +
                    //"AND ISNULL(A2.STARTDATE,19000101)< CAST(CONVERT(VARCHAR(8), GETDATE(), 112) AS INT) "
                   // "AND ISNULL(A2.ENDDATE,22221231)> CAST(CONVERT(VARCHAR(8), DATEADD(D, -1, GETDATE()), 112) AS INT) "
                   " AND A2.ENDDATE IS NULL "
                    +"AND(ISNULL(A2.BUGUID, '') = '{1}' "
                    +"OR ISNULL(A2.SITEGUID, '') = '{1}') "
                    + "WHERE ISNULL(A1.TOSELL,0)= 1 AND A1.STATUS = 1 AND A1.IsDel = 0 AND LEN(A1.COOKING)> 10 AND (ISNULL(A1.ITEMCODE,'')+ISNULL(A1.ItemNameZHCN,'')+ISNULL(A1.ITEMNAMEENUS,'')) LIKE '%{2}%'"
                    + "ORDER BY (CASE WHEN A2.PRICE IS NULL THEN 1 ELSE 0 END )," +
                    "A1.SORT, A1.ITEMCODE";

                strSql = string.Format(strSql, SqlServerHelper.sfeedPicUrl(),param.ContainsKey("addr")? param["addr"] :"",param.ContainsKey("keyWord")? param["keyWord"]:"");

                List<ItemPrice> data = SqlServerHelper.GetEntityList<ItemPrice>(SqlServerHelper.sfeed(), strSql);
                if (data == null || !data.Any())
                    return null;

                if (!data.Any(q => !string.IsNullOrWhiteSpace(q.siteGuid))) //没有定义Site的价目表
                    return data.OrderByDescending(dr=>dr.itemPrice).ToList();                             

                var lg = data.GroupBy(dr => new {
                       itemGUID= dr.itemGUID,
                       itemCode = dr.itemCode
                    }).Select(gr => 
                    {
                        var it = gr.FirstOrDefault();
                        ItemPrice item = new ItemPrice
                        {
                            itemCode = gr.Key.itemCode,
                            itemGUID = gr.Key.itemGUID,
                            itemName_ZH = it.itemName_ZH,
                            itemName_EN = it.itemName_EN,
                            itemImage = it.itemImage,
                            container = it.container,
                            itemWeight = it.itemWeight,
                            
                        };
                        it = gr.FirstOrDefault(q => !string.IsNullOrWhiteSpace(q.siteGuid));
                        if (it != null)
                        {
                            item.itemPrice = it.itemPrice;
                            item.startDate = it.startDate;
                        }

                        return item;
                      
                    }).ToList();
                
                return lg.OrderByDescending(dr=>dr.itemPrice).ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #region
        public List<ItemPrice> GetNewFGItemList(Dictionary<string, dynamic> param)
        {
            try
            {

                string sql = "SELECT DISTINCT GUID AS ITEMGUID,ITEMCODE,ITEMNAME AS ITEMNAME_ZH,ITEMNAMEENUS AS ITEMNAME_EN FROM TBLITEM WHERE STATUS='1' AND ISDEL='0' AND TOSELL='1'";

                List<ItemPrice> AllItem = SqlServerHelper.GetEntityList<ItemPrice>(SqlServerHelper.sfeed(), sql);

                if (AllItem == null || !AllItem.Any())
                    return null;

                List<ItemPrice> OldItem = GetPriceList(param);
                if (OldItem == null || !OldItem.Any())
                    return AllItem;
                List<ItemPrice> NewItem = AllItem.GroupJoin(OldItem,
                    a => new { itemGuid = a.itemGUID },
                    b => new { itemGuid = b.itemGUID },
                    (a, b) => new { a, b }).Where(q => q.b == null || !q.b.Any()).Select(q => q.a).ToList();
                return NewItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        /// <summary>
        /// 更新销售价目表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public int UpdateFGItemPrice(Dictionary<string,dynamic> param)
        {
            try
            {
                string filterAddr = string.Empty;

                string addrGuid = string.Empty;

                if (param.ContainsKey("note") && !string.IsNullOrWhiteSpace(param["note"]))
                    filterAddr = ((string)param["note"]).ToUpper();

                if (param.ContainsKey("addr") && !string.IsNullOrWhiteSpace(param["addr"]))
                    addrGuid = (string)param["addr"];

                if (string.IsNullOrWhiteSpace(filterAddr) || string.IsNullOrWhiteSpace(addrGuid))
                    return 0;

                

                string strSql = "";
                StringBuilder sb = new StringBuilder();
                
                if (param.ContainsKey("changeFlag"))
                {
                    #region 新建价目表
                    //if ((param["changeFlag"]).ToUpper() == "NEW")
                    //{
                    //    int startDate = ((string)param["startDate"]).Substring(0, 10).Replace("-", "").ToStringTrim().ToInt();
                    //    新建价目表
                    //    strSql = string.Format("INSERT INTO TBLITEMPRICE (ITEMGUID,{0},STARTDATE,PRICE,PRICETYPE,CREATETIME) VALUES "
                    //    + "('{1}','{2}','{3}','{4}','{5}','{6}')", filterAddr, param["itemGuid"], filterAddr == "BUGUID" ? buGuid : siteGuid, startDate,
                    //    param["itemPrice"], "Sales", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    //    sb.Append(strSql);
                    //}
                    #endregion

                    if ((param["changeFlag"]).ToUpper() == "UPDATE")
                    {

                        foreach(var item in param["data"])
                        {

                            List<ItemPrice> insertParam = new List<ItemPrice>();

                            ItemPrice DT = new ItemPrice();
                            
                            foreach (var row in item)
                            {
                                KeyValuePair<string, object> kvp = row;

                                var key = kvp.Key.ToUpper();

                                if (key == "STARTDATE")
                                    DT.startDate = (kvp.Value).ToStringTrim();
                                else if (key == "OLDSTARTDATE")
                                    DT.oldSDate = (kvp.Value).ToStringTrim();
                                //else if (key == "ENDDATE")
                                //    DT.endDate = (kvp.Value).ToStringTrim();
                                else if (key == "ITEMPRICE")
                                    DT.itemPrice = (kvp.Value).ToStringTrim().ToDecimal();
                                else if (key == "ITEMGUID")
                                    DT.itemGUID = (kvp.Value).ToStringTrim();
                            }
                            insertParam.Add(DT);

                            if(insertParam.Count>0 && insertParam.Any())
                            {
                                strSql = string.Format("UPDATE TBLITEMPRICE SET ENDDATE='{0}' WHERE ITEMGUID='{1}' AND (BUGUID='{2}' OR SITEGUID='{2}') AND STARTDATE='{3}' AND ENDDATE IS NULL; ", 
                                    DateTime.Parse(insertParam[0].startDate).AddDays(-1).ToString("yyyyMMdd"), insertParam[0].itemGUID, addrGuid, insertParam[0].oldSDate.Replace("-", ""));
                                strSql += string.Format("INSERT INTO TBLITEMPRICE (ITEMGUID,{0},STARTDATE,PRICE,PRICETYPE,CREATETIME) VALUES "
                                    + "('{1}','{2}','{3}','{4}','{5}','{6}'); ", filterAddr, insertParam[0].itemGUID, addrGuid, insertParam[0].startDate.Replace("-", ""),
                                    insertParam[0].itemPrice, "Sales", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                sb.Append(strSql);
                            }
                        }
                    }
                }
                //int i = 1;
                int i = SqlServerHelper.Execute(SqlServerHelper.sfeed(), sb.ToString());
                return i;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取问卷调研反馈汇总
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<SiteSurvey> SiteSurveySummary(Dictionary<string, dynamic> param)
        {
            try
            {
                string filterDate = string.Empty;
                if (param.ContainsKey("startDate") && param.ContainsKey("endDate"))
                    filterDate = string.Format("AND CONVERT(VARCHAR(10),A1.CREATETIME,23) BETWEEN '{0}' AND '{1}'", 
                        string.IsNullOrWhiteSpace(param["startDate"])? "2000-01-01": param["startDate"], 
                        string.IsNullOrWhiteSpace(param["endDate"])?"2099-12-31":param["endDate"]);

                //问卷发起BU和Site
                string buSql = string.Format("SELECT GUID AS SITEGUID,BUGUID AS COMPANYGUID FROM TBLSITE WHERE GUID='{0}' OR BUGUID='{0}'", (string)param["siteGuid"]);
                //问卷汇总
                string strSql = string.Format("SELECT DISTINCT A1.SITEGUID,A3.BUGUID AS COMPANYGUID, "
                        //+ "SUBSTRING(A1.USERANSWER,CHARINDEX('、',A1.USERANSWER)+1,LEN(A1.USERANSWER)-CHARINDEX('、',A1.USERANSWER)) AS USERANSWER, "
                        + "A1.USERANSWER, "
                        + "A1.USERANSWER,COUNT(A1.USERANSWER) AS AMOUNT ,A2.DISPLAYZONE "
                        + "FROM TBLSURVEYRESPONSEDETAILS (NOLOCK) A1,TBLSURVEYLINESDEF (NOLOCK) A2,TBLSURVEYHEADDEF (NOLOCK) A3 "
                        + "WHERE A2.DELETETIME IS NULL AND A1.LINEGUID=A2.LINEGUID AND A3.HEADGUID=A2.HEADGUID "
                        + " {0}"
                        + "AND A2.ALLOWNULL=0 AND A2.DISPLAYZONE='stdTable' "
                        + "GROUP BY A1.SITEGUID,A3.BUGUID,A1.USERANSWER,A2.DISPLAYZONE ORDER BY COUNT(A1.USERANSWER) DESC", filterDate);

                //问卷浏览量（按日期排序）
                string brSql = string.Format("SELECT CONVERT(VARCHAR(10),A1.CREATETIME,23) AS SURVEYTIME,COUNT(A1.USERID) AS AMOUNT FROM TBLUSERLOG (NOLOCK) A1 "
                    +"WHERE A1.PAGENAME LIKE '%{0}%' "
                    +" {1}"
                    +"GROUP BY CONVERT(VARCHAR(10), A1.CREATETIME, 23) ORDER BY CONVERT(VARCHAR(10), A1.CREATETIME, 23)",param["pageName"],filterDate);
                 
                //问卷发起BU和Site
                List<CCMast> BUSiteList = SqlServerHelper.GetEntityList<CCMast>(SqlServerHelper.sfeed(), buSql);
                //问卷汇总
                List<SiteSurvey> SiteSurveySummary = SqlServerHelper.GetEntityList<SiteSurvey>(SqlServerHelper.salesorderConn(), strSql);
                //问卷浏览量（按日期排序）
                List<SiteSurvey> BrTimeList = SqlServerHelper.GetEntityList<SiteSurvey>(SqlServerHelper.sfeed(), brSql);
                //问卷回收量（按日期排序）
                List<SiteSurvey> FBTimeList = new List<SiteSurvey>();
                List<SiteSurvey> list = new List<SiteSurvey>();
                SiteSurvey obj = new SiteSurvey();

                if (BUSiteList == null || !BUSiteList.Any()||SiteSurveySummary==null||!SiteSurveySummary.Any())
                    return null;

                var SurveySummary = SiteSurveySummary.Join(BUSiteList,
                    a => new { siteGuid = a.siteGuid },  
                    b => new { siteGuid = b.siteGuid, },  
                    (a, b) =>
                    {
                        a.companyGuid = b.companyGuid;
                        a.siteGuid = b.siteGuid;

                        return a;
                    }).ToList();


                if (SurveySummary == null || !SurveySummary.Any())
                {
                    string siteGuid = string.Format("{0}{1}{0}", "'", (string)param["siteGuid"]); 
                    list = SiteSurveyDetails(siteGuid, filterDate);
                }
                else
                {
                    List<object> surveySummary = new List<object>();
                    foreach (var item in SurveySummary)
                    {
                        obj = new SiteSurvey();
                        obj.companyGuid = item.companyGuid;
                        obj.siteGuid = item.siteGuid;
                        obj.userAnswer = item.userAnswer;
                        obj.amount = item.amount;
                        surveySummary.Add(obj);
                    }

                    // 问卷调查明细

                    string siteGuid = string.Empty;
                    foreach (var line in SurveySummary)
                    {
                        siteGuid += ",'" + line.siteGuid + "'";
                    }
                    if (!string.IsNullOrWhiteSpace(siteGuid))
                        list = SiteSurveyDetails(siteGuid.Substring(1), filterDate);


                    SurveySummary = SurveySummary.GroupBy(dr => dr.userAnswer).Select(gr => new SiteSurvey()
                    {
                        companyGuid = gr.FirstOrDefault().companyGuid,
                        userAnswer = gr.Key,
                        amount = gr.Sum(d => d.amount),
                        siteGuid = siteGuid.Substring(1),
                    }).ToList();
                }

                List<object> surveyDetails = new List<object>();

                List<object> Items = new List<object>();

                List<SiteSurvey> surveyItems = new List<SiteSurvey>();

                if (list != null && list.Any())
                {
                    
                    foreach (var item in list)
                    {      
                        obj = new SiteSurvey();
                        obj.surveyDetails = item.surveyDetails;
                        obj.createTime = item.createTime;
                        obj.createUserID = item.createUserID;
                        obj.headGuid = item.headGuid;
                        obj.wechatID = item.wechatID;

                        surveyDetails.Add(obj);
                        Items.Add(obj.surveyDetails);
                    }

                    //调研问题汇总
                    if (Items!=null && Items.Any())
                    {
                        for (int i = 0; i < Items.Count; i++)
                        {
                            dynamic Details = Items[i];

                            if (Details.Count == 0)
                                continue;
                            for (int j = 0; j < Details.Count; j++)
                            {
                                obj = new SiteSurvey();
                                obj.headGuid = Details[j].headGuid;
                                obj.lineGuid = Details[j].lineGuid;
                                obj.surveyItem = Details[j].surveyItem;
                                obj.createTime = Details[j].createTime;
                                obj.displayZone = Details[j].displayZone;


                                surveyItems.Add(obj);
                                FBTimeList.Add(obj);
                              
                            }
                        }
                    }
                }
                if(surveyItems!=null && surveyItems.Any())
                    surveyItems = surveyItems.Distinct(q=>q.lineGuid).ToList(); //调研中提出的具体问题

                //问卷回收量（按日期排序）
                if (FBTimeList != null && FBTimeList.Any())
                {
                    List<SiteSurvey> FBTimeList2 = FBTimeList.GroupBy(ds => new
                    {
                        surveyTime = ds.createTime.ToString("yyyy-MM-dd"),
                        headGuid=ds.headGuid.ToString()
                    }).Select(dr => new SiteSurvey()
                    {
                        surveyTime = dr.FirstOrDefault().createTime.ToString("yyyy-MM-dd"),
                        headGuid=dr.FirstOrDefault().headGuid,
                    }).OrderBy(g => g.surveyTime).ToList();

                    FBTimeList = FBTimeList2.GroupBy(dh => new
                    {
                        surveyTime=dh.surveyTime
                    }).Select(dw=>new SiteSurvey() {
                        surveyTime=dw.Key.surveyTime,
                        amount=dw.Count()
                    }).OrderBy(h=>h.surveyTime).ToList();
                }
                

                List<SiteSurvey> SurveyList = new List<SiteSurvey>();
                obj = new SiteSurvey();

                if (SurveySummary != null && SurveySummary.Any())
                    obj.surveySummary = SurveySummary.ToList<object>(); //调研汇总
                if (surveyDetails != null && surveyDetails.Any())
                    obj.surveyDetails = surveyDetails; //调研明细
                if (surveyItems != null && surveyItems.Any())
                    obj.surveyItems = surveyItems.ToList<object>(); //调研问题
                if (BrTimeList != null && BrTimeList.Any())
                    obj.BrTimeList = BrTimeList.ToList<object>(); //问卷浏览量（按日期）
                if (FBTimeList != null && FBTimeList.Any())
                    obj.FBTimeList = FBTimeList.ToList<object>(); //问卷回收量（按日期）
                SurveyList.Add(obj);
                
                return SurveyList;
              
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 问卷调查明细
        /// </summary>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        public List<SiteSurvey>SiteSurveyDetails(string siteGuid,string filterDate)
        {
            try
            {
                // 必填项
                string strSql = string.Format("SELECT DISTINCT A2.SORT,A2.ITEMNAME AS SURVEYITEM,A1.CREATETIME,A1.HEADGUID,A1.LINEGUID,A1.USERANSWER,A1.CREATETIME,A1.CREATEUSER AS CREATEUSERID,A2.DISPLAYZONE,A2.ITEMSTYLE "
                    + "FROM TBLSURVEYRESPONSEDETAILS (NOLOCK) A1,TBLSURVEYLINESDEF (NOLOCK) A2,TBLSURVEYHEADDEF (NOLOCK) A3 "
                    + "WHERE A2.DELETETIME IS NULL AND A1.LINEGUID = A2.LINEGUID AND A3.HEADGUID = A2.HEADGUID "
                    + "AND A1.CREATEUSER<>'' AND A1.SITEGUID in ({0}) {1} "
                    + "ORDER BY A1.CREATEUSER, A1.CREATETIME, A2.SORT ", siteGuid,filterDate);
                // 微信用户账号列表
                string userSql = string.Format("SELECT CONVERT(VARCHAR,A1.USERID) AS CREATEUSERID,A1.WECHATID,A2.CREATETIME LOGINTIME "
                    + "FROM TBLUSER A1 LEFT JOIN(SELECT USERID,MAX(CREATETIME) CREATETIME FROM TBLUSERLOG GROUP BY USERID) A2 ON A1.USERID=A2.USERID ");
                
                //问卷调研明细（必填项）
                List<SiteSurvey> SiteSurveyDetails = SqlServerHelper.GetEntityList<SiteSurvey>(SqlServerHelper.salesorderConn(), strSql);
                //微信用户信息表
                List<SiteSurvey> UserDetails = SqlServerHelper.GetEntityList<SiteSurvey>(SqlServerHelper.sfeed(), userSql);
                

                if ((SiteSurveyDetails == null || !SiteSurveyDetails.Any())||(UserDetails == null || !UserDetails.Any()))
                    return null;


                //按用户对调研明细分组
                SiteSurveyDetails = SiteSurveyDetails.GroupJoin(UserDetails,
                    a => new { createUserID = a.createUserID},b => new { createUserID = b.createUserID},
                    (a,b)=> 
                    {
                        a.wechatID = b.FirstOrDefault().wechatID;
                        a.loginTime = b.FirstOrDefault().loginTime < a.createTime ? a.createTime : b.FirstOrDefault().loginTime;
                        return a;
                    }).GroupBy(dr => 
                     new
                     {
                            createUserID = dr.createUserID,
                            headGuid = dr.headGuid,
                     }).Select(q =>
                     {
                         var details = q.Where(s => s.createUserID == q.Key.createUserID && s.headGuid == q.Key.headGuid).ToList<object>();

                         SiteSurvey data = new SiteSurvey()
                          {
                              headGuid = q.Key.headGuid,
                              createUserID = q.Key.createUserID,
                              createTime = q.FirstOrDefault().createTime,
                              wechatID = q.FirstOrDefault().wechatID
                         };
                     
                         data.surveyDetails = details;
                     
                         return data;
                     }).OrderByDescending(dr => dr.createTime).ToList();

                    return SiteSurveyDetails;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 上传海报图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pictureFile"></param>
        /// <param name="uploadFileName"></param>
        /// <returns></returns>
        public int UploadSitePicture(string filePath, string picVal1, Model.Upload.UploadFileName uploadFileName,string siteGuid,string id,string businessType, string startDate,bool check,string imageType)
        {
            try
            {
                int Count = 0;
                string strUpdate = "";
                string filter = "";
                string imgStr = "";
                //string OrignFileName = System.IO.Path.GetFileNameWithoutExtension(uploadFileName.OrignFileName).ToString();
                if (uploadFileName!=null)
                {
                    imgStr = "\\" + picVal1 + "\\" + uploadFileName.NewFileName;
                    filter = string.Format(" VAL1='{0}', ", imgStr.Replace("\\", "/"));
                }

                if (!string.IsNullOrWhiteSpace(id))
                  strUpdate = string.Format("UPDATE TBLDATAS SET {0} ITEMSTATUS='{1}',STARTDATE='{2}' WHERE ID='{3}'", filter, check ? "1":"0", startDate, id.ToInt());                 
                else if(string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(imgStr))
                  strUpdate = string.Format("INSERT INTO TBLDATAS (SITEGUID,BUSINESSTYPE,DATATYPE,VAL1,STARTDATE,ITEMSTATUS,VAL2) VALUES " +
                        "('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",siteGuid,businessType, "Image", imgStr.Replace("\\", "/"), startDate, check ? "1" : "0",imageType);
                

                Count = SqlServerHelper.Execute(SqlServerHelper.sfeed(), strUpdate);
                return Count;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

    }
}