using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExemploRedis.Stores;
using ExemploRedis.Stores.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace ExemploRedis
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExemploRedis", Version = "v1" });
            });

            RegisterServices(services);

            EnableDecorator(services);

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExemploRedis v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<RedisCacheService>();
            services.AddSingleton<IEmployeeStore, EmployeeStore>();

            services.AddSingleton<EmployeeStore>();
        }

        private static void EnableDecorator(IServiceCollection services)
        {
            //services.Decorate<IEmployeeStore, EmployeeCachingStore>();

            services.AddScoped<IEmployeeStore, EmployeeCachingDecorator<EmployeeStore>>();
        }


    }
}
