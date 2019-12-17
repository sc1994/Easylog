using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ReadLog.Models;

namespace ReadLog
{
    public class Startup
    {
        private ILogger<Startup> _logger;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Logs",
                    Version = "v1"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              IHostingEnvironment env,
                              IApplicationLifetime applicationLifetime,
                              ILogger<Startup> logger)
        {
            _logger = logger;

            app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("../swagger/v1/swagger.json", "Logs");
            });

            applicationLifetime.ApplicationStarted.Register((Started));
        }

        private void Started()
        {
            while (true)
            {
                Parallel.Invoke(
                    SwaggerStorage.ToPoll,
                    SwaggerStorage.ToGame
                );
                _logger.LogInformation($"{DateTime.Now} : DONE 刷新 swagger");
                System.Threading.Thread.Sleep(5 * 60 * 1000);
            }
        }
    }
}
