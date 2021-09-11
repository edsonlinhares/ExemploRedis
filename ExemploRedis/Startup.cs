using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExemploRedis.Filters;
using ExemploRedis.Models;
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
            services.AddAutoMapper(typeof(Startup));

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ApiActionFilter));
            });

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

            Seed(app);
        }


        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IEmployeeStore, EmployeeStore>();

            services.AddSingleton<EmployeeStore>();
        }

        private static void EnableDecorator(IServiceCollection services)
        {
            services.Decorate<IEmployeeStore, EmployeeCachingStore>();
        }

        private void Seed(IApplicationBuilder app)
        {
            var ts = new Task(() =>
            {
                var store = app.ApplicationServices.GetRequiredService<EmployeeStore>();
                var cache = app.ApplicationServices.GetRequiredService<ICacheService>();

                var lista = store.Listar().Result;

                foreach (var item in lista)
                {
                    cache.Set<Models.Employee>($"Employees:{item.Id}", item, TimeSpan.FromHours(4));
                }
            });

            ts.Start();
        }
    }

    public class ViewModelToDomain : AutoMapper.Profile
    {
        public ViewModelToDomain()
        {
            CreateMap<EmployeeViewModel, Employee>()
                .ConstructUsing(c => new Employee(c.Id, c.Name, c.Age));
        }
    }
}
