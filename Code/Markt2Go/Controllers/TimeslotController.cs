using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Markt2Go.Services.ReservationService;
using System;

namespace Markt2Go.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TimeslotController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        public TimeslotController(IReservationService reservationService)
        {
            if (reservationService == null)
                throw new ArgumentNullException(nameof(reservationService));
                
            _reservationService = reservationService;
        }


        [HttpGet("{marketId}/{sellerId}")]
        [Authorize]
        public async Task<IActionResult> GetByMarketId(long marketId, long sellerId)
        {
            return Ok(await _reservationService.GetTimeslots(marketId, sellerId));
        }
    }
}