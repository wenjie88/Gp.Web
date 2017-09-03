using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gp.Web.Code.Entity
{
    public class User
    {
        public int UserId { get;set; }
        public string UserName { get; set; }
        public string Pwd { get; set; }
        public string Sign { get; set; }
    }
}