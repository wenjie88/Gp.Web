using Gp.Web.Code.Common;
using Gp.Web.Code.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gp.Web.Controllers
{
    [RoutePrefix("Login")]
    public class LoginController : Controller
    {

        private Code.Dao.UserDao _UserDao = new Code.Dao.UserDao();


        // GET: Login
        public ActionResult Index()
        {
            return View();
        }




        [HttpPost]
        public ActionResult Login(string UserName, string Pwd)
        {
            var User = _UserDao.GetUser_ByUserNameAndPwd(UserName, Pwd);
            if (User == null)
                return Content(JSONHelper.SerializeObject(new { status = "no", msg = "用户名或密码不正确" }));

            AdminManager.Login(UserName);
            return Content(JSONHelper.SerializeObject(new { status = "ok", msg = "ok" }));
        }






    }
}