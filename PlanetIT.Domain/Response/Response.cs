using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetIT.Domain.Response
{
    public class Response<T>
    {
        public string Description { get; set; }

        public int StatusCode { get; set; }

        public T Data { get; set; }
    }
}
