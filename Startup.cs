using FrMonitor4_0.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrMonitor4_0
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(config =>
          config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
         .UseSimpleAssemblyNameTypeSerializer()
         .UseDefaultTypeSerializer()
         .UseMemoryStorage());

            services.AddHangfireServer();
            services.AddTransient<IMetaDataService, MetaDataService>();
            services.AddTransient<IMonitorService, MonitorService>();
            services.AddTransient<IInstrumentService, InstrumentService>();
            services.AddTransient<ICandleService, CandleService>();
            services.AddTransient<IEmaService, EmaService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IPriceService, PriceService>();
            services.AddTransient<IUnitService, UnitService>();
            services.AddTransient<IBoxService, BoxService>();
            services.AddTransient<IDeloreanService, DeloreanService>();
            services.AddTransient<ITradingService, TradingService>();
            services.AddTransient<IUtilityService, UtilityService>();
            services.AddTransient<IMovingAverageService, MovingAverageService>();
            services.AddTransient<IBollingerBandService, BollingerBandService>();
            services.AddTransient<IHarmonicService, HarmonicService>();
            services.AddTransient<IEmaCrossService, EmaCrossService>();
            services.AddTransient<IRiskCalculationService, RiskCalculationService>();
            services.AddTransient<IRsiService, RsiService>();
            services.AddTransient<IBollingerRisHfxService, BollingerRisHfxService>();
            services.AddTransient<INavService, NavService>();
            services.AddTransient<ITargetUpdateService, TargetUpdateService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
             IServiceProvider serviceProvider, IRecurringJobManager recurringJobManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.UseHangfireDashboard();
            //serviceProvider.GetRequiredService<IMonitorService>().Monitor(240);

            //recurringJobManager.AddOrUpdate(
            //    "Run every minute",
            //    () => serviceProvider.GetRequiredService<IMonitorService>().Monitor(5),
            //    "* * * * *");

            if (!serviceProvider.GetRequiredService<IMetaDataService>().GetMetaConfig().RiskCalculationMode)
            {
                var tfs = serviceProvider.GetRequiredService<IMetaDataService>().GetMetaConfig().TimeFrames.Split("|");

                foreach (var tf in tfs)
                {
                    if (tf == "1")
                    {
                        recurringJobManager.AddOrUpdate(
                            "Run every minute",
                            () => serviceProvider.GetRequiredService<IMonitorService>().Monitor(1),
                            "* * * * *");
                    }

                    if (tf == "5")
                    {
                        recurringJobManager.AddOrUpdate(
                            "5m",
                            () => serviceProvider.GetRequiredService<IMonitorService>().Monitor(5),
                            "*/5 * * * *");
                    }

                    if (tf == "15")
                    {
                        recurringJobManager.AddOrUpdate(
                                       "15m",
                                       () => serviceProvider.GetRequiredService<IMonitorService>().Monitor(15),
                                       "*/15 * * * *");
                    }

                    if (tf == "30")
                    {
                        recurringJobManager.AddOrUpdate(
                                        "30m",
                                        () => serviceProvider.GetRequiredService<IMonitorService>().Monitor(30),
                                        "*/30 * * * *");
                    }

                    if (tf == "60")
                    {
                        recurringJobManager.AddOrUpdate(
                            "1H",
                            () => serviceProvider.GetRequiredService<IMonitorService>().Monitor(60),
                            "0 * * * *");
                    }
                }

                recurringJobManager.AddOrUpdate(
                    "5mTC",
                    () => serviceProvider.GetRequiredService<IMonitorService>().TargetCheck(),
                    "*/5 * * * *");
            }
            else
            {
                serviceProvider.GetRequiredService<IMonitorService>().Monitor(5);
            }
            

        }
    }
}
