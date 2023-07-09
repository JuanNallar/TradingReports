using Axpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingReports
{
    internal class UTCPowerTrade 
    {
        public PowerTrade PowerTrade { get; private set; }
        public DateTime UtcDateTime { get; private set; }

        public UTCPowerTrade(PowerTrade powerTrade)
        {
            this.PowerTrade = powerTrade;
            this.UtcDateTime = powerTrade.Date.ToUniversalTime();
        }
    }
}
