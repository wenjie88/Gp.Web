using Gp.Web.Code.Entity;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gp.Web.Controllers
{
    public class InfoController : Controller
    {

        private Code.Dao.TradeInfoDao _TradeInfoDao = new Code.Dao.TradeInfoDao();

        // GET: Info
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult UpLoad()
        {
            var files = Request.Files[0];
            List<TradeInfo> list = Import(files.InputStream);
            if (list != null)
            {
                list.ForEach(item =>
                {
                    if (item.Code == null)
                    {
                        item.TradeNum = item.DealDate.ToString("yyyyMMddHHmmss" + item.Operation + item.Yu);
                    }
                    else
                    {
                        if(item.Code.Length < 6)
                        {
                            var len = item.Code.Length;
                            for (int i = 0; i < 6 - len; i++)
                            {
                                item.Code = "0" + item.Code;
                            }
                        }
                        item.TradeNum = item.DealDate.ToString("yyyyMMddHHmmss" + item.Code + item.TradeNum);
                    }
                });
                bool b = _TradeInfoDao.Add(list);
            }
            return Content("");
        }


        /// <summary>读取excel  
        /// 默认第一行为标头  
        /// </summary>  
        /// <param name="strFileName">excel文档路径</param>  
        /// <returns></returns>  
        public static List<TradeInfo> Import(Stream fileStream)
        {
            //DataTable dt = new DataTable();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("成交日期", "DealDate");
            dic.Add("成交时间", "DealTime");
            dic.Add("证券代码", "Code");
            dic.Add("证券名称", "Name");
            dic.Add("成交数量", "DealCount");
            dic.Add("成交均价", "DealAvgPrice");
            dic.Add("成交金额", "DealAmount");
            dic.Add("发生金额", "HappenAmount");
            dic.Add("手续费", "Poundage");
            dic.Add("印花税", "Stamp_Tax");
            dic.Add("其他杂费", "Other_Free");
            dic.Add("操作", "Operation");
            dic.Add("资金余额", "Yu");
            //dic.Add("市场名称", "MarketName");
            //dic.Add("股东帐户", "Account");
            dic.Add("合同编号", "TradeNum");

            IWorkbook hssfworkbook = WorkbookFactory.Create(fileStream);
            ISheet sheet = hssfworkbook.GetSheetAt(0);
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;

            List<string> title = new List<string>();
            for (int j = 0; j < cellCount; j++)
            {
                ICell cell = headerRow.GetCell(j);
                title.Add(cell.StringCellValue);

                //dt.Columns.Add(cell.ToString());
            }


            List<TradeInfo> TradeInfoList = new List<TradeInfo>();
            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                //DataRow dataRow = dt.NewRow();

                TradeInfo info = new TradeInfo();
                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    if (row.GetCell(j) != null)
                    {
                        //dataRow[j] = row.GetCell(j).ToString();

                        var titletStr = title[j];
                        if (!dic.ContainsKey(titletStr)) continue;

                        var model_colName = dic[titletStr];
                        System.Reflection.PropertyInfo propertyInfo = info.GetType().GetProperty(model_colName); //获取指定名称的属性
                        if (propertyInfo == null) continue;
                        switch (propertyInfo.PropertyType.Name)
                        {
                            case "DateTime":
                                if (model_colName == "DealDate")
                                {
                                    var d = "";
                                    if (dic.ContainsKey(title[j + 1]) && title[j + 1] == "成交时间")
                                    {
                                        var _date = row.GetCell(j).ToString();
                                        var _time = row.GetCell(j + 1).DateCellValue.ToString("HH:mm:ss");
                                        d = _date + " " + _time;
                                    }
                                    propertyInfo.SetValue(info, DateTime.ParseExact(d, "yyyyMMdd HH:mm:ss", CultureInfo.CurrentCulture));
                                }
                                break;
                            case "Int32":
                                propertyInfo.SetValue(info, Convert.ToInt32(row.GetCell(j).ToString()));
                                break;
                            case "Double":
                                propertyInfo.SetValue(info, Convert.ToDouble(row.GetCell(j).ToString()));
                                break;
                            default:
                                propertyInfo.SetValue(info, row.GetCell(j).ToString());
                                break;
                        }

                    }
                }

                TradeInfoList.Add(info);

                //dt.Rows.Add(dataRow);
            }
            return TradeInfoList;
        }


    }
}