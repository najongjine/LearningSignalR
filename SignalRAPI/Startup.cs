using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using SignalRAPI.hub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRAPI
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
      services.AddSignalR();

      /* API 외부 앱(클라)에서 요청을 받게끔 열어주는 코드 */
      services.AddCors(o => o.AddPolicy("signalr", builder =>
      {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
      }));
      
      /* demo purpose code */
      /*
      services.AddCors(options =>
      {
        options.AddPolicy("signalr",
          builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        //only get moethod is allowed
        options.AddPolicy("AllowAnyGet",
            builder => builder.AllowAnyOrigin()
                .WithMethods("GET")
                .AllowAnyHeader()
                .AllowCredentials());

        //only certain url is allowed 
        options.AddPolicy("AllowExampleDomain",
            builder => builder.WithOrigins("https://example.com")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
      });
    */
      services.AddRouting(option => option.LowercaseUrls = true);
      services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null)
        .AddNewtonsoftJson(opt =>
        {
          opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
          opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        });

      services.AddResponseCompression(opts =>
      {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/octet-stream" });
      });
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SignalRAPI", Version = "v1" });
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseResponseCompression();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalRAPI v1"));
      }

      /* API 외부 앱(클라)에서 요청을 받게끔 열어주는 코드 */
      app.UseCors("signalr");

      /* demo purpose code*/
      //app.UseCors("AllowAnyGet").UseCors("AllowExampleDomain");
      
      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();

        //SignalR
        endpoints.MapHub<LearningHub>("/learningHub");
      });
    }
  }
}
