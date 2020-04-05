using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Markt2Go.Shared.Helper;

namespace Markt2Go.DTOs.Seller
{
    public class AddMarketSellerDTO : IValidatableObject
    {
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public long SellerId { get; set; }
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public long MarketId { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public string Portfolio { get; set; }
        public int? LastReservationOffset { get; set; }
        public bool DebitCardAccepted { get; set; }
        public bool CreditCardAccepted { get; set; }
        public bool Visible { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // check if portfolio is a valid json
            if (!JsonHelper.IsValidJson(Portfolio))
            {
                yield return new ValidationResult(
                    $"{nameof(Portfolio)} must be a valid json object.",
                    new[] { nameof(Portfolio) });
            }
        }
    }
}