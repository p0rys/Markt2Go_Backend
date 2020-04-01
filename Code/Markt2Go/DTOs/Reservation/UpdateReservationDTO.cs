using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Markt2Go.DTOs.Item;

namespace Markt2Go.DTOs.Reservation
{
    public class UpdateReservationDTO
    {
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public long Id { get; set; }
        [Required]
        public DateTime Pickup { get; set; }
        [Required]
        public decimal Price { get; set; }
        [StringLength(500)]
        public string UserComment { get; set; }

        public List<GetItemDTO> Items { get; set; }
    }
}