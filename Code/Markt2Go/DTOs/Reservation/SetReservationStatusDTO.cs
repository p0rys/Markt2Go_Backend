using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs.Reservation
{
    public class SetReservationStatusDTO : IValidatableObject
    {
        [Required]
        public bool Accepted { get; set; }
        [Required]
        public bool Packed { get; set; }
        [Required]
        public decimal Price { get; set; }
        [StringLength(500)]
        public string SellerComment { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // check if packed is valid
            if (!Accepted && Packed)
            {
                yield return new ValidationResult(
                    $"A reservation can not be packed while it is not accepted.",
                    new[] { nameof(Accepted), nameof(Packed) });
            }
        }
    }
}