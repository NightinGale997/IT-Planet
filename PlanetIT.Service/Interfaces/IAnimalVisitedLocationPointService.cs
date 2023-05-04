using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;

namespace PlanetIT.Service.Interfaces
{
    public interface IAnimalVisitedLocationPointService
    {
        Task<Response<AnimalVisitedLocationPointResponse>> SetLocationPointAsVisited(long animalId, long pointId);
        Task<Response<List<AnimalVisitedLocationPointResponse>>> GetLocationPointsBySearch(long animalId, DateTime? startDateTime, DateTime? endDateTime, int from, int size);
        Task<Response<AnimalVisitedLocationPointResponse>> UpdateLocationPoint(long animalId, AnimalVisitedLocationPointRequest animalVisitedLocationRequest);
        Task<Response<bool>> RemoveLocationPointFromVisited(long animalId, long visitedPointId);
    }
}
