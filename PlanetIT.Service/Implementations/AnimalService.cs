using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Extensions;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Service.Implementations
{
    public class AnimalService : IAnimalService
    {
        private readonly IBaseRepository<AnimalVisitedLocation> _animalVisitedLocationRepository;
        private readonly IBaseRepository<Animal> _animalRepository;
        private readonly IBaseRepository<AnimalTypeRelation> _animalTypeRelationRepository;
        private readonly IBaseRepository<AnimalType> _animalTypeRepository;
        private readonly IBaseRepository<Account> _accountRepository;
        private readonly IBaseRepository<LocationPoint> _locationPointRepository;

        public AnimalService(IBaseRepository<AnimalType> animalTypeRepository,
            IBaseRepository<AnimalVisitedLocation> animalVisitedLocationRepository,
            IBaseRepository<Animal> animalRepository,
            IBaseRepository<AnimalTypeRelation> animalTypeRelationRepository,
            IBaseRepository<Account> accountRepository,
            IBaseRepository<LocationPoint> locationPointRepository
            )
        {
            _animalVisitedLocationRepository = animalVisitedLocationRepository;
            _animalRepository = animalRepository;
            _animalTypeRelationRepository = animalTypeRelationRepository;
            _animalTypeRepository = animalTypeRepository;
            _accountRepository = accountRepository;
            _locationPointRepository = locationPointRepository;
        }

        

        public async Task<Response<AnimalResponse>> CreateAnimal(NewAnimalRequest animalRequest)
        {
            var chippingLocation = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint => locationPoint.Id == animalRequest.ChippingLocationId);

            if (chippingLocation == null)
            {
                // 404 | Точка чипирования животного не найдена
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var chipper = await _accountRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Id == animalRequest.ChipperId);

            if (chipper == null)
            {
                // 404 | Аккаунт чипировавшего не найден
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var types = new List<AnimalType>();

            // Итерируемся по массиву id типов
            foreach (var typeId in animalRequest.AnimalTypes)
            {
                var type = await _animalTypeRepository
                    .GetAll()
                    .FirstOrDefaultAsync(animalType => animalType.Id == typeId);
                if (type == null)
                {
                    // 404 | Тип животного не найден
                    return new Response<AnimalResponse>
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                if (types.Contains(type))
                {
                    // 409 | Дубликаты типов
                    return new Response<AnimalResponse>
                    {
                        StatusCode = StatusCodes.Status409Conflict
                    };
                }
                types.Add(type);
            }

            // Создаём животное по запросу и сохраняем в базу данных
            var newAnimal = new Animal
            {
                Length = animalRequest.Length,
                Weight = animalRequest.Weight,
                Height = animalRequest.Height,
                LifeStatus = LifeStatus.ALIVE,
                ChipperId = animalRequest.ChipperId,
                ChippingLocationId = animalRequest.ChippingLocationId,
                // Округление времени до секунд
                ChippingDateTime = DateTime.UtcNow.Round(TimeSpan.FromSeconds(1)), 
                Gender = animalRequest.Gender,
            };

            await _animalRepository.Create(newAnimal);


            // Добавляем в базу данных информацию о связке животного и типов

            var animalTypesRelations = new List<AnimalTypeRelation>();

            foreach (var animalType in types)
            {
                var animalTypeRelation = new AnimalTypeRelation { AnimalId = newAnimal.Id, AnimalTypeId = animalType.Id };
                await _animalTypeRelationRepository.Create(animalTypeRelation);
                animalTypesRelations.Add(animalTypeRelation);
            }

            // Возвращаем животного с пустым массивом посещённых точек

            var animalResponse = new AnimalResponse(newAnimal, animalTypesRelations.ToArray(), Array.Empty<AnimalVisitedLocation>());

            return new Response<AnimalResponse>
            {
                Data = animalResponse,
                StatusCode = StatusCodes.Status201Created
            };
        }
        public async Task<Response<AnimalResponse>> GetAnimalById(long id)
        {
            var animalFromDb = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == id);
            if (animalFromDb == null)
            {
                // 404 | Животное не найдено
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Находим все типы животного
            var types = await _animalTypeRelationRepository
                .GetAll()
                .Where(animalType => animalType.AnimalId == id)
                .ToArrayAsync();
            // Находим все посещённые локации
            var visitedLocations = await _animalVisitedLocationRepository
                .GetAll()
                .Where(visitedLocationPoint => visitedLocationPoint.AnimalId == id)
                .ToArrayAsync();
            // Создаём и отправляем response
            var animalResponse = new AnimalResponse(animalFromDb, types, visitedLocations);

            return new Response<AnimalResponse>
            {
                Data = animalResponse,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<List<AnimalResponse>>> GetAnimalsBySearch(DateTime? startDateTime, DateTime? endDateTime, int? chipperId, long? chippingLocationId, LifeStatus? lifeStatus, Gender? gender, int from, int size)
        {

            // Создаём query и добавляем к ней ограничения поиска, которые присутствуют

            var animalsQuery = _animalRepository.GetAll().AsQueryable();

            if (startDateTime.HasValue)
            {
                animalsQuery = animalsQuery.Where(animal => animal.ChippingDateTime >= startDateTime.Value);
            }

            if (endDateTime.HasValue)
            {
                animalsQuery = animalsQuery.Where(animal => animal.ChippingDateTime <= endDateTime.Value);
            }

            if (chipperId.HasValue)
            {
                animalsQuery = animalsQuery.Where(animal => animal.ChipperId == chipperId.Value);
            }

            if (chippingLocationId.HasValue)
            {
                animalsQuery = animalsQuery.Where(animal => animal.ChippingLocationId == chippingLocationId.Value);
            }

            if (lifeStatus.HasValue)
            {
                animalsQuery = animalsQuery.Where(animal => animal.LifeStatus == lifeStatus);
            }

            if (gender.HasValue)
            {
                animalsQuery = animalsQuery.Where(animal => animal.Gender == gender);
            }

            // Получаем результаты поиска и добавляем их в новый массив

            var animals = await animalsQuery
                .OrderBy(animal => animal.Id)
                .Skip(from)
                .Take(size)
                .ToListAsync();

            var animalsResponse = new List<AnimalResponse>();

            foreach (var animal in animals)
            {
                var types = await _animalTypeRelationRepository
                    .GetAll()
                    .Where(animalType => animalType.AnimalId == animal.Id).ToArrayAsync();
                var visitedLocations = await _animalVisitedLocationRepository
                    .GetAll()
                    .Where(visitedLocation => visitedLocation.AnimalId == animal.Id).ToArrayAsync();

                var animalResponse = new AnimalResponse(animal, types, visitedLocations);
                animalsResponse.Add(animalResponse);
            }

            return new Response<List<AnimalResponse>>
            {
                Data = animalsResponse,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<AnimalResponse>> UpdateAnimal(long id, AnimalRequest animalRequest)
        {
            var animalToUpdate = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Id == id);
            if (animalToUpdate == null)
            {
                // 404 | Животное не найдено
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            if (animalToUpdate.LifeStatus == LifeStatus.DEAD && animalRequest.LifeStatus == LifeStatus.ALIVE)
            {
                // 400 | Попытка смены статуса жизни животного с мёртвого на живое
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            var chippingLocation = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint => locationPoint.Id == animalRequest.ChippingLocationId);

            if (chippingLocation == null)
            {
                // 404 | Точка чипирования не найдена
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var chipper = await _accountRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Id == animalRequest.ChipperId);

            if (chipper == null)
            {
                // 404 | Аккаунт чипировавшего не найден
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Получаем типы животного и посещённые локации
            var types = await _animalTypeRelationRepository
                .GetAll()
                .Where(animalType => animalType.AnimalId == id)
                .ToArrayAsync();
            var visitedLocations = await _animalVisitedLocationRepository
                .GetAll()
                .Where(visitedLocation => visitedLocation.AnimalId == id)
                .ToArrayAsync();

            if (visitedLocations.Length > 0 && animalRequest.ChippingLocationId == visitedLocations[0].LocationPointId)
            {
                // 400 | Первая посещённая точка совпадает с точкой чипирования
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Обновляем поля и сохраняем в базу данных
            animalToUpdate.Length = animalRequest.Length;
            animalToUpdate.Weight = animalRequest.Weight;
            animalToUpdate.Height = animalRequest.Height;
            animalToUpdate.Gender = animalRequest.Gender;
            if (animalRequest.LifeStatus == LifeStatus.DEAD)
            {
                // Так как нужно поставить статус Dead, то сохраняем также дату смерти
                animalToUpdate.LifeStatus = animalRequest.LifeStatus;
                animalToUpdate.DeathDateTime = DateTime.UtcNow.Round(TimeSpan.FromSeconds(1)); // Округление времени до секунд
            }
            animalToUpdate.ChipperId = animalRequest.ChipperId;

            animalToUpdate.ChippingLocationId = animalRequest.ChippingLocationId;

            animalToUpdate = await _animalRepository.Update(animalToUpdate);

            var animalResponse = new AnimalResponse(animalToUpdate, types, visitedLocations);

            return new Response<AnimalResponse>
            {
                Data = animalResponse,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<bool>> DeleteAnimal(long id)
        {
            var animalToDelete = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == id);

            if (animalToDelete == null)
            {
                // 404 | Животное не найдено
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Пытаемся найти любую посещённую точку
            var visitedLocation = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint => visitedLocationPoint.AnimalId == id);

            if (visitedLocation != null)
            {
                // 400 | Животное связано с посещёнными точками
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Удаляем все связи типов и животное
            var types = await _animalTypeRelationRepository
                .GetAll()
                .Where(animalType => animalType.AnimalId == id)
                .ToArrayAsync();
            foreach (var type in types)
            {
                await _animalTypeRelationRepository.Delete(type);
            }

            await _animalRepository.Delete(animalToDelete);

            return new Response<bool>
            {
                Data = true,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<AnimalResponse>> AddTypeToAnimal(long animalId, long typeId)
        {
            var animalFromDb = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == animalId);
            if (animalFromDb == null)
            {
                // 404 | Животное не найдено
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            var type = await _animalTypeRepository
                .GetAll()
                .FirstOrDefaultAsync(animalType => animalType.Id == typeId);
            if (type == null)
            {
                // 404 | Тип не найден
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var sameAnimalTypeRelation = await _animalTypeRelationRepository
                .GetAll()
                .FirstOrDefaultAsync(animalTypeRelation => animalTypeRelation.AnimalId == animalId && animalTypeRelation.AnimalTypeId == typeId);

            if (sameAnimalTypeRelation != null)
            {
                // 409 | Такой тип уже привязан к животному
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Создаём связь между типом и животным
            var animalTypeRelation = new AnimalTypeRelation { AnimalTypeId = typeId, AnimalId = animalId };

            await _animalTypeRelationRepository.Create(animalTypeRelation);

            // Находим все типы животного и посещённые локации, возвращаем в response
            var types = await _animalTypeRelationRepository
                .GetAll()
                .Where(animalTypeRelation => animalTypeRelation.AnimalId == animalId)
                .ToArrayAsync();
            var visitedLocations = await _animalVisitedLocationRepository
                .GetAll()
                .Where(visitedLocationPoint => visitedLocationPoint.AnimalId == animalId)
                .ToArrayAsync();

            var animalResponse = new AnimalResponse(animalFromDb, types, visitedLocations);

            return new Response<AnimalResponse>
            {
                Data = animalResponse,
                StatusCode = StatusCodes.Status201Created
            };
        }
        public async Task<Response<AnimalResponse>> UpdateTypeOfAnimal(long animalId, UpdateTypeOfAnimalRequest request)
        {
            var animalFromDb = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == animalId);
            if (animalFromDb == null)
            {
                // 404 | Животное не найдено
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Находим все типы животного и посещённые локации
            var types = await _animalTypeRelationRepository
                .GetAll()
                .Where(animalTypeRelation => animalTypeRelation.AnimalId == animalId)
                .ToArrayAsync();
            var visitedLocations = await _animalVisitedLocationRepository
                .GetAll()
                .Where(visitedLocationPoint => visitedLocationPoint.AnimalId == animalId)
                .ToArrayAsync();
                
            if (types.FirstOrDefault(animalTypeRelation => animalTypeRelation.AnimalTypeId == request.NewTypeId) != null)
            {
                // 409 | Такой тип уже существует
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            var oldType = await _animalTypeRepository
                .GetAll()
                .FirstOrDefaultAsync(animalType => animalType.Id == request.OldTypeId);
            if (oldType == null)
            {
                // 404 | Тип не найден
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            var newType = await _animalTypeRepository
                .GetAll()
                .FirstOrDefaultAsync(animalType => animalType.Id == request.NewTypeId);
            if (newType == null)
            {
                // 404 | Тип не найден
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            var typeRelation = await _animalTypeRelationRepository
                .GetAll()
                .FirstOrDefaultAsync(animalTypeRelation => 
                    animalTypeRelation.AnimalId == animalId 
                    && (animalTypeRelation.AnimalTypeId == request.OldTypeId || animalTypeRelation.AnimalTypeId == request.NewTypeId));

            if (typeRelation == null)
            {
                // 404 | Животное не связано с этим типом
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Обновляем связь, заносим данные в базу данных
            typeRelation.AnimalTypeId = request.NewTypeId;
            typeRelation = await _animalTypeRelationRepository.Update(typeRelation);

            var animalResponse = new AnimalResponse(animalFromDb, types, visitedLocations);

            return new Response<AnimalResponse>
            {
                Data = animalResponse,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<AnimalResponse>> RemoveTypeFromAnimal(long animalId, long typeId)
        {
            var animalFromDb = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == animalId);
            if (animalFromDb == null)
            {
                // 404 | Животное не найдено
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            var typeRelation = await _animalTypeRelationRepository
                .GetAll()
                .FirstOrDefaultAsync(animalTypeRelation => 
                    animalTypeRelation.AnimalId == animalId
                    && animalTypeRelation.AnimalTypeId == typeId);
            if (typeRelation == null)
            {
                // 404 | Животное не связано с типом
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            // Получаем все посещённые локации и типы животного
            var types = await _animalTypeRelationRepository.GetAll().Where(t => t.AnimalId == animalId).ToArrayAsync();
            var visitedLocations = await _animalVisitedLocationRepository.GetAll().Where(vl => vl.AnimalId == animalId).ToArrayAsync();
            if (types.Length == 1)
            {
                // 400 | Нельзя удалить тип, поскольку он у животного единственный
                return new Response<AnimalResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            await _animalTypeRelationRepository.Delete(typeRelation);

            var animalResponse = new AnimalResponse(animalFromDb, types, visitedLocations);

            return new Response<AnimalResponse>
            {
                Data = animalResponse,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
