using System.ComponentModel.DataAnnotations;

namespace PlanetIT.Domain.Models.Request
{
    public class UpdateTypeOfAnimalRequest
    {
        [Range(1, long.MaxValue)]
        public long OldTypeId { get; set; }
        [Range(1, long.MaxValue)]
        public long NewTypeId { get; set; }
    }
}
