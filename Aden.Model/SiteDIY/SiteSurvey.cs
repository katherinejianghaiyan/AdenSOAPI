using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aden.Model.SiteDIY
{
    public class SiteSurvey:Site
    {
        public Int64 rowNumber { get; set; }
        public string sort { get; set; }
        public string userAnswer { get; set; }
        public int amount { get; set; }
        public string surveyItem { get; set; }
        public string headGuid { get; set; }
        public string lineGuid { get; set; }
        public string createUserID { get; set; }
        public string createUserName { get; set; }
        public string wechatID { get; set; }
        public string department { get; set; }
        public DateTime createTime { get; set; }
        public DateTime loginTime { get; set; }
        public string txtComment { get; set; }
        public string picComment { get; set; }
        public string itemStyle { get; set; }
        public string surveyTime { get; set; }
        public string displayZone { get; set; }
        public List<object> surveyDetails { get; set; }
        public List<object> surveySummary { get; set; }
        public List<object> surveyItems { get; set; }
        //问卷浏览量（按日期）
        public List<object> BrTimeList { get; set; }
        //问卷回收量（按日期排序）
        public List<object> FBTimeList { get; set; }
    }
}