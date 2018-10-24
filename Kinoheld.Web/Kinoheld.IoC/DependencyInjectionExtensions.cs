using System;
using Kinoheld.Application.Abstractions.BackgroundTasks;
using Kinoheld.Application.Abstractions.RequestHandler;
using Kinoheld.Application.BackgroundTasks;
using Kinoheld.Application.Formatter;
using Kinoheld.Application.Intents;
using Kinoheld.Application.Model;
using Kinoheld.Application.RequestHandler;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Application.Services;
using Kinoheld.Base.Formatter;
using Kinoheld.Base.Utils;
using Kinoheld.Domain.Services.Abstractions.Database;
using Kinoheld.Domain.Services.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kinoheld.IoC
{
    public static class DependencyInjectionExtensions
    {
        public static void RegisterDependencies(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<KinoheldDbContext>(p => p.UseMySQL(connectionString));
            services.AddTransient<IAlexaHandler, AlexaHandler>();
            services.AddTransient<IIntentHandler, IntentHandler>();
            services.AddTransient<IStatusHandler, StatusHandler>();

            services.AddTransient<IIntent, AmazonHelpIntent>();
            services.AddTransient<IIntent, AmazonStopIntent>();
            services.AddTransient<IIntent, AmazonCancelIntent>();
            services.AddTransient<IIntent, GetOverviewDayIntent>();
            services.AddTransient<IIntent, ToggleEmailSettingsIntent>();
            services.AddTransient<IIntent, SetUserPreferencesIntent>();
            services.AddTransient<IIntent, ResetUserPreferencesIntent>();
            
            services.AddTransient<IKinoheldService, KinoheldService>();
            services.AddTransient<IKinoheldDbAccess, KinoheldDbAccess>();

            services.AddSingleton<IMessages>(MessageCreator.CreateMessages());
            services.AddSingleton<IAmazonService, AmazonService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<IRandomGenerator, RandomGenerator>();

            services.AddSingleton<IEmailBodyFormatter<DayOverview>,OverviewEmailFormatter>();
            services.AddSingleton<ISsmlMessageFormatter<DayOverview>, AlexaOverviewByShowResponseFormatter>();

            services.AddSingleton<IWorkItemQueue, WorkItemQueue>();
        }

        public static void EnsureDbContextCreated(this IServiceProvider serviceProvider)
        {
            using(var db = serviceProvider.GetService<KinoheldDbContext>())
            {
                db.Database.EnsureCreated();
            }
        }
    }
}