using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gp.Web.Filters
{
    public class ManagerAuthorizeAttribute: AuthorizeAttribute
    {


        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return Code.Manager.AdminManager.GetUser() != null;
        }




        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Login", action = "index" }));

        }
    }
}