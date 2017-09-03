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



        [ManagerAuthorize()]
        public ActionResult TradeList()
        {
            var list = _TradeInfoDao.GetList_ByUserId(AdminManager.GetUser().UserId);
            ViewBag.TaoalDealAmount = list.Sum(x => x.DealAmount);
            ViewBag.TotalHappenAmount = list.Sum(x => x.HappenAmount);
            ViewBag.TotalPoundage = list.Sum(x => x.Poundage);
            ViewBag.TotalStamp_Tax = list.Sum(x => x.Stamp_Tax);
            ViewBag.TotalOther_Free = list.Sum(x => x.Other_Free);
            ViewBag.TotalCount = list.Count;
            return View(list);
        }





        [HttpGet]
        [ManagerAuthorize]
        public ActionResult AddTradInfo()
        {
            return View();
        }




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