using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Markt2Go.DTOs.Reservation;
using Markt2Go.DTOs.Timeslot;
using Markt2Go.Shared.Enums;

namespace Markt2Go.Services.ReservationService
{
    public interface IReservationService
    {
        Task<ServiceResponse<List<GetReservationDTO>>> GetAllReservations();
        Task<ServiceResponse<GetReservationDTO>> GetReservationById(long id);
        Task<ServiceResponse<List<GetReservationDTO>>> GetReservationsByUser(string userId, long? marketId, DateTime createdSince, DateTime pickupSince, DateTime pickup, StatusEnum? status);
        Task<ServiceResponse<List<GetReservationDTO>>> GetReservationsBySeller(long sellerId, long? marketId, DateTime createdSince, DateTime pickupSince, DateTime pickup, StatusEnum? status);
        Task<ServiceResponse<List<GetTimeslotDTO>>> GetTimeslots(long marketId, long sellerId);

        Task<ServiceResponse<GetReservationDTO>> AddReservation(AddReservationDTO newReservation);
        Task<ServiceResponse<GetReservationDTO>> UpdateReservation(UpdateReservationDTO updatedReservation);
        Task<ServiceResponse<GetReservationDTO>> SetReservationStatus(long id, SetReservationStatusDTO reservationStatus);  
        Task<ServiceResponse<GetReservationDTO>> DeleteReservation(long id);
    }
}