using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetIT.Domain.Models.Data
{
    public class AnimalTypeRelation
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long AnimalId { get; set; }

        [Required]
        public long AnimalTypeId { get; set; }
        [ForeignKey("AnimalTypeId")]
        public virtual AnimalType AnimalType { get; set; }
    }
}

