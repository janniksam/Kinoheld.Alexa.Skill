using System;
using System.Configuration;
using Kinoheld.Base;
using Kinoheld.IoC;
using Kinoheld.Web.BackgroundTasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kinoheld.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddDockerSecrets(p => p.Optional = true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            
            Configuration = builder.Build();
        }
            

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(
            IServiceCollection services)
        {
            var kinoheldConnectionString = Configuration[Secrets.KinoheldConnectionstringKey];
            if (string.IsNullOrEmpty(kinoheldConnectionString))
            {
                throw new ConfigurationErrorsException(
                    $"The secret \"{Secrets.KinoheldConnectionstringKey}\" has to be set properly.");
            }

            services.RegisterDependencies(kinoheldConnectionString);
            services.AddSingleton(Configuration);
            services.AddHostedService<WorkItemQueueService>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            serviceProvider.EnsureDbContextCreated();

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
