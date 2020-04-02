using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Markt2Go.DTOs;
using Markt2Go.DTOs.Seller;
using Markt2Go.Services.MarketService;
using Markt2Go.Services.PermissionService;
using Markt2Go.Services.SellerService;
using Markt2Go.Services.UserService;
using Markt2Go.Shared.Extensions;

namespace Markt2Go.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SellerController : ControllerBase
    {
        private readonly ISellerService _sellerService;
        private readonly IMarketService _marketService;
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;

        public SellerController(ISellerService sellerService, IMarketService marketService, IUserService userService, IPermissionService permissionService)
        {
            if (sellerService == null)
                throw new ArgumentNullException(nameof(sellerService));
            if (marketService == null)
                throw new ArgumentNullException(nameof(marketService));
            if (userService == null)          
                throw new ArgumentNullException(nameof(userService));
            if (permissionService == null)
                throw new ArgumentNullException(nameof(permissionService));

            _sellerService = sellerService;
            _marketService = marketService;
            _userService = userService;
            _permissionService = permissionService;
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _sellerService.GetAllSellers());
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(long id)
        {
            return Ok(await _sellerService.GetSellerById(id));
        }
        [HttpGet("{id}/Market")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSellerMarkets(long id)
        {
            return Ok(await _marketService.GetMarketsBySellerId(id));
        }
        [HttpGet("{id}/Market/{marketId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSellerMarketById(long id, long marketId)
        {
            return Ok(await _marketService.GetMarketByIdAndSellerId(marketId, id));
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] AddSellerDTO addedSeller)
        {
            return Ok(await _sellerService.AddSeller(HttpContext.GetUserIdFromToken(), addedSeller));
        }
        [HttpPost("{id}/Market")]
        [Authorize]
        public async Task<IActionResult> AddMarket(long id, [FromBody] AddMarketSellerDTO addedMarketSeller)
        {
            // check if requester is trying to add a market to another seller
            if (id != addedMarketSeller.SellerId)
                return Forbid();

            // check if requester is part of the seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), id))
                return Forbid();

            return Ok(await _sellerService.AddMarketToSeller(addedMarketSeller));
        }
        [HttpPost("{id}/User")]
        [Authorize]
        public async Task<IActionResult> AddUser(long id, [FromBody] IdDTO addedUser)
        {
            // check if requester is part of the seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), id))
                return Forbid();

            return Ok(await _userService.UpdateUserSellerId(addedUser.Id, id));
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateSellerDTO updatedSeller)
        {
            // check if requester is part of the seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), updatedSeller.Id))
                return Forbid();

            return Ok(await _sellerService.UpdateSeller(updatedSeller));
        }
        [HttpPut("{id}/Market")]
        [Authorize]
        public async Task<IActionResult> UpdateMarket(long id, [FromBody] UpdateMarketSellerDTO updatedMarketSeller)
        {
            // check if requester is trying to update markets of other sellers
            if (id != updatedMarketSeller.SellerId)
                return Forbid();

            // check if requester is part of the seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), id))
                return Forbid();

            return Ok(await _sellerService.UpdateMarketSeller(updatedMarketSeller));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            // check if requester is part of the seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), id))
                return Forbid();

            return Ok(await _sellerService.DeleteSeller(id));
        }
        [HttpDelete("{id}/Market/{marketId}")]
        [Authorize]
        public async Task<IActionResult> DeleteMarket(long id, long marketId)
        {
            // check if requester is part of the seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), id))
                return Forbid();

            return Ok(await _sellerService.DeleteMarketFromSeller(id, marketId));
        }
        [HttpDelete("{id}/User/{userId}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(long id, string userId)
        {
            // check if requester is part of the seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), id))
                return Forbid();

            return Ok(await _userService.UpdateUserSellerId(userId, 0));
        }

    }
}