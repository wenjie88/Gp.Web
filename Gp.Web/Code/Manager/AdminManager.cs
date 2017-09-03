using Gp.Web.Code.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gp.Web.Code.Manager
{
    public class AdminManager
    {

        private static string SessionId
        {
            get
            {
                var Context = HttpContext.Current;
                HttpCookie cookie = Context.Request.Cookies["USER_SESSIONID"];
                if(cookie == null)
                {
                    string session_id = Guid.NewGuid().ToString("N");
                    HttpCookie newcookie = new HttpCookie("USER_SESSIONID", session_id)
                    {
                        HttpOnly = true,
                        Expires = DateTime.Now.AddDays(30)
                    };

                    Context.Response.Cookies.Add(newcookie);
                    return session_id;
                }

                return cookie.Value;
            }
        }


        static Code.Dao.UserDao _UserDao = new Dao.UserDao();


        public static void Login(string UserName)
        {
            _UserDao.UpDateSign_ByUserName(SessionId, UserName);
        }




        public static User GetUser()
        {
            return  _UserDao.GetUser_BySign(SessionId);
        }




    }
}