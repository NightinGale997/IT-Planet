using System.ComponentModel.DataAnnotations;


namespace PlanetIT.Domain.Models.Data
{
    public class AnimalType
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
