using Dapper;
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


        #region Add

        public bool Add(TradeInfo info)
        {
            return _Datebase.Execute("INSERT INTO tradeinfo(UserId, DealDate, Code, Name, DealCount, DealAvgPrice, DealAmount, HappenAmount, Poundage, Stamp_Tax, Operation, Other_Free, TradeNum, Yu) VALUES(@UserId, @DealDate, @Code, @Name, @DealCount, @DealAvgPrice, @DealAmount, @HappenAmount, @Poundage, @Stamp_Tax, @Operation, @Other_Free, @TradeNum, @Yu)", info) > 0;
        }



        public bool Add(List<TradeInfo> infoList)
        {
            Exception ex = _Datebase.ExecuteTran((conn, tran) =>
            {
                //插入
                conn.Execute("INSERT INTO tradeinfo(UserId, DealDate, Code, Name, DealCount, DealAvgPrice, DealAmount, HappenAmount, Poundage, Stamp_Tax, Operation, Other_Free, TradeNum, Yu) VALUES(@UserId, @DealDate, @Code, @Name, @DealCount, @DealAvgPrice, @DealAmount, @HappenAmount, @Poundage, @Stamp_Tax, @Operation, @Other_Free, @TradeNum, @Yu)", infoList , tran);

                //去重
                conn.Execute("delete from tradeinfo where `Index` in (select t1.`Index` from(select t2.`Index`, t2.TradeNum from tradeinfo t2 group by t2.TradeNum having count(TradeNum) > 1) as t1)", transaction: tran);
            });
            
            if(ex != null)
            {
                throw (ex);
            }
            else
            {
                return true;
            }

        }

        #endregion




        #region 获取数据
        public List<TradeInfo> GetList_ByUserId(int UserId)
        {
            return _Datebase.Query<TradeInfo>("SELECT *FROM tradeinfo WHERE UserId = @UserId ORDER BY DealDate", new { UserId = UserId});
        }



        public List<TradeInfo> GetList_ByUserIdAndDealDate(int UserId, string StartDate, string EndDate)
        {
            return _Datebase.Query<TradeInfo>("SELECT *FROM tradeinfo WHERE UserId = @UserId AND DealDate between @StartDate and @EndDate ORDER BY DealDate", new { UserId = UserId, StartDate = StartDate, EndDate = EndDate });
        }
        #endregion




    }
}