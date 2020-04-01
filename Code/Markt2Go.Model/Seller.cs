using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Markt2Go.Model
{
    public class Seller
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
        public virtual ICollection<MarketSeller> MarketSellers { get; set; }

    }
}
