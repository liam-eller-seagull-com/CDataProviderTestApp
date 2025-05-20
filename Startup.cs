using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.Linq;
using System.Text.Json;
using System.Globalization;

namespace CDataProviderTestApp
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
         services.Configure<ApiBehaviorOptions>(options =>
         {
            options.InvalidModelStateResponseFactory = context =>
               new BadRequestObjectResult("Invalid route parameters.");
         });

         services.AddControllers().AddJsonOptions(jsonOptions =>
         {
            // Keeps returned responses (in DTOs) Pascal Cased
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
         });

         services.AddLocalization(options => options.ResourcesPath = $"{Configuration["Localization:RelativeResourcePath"]}");
         services.Configure<RequestLocalizationOptions>(options =>
         {
            var languageList = Configuration.GetSection("Localization:Languages").Get<string[]>().ToList();
            var supportedCulturesList = languageList.Select(x => new CultureInfo(x)).ToList();

            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCulturesList;
            options.SupportedUICultures = supportedCulturesList;
         });
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }
         else
         {
            app.UseExceptionHandler(c => c.Run(async context =>
            {
               var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
               var problemDetails = new ProblemDetails
               {
                  Status = StatusCodes.Status500InternalServerError,
                  Title = "An error occurred",
                  Detail = exception?.Message
               };
               await context.Response.WriteAsJsonAsync(problemDetails,
                  new JsonSerializerOptions { WriteIndented = true });
            }));
         }

         app.UseRouting();

         // Below two lines very important to enable app to use request culture/locale
         IOptions<RequestLocalizationOptions> options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
         app.UseRequestLocalization(options.Value);

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });
      }
   }
}
