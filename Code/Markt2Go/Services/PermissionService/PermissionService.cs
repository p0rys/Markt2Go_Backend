using System;
using System.Linq;
using System.Threading.Tasks;
using Markt2Go.Data;
using Microsoft.EntityFrameworkCore;

namespace Markt2Go.Services.PermissionService
{
    public class PermissionService : IPermissionService
    {
        private const int c_maxDailyReservations = 3;

        private readonly DataContext _context;

        public PermissionService(DataContext context)
        {
            if (context == null)
                throw new ArgumentNullException($"{nameof(context)} can't be null.");

            _context = context;
        }

        public async Task<bool> UserIsSeller(string userId, long sellerId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user == null ? false : user.SellerId == sellerId;
        }

        public async Task<bool> CanReadReservation(long reservationId, string userId)
        {
            // prefer AnyAsync because the sellerId would not be available when using FindAsync
            return await _context.Reservations.AnyAsync(x => x.Id == reservationId && (x.UserId == userId || x.MarketSeller.Seller.Users.Any(y => y.Id == userId)));
        }
        public async Task<bool> CanUpdateReservation(long reservationId, string userId)
        {
            // prefer FindAsync because it searches the local context before requesting the database
            var reservation = await _context.Reservations.FindAsync(reservationId);
            return reservation == null ? false : reservation.UserId == userId;
        }
        public async Task<bool> CanSetReservationStatus(long reservationId, string userId)
        {
            // prefer AnyAsync because the sellerId would not be available when using FindAsny
            var reservation = await _context.Reservations.Include(x => x.MarketSeller).SingleOrDefaultAsync(x => x.Id == reservationId);
            return reservation == null ? false : await UserIsSeller(userId, reservation.MarketSeller.SellerId);
        }
    
        public async Task<bool> MaxDailyReservationsReached(string userId, long marketId, long sellerId, DateTime pickup)
        {
            var reservationCount = await _context.Reservations.CountAsync(x => x.UserId == userId 
                && x.MarketSeller.MarketId == marketId 
                && x.MarketSeller.SellerId == sellerId 
                && x.Pickup.Date == pickup.Date);
                
            return reservationCount >= c_maxDailyReservations;
        }
    }
}