using Gp.Web.Code.Common;
using Gp.Web.Code.Entity;
using Gp.Web.Code.Manager;
using Gp.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gp.Web.Controllers
{
    public class HomeController : Controller
    {

        private Code.Dao.TradeInfoDao _TradeInfoDao = new Code.Dao.TradeInfoDao();


        // GET: Home
        [ManagerAuthorize()]
        public ActionResult Index()
        {
            ViewBag.UserName = AdminManager.GetUser().UserName;
            return View();
        }



        //[ManagerAuthorize()]
        public ActionResult GetTradeInfo(string startDate, string EndDate)
        {
            if(String.IsNullOrEmpty(startDate) || String.IsNullOrEmpty(EndDate))
            {
                var Date = DateTime.Now;
                startDate = Date.ToString("yyyy-MM-01");
                EndDate = Date.ToString("yyyy-MM-dd");
            }

            //时间范围的开始日期的卖出 不计算。 结束日期的买入 不计算
            List<TradeInfo> list = _TradeInfoDao.GetList_ByUserIdAndDealDate(AdminManager.GetUser().UserId,startDate,EndDate).Where(x=>!((x.DealDate.ToString("yyyy-MM-dd") == startDate && x.Operation == "证券卖出") ||(x.DealDate.ToString("yyyy-MM-dd") == EndDate && x.Operation == "证券买入"))).ToList();


            var resdata = new
            {
                TotalDealAmount = list.Sum(x => x.DealAmount).ToString("0.00"),  //总成交金额
                TotalHappenAmount = list.Sum(x => x.HappenAmount).ToString("0.00"),  //总收益
                TotalPoundage = list.Sum(x => x.Poundage).ToString("0.00"),  //总手续费
                TotalStamp_Tax = list.Sum(x => x.Stamp_Tax).ToString("0.00"),  //总印花税
                TotalOther_Free = list.Sum(x => x.Other_Free).ToString("0.00"),  //总其他杂费( 过户费 )
                TotalCount = list.Count,  //总成次数
                data = list.GroupBy(x=>x.Code).Select(x=>new
                {
                    InCome = x.Sum(s=>s.HappenAmount).ToString("0.00"),
                    tradeList = x
                })
            };

            return Content(JSONHelper.SerializeObject(new { status = "ok", msg = "ok", result = resdata }));
        }





        //[HttpGet]
        //[ManagerAuthorize]
        //public ActionResult AddTradInfo()
        //{
        //    return View();
        //}




        [HttpPost]
        [ManagerAuthorize]
        public ActionResult AddTradeInfo(TradeInfo info)
        {
            info.UserId = AdminManager.GetUser().UserId;
            switch (info.Operation)
            {
                case "证券买入":
                    info.DealAmount = info.DealCount * info.DealAvgPrice;
                    info.Poundage = Math.Round(info.DealCount * info.DealAvgPrice * 0.0003, 2, MidpointRounding.AwayFromZero);
                    info.Stamp_Tax = 0;
                    info.Other_Free = info.Code.StartsWith("6") ? Math.Round(info.DealAmount * 0.00002, 2, MidpointRounding.AwayFromZero) : 0;
                    info.HappenAmount = -(info.DealAmount + info.Poundage + info.Stamp_Tax);
                    break;
                default:
                    info.DealAmount = info.DealCount * info.DealAvgPrice;
                    info.Poundage = Math.Round(info.DealCount * info.DealAvgPrice * 0.0003, 2, MidpointRounding.AwayFromZero);
                    info.Stamp_Tax = Math.Round(info.DealCount * info.DealAvgPrice * 0.001, 2, MidpointRounding.AwayFromZero);
                    info.Other_Free = info.Code.StartsWith("6") ? Math.Round(info.DealAmount * 0.00002, 2, MidpointRounding.AwayFromZero) : 0;
                    info.HappenAmount = info.DealAmount - info.Poundage - info.Stamp_Tax;
                    break;
            }



            if (_TradeInfoDao.Add(info))
            {
                return Content(JSONHelper.SerializeObject(new { status = "ok", msg = "ok" }));
            }

            return Content(JSONHelper.SerializeObject(new { status = "no", msg = "添加失败" }));
        }


    }
}