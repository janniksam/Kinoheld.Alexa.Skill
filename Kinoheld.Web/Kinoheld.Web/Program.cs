using System;
using Karambolo.Extensions.Logging.File;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinoheld.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((ctx, builder) =>
                {
                    builder.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                    builder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                    builder.Services.Configure<FileLoggerOptions>(ctx.Configuration.GetSection("Logging:File"));
                })
                .Build();
    }
}
