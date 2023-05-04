using PlanetIT.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetIT.Domain.Models.Data
{
    public class Animal
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public float Weight { get; set; }

        [Required]
        public float Length { get; set; }

        [Required]
        public float Height { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public LifeStatus LifeStatus { get; set; }

        [Required]
        public DateTime ChippingDateTime { get; set; }

        [Required]
        public int ChipperId { get; set; }

        [Required]
        public long ChippingLocationId { get; set; }
        [ForeignKey("ChippingLocationId")]
        public virtual LocationPoint ChippingLocation { get; set; }
        public DateTime? DeathDateTime { get; set; }
    }
}

