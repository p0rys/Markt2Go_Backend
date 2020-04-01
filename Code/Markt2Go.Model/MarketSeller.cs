using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Markt2Go.Model
{
    public class MarketSeller
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long SellerId { get; set; }
        public long MarketId { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public string Portfolio { get; set; }
        public int? LastReservationOffset { get; set; }
        public bool DebitCardAccepted { get; set; }
        public bool CreditCardAccepted { get; set; }
        public bool Visible { get; set; }

        public virtual Seller Seller { get; set; }
        public virtual Market Market { get; set; }
    }
}
