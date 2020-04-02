using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Markt2Go.Auth0
{
    public static class Auth0Extension
    {
        public static void AddAuth0(this IServiceCollection services, string authority, string audience)
        {
            // add token authorization with auth0 authority and apiId as audience
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
            });

            // add permission based authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("add:market", policy => policy.Requirements.Add(new HasScopeRequirement("add:market", authority)));
                options.AddPolicy("update:market", policy => policy.Requirements.Add(new HasScopeRequirement("update:market", authority)));
                options.AddPolicy("delete:market", policy => policy.Requirements.Add(new HasScopeRequirement("delete:market", authority)));
            });
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        }
        public static void UseAuth0(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }    
    }
}