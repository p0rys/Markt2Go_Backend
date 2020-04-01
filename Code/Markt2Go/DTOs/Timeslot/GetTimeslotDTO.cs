using System;

namespace Markt2Go.DTOs.Timeslot
{
    public class GetTimeslotDTO
    {
        public long MarketId { get; set; }
        public long SellerId { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime TimestampUTC { get; set; }
        public long ReservationCount { get; set; }
    }
}