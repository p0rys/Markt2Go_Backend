using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs.Market
{
    public class UpdateMarketDTO : AddMarketDTO
    {
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public long Id { get; set; }
    }
}