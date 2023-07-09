using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingReports
{
    internal class Requester: IHostedService
    {
        System.Timers.Timer _timer;

        ILogger<Requester> _logger;
        public Requester(ILogger<Requester> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await DoWork();

            _timer = new System.Timers.Timer(TradingReportsConfiguration.IntervalInMinutes * 1000 * 60);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        async Task DoWork()
        {
            var tomorrow = DateTime.Today.AddDays(1);
            await new Request(_logger).GeneratePowerPeriodsReport(tomorrow);
        }

        private async void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            await DoWork();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_timer != null)
            {
                _timer.Elapsed -= TimerElapsed;
            }
            return Task.CompletedTask;
        }

    }
}
