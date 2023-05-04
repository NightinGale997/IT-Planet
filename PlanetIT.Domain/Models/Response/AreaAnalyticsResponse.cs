using PlanetIT.Domain.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetIT.Domain.Models.Response
{
    public class AreaAnalyticsResponse
    {
        public long TotalQuantityAnimals { get; set; }
        public long TotalAnimalsArrived { get; set; }
        public long TotalAnimalsGone { get; set; }
        public List<AreaTypeAnalytics> AnimalsAnalytics { get; set; }
        //public List<AnimalVisitedLocation> AnimalVisitedLocations { get; set; }
        //public List<AnimalVisitedLocation> AnimalVisitedLocationsInArea { get; set; }
        //public List<Animal> AnimalsRelatedToArea { get; set; }
        //public List<Animal> AnimalsChippedInArea { get; set; }
    }
}
