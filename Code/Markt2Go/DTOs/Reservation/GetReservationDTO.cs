using System;
using System.Collections.Generic;
using Markt2Go.DTOs.Item;
using Markt2Go.DTOs.Market;
using Markt2Go.DTOs.Seller;
using Markt2Go.DTOs.User;

namespace Markt2Go.DTOs.Reservation
{
    public class GetReservationDTO
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public long MarketId { get; set; }
        public long SellerId { get; set; }
        public DateTime Pickup { get; set; }
        public decimal Price { get; set; }
        public string UserComment { get; set; }
        public string SellerComment { get; set; }
        public bool? Accepted { get; set; }
        public bool Packed { get; set; }

        public GetMarketDTO Market { get; set; }
        public GetSellerDTO Seller { get; set; }
        public GetUserDTO User { get; set; }

        public List<GetItemDTO> Items { get; set; }
    }
}