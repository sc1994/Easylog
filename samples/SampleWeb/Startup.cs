using System;
using EasyLog.WriteLog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleWeb.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace SampleWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHttpContextAccessor();
            services.AddEasyLog();

            services.AddScoped<LogTestService, LogTestService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env
        )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseEasyLog(options =>
            {
                options.App = "SampleWeb";
                options.Debug = true;
                options.Serilog =
                    new LoggerConfiguration()
                        .WriteTo.ColoredConsole(
                            restrictedToMinimumLevel: LogEventLevel.Debug
                        )
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9222/"))
                        {
                            AutoRegisterTemplate = true,
                            MinimumLogEventLevel = LogEventLevel.Debug
                        })
                        .CreateLogger();
            });
        }
    }
}
