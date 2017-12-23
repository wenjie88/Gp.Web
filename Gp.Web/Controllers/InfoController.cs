using Gp.Web.Code.Common;
using Gp.Web.Code.Entity;
using Gp.Web.Code.Manager;
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
    [Filters.ManagerAuthorize]
    public class InfoController : Controller
    {
        private Code.Dao.TradeInfoDao _TradeInfoDao = new Code.Dao.TradeInfoDao();


        // GET: Info
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GetInfo(string StartTime, string EndTime)
        {
            //
            List<TradeInfo> list = _TradeInfoDao.GetList_ByUserId(AdminManager.GetUser().UserId).ToList();


            //无论前端选择日期范围从哪里开始到哪里，  这里必须从用户第一次买股开始计算,  最后返回用户选择日期范围的数据就行了。 

            //上个月还没结算的股票缓存，到下个月拿出来看是否可以结算
            Dictionary<string, List<TradeInfo>> d = new Dictionary<string, List<TradeInfo>>();

            //统计每个月收益
            var month_group = list.Where(x => !String.IsNullOrEmpty(x.Code)).GroupBy(x => "" + x.DealDate.Year + "年" + x.DealDate.Month.ToString().PadLeft(2, '0') + "月");
            List<MonthSummary> MonthList = new List<MonthSummary>();
            foreach (var group in month_group)
            {
                //这个月的所有股票买卖交割单
                var j_list = group.OrderBy(x => x.DealDate);

                //这个月的所有股票结算 (可以结算的股票， 不包含没完全卖出的股票)
                List<Summary> AllSummary = new List<Summary>();


                ////一个月当中，可能会对同一个股票，多次完整结算。即多次完整买卖(今日买入100股，明天卖出100股，然后过10天又重新买100股，后天又卖出100股) 
                //需要结算的股票缓存, 结算完就会移除
                foreach (TradeInfo item in j_list)
                {
                    if (d.ContainsKey(item.Code))
                    {
                        d[item.Code].Add(item);

                        //判断是否可以结算了, 一旦买入数量等于卖出数量， 那就等同已经结算(完成一个完整的买入卖出) ,那就可以统计收益
                        Summary summary = IsCanJieSuan(d[item.Code]);
                        //可以结算
                        if (summary != null)
                        {
                            AllSummary.Add(summary);

                            //移除缓存
                            d.Remove(item.Code);
                        }

                    }
                    else
                    {
                        d.Add(item.Code, new List<TradeInfo>() { item });
                    }
                }

                MonthList.Add(new MonthSummary
                {
                    Date = group.Key,
                    MonthShouYi = Math.Round(AllSummary.Sum(x => x.TotalHappenAmount), 2, MidpointRounding.AwayFromZero),
                    MonthTradeCount = AllSummary.Count,
                    DataList = AllSummary
                });


            }

            //如果有选择日期范围， 则返回日期范围的数据
            if (!(String.IsNullOrEmpty(StartTime) && String.IsNullOrEmpty(EndTime)))
            {
                DateTime start = DateTime.ParseExact(StartTime, "yyyy-MM", CultureInfo.InvariantCulture);
                DateTime end = DateTime.ParseExact(EndTime, "yyyy-MM", CultureInfo.InvariantCulture);

                MonthList = MonthList.Where(x =>
                {
                    var m = DateTime.ParseExact(x.Date, "yyyy年MM月", CultureInfo.InvariantCulture);
                    return m >= start && m <= end;
                }).ToList();
            }




            var allSummaryList = MonthList.SelectMany(x => x.DataList);

            return Content(JSONHelper.SerializeObject(new
            {
                status = "ok",
                msg = "ok",
                totalSouyi = Math.Round(MonthList.Sum(x => x.MonthShouYi), 2, MidpointRounding.AwayFromZero),
                totalDeal = MonthList.Sum(x => x.MonthTradeCount),
                MinRang = allSummaryList.OrderBy(x => x.TotalHappenAmount).Take(5),
                MaxRang = allSummaryList.OrderByDescending(x => x.TotalHappenAmount).Take(5),
                SigGpMaxRang = allSummaryList.GroupBy(x=>x.Code).Select(x=>x.Sum(y=>y.TotalHappenAmount)),
                SuccessRate = Math.Round(allSummaryList.Where(x => x.TotalHappenAmount > 0).Count() * 1.0 / allSummaryList.Count(), 4, MidpointRounding.AwayFromZero),
                result = MonthList,
                d = d
            }));


        }



        private Summary IsCanJieSuan(List<TradeInfo> tradelist)
        {
            var AllDealCount = tradelist.Sum(x =>
            {
                if (x.Operation == "证券买入")
                    return x.DealCount;
                else if (x.Operation == "证券卖出")
                    return -(x.DealCount);
                else
                    return 0;
            });


            //可以结算
            if (AllDealCount == 0)
            {
                //开始结算
                Summary summary = new Summary();

                foreach (TradeInfo trade in tradelist)
                {
                    summary.TotalDealAmount += trade.DealAmount;
                    summary.TotalHappenAmount += trade.HappenAmount;
                    summary.TotalPoundage += trade.Poundage;
                    summary.TotalStampTax += trade.Stamp_Tax;
                    summary.TotalOtherFree += trade.Other_Free;
                    summary.TotalCount += 1;
                    switch (trade.Operation)
                    {
                        case "证券买入":
                            summary.TotalBuyCount += trade.DealCount;
                            break;
                        case "证券卖出":
                            summary.TotalSellCount += trade.DealCount;
                            break;
                        default:

                            break;
                    }
                }
                summary.Code = tradelist.First().Code;
                summary.Name = tradelist.First().Name;
                summary.JiaoGeList = tradelist;

                summary.TotalDealAmount = Math.Round(summary.TotalDealAmount, 2, MidpointRounding.AwayFromZero);
                summary.TotalHappenAmount = Math.Round(summary.TotalHappenAmount, 2, MidpointRounding.AwayFromZero);
                summary.TotalPoundage = Math.Round(summary.TotalPoundage, 2, MidpointRounding.AwayFromZero);
                summary.TotalStampTax = Math.Round(summary.TotalStampTax, 2, MidpointRounding.AwayFromZero);
                summary.TotalOtherFree = Math.Round(summary.TotalOtherFree, 2, MidpointRounding.AwayFromZero);

                return summary;
            }
            else
            {
                return null;
            }
        }



        public class MonthSummary
        {
            public string Date { get; set; }

            public double MonthShouYi { get; set; }

            public double MonthTradeCount { get; set; }

            public List<Summary> DataList { get; set; }
        }

        public class Summary
        {
            public string Code { get; set; }

            public string Name { get; set; }

            public List<TradeInfo> JiaoGeList { get; set; }

            public double TotalDealAmount { get; set; }    //总成交金额

            public double TotalHappenAmount { get; set; }  //总收益

            public double TotalPoundage { get; set; }      //总手续费

            public double TotalStampTax { get; set; }     //总印花税

            public double TotalOtherFree { get; set; }    //总其他杂费( 过户费 )

            public int TotalCount { get; set; }            //总成交次数

            public int TotalSellCount { get; set; }             //卖出股数

            public int TotalBuyCount { get; set; }              //买入股数


        }

    }
}