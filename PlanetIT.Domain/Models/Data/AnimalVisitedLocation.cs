using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetIT.Domain.Models.Data
{
    public class AnimalVisitedLocation
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public DateTime DateTimeOfVisitLocationPoint { get; set; }

        public long LocationPointId { get; set; }

        public long AnimalId { get; set; }

        [ForeignKey("LocationPointId")]
        public virtual LocationPoint LocationPoint { get; set; }
        [ForeignKey("AnimalId")]
        public virtual Animal Animal { get; set; }
    }
}

