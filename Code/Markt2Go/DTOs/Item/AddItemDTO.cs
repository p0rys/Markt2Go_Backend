using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs.Item
{
    public class AddItemDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [Range(0.01, float.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public float Amount { get; set; }
        [StringLength(50)]
        public string ArticleId { get; set; }
        [Required]
        [StringLength(10)]
        public string Unit { get; set; }
    }
}