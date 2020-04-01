using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs
{
    public class IdDTO
    {
        [Required]
        public string Id { get; set; }
    }
}