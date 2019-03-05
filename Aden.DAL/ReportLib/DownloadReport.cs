using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aspose.Cells;
using System.IO;
using System.Web;
using System.Data;
using System.Configuration;
using Aden.Util.Database;
using Aden.Util.Reports;
using Aden.DAL.SiteDIY;
using Aden.Model.SiteDIY;

namespace Aden.DAL.ReportLib
{
    public class DownloadReport
    {
        //下载Excel周单
        public static MemoryStream ExportExcelWeeklyMenu(Dictionary<string, string> keyDic)
        {
            try
            {
                //如果传入非空窗口，所有的空窗口就被过滤掉
                string filter = "";
                string filterwd = "";
                if (!string.IsNullOrWhiteSpace(keyDic["windowName"]))
                {
                    filter = " and isnull(M.windowGUID,'')<>'' ";
                    filterwd = " and isnull(N.windowGuid,'')<>'' and isnull(M.windowGUID,'')<>'' ";
                }
                else
                {
                    filter = " and isnull(M.windowGUID,'')='' ";
                    filterwd = " and isnull(N.windowGuid,'')='' and isnull(M.windowGUID,'')='' ";
                }
                    
                //列示一周的日期
                string sql = string.Format("select convert(varchar(10),(DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 0)),23) 'Mon', "
                    + "convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 1)), 23) 'Tue', "
                    + "convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 2)), 23) 'Wed', "
                    + "convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 3)), 23) 'Thu', "
                    + "convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 4)), 23) 'Fri', "
                    + "convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 5)), 23) 'Sat', "
                    + "convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 6)), 23) 'Sun' ", keyDic["startDate"]);

                //周菜单数据
                string sql2 = string.Format("SELECT M.ID, M.COSTCENTERCODE, M.WINDOWGUID,isnull(N.WINDOWNAME,'(none)') WINDOWNAME,CONVERT(VARCHAR(100), M.REQUIREDDATE, 20) AS REQUIREDDATE, M.SOITEMGUID, "
                    + "L.PRODUCTDESC, M.ITEMGUID, M.ITEMCODE, M.ITEMTYPE, M.ITEMNAME_ZH, M.ITEMNAME_EN, M.REQUIREDQTY, "
                    + "M.ITEMCOST, M.OTHERCOST, M.ITEMUNITCODE, M.HEADGUID, M.PRODUCTGUID, M.PURCHASEPOLICY, CONVERT(VARCHAR(100), M.CREATETIME, 20) AS CREATETIME, M.CREATEUSER, "
                    + "CONVERT(VARCHAR(100), M.DELETETIME, 20) AS DELETETIME, M.DELETEUSER,(CASE WHEN M.LINKID = 0 THEN M.ID ELSE M.LINKID END) AS LINKID, "
                    + "datename(weekday, CONVERT(VARCHAR(100), M.REQUIREDDATE, 20)) WEEKDAY,P.ItemColor,convert(int,isnull(P.ItemSpicy,-1)) ItemSpicy FROM MENUORDERHEAD (nolock) AS M "
                    + "INNER JOIN SALESORDERITEM (nolock) AS L ON M.SOITEMGUID = L.ITEMGUID AND L.STATUS <> '0' INNER JOIN ProductTypeData K ON K.ID=L.ProductCode LEFT JOIN (select distinct WindowSort as Sort,windowGuid,WindowName,SOItemGuid from CCWindowMeals where deletetime is null) N ON M.WINDOWGUID=N.WINDOWGUID and M.SOITEMGUID=N.SOItemGuid "
                    + "LEFT JOIN TBLITEM P ON P.ITEMCODE=M.ITEMCODE "
                    + "LEFT JOIN (select distinct a.* from TBLITEMCLASS a join TBLITEMCLASS b on a.pguid = b.guid) Q ON P.CATEGORIESClassGUID=Q.GUID "
                    + "WHERE P.DELETETIME IS NULL AND M.COSTCENTERCODE = '{1}' "
                    + "AND M.DELETEUSER IS NULL AND M.REQUIREDDATE BETWEEN convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 0)), 23) "
                    + "AND convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{0}'), 6)), 23) {2} order by k.Sort,L.PRODUCTDESC,N.Sort,N.WINDOWNAME,M.ItemType,M.ItemCode", keyDic["startDate"], keyDic["costCenterCode"],filter);

                //周菜单大类
                string sql3 = string.Format("select convert(int,row_number() OVER (ORDER BY A.Sort)) 'Index',  isnull(A.WindowName,'(none)') WindowName, "
                    + "A.PRODUCTDESC,Max(A.Count) as Max from (SELECT L.PRODUCTDESC,K.Sort,datename(weekday, CONVERT(VARCHAR(100), M.REQUIREDDATE, 20)) WEEKDAY, "
                    + "Count(datename(weekday, CONVERT(VARCHAR(100), M.REQUIREDDATE, 20))) as Count,N.windowGuid,N.WindowName,N.Sort as windowSort FROM MENUORDERHEAD (nolock) AS M INNER JOIN SALESORDERITEM (nolock) AS L "
                    + "ON M.SOITEMGUID = L.ITEMGUID AND L.STATUS <> '0' INNER JOIN ProductTypeData K ON K.ID=L.ProductCode LEFT JOIN (select distinct WindowSort as Sort,windowGuid,WindowName,SOItemGuid from CCWindowMeals where deletetime is null) N ON N.WindowGUID=M.WindowGUID and M.SOITEMGUID=N.SOItemGuid "
                    + "WHERE M.COSTCENTERCODE = '{0}' AND M.DELETEUSER IS NULL AND M.REQUIREDDATE BETWEEN convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{1}'), 0)), 23) "
                    + "AND convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{1}'), 6)), 23) {2} group by K.Sort, L.PRODUCTDESC,M.REQUIREDDATE,N.WindowName,N.Sort,N.windowGuid) A "
                    + "group by A.Sort, A.windowSort,A.ProductDesc,A.WindowName order by A.Sort, A.PRODUCTDESC,A.windowSort,A.WindowName", keyDic["costCenterCode"], keyDic["startDate"],filterwd);

                var data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);

                var menu = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql2);

                var menuClass = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql3);

                var firstCol = menuClass.AsEnumerable().Select(dr => dr.Field<string>("WindowName")).FirstOrDefault();
                
                int lstCol = 2;
                if (firstCol.Contains("none")) lstCol = 1; 


