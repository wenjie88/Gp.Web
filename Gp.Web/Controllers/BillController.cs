using Gp.Web.Code.Common;
using Gp.Web.Code.Entity;
using Gp.Web.Code.Manager;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gp.Web.Controllers
{
    [Filters.ManagerAuthorize]
    public class BillController : Controller
    {

        private Code.Dao.TradeInfoDao _TradeInfoDao = new Code.Dao.TradeInfoDao();

        // GET: Bill
        public ActionResult Index()
        {
            return View();
        }





        //
        public ActionResult GetBill(string StartTime, string EndTime)
        {
            //时间范围的开始日期的卖出 不计算。 结束日期的买入 不计算
            List<TradeInfo> list = new List<TradeInfo>();
            if(String.IsNullOrEmpty(StartTime) || String.IsNullOrEmpty(EndTime))
            {
                list = _TradeInfoDao.GetList_ByUserId(AdminManager.GetUser().UserId).ToList();
            }
            else
            {
                list = _TradeInfoDao.GetList_ByUserIdAndDealDate(AdminManager.GetUser().UserId, StartTime + " 00:00:00", EndTime + " 23:59:59").ToList();
            }

            double _TotalDealAmount = 0;    //总成交金额
            double _TotalHappenAmount = 0;  //总收益
            double _TotalPoundage = 0;      //总手续费
            double _TotalStamp_Tax = 0;     //总印花税
            double _TotalOther_Free = 0;    //总其他杂费( 过户费 )
            int _TotalCount = 0;            //总成次数

            foreach (TradeInfo item in list)
            {
                if (item.Code == null || item.Code == "") continue;

                _TotalDealAmount += item.DealAmount;
                _TotalHappenAmount += item.HappenAmount;
                _TotalPoundage += item.Poundage;
                _TotalStamp_Tax += item.Stamp_Tax;
                _TotalOther_Free += item.Other_Free;
                _TotalCount += 1;

            }

            var resdata = new
            {
                TotalDealAmount = _TotalDealAmount,      //总成交金额
                TotalHappenAmount = _TotalHappenAmount,  //总收益
                TotalPoundage = _TotalPoundage,          //总手续费
                TotalStamp_Tax = _TotalStamp_Tax,        //总印花税
                TotalOther_Free = _TotalOther_Free,      //总其他杂费( 过户费 )
                TotalCount = _TotalCount,                //总成次数
                Data = list
            };


            //var resdata = new
            //{
            //    TotalDealAmount = list.Sum(x => x.DealAmount).ToString("0.00"),  //总成交金额
            //    TotalHappenAmount = list.Sum(x => x.HappenAmount).ToString("0.00"),  //总收益
            //    TotalPoundage = list.Sum(x => x.Poundage).ToString("0.00"),  //总手续费
            //    TotalStamp_Tax = list.Sum(x => x.Stamp_Tax).ToString("0.00"),  //总印花税
            //    TotalOther_Free = list.Sum(x => x.Other_Free).ToString("0.00"),  //总其他杂费( 过户费 )
            //    TotalCount = list.Count,  //总成次数
            //    data = list.GroupBy(x => x.Code).Select(x => new
            //    {
            //        InCome = x.Sum(s => s.HappenAmount).ToString("0.00"),
            //        tradeList = x
            //    })
            //};
            return Content(JSONHelper.SerializeObject(new { status = "ok", msg = "ok", result = resdata }));
        }





        public ActionResult UpLoad()
        {
            if (Request.HttpMethod.ToUpper() == "GET")
            {
                return View();
            }
            else if (Request.HttpMethod.ToUpper() == "POST")
            {
                var files = Request.Files[0];
                //excel 导入
                List<TradeInfo> list = Import(files.InputStream);
                if (list != null)
                {
                    //编辑合同编号 , （方便数据库删除重复数据）
                    var User = Code.Manager.AdminManager.GetUser();
                    if (User != null)
                    {
                        list.ForEach(item =>
                        {
                            //用户Id
                            item.UserId = User.UserId;

                            if (item.Code == null)
                            {
                                item.TradeNum = item.DealDate.ToString("yyyyMMddHHmmss" + item.Operation + item.Yu);
                            }
                            else
                            {
                                if (item.Code.Length < 6)
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

                }
                return Content("");
            }
            else
            {
                return View("Error");
            }

        }


        /// <summary>读取excel  
        /// 默认第一行为标头  
        /// </summary>  
        /// <param name="strFileName">excel文档路径</param>  
        /// <returns></returns>  
        private static List<TradeInfo> Import(Stream fileStream)
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