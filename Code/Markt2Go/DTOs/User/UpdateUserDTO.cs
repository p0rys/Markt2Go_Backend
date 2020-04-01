using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs.User
{
    public class UpdateUserDTO : AddUserDTO
    {
        [Required]
        public string Id { get; set; }
    }
}