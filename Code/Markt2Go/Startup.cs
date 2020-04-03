using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Markt2Go.Auth0;

namespace Markt2Go
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
            // add helpers such as automapper
            services.AddHelper();
            // add everything that is needed by the DAL
            services.AddDataAccessLayer(Configuration["ConnectionStrings:DefaultConnection"]);
            // add all services needed for the business logic
            services.AddBusinessLogic();
            // add controllers
            services.AddControllers();
            // add authentication and authorization via auth0
            services.AddAuth0(Configuration["Auth0:Domain"], Configuration["Auth0:ApiIdentifier"]);            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            // search for (and serve) default files (default.htm, default.html, index.htm, index.html) from wwwroot
            app.UseDefaultFiles();
            // return files from "wwwroot" if no api route is matched
            app.UseStaticFiles();
            // http to https redirection
            app.UseHttpsRedirection();
            // set all authorization / authentication for auth0
            app.UseAuth0();
            // check if database exists, create when needed and check if all migrations are applied
            app.CheckDatabase();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }   
    }
}

