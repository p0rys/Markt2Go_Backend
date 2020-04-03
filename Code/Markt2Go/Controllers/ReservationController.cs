using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Markt2Go.DTOs.Reservation;
using Markt2Go.Services.PermissionService;
using Markt2Go.Services.ReservationService;
using Markt2Go.Shared.Extensions;
using Markt2Go.Shared.Enums;

namespace Markt2Go.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {        
        private readonly IReservationService _reservationService;
        private readonly IPermissionService _permissionService;
        public ReservationController(IReservationService reservationService, IPermissionService permissionService)
        {
            if (reservationService == null)
                throw new ArgumentNullException(nameof(reservationService));

            if (permissionService == null)
                throw new ArgumentNullException(nameof(permissionService));

            _reservationService = reservationService;
            _permissionService = permissionService;
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(long id)
        {            
            // check if requester is allowed to read the reservation
            if (!await _permissionService.CanReadReservation(id, HttpContext.GetUserIdFromToken()))
                return Forbid();

            return Ok(await _reservationService.GetReservationById(id));
        }
        [HttpGet("User/{userId}")]
        [HttpGet("User/{userId}/Market/{marketId}")]
        [Authorize]
        public async Task<IActionResult> GetAllByUser(string userId, long? marketId, DateTime createdSince, DateTime pickupSince, DateTime pickup, StatusEnum? status)
        {
            // check if requester is trying to get reservations from someone else
            if (HttpContext.GetUserIdFromToken() != userId)
                return Forbid();

            return Ok(await _reservationService.GetReservationsByUser(userId, marketId, createdSince, pickupSince, pickup, status));
        }
        [HttpGet("Seller/{sellerId}")]
        [HttpGet("Seller/{sellerId}/Market/{marketId}")]
        [Authorize]
        public async Task<IActionResult> GetAllBySeller(long sellerId, long? marketId, DateTime createdSince, DateTime pickupSince, DateTime pickup, StatusEnum? status)
        {
            // check if requester is part of the requested seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), sellerId))
                return Forbid();

            return Ok(await _reservationService.GetReservationsBySeller(sellerId, marketId, createdSince, pickupSince, pickup, status));
        }

        [HttpGet("Excel/{marketId}/{sellerId}/{pickup}")]
        [Authorize]
        public async Task<IActionResult> GetReservationsAsExcel(long marketId, long sellerId, DateTime pickup, StatusEnum? status)
        {
            // check if requester is part of the requested seller
            if (!await _permissionService.UserIsSeller(HttpContext.GetUserIdFromToken(), sellerId))
                return Forbid();

            // if creating file was successful send file, if not send a HTTP 200 with the error message
            var fileRequest = await _reservationService.GetReservationsAsExcelFile(marketId, sellerId, pickup, status);
            if (fileRequest.Success)
                return File(fileRequest.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reservierungen_{pickup.ToShortDateString()}.xlsx");
            else
                return Ok(fileRequest);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] AddReservationDTO addedReservation)
        {
            // check if requester is trying to add a reservation for someone else
            if (HttpContext.GetUserIdFromToken() != addedReservation.UserId)
                return Forbid();

            // check if requester is validated
            if (!await _permissionService.UserIsValidated(HttpContext.GetUserIdFromToken()))
                return Forbid();

            // check if requester has reached the maximum number of reservations
            if (await _permissionService.MaxDailyReservationsReached(addedReservation.UserId, addedReservation.MarketId, addedReservation.SellerId, addedReservation.Pickup))
                return Forbid();

            return Ok(await _reservationService.AddReservation(addedReservation));
        }
        /*         
        [HttpPost("{id}/Item")]
        [Authorize]
        public async Task<IActionResult> AddItem(long id, [FromBody] AddItemDTO addedItem)
        {
            return Ok(await _reservationService.AddItem(id, addedItem));
        }               
        */

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateReservationDTO updatedReservation)
        {
            // check if requester is allowed to update this reservation
            if (!await _permissionService.CanUpdateReservation(updatedReservation.Id, HttpContext.GetUserIdFromToken()))
                return Forbid();

            return Ok(await _reservationService.UpdateReservation(updatedReservation));
        }
        /*
        [HttpPut("{id}/Item")]
        [Authorize]
        public async Task<IActionResult> UpdateItem(long id, [FromBody] UpdateItemDTO updateItem)
        {           
            // TODO: check if user is allowed to change items on this reservation
            return Ok(await _reservationService.UpdateItem(id, updateItem));
        }  
        */
        [HttpPut("{id}/SetStatus")]
        [Authorize]
        public async Task<IActionResult> SetStatus(long id, [FromBody] SetReservationStatusDTO reservationStatus)
        {
            // check if requester is allowed to set reservation status
            if (!await _permissionService.CanSetReservationStatus(id, HttpContext.GetUserIdFromToken()))
                return Forbid();

            return Ok(await _reservationService.SetReservationStatus(id, reservationStatus));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            // check if requester is allowed to delete the reservation
            if (!await _permissionService.CanUpdateReservation(id, HttpContext.GetUserIdFromToken()))
                return Forbid();

            return Ok(await _reservationService.DeleteReservation(id));
        }
        /*
        [HttpDelete("{id}/Item/{itemId}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id, long itemId)
        {
            // TODO: check if user is allowed to delete items on this reservation
            return Ok(await _reservationService.DeleteItem(id, itemId));
        }
        */
    }
}