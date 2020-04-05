using System.Net;
using System.Net.Http;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Markt2Go.Auth0
{
    public static class Auth0Extension
    {
        public static void AddAuth0(this IServiceCollection services, string authority, string audience, bool useProxy, string proxyAddress)
        {
            // add token authorization with auth0 authority and apiId as audience
            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwtBearerOptions =>
            {
                // this will redirect jwt authentification requests through the given proxy (needed for deployment on IONOS)
                if (useProxy)
                {
                    jwtBearerOptions.BackchannelHttpHandler = new HttpClientHandler
                    {
                        Proxy = new WebProxy(proxyAddress)
                    };
                }
                jwtBearerOptions.Authority = authority;
                jwtBearerOptions.Audience = audience;
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