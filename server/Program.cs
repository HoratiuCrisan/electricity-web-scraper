using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Electricity_Web_Scraper {
    class Program {
        static async Task Main(String[] args) {

            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services => {
                    services.AddSingleton<Scraper>();

                    services.AddHostedService<TimedHostedService>();
                }) .Build();

            await host.RunAsync();
        }
    } 
}