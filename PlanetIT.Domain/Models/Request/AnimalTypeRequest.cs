using System.ComponentModel.DataAnnotations;

namespace PlanetIT.Domain.Models.Request
{
    public class AnimalTypeRequest
    {
        [Required]
        public string Type { get; set; }
    }
}
