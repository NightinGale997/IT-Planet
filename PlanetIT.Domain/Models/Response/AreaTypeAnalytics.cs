using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetIT.Domain.Models.Response
{
    public class AreaTypeAnalytics
    {
        public string AnimalType { get; set; }
        public long AnimalTypeId { get; set; }
        public long QuantityAnimals { get; set; }
        public long AnimalsArrived { get; set; }
        public long AnimalsGone { get; set; }
    }
}
