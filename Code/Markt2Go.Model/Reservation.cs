
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Markt2Go.Model
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }

        // user
        [Required]
        public string UserId { get; set; }
        [Required]
        [StringLength(50)]
        public string Firstname { get; set; }
        [Required]
        [StringLength(50)]
        public string Lastname { get; set; }
        [Required]
        [StringLength(20)]
        public string Phone { get; set; }
        [Required]
        public DateTime Pickup { get; set; }
        [StringLength(500)]
        public string UserComment { get; set; }
        [Required]
        public bool RulesAccepted {get; set;}
        
        // seller
        [Required]
        public long MarketSellerId { get; set; }
        [Column(TypeName = "money")]
        public decimal Price { get; set; }
        [StringLength(500)]
        public string SellerComment { get; set; }
        public bool? Accepted { get; set; }
        public bool Packed { get; set; }


        public virtual User User { get; set; }
        public virtual MarketSeller MarketSeller { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
