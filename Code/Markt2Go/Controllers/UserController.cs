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
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            if (userService == null)
                throw new ArgumentNullException(nameof(userService));

            _userService = userService;
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
            return Ok(await _userService.AddUser(await HttpContext.GetTokenAsync("access_token"), addedUser));
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
    }
}