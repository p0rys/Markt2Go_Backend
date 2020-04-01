using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using Markt2Go.DTOs.Item;

namespace Markt2Go.DTOs.Reservation
{
    public class AddReservationDTO
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public long MarketId { get; set; }
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public long SellerId { get; set; }
        [Required]
        public DateTime Pickup { get; set; }
        [StringLength(500)]
        public string UserComment { get; set; }


        public List<AddItemDTO> Items { get; set; }
    }
}