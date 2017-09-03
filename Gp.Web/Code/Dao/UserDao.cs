using Gp.Web.Code.Common;
using Gp.Web.Code.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gp.Web.Code.Dao
{
    public class UserDao
    {
        private Factory.Database _Database = Factory.DatabaseFactory.CreateDataBase(config.Dbname_gp);


        public User GetUser_ByUserNameAndPwd(string UserName,string Pwd)
        {
            return _Database.Query<User>("SELECT *FROM user WHERE UserName = @UserName AND Pwd = @Pwd", new { UserName = UserName, Pwd = Pwd }).FirstOrDefault();
        }




        
        public User GetUser_BySign(string Sign)
        {
            return _Database.Query<User>("SELECT *FROM user WHERE Sign = @Sign", new { Sign = Sign }).FirstOrDefault();
        }






        public bool UpDateSign_ByUserName(string Sign,string UserName)
        {
            return _Database.Execute("UPDATE user SET Sign = @Sign WHERE UserName = @UserName", new { Sign = Sign, UserName = UserName }) > 0;
        }

    }
}