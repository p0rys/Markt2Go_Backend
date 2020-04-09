using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace Markt2Go.Services.Auth0Service
{
    public class Auth0Service : IAuth0Service
    {
        private readonly IConfiguration _configuration;        
        private static IHttpClientFactory _httpClientFactory;

        public Auth0Service(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));         
            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }


        public async Task<IDictionary<string, object>> GetUserInfo(string userToken)
        {
            if (string.IsNullOrEmpty(userToken))
                throw new ArgumentNullException(nameof(userToken));

            var uri = $"{_configuration["Auth0:Domain"]}userinfo";
            using (var client = _httpClientFactory.CreateClient("Default"))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IDictionary<string, object>>(responseBody);
            }
        }
        public async Task<bool> DeleteUser(string id)
        {
            // delete user from auth0
            var uri = $"{_configuration["Auth0:Domain"]}api/v2/users/{id}";
            var apiToken = await GetToken();
            using (var client = _httpClientFactory.CreateClient("Default"))
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, uri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

                var response = await client.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
        }


        private async Task<string> GetToken()
        {
            var uri = $"{_configuration["Auth0:Domain"]}oauth/token";
            var audience = $"{_configuration["Auth0:Domain"]}api/v2/";
            var requestBody = new Dictionary<string, string>() {
                { "client_id", _configuration["Auth0:ClientId"] },
                { "client_secret", _configuration["Auth0:ClientSecret"] },
                { "audience", audience },
                { "grant_type", "client_credentials" }
            };
            
            using (var client = _httpClientFactory.CreateClient("Default"))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<IDictionary<string, object>>(responseBody);
                return responseJson["access_token"].ToString();
            }
        }

    }
}