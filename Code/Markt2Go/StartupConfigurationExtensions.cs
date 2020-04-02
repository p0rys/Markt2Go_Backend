using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using Markt2Go.Data;
using Markt2Go.Services.MailService;
using Markt2Go.Services.MarketService;
using Markt2Go.Services.UserService;
using Markt2Go.Services.SellerService;
using Markt2Go.Services.ReservationService;
using Markt2Go.Services.PermissionService;

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
        }
        public static void AddDataAccessLayer(this IServiceCollection services, string connectionString)
        {
            // add data layer
            services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
        }
        public static void AddHelper(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddAutoMapper(typeof(Startup));
        }
    }
}