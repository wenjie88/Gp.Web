using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gp.Web.Code.Common
{
    public class JSONHelper
    {
        public static string SerializeObject(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }



        public static T DeserializeObject<T>(string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        }
    }
}