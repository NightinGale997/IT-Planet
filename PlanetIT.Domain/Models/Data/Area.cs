using System.ComponentModel.DataAnnotations;

namespace PlanetIT.Domain.Models.Data
{
    public class Area
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
