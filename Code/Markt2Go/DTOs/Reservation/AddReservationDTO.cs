using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper.Configuration.Annotations;
using Markt2Go.DTOs.Item;

namespace Markt2Go.DTOs.Reservation
{
    public class AddReservationDTO : IValidatableObject
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
        [StringLength(50)]
        public string Firstname { get; set; }
        [Required]
        [StringLength(50)]
        public string Lastname { get; set; }
        [Required]
        [StringLength(20)]
        public string Phone { get; set; }
        [Required]
        public DateTime Pickup { get; set; }
        [StringLength(500)]
        public string UserComment { get; set; }        
        public bool RememberMe { get; set; }
        public bool RulesAccepted { get; set; }

        public List<AddItemDTO> Items { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // check if user has accepted the rules
            if (!RulesAccepted)
            {
                yield return new ValidationResult(
                    $"You must accepted the rules to add a new reservation.",
                    new[] { nameof(RulesAccepted) });
            }
        }
    }
}