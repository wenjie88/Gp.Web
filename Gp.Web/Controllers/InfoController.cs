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
            List<TradeInfo> list = new List<TradeInfo>();
            if (String.IsNullOrEmpty(StartTime) || String.IsNullOrEmpty(EndTime))
            {
                list = _TradeInfoDao.GetList_ByUserId(AdminManager.GetUser().UserId).ToList();
            }
            else
            {
                list = _TradeInfoDao.GetList_ByUserIdAndDealDate(AdminManager.GetUser().UserId, StartTime + " 00:00:00", EndTime + " 23:59:59").ToList();
            }


            //无论前端选择日期范围从哪里开始到哪里，  这里必须从用户第一次买股开始计算,  最后返回用户选择日期范围的数据就行了。 

            //统计每个月收益
            var g = list.GroupBy(x => "" + x.DealDate.Year + x.DealDate.Month).Select(group =>
            {
                //这个月的股票分组

                //根据代码分组股票
                group.GroupBy(x => x.Code).Select(codeGroup =>
                {
                    //这里就是同一只股票的所有交割单 

                    //遍历， 统计这个股票在这个月中 买入卖出总数。
                    int buyCount = 0;
                    int sellerCount = 0;
                    double shouyi = 0.00;
                    foreach (TradeInfo item in codeGroup)
                    {
                        switch (item.Operation)
                        {
                            case "证券买入":
                                buyCount += item.DealCount;
                                break;
                            case "证券卖出":
                                sellerCount += item.DealCount;
                                break;
                        }

                        //一旦买入数量等于卖出数量， 那就等同已经结算(完成一个完整的买入卖出) ,那就可以统计收益
                        //一个月当中，可能会对同一个股票，多次完整结算。即多次完整买卖(今日买入100股，明天卖出100股，然后过10天又重新买100股，后天又卖出100股) 
                        if (buyCount == sellerCount)
                        {
                            shouyi = codeGroup.Sum(x => x.HappenAmount);
                        }
                    }


                    //如果不等于，  那就证明没全部卖出， 那就不结算。


                    return new
                    {
                        Code = codeGroup.First().Code,
                        Name = codeGroup.First().Name,
                        Shouyi = shouyi
                    };
                });



                double _TotalDealAmount = 0;    //总成交金额
                double _TotalHappenAmount = 0;  //总收益
                double _TotalPoundage = 0;      //总手续费
                double _TotalStamp_Tax = 0;     //总印花税
                double _TotalOther_Free = 0;    //总其他杂费( 过户费 )
                int _TotalCount = 0;            //总成交次数
                int _TotalSellCount = 0;             //卖出股数
                int _TotalBuyCount = 0;              //买入股数

                foreach (TradeInfo item in group)
                {
                    _TotalDealAmount += item.DealAmount;
                    _TotalHappenAmount += item.HappenAmount;
                    _TotalPoundage += item.Poundage;
                    _TotalStamp_Tax += item.Stamp_Tax;
                    _TotalOther_Free += item.Other_Free;
                    _TotalCount += 1;
                    switch (item.Operation)
                    {
                        case "证券买入":
                            _TotalBuyCount += item.DealCount;
                            break;
                        case "证券卖出":
                            _TotalSellCount += item.DealCount;
                            break;
                        default:

                            break;
                    }
                }

                return new
                {
                    date = group.Key,
                    TotalDealAmount = _TotalDealAmount.ToString("0.00"),    //总成交金额
                    TotalHappenAmount = _TotalHappenAmount.ToString("0.00"),  //总收益
                    TotalPoundage = _TotalPoundage.ToString("0.00"),      //总手续费
                    TotalStamp_Tax = _TotalStamp_Tax.ToString("0.00"),    //总印花税
                    TotalOther_Free = _TotalOther_Free.ToString("0.00"),    //总其他杂费( 过户费 )
                    TotalCount = _TotalCount,            //总成交次数
                    TotalSellCount = _TotalSellCount,              //卖出股数
                    TotalBuyCount = _TotalBuyCount,                //买入股数
                    Data = group.ToList()
                };
            });









            //int y_to_z_count = 0; //银行转证券次数
            //double y_to_z_total = 0; //银行转证券总金额

            //int z_to_y_count = 0; //证券转银行次数
            //double z_to_y_total = 0; //证券转银行总金额

            //代码分组
            var code_group = list.GroupBy(x => x.Code).Select(group =>
            {
                double _TotalDealAmount = 0;    //总成交金额
                double _TotalHappenAmount = 0;  //总收益
                double _TotalPoundage = 0;      //总手续费
                double _TotalStamp_Tax = 0;     //总印花税
                double _TotalOther_Free = 0;    //总其他杂费( 过户费 )
                int _TotalCount = 0;            //总成交次数
                int _TotalSellCount = 0;             //卖出股数
                int _TotalBuyCount = 0;              //买入股数

                foreach (TradeInfo item in group)
                {
                    _TotalDealAmount += item.DealAmount;
                    _TotalHappenAmount += item.HappenAmount;
                    _TotalPoundage += item.Poundage;
                    _TotalStamp_Tax += item.Stamp_Tax;
                    _TotalOther_Free += item.Other_Free;
                    _TotalCount += 1;
                    switch (item.Operation)
                    {
                        case "证券买入":
                            _TotalBuyCount += item.DealCount;
                            break;
                        case "证券卖出":
                            _TotalSellCount += item.DealCount;
                            break;
                        default:

                            break;
                    }
                }

                return new
                {
                    Code = group.Key,
                    Name = group.First().Name,
                    TotalDealAmount = _TotalDealAmount.ToString("0.00"),    //总成交金额
                    TotalHappenAmount = _TotalHappenAmount.ToString("0.00"),  //总收益
                    TotalPoundage = _TotalPoundage.ToString("0.00"),      //总手续费
                    TotalStamp_Tax = _TotalStamp_Tax.ToString("0.00"),    //总印花税
                    TotalOther_Free = _TotalOther_Free.ToString("0.00"),    //总其他杂费( 过户费 )
                    TotalCount = _TotalCount,            //总成交次数
                    TotalSellCount = _TotalSellCount,              //卖出股数
                    TotalBuyCount = _TotalBuyCount,                //买入股数
                    Data = group.ToList()
                };
            });

            //foreach (var group in code_group)
            //{
            //    foreach (TradeInfo item in list)
            //    {
            //        //非正常交易的
            //        if (String.IsNullOrEmpty(item.Code))
            //        {
            //            if (item.Operation == "银行转证券")
            //            {
            //                y_to_z_count += 1;
            //                y_to_z_total += item.HappenAmount;
            //            }
            //            else if (item.Operation == "证券转银行")
            //            {
            //                z_to_y_count += 1;
            //                z_to_y_total += item.HappenAmount;
            //            }
            //        }
            //        //正常交易的
            //        else
            //        {
            //            _TotalDealAmount += item.DealAmount;
            //            _TotalHappenAmount += item.HappenAmount;
            //            _TotalPoundage += item.Poundage;
            //            _TotalStamp_Tax += item.Stamp_Tax;
            //            _TotalOther_Free += item.Other_Free;
            //            _TotalCount += 1;
            //        }

            //    }
            //}

            //var resData = new
            //{
            //    Count_y_to_z = y_to_z_count, //银行转证券次数
            //    Total_y_to_z = y_to_z_total.ToString("0.00"), //银行转证券总金额
            //    Count_z_to_y = z_to_y_count, //证券转银行次数
            //    Total_z_to_y = z_to_y_total.ToString("0.00"), //证券转银行总金额
            //    TotalDealAmount = _TotalDealAmount.ToString("0.00"),    //总成交金额
            //    TotalHappenAmount = _TotalHappenAmount.ToString("0.00"),  //总收益
            //    TotalPoundage = _TotalPoundage.ToString("0.00"),      //总手续费
            //    TotalStamp_Tax = _TotalStamp_Tax.ToString("0.00"),    //总印花税
            //    TotalOther_Free = _TotalOther_Free.ToString("0.00"),    //总其他杂费( 过户费 )
            //    TotalCount = _TotalCount            //总成交次数
            //};


            return Content(JSONHelper.SerializeObject(new { status = "ok", msg = "ok", result = code_group }));


        }




    }
}