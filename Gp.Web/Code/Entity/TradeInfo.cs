using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gp.Web.Code.Entity
{
    public class TradeInfo
    {
        public int Index { get; set; }
        public int UserId { get; set; }
        public DateTime DealDate { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int DealCount { get; set; }
        public double DealAvgPrice { get; set; }
        public double DealAmount { get; set; }
        public double HappenAmount { get; set; }
        public double Poundage { get; set; }
        public double Stamp_Tax { get; set; }
        public double Other_Free { get; set; }
        public double Yu { get; set; }
        public string Operation { get; set; }
        public string TradeNum { get; set; }
        
    }
}