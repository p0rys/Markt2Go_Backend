using System;

namespace Markt2Go.DTOs.Market
{
    public class GetSellerMarketDTO : GetMarketDTO
    {
        public string Description { get; set; }
        public string Portfolio { get; set; }
        public int? LastReservationOffset { get; set; }
        public bool DebitCardAccepted { get; set; }
        public bool CreditCardAccepted { get; set; }
        
        public DateTime LastReservation
        {
            get
            {
                return Next.AddHours(LastReservationOffset ?? 0);
            }
        }
        public bool ReservationPossible 
        {
            get            
            {
                return DateTime.UtcNow < LastReservation;
            }
        }
    }
}