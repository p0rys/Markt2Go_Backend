using System;

namespace Markt2Go.DTOs.Seller
{
    public class GetMarketSellerDTO : GetSellerDTO
    {
        public string Description { get; set; }
        public string Portfolio { get; set; }
        public int? LastReservationOffset { get; set; }
        public bool DebitCardAccepted { get; set; }
        public bool CreditCardAccepted { get; set; }
        public bool Visible { get; set; }

        public DateTime LastReservation { get; set; }
        public bool ReservationPossible 
        {
            get            
            {
                return DateTime.Now < LastReservation;
            }
        }
    }
}