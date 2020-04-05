using System;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

using Markt2Go.DTOs.User;
using Markt2Go.Services.UserService;
using Markt2Go.Shared.Extensions;


namespace Markt2Go.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static IHttpClientFactory _httpClientFactory;
        private readonly IUserService _userService;
        public UserController(IUserService userService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            if (userService == null)
                throw new ArgumentNullException(nameof(userService));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _userService = userService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            if (HttpContext.GetUserIdFromToken() != id)
                return Forbid();

            return Ok(await _userService.GetUser(id));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] AddUserDTO addedUser)
        {
            try
            {
                // request user information from auth0
                var userInformation = await GetAuth0UserInformation();
                var userId = userInformation.Single(x => x.Key == "sub").Value.ToString();
                var userName = userInformation.Single(x => x.Key == "name").Value.ToString();
                var userMail = userInformation.Single(x => x.Key == "email").Value.ToString();

                return Ok(await _userService.AddUser(userId, userName, userMail, addedUser));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error while creating user: {ex.Message}");
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateUserDTO updatedUser)
        {
            // check if requester is trying to update another user
            if (HttpContext.GetUserIdFromToken() != updatedUser.Id)
                return Forbid();

            return Ok(await _userService.UpdateUser(updatedUser));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            // check if requester is trying to delete another user
            if (HttpContext.GetUserIdFromToken() != id)
                return Forbid();

            return Ok(await _userService.DeleteUser(id));
        }


        private async Task<Dictionary<string, object>> GetAuth0UserInformation()
        {
            var userToken = await HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(userToken))
            {
                throw new ArgumentNullException("Could not found jwt token to request user information.");
            }
            var userInformationApi = User.Claims.FirstOrDefault(x => x.Type == "aud" && !x.Value.Contains(_configuration["Auth0:ApiIdentifier"]))?.Value;
            if (string.IsNullOrEmpty(userInformationApi))
            {
                throw new ArgumentNullException("Could not found the api url to request user information.");
            }

            // get "default" http client (will have proxy if configurated)
            var client = _httpClientFactory.CreateClient("Default");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            var response = await client.GetStringAsync(userInformationApi);             
            return JsonSerializer.Deserialize<Dictionary<string, object>>(response);
            //return JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
        }
    }
}