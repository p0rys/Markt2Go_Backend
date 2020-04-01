using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs.Item
{
    public class UpdateItemDTO : AddItemDTO
    {
        [Required]
        public long Id { get; set; }
    }
}