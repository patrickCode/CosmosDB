using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

//using DocumentDB.Data;
using CosmosDB.Web.Models;
using Table.Data;
using Table.Data.Entities;


namespace API
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
            services.AddMvc();
            
            var conn = Configuration.GetSection("CosmosDB").GetSection("Table").GetValue<string>("ConnectionString");

            services.AddSingleton<ICloudStorageContext, CloudStorageContext>(sp => new CloudStorageContext(
                connectionString: Configuration.GetSection("CosmosDB").GetSection("Table").GetValue<string>("ConnectionString")));

            services.AddSingleton<ITableRepository<UserTableEntity>, TableRepository<UserTableEntity>>(sp => new TableRepository<UserTableEntity>(
                cloudStorageContext: sp.GetService<ICloudStorageContext>(),
                tableName: Configuration.GetSection("CosmosDB").GetSection("Table").GetValue<string>("UsersTable")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
