using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.W3C;
using Microsoft.Extensions.Options;

namespace HttpLogging.Sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpLogging();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/", async context =>
                {
                    var x = context.RequestServices.GetService<IConfigureOptions<LoggerFilterOptions>>();
                    //Debugger.Launch();
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
