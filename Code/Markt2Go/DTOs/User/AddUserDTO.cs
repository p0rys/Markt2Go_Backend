using System.ComponentModel.DataAnnotations;

namespace Markt2Go.DTOs.User
{
    public class AddUserDTO
    {
        [StringLength(50)]
        public string Firstname { get; set; }
        [StringLength(50)]
        public string Lastname { get; set; }
        [StringLength(50)]
        public string Address { get; set; }
        [StringLength(5)]
        public string Zip { get; set; }
        [StringLength(50)]
        public string Town { get; set; }
        [StringLength(20)]
        public string Phone { get; set; }
    }
}