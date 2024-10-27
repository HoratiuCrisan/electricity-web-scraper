using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace Electricity_Web_Scraper {
    class TimedHostedService(Scraper scraper) : BackgroundService{
        //private readonly TimeSpan _weeklyChecker = new TimeSpan(13, 0, 0); 
        private readonly TimeSpan _triggerInterval = TimeSpan.FromMinutes(1);
        private readonly Scraper _scraper = scraper;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                DateTime now = DateTime.Now;
                Console.WriteLine($"Task triggered now: ${now}");

                await _scraper.ScrapeWebPage();

                await Task.Delay(_triggerInterval, cancellationToken);
            }
        }
    }
}