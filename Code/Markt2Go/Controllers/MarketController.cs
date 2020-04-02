using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Markt2Go.Services.MarketService;
using Markt2Go.Services.SellerService;
using Markt2Go.DTOs.Market;
using System;

namespace Markt2Go.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MarketController : ControllerBase
    {
        private readonly IMarketService _marketService;
        private readonly ISellerService _sellerService;
        public MarketController(IMarketService marketService, ISellerService sellerService)
        {
            if (marketService == null)
                throw new ArgumentNullException(nameof(marketService));
            if (sellerService == null)
                throw new ArgumentNullException(nameof(sellerService));
                
            _marketService = marketService;
            _sellerService = sellerService;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _marketService.GetAllMarkets());
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(long id)
        {
            return Ok(await _marketService.GetMarketById(id));
        }
        [HttpGet("{id}/Seller")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMarketSellers(long id)
        {
            return Ok(await _sellerService.GetSellersByMarketId(id));
        }
        [HttpGet("{id}/Seller/{sellerId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMarketSellerById(long id, long sellerId)
        {
            return Ok(await _sellerService.GetSellerByIdAndMarketId(sellerId, id));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] AddMarketDTO addedMarket)
        {
            return Forbid();

            return Ok(await _marketService.AddMarket(addedMarket));
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateMarketDTO updatedMarket)
        {
            return Forbid();

            return Ok(await _marketService.UpdateMarket(updatedMarket));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            return Forbid();

            return Ok(await _marketService.DeleteMarket(id));
        }
    }
}