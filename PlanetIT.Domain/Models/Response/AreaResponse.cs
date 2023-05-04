using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetIT.Domain.Models.Response
{
    public class AreaResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<LocationPointRequest> AreaPoints { get; set; }

        public AreaResponse()
        {
        }
        public AreaResponse(Area area, List<AreaPoint> areaPoints)
        {
            Id = area.Id;
            Name = area.Name;

            AreaPoints = new List<LocationPointRequest>();

            foreach (var point in areaPoints)
            {
                var locationPoint = new LocationPointRequest
                {
                    Latitude = point.Latitude,
                    Longitude = point.Longitude
                };
                AreaPoints.Add(locationPoint);
            }
        }
    }
}
