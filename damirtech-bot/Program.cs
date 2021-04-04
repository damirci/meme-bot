using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;

namespace MemeBot
{
    class Program
    {
        static readonly ManualResetEventSlim exitEvent = new ManualResetEventSlim(false);
        static void Main(string[] args)
        {

            // Establish an event handler to process key press events.
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            var services = new ServiceCollection();
            ConfigureServices(services);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            MemeBotApp memeBotApp = serviceProvider.GetService<MemeBotApp>();

            ILogger logger = serviceProvider.GetService<ILogger<Program>>();

            logger.LogInformation("Application Started at {dateTime}", DateTime.Now);
            try
            {
                memeBotApp.Run();
            }
            catch (Exception ex)
            {
                memeBotApp = serviceProvider.GetService<MemeBotApp>();
                memeBotApp.Run();
                logger.LogError(ex.ToString());
            }


            exitEvent.Wait();

            memeBotApp.Stop();

            logger.LogInformation("Application Ended at {dateTime}", DateTime.Now);
        }

        protected static void ExitHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            exitEvent.Set();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            services.AddSingleton<IConfiguration>(config);

            services.AddLogging(logging =>
            {
                logging.AddConfiguration(config.GetSection("Logging"));
                logging.AddConsole();
            }).Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            services.AddTransient<MemeBotApp>();
        }
    }
}
