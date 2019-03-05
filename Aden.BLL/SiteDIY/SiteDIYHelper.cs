using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aden.Model;
using Aden.Model.SOAccount;
using Aden.Model.Common;
using Aden.Model.SiteDIY;
using Aden.Model.MastData;
using Aden.Model.SOMastData;
using Aden.Model.Upload;
using Aden.DAL.SiteDIY;
using System.Data;


namespace Aden.BLL.SiteDIY
{
    public class SiteDIYHelper
    {
        /// <summary>
        /// 从DAL层获取对象
        /// </summary>
        private readonly static SiteDIYFactory SiteDIYFacotry = new SiteDIYFactory();
        private readonly SiteDIYFactory SiteDIYFactory2 = new SiteDIYFactory();

        /// <summary>
        /// 获取用户有权限获得的分公司-营运点下拉表
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public static List<DBSite> GetCompanySite(AuthCompany auth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(auth.action) || string.IsNullOrWhiteSpace(auth.userGuid))
                    throw new Exception("GetCompanySite is null");
                List<DBSite> list = SiteDIYFacotry.GetCompanySite(auth);
                if (list == null || !list.Any())
                    throw new Exception("DAL.SiteDIYFactory.GetCompanySite()==null");
                return list;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                },"GetCompanySite");
                return null;
            }
        }
        /// <summary>
        /// 获取海报
        /// </summary>
        /// <param name="siteGuid"></param>
        /// <returns></returns>
        public static List<SitePost> GetSitePosts(string dbName,string siteGuid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteGuid))
                    throw new Exception("SiteGuid is null");
                List<SitePost> GetSitePosts = SiteDIYFacotry.GetSitePosts(dbName,siteGuid);

                if (GetSitePosts == null || !GetSitePosts.Any())
                    throw new Exception("DAL.SiteDIYFacotory.GetSitePosts()==null");

                return GetSitePosts;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                },"GetSitePosts");
                return null;
            }
        }

        public static List<ItemPrice> GetPriceList(Dictionary<string,dynamic> param)
        {
            try
            {
               List<ItemPrice> GetPriceList = null;
               if (param.ContainsKey("mark") && ((string)param["mark"]).ToUpper()== "PRICELIST")
                    GetPriceList = SiteDIYFacotry.GetPriceList(param);
               else if(param.ContainsKey("mark") && ((string)param["mark"]).ToUpper() == "NEWFGITEMLIST")
                    GetPriceList = SiteDIYFacotry.GetNewFGItemList(param);
  
                if (GetPriceList == null || !GetPriceList.Any())
                    throw new Exception("GetPriceList() is null");
                return GetPriceList;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "GetPriceList");
                return null;
            }
        }

        /// <summary>
        /// 更新销售价目表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int UpdateFGItemPrice(Dictionary<string, dynamic> param)
        {
            try
            {
                int result = SiteDIYFacotry.UpdateFGItemPrice(param);
                if (result == 0) throw new Exception("DAL.SiteDIYFacotry.UpdateFGItemPrice()==0");
                return result;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "UpdateFGItemPrice");
                return 0;
            }
        }
        /// <summary>
        /// 获取问卷调研反馈汇总
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<SiteSurvey> SiteSurveySummary(Dictionary<string, dynamic> param)
        {
            try
            {
                if (!param.ContainsKey("siteGuid") || string.IsNullOrWhiteSpace((string)param["siteGuid"])|| !param.ContainsKey("pageName")|| string.IsNullOrWhiteSpace((string)param["pageName"]))
                    throw new Exception("param is null");
                List<SiteSurvey> SiteSurveySummary = SiteDIYFacotry.SiteSurveySummary(param);
                if (SiteSurveySummary == null || !SiteSurveySummary.Any())
                    throw new Exception("DAL.SiteDIYFacotry.UpdateFGItemPrice() is null");
                return SiteSurveySummary;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                },"SiteSurveySummary");
                return null;
            }
        }

        /// <summary>
        /// 上传海报图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pictureFile"></param>
        /// <param name="uploadFileName"></param>
        /// <returns></returns>
        public static int UploadSitePicture(string filePath,string picVal1, Model.Upload.UploadFileName uploadFileName,string siteGuid,string id,string businessType,string startDate,bool check,string imageType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteGuid) || string.IsNullOrWhiteSpace(startDate) ||string.IsNullOrWhiteSpace(imageType))
                    throw new Exception("param is null");
                int i = SiteDIYFacotry.UploadSitePicture(filePath, picVal1, uploadFileName,siteGuid,id,businessType,startDate,check,imageType);
               
                return i;
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(new Log()
                {
                    message = ex.Message
                }, "UploadSitePicture");
                return 0;
            }
           
        }


    }
}