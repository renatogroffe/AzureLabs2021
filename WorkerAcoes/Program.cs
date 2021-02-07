using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkerAcoes.Data;

namespace WorkerAcoes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var settings = config.Build();
                    config.AddAzureAppConfiguration(
                        settings["ConnectionStrings:AppConfiguration"]);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<AcoesRepository>();
                    services.AddHostedService<Worker>();
                    services.AddApplicationInsightsTelemetryWorkerService();
                });
    }
}