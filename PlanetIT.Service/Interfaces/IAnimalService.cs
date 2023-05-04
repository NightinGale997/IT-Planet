using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;

namespace PlanetIT.Service.Interfaces
{
    public interface IAnimalService
    {
        Task<Response<AnimalResponse>> CreateAnimal(NewAnimalRequest animalRequest);
        Task<Response<AnimalResponse>> GetAnimalById(long id);
        Task<Response<List<AnimalResponse>>> GetAnimalsBySearch(DateTime? startDateTime, DateTime? endDateTime, int? chipperId, long? chippingLocationId, LifeStatus? lifeStatus, Gender? gender, int from, int size);
        Task<Response<AnimalResponse>> UpdateAnimal(long id, AnimalRequest animalRequest);
        Task<Response<bool>> DeleteAnimal(long id);
        Task<Response<AnimalResponse>> AddTypeToAnimal(long animalId, long typeId);
        Task<Response<AnimalResponse>> UpdateTypeOfAnimal(long animalId, UpdateTypeOfAnimalRequest updateTypeOfAnimalRequest);
        Task<Response<AnimalResponse>> RemoveTypeFromAnimal(long animalId, long typeId);
    }
}
