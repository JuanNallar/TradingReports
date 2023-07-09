using Axpo;
using Microsoft.Extensions.Logging;
using System.Data;

namespace TradingReports
{
    internal class Request
    {
        ILogger<Requester> _logger;
        public Request(ILogger<Requester> logger)
        {
            _logger = logger;
        }
        public async Task GeneratePowerPeriodsReport(DateTime tradeDateTime)
        {
            var uid = Guid.NewGuid();
            _logger.LogInformation($"[{uid}] Requesting for {tradeDateTime}...");

            IEnumerable<PowerTrade> powerTrades = null;

            int retryCount = 0;
            while(retryCount < 5)
            {
                try
                {
                    powerTrades = await new PowerService().GetTradesAsync(tradeDateTime);
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogInformation($"[{uid}] Error getting trades[{ex.Message}]. Retrying ({retryCount})");
                }
            }

            if (powerTrades == null)
            {
                _logger.LogError($"[{uid}] CRITICAL [Could not get trades after 5 attempts]");
            }

            var cumulatedUtcPeriods = CumulatedPowePeriods(powerTrades);

            retryCount = 0;
            while (retryCount < 5)
            {
                try
                {
                    await PrintFile(cumulatedUtcPeriods);
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogInformation($"[{uid}] Error saving report [{ex.Message}]. Retrying ({retryCount})");
                }
            }

            if (retryCount == 5)
            {
                _logger.LogError($"[{uid}] CRITICAL [Could not save report after 5 attempts]");
            }
            else
            {
                _logger.LogInformation($"[{uid}] Successfull");
            }
        }

        private DataTable CumulatedPowePeriods(IEnumerable<PowerTrade> powerTrades)
        {
            var dt = new DataTable();
            dt.Columns.Add("dateTime", typeof(string));
            dt.Columns.Add("Volume", typeof(double));


            var UtcPowerPeriods = TransformPowerPeriodsRecordsInUtcFormat(powerTrades);
            foreach (var g in UtcPowerPeriods.GroupBy(x => x.UtcDateTime))
            {
                dt.Rows.Add(g.Key, g.Sum(x=>x.Volume));
            }
            dt.AcceptChanges();

            return dt;
        }

        private List<UtcPowerPeriod> TransformPowerPeriodsRecordsInUtcFormat(IEnumerable<PowerTrade> powerTrades)
        {
            var records = new List<UtcPowerPeriod>();

            foreach (var powerTrade in powerTrades)
            {
                foreach (var powerPeriod in powerTrade.Periods)
                {
                    var dateTimePeriod = powerTrade.Date.AddHours(powerPeriod.Period-1);
                    var utcDateTimePeriod = new DateTime(dateTimePeriod.Year, dateTimePeriod.Month, dateTimePeriod.Day, dateTimePeriod.Hour, 0, 0, DateTimeKind.Utc);
                    records.Add(new UtcPowerPeriod
                    {
                        UtcDateTime = $"{utcDateTimePeriod:O}",
                        Volume = powerPeriod.Volume
                    });
                }
            }

            return records;
        }

        private async Task PrintFile(DataTable dt)
        {
            var csv = Helpers.CsvHelper.DataTableToCSV(dt, ',');
            var fileName = $"PowerPosition_{DateTime.Now.ToString("yyyMMdd_hhss")}.csv";
            await File.WriteAllTextAsync(Path.Combine(TradingReportsConfiguration.OutputFolder, fileName), csv);
        }
    }

    class UtcPowerPeriod
    {
        public string UtcDateTime { get; set; }
        public double Volume { get; set; }
    }

}
