using System.ComponentModel.DataAnnotations;

namespace PlanetIT.Domain.Models.Request
{
    public class AnimalVisitedLocationPointRequest
    {
        [Range(1, long.MaxValue)]
        public long VisitedLocationPointId { get; set; }
        [Range(1, long.MaxValue)]
        public long LocationPointId { get; set; }
    }
}
