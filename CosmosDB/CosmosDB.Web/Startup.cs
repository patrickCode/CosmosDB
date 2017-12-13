using DocumentDB.Data;
using CosmosDB.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDB.Web
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

            services.AddSingleton<IDocumentRepository<Book>, DocumentRepository<Book>>(sp => new DocumentRepository<Book>(
                endpoint: Configuration.GetSection("CosmosDB").GetSection("DocumentDB").GetValue<string>("Endpoint"),
                authKey: Configuration.GetSection("CosmosDB").GetSection("DocumentDB").GetValue<string>("AuthKey"),
                databaseId: Configuration.GetSection("CosmosDB").GetSection("DocumentDB").GetValue<string>("Database"),
                collectionId: Configuration.GetSection("CosmosDB").GetSection("DocumentDB").GetValue<string>("BookCollection")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}