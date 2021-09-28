using System.Collections.Generic;
using System.Linq;
using Marten;
using martendbtest.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Weasel.Core;
using Weasel.Postgresql;
using Weasel.Postgresql.Tables;

namespace martendbtest
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "martendbtest", Version = "v1" });
            });


            services.AddMarten(_ =>
            {
                _.Connection(Configuration.GetConnectionString("db"));
                _.Schema.Include<DbIndexSetup>();
                _.UseDefaultSerialization(EnumStorage.AsString);
                // _.AutoCreateSchemaObjects = AutoCreate.None;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "martendbtest v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }

    public class DbIndexSetup : MartenRegistry
    {
        public DbIndexSetup()
        {
            For<User>().Duplicate(x => x.UserName, configure: docIndex =>
            {
                docIndex.Name = "idx_userName";
                docIndex.Method = IndexMethod.hash;
            });

            For<User>().Index(x => x.Email);

            For<Order>().GinIndexJsonData();

            For<Order>().Index(x => x.UserId, configure: docIndex =>
            {
                docIndex.Name = "idx_userId";
                docIndex.Method = IndexMethod.hash;
            });
        }
    }
}