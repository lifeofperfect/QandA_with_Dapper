using DbUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QandA.Data;
using QandA.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QandA
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
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            EnsureDatabase.For.SqlDatabase(connectionString);
            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString, null)
                .WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetExecutingAssembly())
                .WithTransaction()
                .LogToConsole()
                .Build();
            if (upgrader.IsUpgradeRequired())
            {
                upgrader.PerformUpgrade();
            }
            services.AddControllers();
            services.AddScoped<IDataRepository, DataRepository>();

            services.AddCors(opt =>
                opt.AddPolicy("CorsPolicy", builder=>
                    builder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins("http://localhost:3000")
                        .AllowCredentials()
                ));

            services.AddSignalR();

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<QuestionHubs>("/questionshub");
            });
        }
    }
}
