using Gp.Web.Code.Common;
using Gp.Web.Code.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gp.Web.Code.Dao
{
    public class TradeInfoDao
    {

        private Code.Factory.Database _Datebase = Factory.DatabaseFactory.CreateDataBase(config.Dbname_gp);



        public bool Add(TradeInfo info)
        {
            return _Datebase.Execute("INSERT INTO tradeinfo(UserId, DealDate, Code, Name, DealCount, DealAvgPrice, DealAmount, HappenAmount, Poundage, Stamp_Tax, Operation, Other_Free) VALUES(@UserId, @DealDate, @Code, @Name, @DealCount, @DealAvgPrice, @DealAmount, @HappenAmount, @Poundage, @Stamp_Tax, @Operation, @Other_Free)", info) > 0;
        }

        


        public List<TradeInfo> GetList_ByUserIdAndDealDate(int UserId,string StartDate, string EndDate)
        {
            return _Datebase.Query<TradeInfo>("SELECT *FROM tradeinfo WHERE UserId = @UserId AND DealDate between @StartDate and @EndDate ORDER BY DealDate", new { UserId = UserId, StartDate = StartDate, EndDate= EndDate });
        }

    }
}