using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetIT.Domain.Models.Request
{
    public class AreaRequest
    {
        [Required]
        public string Name { get; set; }
        public List<LocationPointRequest> AreaPoints { get; set; }

    }
}
