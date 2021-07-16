using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TagTriggerService.Logic.GoogleLogic;
using TagTriggerService.Logic.RssLogic;
using TagTriggerService.Quartz;

namespace TagTriggerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                  .ConfigureServices((hostContext, services) =>
                  {
                      // Add the required Quartz.NET services
                      services.AddQuartz(q =>
                      {
                          // Use a Scoped container to create jobs. I'll touch on this later
                          q.UseMicrosoftDependencyInjectionScopedJobFactory();

                          // Register the job, loading the schedule from configuration
                          q.AddJobAndTrigger<CreateTagsAndTriggersJob>(hostContext.Configuration);
                      });

                      // Add the Quartz.NET hosted service
                      services.AddQuartzHostedService(
                          q => q.WaitForJobsToComplete = true);

                      
                      // other config
                      services.AddScoped<IGoogleTagManagerServiceWrapper, GoogleTagManagerServiceWrapper>();
                      services.AddScoped<IWorkspaceAndContainerHandler, WorkspaceAndContainerHandler>();
                      services.AddScoped<ITagHandler, TagHandler>();
                      services.AddScoped<ITriggerHandler, TriggerHandler>();
                      services.AddScoped<IRssService, RssService>();
                      services.AddScoped<IGoogleTagManagerOrchestrator, GoogleTagManagerOrchestrator>();

                      services.AddHttpClient<IRssService, RssService>()
                                .AddPolicyHandler(GetRetryPolicy());
                      //.SetHandlerLifetime(TimeSpan.FromMinutes(5)); //Set lifetime to five minutes
                      //services.AddHttpClient(Options.DefaultName, c =>
                      //{
                      //    // ...
                      //}).AddPolicyHandler(GetGTMRetryPolicy());
                  });

        private static IAsyncPolicy<HttpResponseMessage> GetGTMRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(60), retryCount: 15);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(delay);
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
           

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
