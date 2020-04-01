using System;

namespace Markt2Go.DTOs.User
{
    public class GetUserDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Mail { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Address { get; set; }
        public string Zip { get; set; }
        public string Town { get; set; }
        public string Phone { get; set; }
        public long? SellerId { get; set; }
        public bool IsValidated { get; set; }
    }
}