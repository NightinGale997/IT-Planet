using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;

namespace PlanetIT.Service.Interfaces
{
    public interface ILocationPointService
    {
        Task<Response<LocationPoint>> GetLocationPointById(long id);
        Task<Response<LocationPoint>> CreateLocationPoint(LocationPointRequest locationPointRequest);
        Task<Response<LocationPoint>> UpdateLocationPoint(long id, LocationPointRequest locationPointRequest);
        Task<Response<bool>> DeleteLocationPoint(long id);
        Task<Response<long>> GetId(double latitude, double longitude);
        Task<Response<string>> GetGeoHash(double latitude, double longitude, int v);
    }
}
