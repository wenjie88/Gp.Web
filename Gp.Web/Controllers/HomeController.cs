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

        //private Code.Dao.TradeInfoDao _TradeInfoDao = new Code.Dao.TradeInfoDao();


        // GET: Home
        [ManagerAuthorize()]
        public ActionResult Index()
        {
            ViewBag.UserName = AdminManager.GetUser().UserName;
            return View();
        }







    }
}