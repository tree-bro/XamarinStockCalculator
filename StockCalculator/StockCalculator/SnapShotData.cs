using System;
using System.Collections.Generic;
using System.Text;

namespace StockCalculator
{
    public class SnapShotData
    {
        public long date;
        public long time;
        public StockBasicData stockBasic;
        public double preClose;
        public double high;
        public double open;
        public double low;
        public double close;
        public long volume;
        public long nowVol;
        public long amount;
        public double peRatio;
        public double perShareEarn;
    }
}
