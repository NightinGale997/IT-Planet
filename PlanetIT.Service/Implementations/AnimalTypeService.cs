using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Response;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Service.Implementations
{
    public class AnimalTypeService : IAnimalTypeService
    {
        private readonly IBaseRepository<AnimalType> _animalTypeRepository;
        private readonly IBaseRepository<AnimalTypeRelation> _animalTypeRelationRepository;

        public AnimalTypeService(IBaseRepository<AnimalType> animalTypeRepository, IBaseRepository<AnimalTypeRelation> animalTypeRelationRepository)
        {
            _animalTypeRepository = animalTypeRepository;
            _animalTypeRelationRepository = animalTypeRelationRepository;
        }

        public async Task<Response<AnimalType>> GetAnimalTypeById(long id)
        {
            // Находим тип в базе данных
            var animalTypeFromDb = await _animalTypeRepository
                .GetAll()
                .FirstOrDefaultAsync(animalType => animalType.Id == id);

            if (animalTypeFromDb == null)
            {
                // 404 | Тип животного не найден
                return new Response<AnimalType>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            // Возвращаем тип
            return new Response<AnimalType>
            {
                Data = animalTypeFromDb,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<AnimalType>> CreateAnimalType(AnimalTypeRequest animalTypeRequest)
        {
            var animalTypeWithSameName = await _animalTypeRepository
                .GetAll()
                .FirstOrDefaultAsync(animalType => animalType.Type == animalTypeRequest.Type);

            if (animalTypeWithSameName != null)
            {
                // 409 | Конфликт имён типов животных
                return new Response<AnimalType>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Создаём новый тип и добавляем в базу данных

            var newAnimalType = new AnimalType { Type = animalTypeRequest.Type };

            await _animalTypeRepository.Create(newAnimalType);

            // Возвращаем созданный тип

            return new Response<AnimalType>
            {
                Data = newAnimalType,
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<Response<AnimalType>> UpdateAnimalType(long id, AnimalTypeRequest animalTypeRequest)
        {
            var animalTypeFromDb = await _animalTypeRepository
                .GetAll()
                .FirstOrDefaultAsync(animalType => animalType.Id == id);

            if (animalTypeFromDb == null)
            {
                // 404 | Тип животного не найден
                return new Response<AnimalType>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Есть ли в базе данных тип с таким же названием?
            var animalTypeWithSameName = await _animalTypeRepository
                .GetAll()
                .FirstOrDefaultAsync(animalType => animalType.Type == animalTypeRequest.Type);
            
            if (animalTypeWithSameName != null)
            {
                // 409 |  Конфликт имён типов животных
                return new Response<AnimalType>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Меняем название типа, сохраняем в базу данных и возвращаем
            animalTypeFromDb.Type = animalTypeRequest.Type;
            animalTypeFromDb = await _animalTypeRepository.Update(animalTypeFromDb);
            
            return new Response<AnimalType>
            {
                Data = animalTypeFromDb,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<bool>> DeleteAnimalType(long id)
        {
            var animalType = await _animalTypeRepository
                .GetAll()
                .FirstOrDefaultAsync(animalType => animalType.Id == id);
            
            if (animalType == null)
            {
                // 404 | Тип животного не найден
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Находим любую связь этого типа с животным
            var animalTypeRelation = await _animalTypeRelationRepository
                .GetAll()
                .FirstOrDefaultAsync(animalTypeRelation => animalTypeRelation.AnimalTypeId == id);

            if (animalTypeRelation != null)
            {
                // 400 | Нельзя удалить тип, так как есть животное с таким типом
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Удаляем тип животного
            await _animalTypeRepository.Delete(animalType);

            return new Response<bool>
            {
                Data = true,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
