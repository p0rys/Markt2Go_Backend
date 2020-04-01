using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs.Seller
{
    public class AddSellerDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
    }
}