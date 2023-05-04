using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;

namespace PlanetIT.Service.Interfaces
{
    public interface IAreaService
    {
        Task<Response<AreaResponse>> CreateArea(AreaRequest areaRequest);
        Task<Response<AreaResponse>> GetAreaById(long id);
        Task<Response<AreaResponse>> UpdateArea(long id, AreaRequest areaRequest);
        Task<Response<bool>> DeleteArea(long id);
        Task<Response<AreaAnalyticsResponse>> GetAreaAnalytics(long id, DateTime startDate, DateTime endDate);
    }
}
