using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Response;

namespace PlanetIT.Service.Interfaces
{
    public interface IAnimalTypeService
    {
        Task<Response<AnimalType>> GetAnimalTypeById(long id);
        Task<Response<AnimalType>> CreateAnimalType(AnimalTypeRequest animalTypeRequest);
        Task<Response<AnimalType>> UpdateAnimalType(long id, AnimalTypeRequest animalTypeRequest);
        Task<Response<bool>> DeleteAnimalType(long id);
    }

}
