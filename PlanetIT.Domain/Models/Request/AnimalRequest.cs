using PlanetIT.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PlanetIT.Domain.Models.Request
{
    public class AnimalRequest
    {
        [Range(float.Epsilon, float.MaxValue)]
        public float Weight { get; set; }
        [Range(float.Epsilon, float.MaxValue)]
        public float Length { get; set; }
        [Range(float.Epsilon, float.MaxValue)]
        public float Height { get; set; }
        public Gender Gender { get; set; }
        public LifeStatus LifeStatus { get; set; }
        [Range(1, int.MaxValue)]
        public int ChipperId { get; set; }
        [Range(1, long.MaxValue)]
        public long ChippingLocationId { get; set; }
    }
}
