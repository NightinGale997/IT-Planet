using System.ComponentModel.DataAnnotations;

namespace PlanetIT.Domain.Models.Data
{
    public class AreaPoint
    {
        [Key]
        public long Id { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }
        public long AreaId { get; set; }
    }
}
