using Gp.Web.Code.Common;
using Gp.Web.Code.Entity;
using Gp.Web.Code.Manager;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gp.Web.Controllers
{
    [Filters.ManagerAuthorize]
    public class InfoController : Controller
    {
        private Code.Dao.TradeInfoDao _TradeInfoDao = new Code.Dao.TradeInfoDao();


        // GET: Info
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GetInfo()
        {
            List<TradeInfo> list = _TradeInfoDao.GetList_ByUserId(AdminManager.GetUser().UserId);
            

            int y_to_z_count = 0; //银行转证券次数
            double y_to_z_total = 0; //银行转证券总金额

            int z_to_y_count = 0; //证券转银行次数
            double z_to_y_total = 0; //证券转银行总金额

            double _TotalDealAmount = 0;    //总成交金额
            double _TotalHappenAmount = 0;  //总收益
            double _TotalPoundage = 0;      //总手续费
            double _TotalStamp_Tax = 0;     //总印花税
            double _TotalOther_Free = 0;    //总其他杂费( 过户费 )
            int _TotalCount = 0;            //总成次数

            foreach (TradeInfo item in list)
            {
                //非正常交易的
                if (item.Code == null || item.Code == "")
                {
                    if (item.Operation == "银行转证券")
                    {
                        y_to_z_count += 1;
                        y_to_z_total += item.HappenAmount;
                    }
                    else if (item.Operation == "证券转银行")
                    {
                        z_to_y_count += 1;
                        z_to_y_total += item.HappenAmount;
                    }
                }
                //正常交易的
                else
                {
                    _TotalDealAmount += item.DealAmount;
                    _TotalHappenAmount += item.HappenAmount;
                    _TotalPoundage += item.Poundage;
                    _TotalStamp_Tax += item.Stamp_Tax;
                    _TotalOther_Free += item.Other_Free;
                    _TotalCount += 1;
                }

            }

        }




    }
}