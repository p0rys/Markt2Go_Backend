using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Markt2Go.Model
{
    public class Market
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        public int DayOfWeek { get; set; }
        [Required]
        [StringLength(5)]
        public string StartTime { get; set; }
        [Required]
        [StringLength(5)]
        public string EndTime { get; set; }
        [Required]
        [StringLength(100)]
        public string Location { get; set; }
        public bool Visible { get; set; }       

        public virtual ICollection<MarketSeller> MarketSellers { get; set; }
    }
}
