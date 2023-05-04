using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;
using PlanetIT.Domain.Extensions;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Service.Implementations
{
    public class AnimalVisitedLocationService : IAnimalVisitedLocationPointService
    {
        private readonly IBaseRepository<AnimalVisitedLocation> _animalVisitedLocationRepository;
        private readonly IBaseRepository<Animal> _animalRepository;
        private readonly IBaseRepository<LocationPoint> _locationPointRepository;

        public AnimalVisitedLocationService(IBaseRepository<AnimalVisitedLocation> animalVisitedLocationRepository, IBaseRepository<Animal> animalRepository, IBaseRepository<LocationPoint> locationPointRepository)
        {
            _animalVisitedLocationRepository = animalVisitedLocationRepository;
            _animalRepository = animalRepository;
            _locationPointRepository = locationPointRepository;
        }


        public async Task<Response<AnimalVisitedLocationPointResponse>> SetLocationPointAsVisited(long animalId, long pointId)
        {
            var animalFromDb = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == animalId);

            if (animalFromDb == null)
            {
                // 404 | Животное не найдено
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            else if (animalFromDb.LifeStatus == LifeStatus.DEAD)
            {
                // 400 | Животное мертво
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var locationPointFromDb = await _locationPointRepository
                .GetAll()
                .OrderBy(locationPoint => locationPoint.Id)
                .LastOrDefaultAsync(locationPoint => locationPoint.Id == pointId);

            if (locationPointFromDb == null)
            {
                // 404 | Локация не найдена
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var lastVisitedPoint = await _animalVisitedLocationRepository
                .GetAll()
                .OrderBy(visitedLocationPoint => visitedLocationPoint.Id)
                .LastOrDefaultAsync(visitedLocationPoint => visitedLocationPoint.AnimalId == animalId);

            // Проверка на то, что pointId не является локацией нахождения животного, но оно перемещалось
            if (lastVisitedPoint != null && lastVisitedPoint.LocationPointId == pointId) 
            {
                // 400 | Животное уже находится в этой локации
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            // Проверка на то, что pointId не является точкой чиппирования животного
            else if (lastVisitedPoint == null && pointId == animalFromDb.ChippingLocationId) 
            {
                // 400 | Животное никуда не перемещалось и локация совпадает с локацией чиппирования
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }


            // Локация добавляется в базу данных
            var newVisitedLocation = new AnimalVisitedLocation
            {
                // Округление времени до секунд
                DateTimeOfVisitLocationPoint = DateTime.UtcNow.Round(TimeSpan.FromSeconds(1)), 
                AnimalId = animalId,
                LocationPointId = pointId
            };

            await _animalVisitedLocationRepository.Create(newVisitedLocation);

            var response = new AnimalVisitedLocationPointResponse
            {
                Id = newVisitedLocation.Id,
                DateTimeOfVisitLocationPoint = newVisitedLocation.DateTimeOfVisitLocationPoint.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                LocationPointId = newVisitedLocation.LocationPointId
            };

            // Возвращаем новую посещённую локацию
            return new Response<AnimalVisitedLocationPointResponse>
            {
                Data = response,
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<Response<List<AnimalVisitedLocationPointResponse>>> GetLocationPointsBySearch(long animalId, DateTime? startDateTime, DateTime? endDateTime, int from, int size)
        {
            var animalFromDb = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == animalId);

            if (animalFromDb == null)
            {
                // 404 | Животное не найдено
                return new Response<List<AnimalVisitedLocationPointResponse>>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Создаём Query, к которой будем добавлять условия в зависимости от полученных данных
            var visitedLocationPointsQuery = _animalVisitedLocationRepository.GetAll().AsQueryable();

            // Выбираем локации, относящиеся к нужному животному
            visitedLocationPointsQuery = visitedLocationPointsQuery
                .Where(visitedLocationPoint => visitedLocationPoint.AnimalId == animalId);

            // Добавляем ограничения по времени, если они присутствуют
            if (startDateTime.HasValue)
            {
                visitedLocationPointsQuery = visitedLocationPointsQuery
                    .Where(visitedLocationPoint => visitedLocationPoint.DateTimeOfVisitLocationPoint >= startDateTime);
            }
            if (endDateTime.HasValue)
            {
                visitedLocationPointsQuery = visitedLocationPointsQuery
                    .Where(visitedLocationPoint => visitedLocationPoint.DateTimeOfVisitLocationPoint <= endDateTime.Value);
            }

            // Получаем все данные в один список
            var visitedLocationPoints = await visitedLocationPointsQuery
                .OrderBy(visitedLocationPoint => visitedLocationPoint.Id)
                .Skip(from)
                .Take(size)
                .ToListAsync();

            var searchResult = new List<AnimalVisitedLocationPointResponse>();

            // Преобразовываем данные из базы данных в данные для response
            // + Форматируем дату посещения
            foreach (var visitedLocationPoint in visitedLocationPoints)
            {
                searchResult.Add(new AnimalVisitedLocationPointResponse
                {
                    Id = visitedLocationPoint.Id,
                    DateTimeOfVisitLocationPoint = visitedLocationPoint.DateTimeOfVisitLocationPoint.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    LocationPointId = visitedLocationPoint.LocationPointId,
                });
            }
            // Возвращаем результат поиска
            return new Response<List<AnimalVisitedLocationPointResponse>>
            {
                Data = searchResult,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<AnimalVisitedLocationPointResponse>> UpdateLocationPoint(long animalId, AnimalVisitedLocationPointRequest animalVisitedLocationPointRequest)
        {
            var animalFromDb = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == animalId);

            if (animalFromDb == null)
            {
                // 404 | Животное не найдено
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            else if (animalFromDb.LifeStatus == LifeStatus.DEAD)
            {
                // 400 | Животное мертво
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Находим локацию для редактирования
            var visitedLocationPointFromDb = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint =>
                    visitedLocationPoint.Id == animalVisitedLocationPointRequest.VisitedLocationPointId
                    && visitedLocationPoint.AnimalId == animalId);

            if (visitedLocationPointFromDb == null)
            {
                // 404 | Локация не найдена
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Есть ли локация с такими же данными посещения
            var sameVisitedLocationPoint = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint => 
                    visitedLocationPoint.LocationPointId == animalVisitedLocationPointRequest.LocationPointId
                    && visitedLocationPoint.AnimalId == animalId);

            if (sameVisitedLocationPoint != null)
            {
                // 400 | Посещённая локация с такими данными уже существует
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Находим локацию, на которую надо будет заменить старую
            var locationPointFromRequest = await _locationPointRepository
                .GetAll()
                .OrderBy(locationPoint => locationPoint.Id)
                .LastOrDefaultAsync(locationPoint => locationPoint.Id == animalVisitedLocationPointRequest.LocationPointId);

            if (locationPointFromRequest == null)
            {
                // 404 | Локация не найдена
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (visitedLocationPointFromDb.LocationPointId == animalVisitedLocationPointRequest.LocationPointId)
            {
                // 400 | Обновление локации на такую же
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Первая посещённая животным локация
            var firstVisitedLocationPoint = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint => visitedLocationPoint.AnimalId == animalId);

            if (firstVisitedLocationPoint == visitedLocationPointFromDb
                && animalVisitedLocationPointRequest.LocationPointId == animalFromDb.ChippingLocationId)
            {
                // 400 | Замена первой посещённой локации на точку чипирования
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Находим следующую и предыдущую посещённые локации
            var nextVisitedLocationPoint = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint => 
                    visitedLocationPoint.AnimalId == animalId 
                    && visitedLocationPoint.Id > animalVisitedLocationPointRequest.VisitedLocationPointId);
            var prevVisitedLocationPoint = await _animalVisitedLocationRepository
                .GetAll()
                .OrderBy(visitedLocationPoint => visitedLocationPoint.Id)
                .LastOrDefaultAsync(visitedLocationPoint => 
                    visitedLocationPoint.AnimalId == animalId 
                    && visitedLocationPoint.Id < animalVisitedLocationPointRequest.VisitedLocationPointId);
            
            if (nextVisitedLocationPoint != null
                && (prevVisitedLocationPoint == visitedLocationPointFromDb
                || nextVisitedLocationPoint == visitedLocationPointFromDb))
            {
                return new Response<AnimalVisitedLocationPointResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            visitedLocationPointFromDb.LocationPointId = animalVisitedLocationPointRequest.LocationPointId;
            visitedLocationPointFromDb = await _animalVisitedLocationRepository.Update(visitedLocationPointFromDb);


            // Возвращаем обновлённую посещённую локацию, форматируем дату
            var response = new AnimalVisitedLocationPointResponse
            {
                Id = visitedLocationPointFromDb.Id,
                DateTimeOfVisitLocationPoint = visitedLocationPointFromDb.DateTimeOfVisitLocationPoint.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                LocationPointId = visitedLocationPointFromDb.LocationPointId
            };

            return new Response<AnimalVisitedLocationPointResponse>
            {
                Data = response,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<bool>> RemoveLocationPointFromVisited(long animalId, long visitedPointId)
        {
            var animalFromDb = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.Id == animalId);

            if (animalFromDb == null)
            {
                // 404 | Животное не найдено
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Количество посещённых точек этим животным
            int visitedLocationPointsAmount = await _animalVisitedLocationRepository
                .GetAll()
                .Where(visitedLocationPoint => visitedLocationPoint.AnimalId == animalId)
                .CountAsync();
            // Посещённая точка, которую надо удалить
            var visitedLocationToDelete = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint => 
                    visitedLocationPoint.Id == visitedPointId 
                    && visitedLocationPoint.AnimalId == animalId);

            if (visitedLocationToDelete == null)
            {
                // 404 | Локация не найдена
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            
            // Следующая и предыдущие посещённые локации, этим животным
            var nextVisitedLocationPoint = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint => 
                    visitedLocationPoint.AnimalId == animalId 
                    && visitedLocationPoint.Id > visitedPointId);
            var prevVisitedLocationPoint = await _animalVisitedLocationRepository
                .GetAll()
                .OrderBy(visitedLocationPoint => visitedLocationPoint.Id)
                .LastOrDefaultAsync(visitedLocationPoint => 
                    visitedLocationPoint.AnimalId == animalId 
                    && visitedLocationPoint.Id < visitedPointId);

            // Удаляем точку
            await _animalVisitedLocationRepository.Delete(visitedLocationToDelete);

            // Находим есть ли среди посещённых точек точка чипирования этого животного
            var visitedChippingLocationPoint = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint => 
                    visitedLocationPoint.AnimalId == animalId 
                    && visitedLocationPoint.LocationPointId == animalFromDb.ChippingLocationId);

            // Если удалённая точка была первой и за ней следует точка чипирования, то точку чипирования нужно удалить
            if (prevVisitedLocationPoint == null 
                && nextVisitedLocationPoint != null 
                && nextVisitedLocationPoint == visitedChippingLocationPoint)
            {
                await _animalVisitedLocationRepository.Delete(visitedChippingLocationPoint);
            }

            return new Response<bool>
            {
                Data = true,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
