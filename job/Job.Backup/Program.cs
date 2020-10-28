using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WorkerService;

namespace Job.Backup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).UseWindowsService().Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureLogging(ConfigureLogging)
             .ConfigureServices((hostContext, services) =>
             {
                 var configuration = hostContext.Configuration;

                 services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

                 services.AddJobScheduler(configuration.GetSection("JobSettings"))
                     .AddJob<Backup>()
                     .Configure();
             });

        private static readonly Action<HostBuilderContext, ILoggingBuilder> ConfigureLogging =
             (hostingContext, logging) =>
             {
                 var configuration = hostingContext.Configuration;

                 logging.AddConfiguration(configuration.GetSection("Logging"));

                 logging.AddSerilog();

                 var logPath = configuration.GetValue<string>("LogPath");

                 Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .Enrich.WithMachineName()
                     .WriteTo.Console()
                     .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                     .CreateLogger();
             };
    }
}