                //一周的日期
                var tbl1 = from dr in data.AsEnumerable()
                           select new
                           {
                               Mon = dr.Field<string>("Mon"),
                               Tue = dr.Field<string>("Tue"),
                               Wed = dr.Field<string>("Wed"),
                               Thu = dr.Field<string>("Thu"),
                               Fri = dr.Field<string>("Fri"),
                               Sat = dr.Field<string>("Sat"),
                               Sun = dr.Field<string>("Sun")
                           };

                Workbook excel = new Workbook();
                Worksheet dataSheet = excel.Worksheets[0];
                
                dataSheet.Name = "周采购订单";
                Cells dataCells = dataSheet.Cells;
                
                Style titleStyle = excel.Styles[excel.Styles.Add()];
                Style dataStyle = excel.Styles.CreateBuiltinStyle(BuiltinStyleType.Normal);

                Style sidepanelStyle = excel.Styles[excel.Styles.Add()];
                Style sidepanelStyle2 = excel.Styles[excel.Styles.Add()];
                Style sidepanelStyle3 = excel.Styles[excel.Styles.Add()];

                Style subpanelStyle = excel.Styles[excel.Styles.Add()];

                Style weektitleStyle = excel.Styles[excel.Styles.Add()];

                #region 设置表头样式和数据样式
                titleStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                titleStyle.Font.Size = 26;
                titleStyle.Font.IsBold = true;
                titleStyle.Pattern = BackgroundType.Solid;
                titleStyle.ForegroundColor = System.Drawing.ColorTranslator.FromHtml("#32CD32"); //System.Drawing.Color.LimeGreen;

                weektitleStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                weektitleStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                weektitleStyle.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                weektitleStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                weektitleStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);

                weektitleStyle.Font.Size = 11;
                weektitleStyle.Font.IsBold = false;
                weektitleStyle.Pattern = BackgroundType.Solid;
                weektitleStyle.ForegroundColor = System.Drawing.Color.Yellow;

                #endregion

                #region 设置侧边格式

                subpanelStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                subpanelStyle.Font.Size = 11;
                subpanelStyle.Font.Name = "宋体";
                subpanelStyle.Font.IsBold = true;
                subpanelStyle.Pattern = BackgroundType.Solid;
                subpanelStyle.ForegroundColor = System.Drawing.Color.Yellow;
                subpanelStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                subpanelStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                subpanelStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);


                sidepanelStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                sidepanelStyle.Font.Size = 11;
                sidepanelStyle.Font.Name = "宋体";

                sidepanelStyle.Font.IsBold = true;
                sidepanelStyle.Pattern = BackgroundType.Solid;
                sidepanelStyle.ForegroundColor = System.Drawing.Color.Yellow;
                sidepanelStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                sidepanelStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                sidepanelStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);


                sidepanelStyle2.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                sidepanelStyle2.Font.Size = 11;
                sidepanelStyle2.Font.Name = "宋体";
                sidepanelStyle2.Font.IsBold = true;
                sidepanelStyle2.Pattern = BackgroundType.Solid;
                sidepanelStyle2.ForegroundColor = System.Drawing.Color.Yellow;
                sidepanelStyle2.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                sidepanelStyle2.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);

                sidepanelStyle3.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                sidepanelStyle3.Font.Size = 11;
                sidepanelStyle3.Font.Name = "宋体";
                sidepanelStyle3.Font.IsBold = true;
                sidepanelStyle3.Pattern = BackgroundType.Solid;
                sidepanelStyle3.ForegroundColor = System.Drawing.Color.Yellow;
                sidepanelStyle3.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                sidepanelStyle3.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                sidepanelStyle3.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                #endregion

                #region 单元格格式
                dataStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataStyle.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataStyle.Font.Size = 11;
                dataStyle.Font.Name = "宋体";
                dataStyle.IsTextWrapped = true;
                
                
                #endregion

                int j = 0;

                dataCells.Merge(0, 0, 1, lstCol+7);

                dataCells[0, j].PutValue(keyDic["costCenterName"] +" 周菜单 ");

                dataCells[0, j++].SetStyle(titleStyle);

                Array Logo = Array.CreateInstance(typeof(Stream), new int[1] { 1 }, new int[1] { 1 });

                string logoPath = string.Format(@"Images\Logo\Logo.png");
                logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logoPath);
                Logo.SetValue(Util.Common.FileHelper.FileToStream(logoPath), 1);

                dataSheet.Pictures.Add(0, 0, (Stream)Logo.GetValue(1), 100, 30);

                //dataCells.Merge(1, 0, 2, lstCol);

                //列表头套系字段
                dataCells[1, 0].PutValue("时间");
                dataCells[1, 0].SetStyle(subpanelStyle);
                dataCells[2, 0].PutValue("餐次");
                dataCells[2, 0].SetStyle(subpanelStyle);
                dataCells.Merge(1, 1, 2, 1);
                dataCells[1, 1].PutValue("套系");
                dataCells[1, 1].SetStyle(subpanelStyle);


                //列表一周的周一至周日
                dataCells[2, lstCol].PutValue("星期一");
                dataCells[2, lstCol+1].PutValue("星期二");
                dataCells[2, lstCol+2].PutValue("星期三");
                dataCells[2, lstCol+3].PutValue("星期四");
                dataCells[2, lstCol+4].PutValue("星期五");
                dataCells[2, lstCol+5].PutValue("星期六");
                dataCells[2, lstCol+6].PutValue("星期日");

                dataCells[2, lstCol].SetStyle(weektitleStyle);
                dataCells[2, lstCol+1].SetStyle(weektitleStyle);
                dataCells[2, lstCol+2].SetStyle(weektitleStyle);
                dataCells[2, lstCol+3].SetStyle(weektitleStyle);
                dataCells[2, lstCol+4].SetStyle(weektitleStyle);
                dataCells[2, lstCol+5].SetStyle(weektitleStyle);
                dataCells[2, lstCol+6].SetStyle(weektitleStyle);

                //一周的日期
                int i = 0;
                foreach (var item in tbl1)
                {
                    j = lstCol;
                    i++;
                    dataCells[i, j].PutValue(item.Mon);
                    dataCells[i, j++].SetStyle(weektitleStyle);
                    dataCells[i, j].PutValue(item.Tue);
                    dataCells[i, j++].SetStyle(weektitleStyle);
                    dataCells[i, j].PutValue(item.Wed);
                    dataCells[i, j++].SetStyle(weektitleStyle);
                    dataCells[i, j].PutValue(item.Thu);
                    dataCells[i, j++].SetStyle(weektitleStyle);
                    dataCells[i, j].PutValue(item.Fri);
                    dataCells[i, j++].SetStyle(weektitleStyle);
                    dataCells[i, j].PutValue(item.Sat);
                    dataCells[i, j++].SetStyle(weektitleStyle);
                    dataCells[i, j].PutValue(item.Sun);
                    dataCells[i, j++].SetStyle(weektitleStyle);
                }

                int s = 0;
                int e = 0;
                int h = 0;
                foreach (var item in menuClass.AsEnumerable())
                {
                    e += item.Field<int>("Max");
                    int y = item.Field<int>("Index");
                    if (item.Field<int>("Index") == 1)
                    {
                        s = 3;
                        h = 3 + item.Field<int>("Max");
                    }
                    else
                    {
                        s = e - item.Field<int>("Max") + 3;
                        h = s + item.Field<int>("Max");
                    }
                    for (var x = s; x < h; x++)
                    {
                        dataCells[x, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        if (lstCol == 2) 
                            dataCells[x, 1].PutValue(item.Field<string>("windowName"));
                    }
                    if (y == 1)
                    {


                        dataCells[3, 0].SetStyle(sidepanelStyle3);

                        if (lstCol == 2)
                        {
                            sidepanelStyle3.ForegroundColor = System.Drawing.Color.DodgerBlue;
                            sidepanelStyle3.Font.Color = System.Drawing.Color.Red;
                            dataCells[3, 1].SetStyle(sidepanelStyle3);
                        }
                        for (var x = s; x < h; x++)
                        {
                            if (x != 3)
                            {
                                sidepanelStyle2.Font.Color= System.Drawing.Color.Yellow;
                                sidepanelStyle2.ForegroundColor = System.Drawing.Color.Yellow;
                                dataCells[x, 0].SetStyle(sidepanelStyle2);
                                if (lstCol == 2)
                                {
                                    sidepanelStyle2.Font.Color = System.Drawing.Color.DodgerBlue;
                                    sidepanelStyle2.ForegroundColor = System.Drawing.Color.DodgerBlue;
                                    sidepanelStyle2.Font.Color = System.Drawing.Color.DodgerBlue;
                                    dataCells[x, 1].SetStyle(sidepanelStyle2);
                                } 
                                    
                            }
                        }
                    }
                    else if (y > 1)
                    {
                        for (var x = s; x < h; x++)
                        {
                            if (x == e - item.Field<int>("Max") + 3)
                            {
                                if (lstCol == 2)
                                    sidepanelStyle.ForegroundColor = System.Drawing.Color.DodgerBlue;
                                    sidepanelStyle.Font.Color = System.Drawing.Color.Red;
                                    dataCells[x, 1].SetStyle(sidepanelStyle);
                            }
                            else
                            {
                                if (lstCol == 2)
                                    sidepanelStyle2.ForegroundColor = System.Drawing.Color.DodgerBlue;
                                    sidepanelStyle2.Font.Color = System.Drawing.Color.DodgerBlue;
                                    dataCells[x, 1].SetStyle(sidepanelStyle2);
                            }
                        }
                    }
                }

                Array peppers = Array.CreateInstance(typeof(Stream), new int[1] { 3 }, new int[1] { 1 });
                for (int x = 1; x < 4; x++)
                {
                    string ss = string.Format(@"Images\Peppers\p{0}.jpg", x);
                    ss = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ss);
                    peppers.SetValue(Util.Common.FileHelper.FileToStream(ss), x);
                }
               

                int menulength = e;

                int a = 3;

                foreach (var item in menu.AsEnumerable().Where(g => g.Field<string>("WEEKDAY") == "Monday")) //menu.AsEnumerable().Where(g => g.Field<string>("WEEKDAY") == "Monday")
                {
                    for (var b = a; b < menulength + 3; b++)
                    {
                        string product = dataCells[b, 0].StringValue.ToString();
                        string window = "(none)";
                        if (lstCol == 2)
                            window = dataCells[b, 1].StringValue.ToString();
                        string it = item.Field<string>("PRODUCTDESC");
                        string wd = item.Field<string>("WINDOWNAME");
                        if (it != product || wd != window) continue;
                        dataCells[b, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        dataCells[b, lstCol].PutValue(item.Field<string>("ItemName_ZH") + ' ' + item.Field<string>("ItemName_EN"));
                        
                        
                        if (item.Field<int>("ItemSpicy") > 0)
                        {
                            dataSheet.Pictures.Add(b, lstCol, (Stream)peppers.GetValue(item.Field<int>("ItemSpicy")), 5, 5);

                            dataSheet.Pictures[dataSheet.Pictures.Count - 1].AlignTopRightCorner(b, lstCol);
                        }
                        

                        //为菜上色
                        //dataStyle.Pattern = BackgroundType.Solid;
                        //dataStyle.ForegroundColor = System.Drawing.ColorTranslator.FromHtml(item.Field<string>("ItemColor").ToString().Substring(0, 1) == "#" ? item.Field<string>("ItemColor").ToString() : "#FFFFFF");//System.Drawing.Color.Yellow;
                        //dataCells[b, lstCol].SetStyle(dataStyle);
                        a = b;
                        a++;
                        break;
                    }
                }

                a = 3;
                foreach (var item in menu.AsEnumerable().Where(g => g.Field<string>("WEEKDAY") == "Tuesday"))
                {
                    for (var b = a; b < menulength + 3; b++)
                    {
                        string product = dataCells[b, 0].StringValue.ToString();
                        string window = "(none)";
                        if (lstCol == 2)
                            window = dataCells[b, 1].StringValue.ToString();
                        if (item.Field<string>("PRODUCTDESC") != product || item.Field<string>("WINDOWNAME") != window) continue;
                        dataCells[b, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        dataCells[b, lstCol+1].PutValue(item.Field<string>("ItemName_ZH") + ' ' + item.Field<string>("ItemName_EN"));

                        //菜色辣度
                        if (item.Field<int>("ItemSpicy") > 0)
                        {
                            dataSheet.Pictures.Add(b, lstCol+1, (Stream)peppers.GetValue(item.Field<int>("ItemSpicy")), 5, 5);

                            dataSheet.Pictures[dataSheet.Pictures.Count - 1].AlignTopRightCorner(b, lstCol+1);
                        }

                        
                        a = b;
                        a++;
                        break;
                    }
                }

                a = 3;
                foreach (var item in menu.AsEnumerable().Where(g => g.Field<string>("WEEKDAY") == "Wednesday"))
                {
                    for (var b = a; b < menulength + 3; b++)
                    {

                        string product = dataCells[b, 0].StringValue.ToString();
                        string window = "(none)";
                        if (lstCol == 2)
                            window = dataCells[b, 1].StringValue.ToString();
                        if (item.Field<string>("PRODUCTDESC") != product || item.Field<string>("WINDOWNAME") != window) continue;
                        dataCells[b, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        dataCells[b, lstCol+2].PutValue(item.Field<string>("ItemName_ZH") + ' ' + item.Field<string>("ItemName_EN"));

                        //菜色辣度
                        if (item.Field<int>("ItemSpicy") > 0)
                        {
                            dataSheet.Pictures.Add(b, lstCol + 2, (Stream)peppers.GetValue(item.Field<int>("ItemSpicy")), 5, 5);

                            dataSheet.Pictures[dataSheet.Pictures.Count - 1].AlignTopRightCorner(b, lstCol + 2);
                        }
                        a = b;
                        a++;
                        break;
                    }
                }

                a = 3;
                foreach (var item in menu.AsEnumerable().Where(g => g.Field<string>("WEEKDAY") == "Thursday"))
                {
                    for (var b = a; b < menulength + 3; b++)
                    {
                        string product = dataCells[b, 0].StringValue.ToString();
                        string window = "(none)";
                        if (lstCol == 2)
                            window = dataCells[b, 1].StringValue.ToString();
                        if (item.Field<string>("PRODUCTDESC") != product || item.Field<string>("WINDOWNAME") != window) continue;
                        dataCells[b, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        dataCells[b, lstCol+3].PutValue(item.Field<string>("ItemName_ZH") + ' ' + item.Field<string>("ItemName_EN"));

                        //菜色辣度
                        if (item.Field<int>("ItemSpicy") > 0)
                        {
                            dataSheet.Pictures.Add(b, lstCol + 3, (Stream)peppers.GetValue(item.Field<int>("ItemSpicy")), 5, 5);

                            dataSheet.Pictures[dataSheet.Pictures.Count - 1].AlignTopRightCorner(b, lstCol + 3);
                        }

                        a = b;
                        a++;
                        break;
                    }
                }

                a = 3;
                foreach (var item in menu.AsEnumerable().Where(g => g.Field<string>("WEEKDAY") == "Friday"))
                {
                    for (var b = a; b < menulength + 3; b++)
                    {
                        string product = dataCells[b, 0].StringValue.ToString();
                        string window = "(none)";
                        if (lstCol == 2)
                            window = dataCells[b, 1].StringValue.ToString();
                        if (item.Field<string>("PRODUCTDESC") != product || item.Field<string>("WINDOWNAME") != window) continue;
                        dataCells[b, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        dataCells[b, lstCol + 4].PutValue(item.Field<string>("ItemName_ZH") + ' ' + item.Field<string>("ItemName_EN"));

                        //菜色辣度
                        if (item.Field<int>("ItemSpicy") > 0)
                        {
                            dataSheet.Pictures.Add(b, lstCol + 4, (Stream)peppers.GetValue(item.Field<int>("ItemSpicy")), 5, 5);

                            dataSheet.Pictures[dataSheet.Pictures.Count - 1].AlignTopRightCorner(b, lstCol + 4);
                        }

                        a = b;
                        a++;
                        break;
                    }
                }

                a = 3;
                foreach (var item in menu.AsEnumerable().Where(g => g.Field<string>("WEEKDAY") == "Saturday"))
                {
                    for (var b = a; b < menulength + 3; b++)
                    {
                        string product = dataCells[b, 0].StringValue.ToString();
                        string window = "(none)";
                        if (lstCol == 2)
                            window = dataCells[b, 1].StringValue.ToString();
                        if (item.Field<string>("PRODUCTDESC") != product || item.Field<string>("WINDOWNAME") != window) continue;
                        dataCells[b, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        dataCells[b, lstCol + 5].PutValue(item.Field<string>("ItemName_ZH") + ' ' + item.Field<string>("ItemName_EN"));

                        //菜色辣度
                        if (item.Field<int>("ItemSpicy") > 0)
                        {
                            dataSheet.Pictures.Add(b, lstCol + 5, (Stream)peppers.GetValue(item.Field<int>("ItemSpicy")), 5, 5);

                            dataSheet.Pictures[dataSheet.Pictures.Count - 1].AlignTopRightCorner(b, lstCol + 5);
                        }

                        a = b;
                        a++;
                        break;
                    }
                }

                a = 3;
                foreach (var item in menu.AsEnumerable().Where(g => g.Field<string>("WEEKDAY") == "Sunday"))
                {
                    for (var b = a; b < menulength + 3; b++)
                    {
                        string product = dataCells[b, 0].StringValue.ToString();
                        string window = "(none)";
                        if (lstCol == 2)
                            window = dataCells[b, 1].StringValue.ToString();
                        if (item.Field<string>("PRODUCTDESC") != product || item.Field<string>("WINDOWNAME") != window) continue;
                        dataCells[b, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        dataCells[b, lstCol + 6].PutValue(item.Field<string>("ItemName_ZH") + ' ' + item.Field<string>("ItemName_EN"));

                        //菜色辣度
                        if (item.Field<int>("ItemSpicy") > 0)
                        {
                            dataSheet.Pictures.Add(b, lstCol + 6, (Stream)peppers.GetValue(item.Field<int>("ItemSpicy")), 5, 5);

                            dataSheet.Pictures[dataSheet.Pictures.Count - 1].AlignTopRightCorner(b, lstCol + 6);
                        }
                        a = b;
                        a++;
                        break;
                    }
                }

                for (int c = 3; c < menulength + 3; c++)
                {
                    sidepanelStyle2.ForegroundColor = System.Drawing.Color.Yellow;
                    sidepanelStyle.ForegroundColor = System.Drawing.Color.Yellow;
                    sidepanelStyle2.Font.Color = System.Drawing.Color.Yellow;
                    sidepanelStyle.Font.Color = System.Drawing.Color.Yellow;

                    var cur = dataCells[c, 0].StringValue.ToString();
                    var nex = dataCells[c + 1, 0].StringValue.ToString();

                    if (cur == nex)
                    {
                        //设置字体颜色和背景色
                        dataCells[c + 1, 0].SetStyle(sidepanelStyle2);
                    }
                    else
                    {                       
                        if (c + 1 != menulength + 3)
                        {
                            sidepanelStyle.Font.Color = System.Drawing.Color.Black;
                            dataCells[c + 1, 0].SetStyle(sidepanelStyle);
                        }
                    }
                    ////如果单元格无值，则背景色为白色
                    //if(string.IsNullOrWhiteSpace(dataCells[c, lstCol].StringValue.ToString()))
                    //{
                    //    dataStyle.ForegroundColor = System.Drawing.Color.White;
                    //    dataCells[c, lstCol].SetStyle(dataStyle);
                    //}
                    
                    dataCells[c, lstCol].SetStyle(dataStyle);
                    dataCells[c, lstCol + 1].SetStyle(dataStyle);
                    dataCells[c, lstCol + 2].SetStyle(dataStyle);
                    dataCells[c, lstCol + 3].SetStyle(dataStyle);
                    dataCells[c, lstCol + 4].SetStyle(dataStyle);
                    dataCells[c, lstCol + 5].SetStyle(dataStyle);
                    dataCells[c, lstCol + 6].SetStyle(dataStyle);
                }

                int bgn = 3;
                int egn = 1;
                for (int c = 3; c < menulength + 3; c++)
                {
                    var cur = dataCells[c, 0].StringValue.ToString();
                    var nex = dataCells[c + 1, 0].StringValue.ToString();

                    if (cur == nex)
                    {
                        egn++;
                        continue;
                    }
                    else if (cur != nex)
                    {
                        dataCells.Merge(bgn, 0, egn, 1);
                        bgn = c + 1;
                        egn = 1;
                    }
                }

                int bgn2 = 3;
                int egn2 = 1;
                for (int c = 3; c < menulength + 3; c++)
                {
                    var cur = dataCells[c, 1].StringValue.ToString();
                    var nex = dataCells[c + 1, 1].StringValue.ToString();

                    if (cur == nex)
                    {
                        egn2++;
                        continue;
                    }
                    else if (cur != nex)
                    {
                        dataCells.Merge(bgn2, 1, egn2, 1);
                        bgn2 = c + 1;
                        egn2 = 1;
                    }
                }

                dataSheet.AutoFitRows();
                dataCells.SetRowHeight(0, 60);
                dataCells.SetColumnWidth(lstCol, (int)dataCells.GetColumnWidth(lstCol) + 10);
                dataCells.SetColumnWidth(lstCol + 1, (int)dataCells.GetColumnWidth(lstCol + 1) + 10);
                dataCells.SetColumnWidth(lstCol + 2, (int)dataCells.GetColumnWidth(lstCol + 2) + 10);
                dataCells.SetColumnWidth(lstCol + 3, (int)dataCells.GetColumnWidth(lstCol + 3) + 10);
                dataCells.SetColumnWidth(lstCol + 4, (int)dataCells.GetColumnWidth(lstCol + 4) + 10);
                dataCells.SetColumnWidth(lstCol + 5, (int)dataCells.GetColumnWidth(lstCol + 5) + 10);
                dataCells.SetColumnWidth(lstCol + 6, (int)dataCells.GetColumnWidth(lstCol + 6) + 10);

                return excel.SaveToStream();

                //DownloadExcelFile(excel.SaveToStream());
                //var excelName = (DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + "WeeklyReport.xls").ToString();
                //excel.Save("C:/" + excelName);
                //System.Diagnostics.Process.Start("C:/" + excelName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //下载Excel工艺单
        public static MemoryStream ExportExcelMenuProcess(Dictionary<string, string> keyDic)
        {
            try
            {
                string filter = "";
                if (!string.IsNullOrWhiteSpace(keyDic["windowName"]))
                    filter = " and isnull(M.windowGUID,'')<>'' and isnull(S.windowGuid,'')<>'' ";
                else
                    filter = " and isnull(M.windowGUID,'')='' and isnull(S.windowGuid,'')='' ";


                string sql = string.Format("SELECT convert(int,row_number() OVER (partition by L.PRODUCTDESC ORDER BY L.PRODUCTDESC)) 'Index', M.COSTCENTERCODE,isnull(S.WINDOWNAME,'(none)') WINDOWNAME,CONVERT(VARCHAR(100), M.REQUIREDDATE, 20) AS REQUIREDDATE, L.PRODUCTDESC, M.ITEMGUID, M.ITEMCODE, M.ITEMTYPE, M.ITEMNAME_ZH, M.ItemName_EN, "
                    + "isnull(Q.Step,0) Step,Q.Content,M.ITEMNAME_EN, convert(decimal(18,0),M.REQUIREDQTY) REQUIREDQTY, M.ITEMCOST, M.OTHERCOST, M.ITEMUNITCODE, M.HEADGUID, M.PRODUCTGUID, M.PURCHASEPOLICY, CONVERT(VARCHAR(100), M.CREATETIME, 20) AS CREATETIME, "
                    + "M.CREATEUSER, CONVERT(VARCHAR(100), M.DELETETIME, 20) AS DELETETIME, M.DELETEUSER,(CASE WHEN M.LINKID = 0 THEN M.ID ELSE M.LINKID END) AS LINKID, "
                    + "datename(weekday, CONVERT(VARCHAR(100), M.REQUIREDDATE, 20)) WEEKDAY FROM MENUORDERHEAD (nolock) AS M INNER JOIN SALESORDERITEM (nolock) AS L ON M.SOITEMGUID = L.ITEMGUID AND L.STATUS <> '0' "
                    + "INNER JOIN PRODUCTTYPEDATA K ON K.id=L.ProductCode LEFT JOIN (select distinct WindowSort as Sort,windowGuid,WindowName,SOItemGuid from CCWindowMeals where deletetime is null) S ON S.WINDOWGUID=M.WindowGUID and M.SOITEMGUID=S.SOItemGuid "
                    + "LEFT JOIN TBLITEMPROCESS (nolock) Q on Q.ItemGUID = M.ItemGUID and Q.DeleteTime is null "
                    + "LEFT JOIN TBLITEM P ON P.ItemCode=M.ITEMCODE "
                    + "WHERE P.DELETETIME IS NULL and M.COSTCENTERCODE = '{0}' and Q.Type='cookway' "
                    + "AND M.DELETEUSER IS NULL AND M.REQUIREDDATE BETWEEN "
                    + "convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{1}'), 0)), 23) AND convert(varchar(10), (DATEADD(wk, DATEDIFF(wk, 0, '{1}'), 6)), 23) {2} "
                    + "order by L.ProductCode,L.PRODUCTDESC,S.Sort,M.REQUIREDDATE,S.WINDOWNAME,M.ITEMCODE,Q.Step", keyDic["costCenterCode"], keyDic["startDate"],filter);

                var menuProcessdata = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);
                var firstCol = menuProcessdata.AsEnumerable().Select(dr => dr.Field<string>("WINDOWNAME")).FirstOrDefault();

                int lstCol = 2;
                if (firstCol.Contains("none")) lstCol = 1;

                Workbook excel = new Workbook();

                Style titleStyle = excel.Styles[excel.Styles.Add()];
                titleStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                titleStyle.Font.Size = 15;
                titleStyle.Font.IsBold = true;

                Style tabletitleStyle = excel.Styles[excel.Styles.Add()];
                tabletitleStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                tabletitleStyle.Font.Size = 10;
                tabletitleStyle.Font.IsBold = true;
                tabletitleStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                tabletitleStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                tabletitleStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                tabletitleStyle.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);

                Style cellStyle = excel.Styles[excel.Styles.Add()];
                cellStyle.Font.Size = 10;
                cellStyle.Font.Color = System.Drawing.Color.Black;
                cellStyle.Pattern = BackgroundType.Solid;
                cellStyle.ForegroundColor = System.Drawing.Color.White;
                cellStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyle.IsTextWrapped = true;

                Style cellStyleWhite = excel.Styles[excel.Styles.Add()];
                cellStyleWhite.Font.Size = 10;
                cellStyleWhite.Font.Color = System.Drawing.Color.White;
                cellStyleWhite.Pattern = BackgroundType.Solid;
                cellStyleWhite.ForegroundColor = System.Drawing.Color.White;
                cellStyleWhite.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyleWhite.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyleWhite.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.White);

                Style cellProcess = excel.Styles[excel.Styles.Add()];
                cellProcess.Font.Size = 10;
                cellProcess.Font.Color = System.Drawing.Color.Black;
                cellProcess.Pattern = BackgroundType.Solid;
                cellProcess.ForegroundColor = System.Drawing.Color.White;
                cellProcess.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellProcess.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellProcess.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellProcess.IsTextWrapped = true;

                Style cellLastRow = excel.Styles[excel.Styles.Add()];
                cellLastRow.Font.Size = 10;
                cellLastRow.Font.Color = System.Drawing.Color.White;
                cellLastRow.Pattern = BackgroundType.Solid;
                cellLastRow.ForegroundColor = System.Drawing.Color.White;
                cellLastRow.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellLastRow.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellLastRow.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellLastRow.IsTextWrapped = true;

                string n = ""; string wd = "";
                for (int i = 0; i < 7; i++)
                {
                    if (i == 0) { n = "一"; wd = "Monday"; }
                    else if (i == 1) { n = "二"; wd = "Tuesday"; }
                    else if (i == 2) { n = "三"; wd = "Wednesday"; }
                    else if (i == 3) { n = "四"; wd = "Thursday"; }
                    else if (i == 4) { n = "五"; wd = "Friday"; }
                    else if (i == 5) { n = "六"; wd = "Saturday"; }
                    else if (i == 6) { n = "日"; wd = "Sunday"; }
                    if (i == 0) excel.Worksheets[i].Name = "周" + n;
                    else if (i > 0) excel.Worksheets.Add("周" + n);

                    excel.Worksheets[i].Cells.Merge(0, 0, 2, 3+lstCol);
                    excel.Worksheets[i].Cells[0, 0].PutValue(keyDic["costCenterCode"] + "菜谱工艺单" + ' ' + "日期:" + DateTime.Parse(keyDic["startDate"]).AddDays(i).ToString("yyyy-MM-dd")+ ' ' + "周" + n);
                    excel.Worksheets[i].Cells[0, 0].SetStyle(titleStyle);
                    excel.Worksheets[i].Cells[3, 0].PutValue("分类");
                    if (lstCol == 2) excel.Worksheets[i].Cells[3, 1].PutValue("窗口");
                    excel.Worksheets[i].Cells[3, lstCol].PutValue("菜名");
                    excel.Worksheets[i].Cells[3, lstCol+1].PutValue("份数");
                    excel.Worksheets[i].Cells[3, lstCol+2].PutValue("加工步骤");
                    excel.Worksheets[i].Cells[3, lstCol + 3].PutValue("备注");

                    excel.Worksheets[i].Cells[3, 0].SetStyle(tabletitleStyle);
                    if(lstCol==2) excel.Worksheets[i].Cells[3, 1].SetStyle(tabletitleStyle);
                    excel.Worksheets[i].Cells[3, lstCol].SetStyle(tabletitleStyle);
                    excel.Worksheets[i].Cells[3, lstCol+1].SetStyle(tabletitleStyle);
                    excel.Worksheets[i].Cells[3, lstCol+2].SetStyle(tabletitleStyle);
                    excel.Worksheets[i].Cells[3, lstCol + 3].SetStyle(tabletitleStyle);
                    
                    int row = 3;
                    foreach (var item in menuProcessdata.AsEnumerable().Where(dr => dr.Field<string>("WEEKDAY") == wd))
                    {

                        row++;
                        excel.Worksheets[i].Cells[row, 0].PutValue(item.Field<string>("PRODUCTDESC"));
                        if (lstCol == 2) excel.Worksheets[i].Cells[row, 1].PutValue(item.Field<string>("WINDOWNAME"));
                        excel.Worksheets[i].Cells[row, lstCol].PutValue(item.Field<string>("ITEMNAME_ZH") + ' ' + item.Field<string>("ItemName_EN"));
                        excel.Worksheets[i].Cells[row, lstCol+1].PutValue(item.Field<decimal>("REQUIREDQTY")+" "+item.Field<string>("ITEMUNITCODE"));
                        excel.Worksheets[i].Cells[row, lstCol+2].PutValue((item.Field<int>("Step").ToString() == "0" ? "" : item.Field<int>("Step").ToString()) + (item.Field<int>("Step").ToString() == "0" ? "" : "、")
                        + item.Field<string>("Content"));

                        if (item.Field<int>("Step").ToString() != "1" && item.Field<int>("Step").ToString() != "0")
                        {
                            excel.Worksheets[i].Cells[row, 0].SetStyle(cellStyleWhite);
                            if (lstCol == 2) excel.Worksheets[i].Cells[row, 1].SetStyle(cellStyleWhite);
                            excel.Worksheets[i].Cells[row, lstCol].SetStyle(cellStyleWhite);
                            excel.Worksheets[i].Cells[row, lstCol+1].SetStyle(cellStyleWhite);
                            excel.Worksheets[i].Cells[row, lstCol+2].SetStyle(cellProcess);
                            excel.Worksheets[i].Cells[row, lstCol+3].SetStyle(cellProcess);
                        }
                        else
                        {
                            if (excel.Worksheets[i].Cells[row, 0].StringValue.ToString() == excel.Worksheets[i].Cells[row - 1, 0].StringValue.ToString())
                                excel.Worksheets[i].Cells[row, 0].SetStyle(cellStyleWhite);
                            else
                                excel.Worksheets[i].Cells[row, 0].SetStyle(cellStyle);
                            if (lstCol == 2) {
                                if (excel.Worksheets[i].Cells[row, 1].StringValue.ToString() == excel.Worksheets[i].Cells[row - 1, 1].StringValue.ToString())
                                    excel.Worksheets[i].Cells[row, 1].SetStyle(cellStyleWhite);
                                else
                                    excel.Worksheets[i].Cells[row, 1].SetStyle(cellStyle);
                            } 
                            excel.Worksheets[i].Cells[row, lstCol].SetStyle(cellStyle);
                            excel.Worksheets[i].Cells[row, lstCol+1].SetStyle(cellStyle);
                            excel.Worksheets[i].Cells[row, lstCol+2].SetStyle(cellProcess);
                            excel.Worksheets[i].Cells[row, lstCol+3].SetStyle(cellProcess);
                        }
                    }

                    var s = menuProcessdata.AsEnumerable().Where(dr => dr.Field<string>("WEEKDAY") == wd).ToList().ToList();
                    if (menuProcessdata.AsEnumerable().Where(dr => dr.Field<string>("WEEKDAY") == wd).ToList().Count() == 0)
                    {
                        excel.Worksheets[i].Cells[row + 1, 0].PutValue("无数据 (No Data)");
                        continue;
                    }

                    excel.Worksheets[i].Cells[row, 0].SetStyle(cellLastRow);
                    if (lstCol == 2) excel.Worksheets[i].Cells[row, 1].SetStyle(cellLastRow);
                    excel.Worksheets[i].Cells[row, lstCol].SetStyle(cellLastRow);
                    excel.Worksheets[i].Cells[row, lstCol+1].SetStyle(cellLastRow);

                    excel.Worksheets[i].Cells.SetColumnWidth(0, 20);
                    if (lstCol == 2) excel.Worksheets[i].Cells.SetColumnWidth(1, 20);
                    excel.Worksheets[i].Cells.SetColumnWidth(lstCol, 30);
                    excel.Worksheets[i].Cells.SetColumnWidth(lstCol+1, 8);
                    excel.Worksheets[i].Cells.SetColumnWidth(lstCol+2, 130);
                    excel.Worksheets[i].Cells.SetColumnWidth(lstCol+3, 80);

                    excel.Worksheets[i].AutoFitColumns();
                }

                return excel.SaveToStream();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //下载微信充值报告
        public static MemoryStream WechatRechargeReport(Dictionary<string, string> keyDic)
        {
            try
            {
                string filter = "";
                string filterDBCC = "";

                if (!string.IsNullOrWhiteSpace(keyDic["startDate"]) && !string.IsNullOrWhiteSpace(keyDic["endDate"]))
                    filter = string.Format(" and (cast(left(a1.time_end, 8) as date)) between '{0}' and '{1}' ", 
                        DateTime.Parse(keyDic["startDate"]).ToString("yyyy-MM-dd"), DateTime.Parse(keyDic["endDate"]).ToString("yyyy-MM-dd"));

                if (!string.IsNullOrWhiteSpace(keyDic["companyCode"]))
                    filterDBCC = string.Format(" and a3.DBName = '{0}'", keyDic["companyCode"]);
                else
                    filterDBCC = string.Format(" and a2.CostcenterCode = '{0}'", keyDic["costCenterCode"]);

                string sql = string.Format("select distinct a3.DBName,a2.CostcenterCode,a1.cardId,a4.UserName, "
                    + "(cast(left(a1.time_end,8) as date)) as endDate,sum(isnull(a1.total_fee,0)*1.0 /100) value "
                    + "from WxOrder a1,WxUserMast a2, CCMast a3,wxUserCard a4 "
                    + "where a1.NotifyTime is not null and a1.return_code = 'SUCCESS' "
                    + "and a1.result_code = 'SUCCESS' and a1.openid = a2.WechatId and a4.WechatId=a1.openid and a1.CardId=a4.CardId "
                    + "and a3.CostCenterCode = a2.CostcenterCode "+ filterDBCC + " and a2.DeleteTime is null "
                    + filter
                    + "group by a3.DBName,a2.CostcenterCode,a1.CardId,a4.UserName,(cast(left(a1.time_end, 8) as date)) order by a2.CostcenterCode,cast(left(a1.time_end,8) as date),a1.cardId ", keyDic["companyCode"]);

                //and a4.DeleteTime is null

                var data = SqlServerHelper.GetDataTable(SqlServerHelper.salesorderConn(), sql);

                Workbook excel = new Workbook();
                if (data == null || data.Rows.Count == 0)
                {
                    excel.Worksheets[0].Cells[0, 0].PutValue("No Data");
                }

                else
                {
                    var siteList = data.AsEnumerable().
                        GroupBy(dr => dr.Field<string>("costCenterCode")).
                        Select(r => new
                        {
                            costCenterCode = r.Key,
                            value = data.AsEnumerable().Where(dr => dr.Field<string>("costCenterCode") == r.Key).Sum(x => x.Field<decimal>("value"))
                        }
                    ).Distinct().ToList();


                    Style titleStyle = excel.Styles[excel.Styles.Add()];
                    titleStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    titleStyle.Font.Size = 10;
                    titleStyle.Font.IsBold = true;

                    Style numberStyle = excel.Styles[excel.Styles.Add()];
                    numberStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    numberStyle.Custom = "#,###.00";

                    excel.Worksheets[0].Name = "汇总";
                    excel.Worksheets[0].Cells[0, 0].PutValue("营运点");
                    excel.Worksheets[0].Cells[0, 0].SetStyle(titleStyle);

                    excel.Worksheets[0].Cells[0, 1].PutValue("充值金额");
                    excel.Worksheets[0].Cells[0, 1].SetStyle(titleStyle);


                    int row = 0;
                    foreach (var item in siteList)
                    {
                        row++;
                        excel.Worksheets[0].Cells[row, 0].PutValue(item.costCenterCode);
                        excel.Worksheets[0].Cells[row, 1].PutValue(item.value);
                        excel.Worksheets[0].Cells[row, 1].SetStyle(numberStyle);

                        excel.Worksheets.Add(item.costCenterCode);
                        excel.Worksheets[row].Cells[0, 0].PutValue("营运点");
                        excel.Worksheets[row].Cells[0, 0].SetStyle(titleStyle);

                        excel.Worksheets[row].Cells[0, 1].PutValue("卡号");
                        excel.Worksheets[row].Cells[0, 1].SetStyle(titleStyle);

                        excel.Worksheets[row].Cells[0, 2].PutValue("用户");
                        excel.Worksheets[row].Cells[0, 2].SetStyle(titleStyle);

                        excel.Worksheets[row].Cells[0, 3].PutValue("充值时间");
                        excel.Worksheets[row].Cells[0, 3].SetStyle(titleStyle);

                        excel.Worksheets[row].Cells[0, 4].PutValue("充值金额");
                        excel.Worksheets[row].Cells[0, 4].SetStyle(titleStyle);


                        var detailData = data.AsEnumerable().Where(dr => dr.Field<string>("CostcenterCode") == item.costCenterCode).ToList();


                        for (int i = 0; i < detailData.Count; i++)
                        {
                            var dr = detailData[i];
                            excel.Worksheets[row].Cells[i + 1, 0].PutValue(dr.Field<string>("CostcenterCode"));
                            excel.Worksheets[row].Cells[i + 1, 1].PutValue(dr.Field<string>("cardId"));
                            excel.Worksheets[row].Cells[i + 1, 2].PutValue(dr.Field<string>("UserName"));
                            excel.Worksheets[row].Cells[i + 1, 3].PutValue(dr.Field<DateTime>("endDate").ToString("yyyy-MM-dd"));
                            excel.Worksheets[row].Cells[i + 1, 4].PutValue(dr.Field<decimal>("value"));
                            excel.Worksheets[row].Cells[i + 1, 4].SetStyle(numberStyle);
                        }

                        excel.Worksheets[row].AutoFitColumns();
                    }
                }
                excel.Worksheets[0].AutoFitColumns();

                return excel.SaveToStream();
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public static MemoryStream SiteSurveyReport(Dictionary<string, string> keyDic)
        {
            try
            {
               
                List<SiteSurvey> list = new Aden.DAL.SiteDIY.SiteDIYFactory().SiteSurveyDetails(keyDic["siteGuid"],"");
                if (list == null || !list.Any())
                    return null;

                Workbook excel = new Workbook();
                Worksheet dataSheet = excel.Worksheets[0];
                dataSheet.Name = "餐厅满意度评估表";
                Cells dataCells = dataSheet.Cells;

                Style titleStyle = excel.Styles[excel.Styles.Add()];
                titleStyle.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                titleStyle.Font.Size = 10;
                titleStyle.Font.IsBold = true;
                titleStyle.Pattern = BackgroundType.Solid;
                titleStyle.ForegroundColor = System.Drawing.Color.LightSkyBlue;

                Style cellStyle = excel.Styles[excel.Styles.Add()];
                cellStyle.Font.Size = 10;
                cellStyle.Font.Color = System.Drawing.Color.Black;
                cellStyle.Pattern = BackgroundType.Solid;
                cellStyle.ForegroundColor = System.Drawing.Color.White;
                cellStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyle.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                cellStyle.IsTextWrapped = true;

                #region 表头

                dataCells[0, 0].PutValue("用户代码");
                dataCells[0, 0].SetStyle(titleStyle);
                dataCells[0, 1].PutValue("问卷提交时间");
                dataCells[0, 1].SetStyle(titleStyle);

                List<SiteSurvey> SurveyDetails = list;
                List<dynamic> surveyItems =new List<dynamic>();
                foreach (SiteSurvey row in SurveyDetails)
                {
                    if (row.surveyDetails.Count > 0)
                    {
                        var details = row.surveyDetails;
                        foreach(SiteSurvey item in details)
                        {

                            SiteSurvey obj= new SiteSurvey() ;
                            obj.surveyItem = item.surveyItem;
                            obj.createUserID = item.createUserID;
                            obj.createUserName = item.createUserName;
                            obj.department = item.department;
                            obj.createTime = item.createTime;
                            obj.userAnswer = item.userAnswer;
                            obj.sort = item.sort;
                            surveyItems.Add(obj);
                        }
                    }
                   
                }

                if (surveyItems == null || !surveyItems.Any())
                    return null;

                //问卷提问项
                List<SiteSurvey> ItemsTemp = new List<SiteSurvey>();
                List<string> Items = new List<string>();
                for (int i = 0; i < surveyItems.Count; i++)
                {
                    SiteSurvey obj = new SiteSurvey();
                    obj.surveyItem = surveyItems[i].surveyItem;
                    obj.sort= surveyItems[i].sort;
                    ItemsTemp.Add(obj);
                }

                if (ItemsTemp != null && ItemsTemp.Any())
                   Items= ItemsTemp.OrderBy(dr=>dr.sort).Select(ds=>ds.surveyItem).Distinct().ToList<string>();//问卷提问项

                int a = 1;
                //设置表头-问卷提问项
                foreach (var item in Items)
                {
                    a++;
                    dataCells[0, a].PutValue(item);
                    dataCells[0, a].SetStyle(titleStyle);
                }

                List<SiteSurvey> userInfo = new List<SiteSurvey>();

                #endregion

                int r = 0;
               
                foreach (var item in list)
                {
                    int j = 0;
                    r++;
                    dataCells[r, j].PutValue(item.createUserID);
                    dataCells[r, j++].SetStyle(cellStyle);
                    dataCells[r, j].PutValue(item.createTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    dataCells[r, j++].SetStyle(cellStyle);

                    if(item.surveyDetails!=null && item.surveyDetails.Any())
                    {
                        dynamic row = item.surveyDetails;
                        for(int s=0;s<row.Count; s++)
                        {
                            for(int x = j; x <= j + surveyItems.Count; x++)
                            {
                                if (row[s].surveyItem == dataCells[0, x].StringValue)
                                {
                                    dataCells[r, x].PutValue(row[s].userAnswer);
                                    dataCells[r, x].SetStyle(cellStyle);
                                }
                            }

                        }
                    }
                }

                int maxCol = 2+Items.Count;
                int maxRow = r;

                for(int rw = 1; rw < maxRow+1; rw++)
                {
                    
                    for(int col = 0; col < maxCol; col++)
                    {
                        
                        dataCells[rw, col].SetStyle(cellStyle);
                    }
                }

                dataSheet.AutoFitRows();
                return excel.SaveToStream();

            }
            catch(Exception ex)
            {
                throw ex;
            }

        }
        

    }
        
}
