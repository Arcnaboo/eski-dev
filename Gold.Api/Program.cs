using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace WebApplication2
{
    public class Program
    {

        
        public static void Main(string[] args)
        {
            var date = DateTime.Now;
            var logFileName = "logg-" + date.Year + "-" + date.Month + "-" + date.Day + "-" + date.Hour + "-" + date.Minute + ".txt";
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("\\logs\\" + logFileName, rollingInterval: RollingInterval.Hour,
            rollOnFileSizeLimit:true, fileSizeLimitBytes: 107374182)
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

           
            try
            {
                Log.Information("Starting web host");
                var host = CreateHostBuilder(args)
                .UseSerilog()
                .Build();

                host.Run();
                
            } 
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
                
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// changed this to Host to WebHost for testing on Server IIS
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
