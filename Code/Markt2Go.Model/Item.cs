using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Markt2Go.Model
{
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(50)]
        public long ReservationId { get; set; }
        [Required]
        public string Name { get; set; }
        [StringLength(50)]
        public string ArticleId { get; set; }
        [Required]
        public float Amount { get; set; }
        [StringLength(10)]
        public string Unit { get; set; }

        public virtual Reservation Reservation { get; set; }
    }
}
