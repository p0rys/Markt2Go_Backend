using System;
using System.Threading.Tasks;

namespace Markt2Go.Services.PermissionService
{
    public interface IPermissionService
    {
        Task<bool> UserIsSeller(string userId, long sellerId);
        Task<bool> CanReadReservation(long reservationId, string userId);
        Task<bool> CanUpdateReservation(long reservationId, string userId);
        Task<bool> CanSetReservationStatus(long reservationId, string userId);
        Task<bool> MaxDailyReservationsReached(string userId, long marketId, long sellerId, DateTime pickup);
    }
}