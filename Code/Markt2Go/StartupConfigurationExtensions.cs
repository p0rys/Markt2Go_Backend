using System.Net.Http;
using System.Net;
using System.Linq;

using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;

using Markt2Go.Data;
using Markt2Go.Services.MailService;
using Markt2Go.Services.MarketService;
using Markt2Go.Services.UserService;
using Markt2Go.Services.SellerService;
using Markt2Go.Services.ReservationService;
using Markt2Go.Services.PermissionService;
using Markt2Go.Services.FileService;
using Markt2Go.Services.Auth0Service;

namespace Markt2Go
{    
    public static class StartupConfigurationExtensions
    {
        public static void AddBusinessLogic(this IServiceCollection services)
        {
            // add services
            services.AddTransient<IMailService, MailService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMarketService, MarketService>();
            services.AddScoped<ISellerService, SellerService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IAuth0Service, Auth0Service>();
        }
        public static void AddDataAccessLayer(this IServiceCollection services, string connectionString)
        {
            // add data layer
            services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
        }
        public static void AddHelper(this IServiceCollection services, bool useProxy, string proxyAddress)
        {
            // add HttpClientFactory with proxy if needed
            if (useProxy)
            {
                  services.AddHttpClient("Default")
                    .ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler
                        {
                            UseProxy = useProxy,
                            Proxy = new WebProxy(proxyAddress)
                        };
                    });
            }
            else
                services.AddHttpClient("Default");

            services.AddAutoMapper(typeof(Startup));
        }

        public static void CheckDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DataContext>())
                {
                    // check if database exists, if not create it
                    context.Database.EnsureCreated();

                    var pendingMigrations = context.Database.GetPendingMigrations();
                    if (pendingMigrations.Count() > 0)
                    {
                        // TODO: Not working when database is new created (ef migrations missing)
                        //throw new DbUpdateException($"Database is missing the following migrations: {string.Join(", ", pendingMigrations)}");
                    }
                }
            }
        }
    }
}