using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Helpers;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Service.Implementations
{
    public class AreaService : IAreaService
    {
        private readonly IBaseRepository<Area> _areaRepository;
        private readonly IBaseRepository<AreaPoint> _areaPointRepository;
        private readonly IBaseRepository<AnimalVisitedLocation> _animalVisitedLocationRepository;
        private readonly IBaseRepository<Animal> _animalRepository;
        private readonly IBaseRepository<AnimalTypeRelation> _animalTypeRelationRepository;

        public AreaService(IBaseRepository<Area> areaRepository,
            IBaseRepository<AreaPoint> areaPointRepository,
            IBaseRepository<AnimalVisitedLocation> animalVisitedLocationRepository,
            IBaseRepository<Animal> animalRepository,
            IBaseRepository<AnimalTypeRelation> animalTypeRelationRepository)
        {
            _areaRepository = areaRepository;
            _areaPointRepository = areaPointRepository;
            _animalVisitedLocationRepository = animalVisitedLocationRepository;
            _animalRepository = animalRepository;
            _animalTypeRelationRepository = animalTypeRelationRepository;
        }

        public async Task<Response<AreaResponse>> CreateArea(AreaRequest areaRequest)
        {
            var areaWithSameName = await _areaRepository.GetAll().FirstOrDefaultAsync(a => a.Name == areaRequest.Name);

            if (areaWithSameName != null)
            {
                // 409 | Конфликт имён зоны
                return new Response<AreaResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            if (areaRequest.AreaPoints.Count < 3 
                || areaRequest.AreaPoints.Any(areaPoint => areaPoint == null)
                || !AreasHelper.IsSelfDiscountingPolygon(areaRequest.AreaPoints))
            {
                // 400 | Содержит менее трёх точек, null значения вместо точек
                // или нельзя создать самонепересекающейся многоугольник
                return new Response<AreaResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }


            var allAreas = await _areaRepository.GetAll().ToListAsync();

            var polygonFromRequestPoints = new Polygon(areaRequest.AreaPoints);

            //Проверяется наличие пересечения со всеми другими зонами в базе данных
            foreach (var area in allAreas)
            {
                var areaPoints = _areaPointRepository
                            .GetAll()
                            .Where(areaPoint => areaPoint.AreaId == area.Id)
                            .OrderBy(areaPoint => areaPoint.Id)
                            .ToList();
                var polygonFromAreaPoints = new Polygon(areaPoints);
                if (AreasHelper.PolygonCollision(polygonFromAreaPoints, polygonFromRequestPoints))
                {
                    // 400 | Пересекается с уже существующей зоной
                    return new Response<AreaResponse>
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }

            // Создаём новую зону
            var newArea = new Area
            {
                Name = areaRequest.Name
            };
            await _areaRepository.Create(newArea); 

            // Добавляем точки зоны в базу данных
            var newAreaPoints = new List<AreaPoint>();
            foreach (var areaPoint in areaRequest.AreaPoints)
            {
                var newAreaPoint = new AreaPoint
                {
                    Latitude = areaPoint.Latitude,
                    Longitude = areaPoint.Longitude,
                    AreaId = newArea.Id
                };
                await _areaPointRepository.Create(newAreaPoint);
                newAreaPoints.Add(newAreaPoint);
            }

            var areaResponse = new AreaResponse(newArea, newAreaPoints);

            // Возвращаем созданную зону
            return new Response<AreaResponse>
            {
                Data = areaResponse,
                StatusCode = StatusCodes.Status201Created
            };
        }

        public async Task<Response<bool>> DeleteArea(long id)
        {
            var areaToDelete = await _areaRepository
                .GetAll()
                .FirstOrDefaultAsync(area => area.Id == id);
            if (areaToDelete == null)
            {
                // 404 | Зона не найдена
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Удаляем все точки зоны из базы данных

            var areaToDeletePoints = await _areaPointRepository
                .GetAll()
                .Where(areaPoint => areaPoint.AreaId == areaToDelete.Id)
                .OrderBy(areaPoint => areaPoint.Id)
                .ToListAsync();

            foreach (var point in areaToDeletePoints)
            {
                await _areaPointRepository.Delete(point);
            }

            await _areaRepository.Delete(areaToDelete);

            // Зона успешно удалена
            return new Response<bool>
            {
                Data = true,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<AreaResponse>> GetAreaById(long id)
        {
            var areaFromDb = await _areaRepository
                .GetAll()
                .FirstOrDefaultAsync(area => area.Id == id);
            
            if (areaFromDb == null)
            {
                // 404 | Зона не найдена
                return new Response<AreaResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Берём все точки зоны
            List<AreaPoint> areaFromDbPoints = await _areaPointRepository
                .GetAll()
                .Where(areaPoint => areaPoint.AreaId == areaFromDb.Id)
                .OrderBy(areaPoint => areaPoint.Id)
                .ToListAsync();

            // Возвращение зоны
            return new Response<AreaResponse>
            {
                Data = new AreaResponse(areaFromDb, areaFromDbPoints),
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<AreaResponse>> UpdateArea(long id, AreaRequest areaRequest)
        {
            // Пробуем найти зону с таким же именем, но другим id
            var areaWithSameName = await _areaRepository
                .GetAll()
                .FirstOrDefaultAsync(area => area.Name == areaRequest.Name && area.Id != id);

            if (areaWithSameName != null)
            {
                // 409 | Конфликт имён зоны
                return new Response<AreaResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            var areaToUpdate = await _areaRepository.GetAll().FirstOrDefaultAsync(a => a.Id == id);

            if (areaToUpdate == null)
            {
                // 404 | Зона не найдена
                return new Response<AreaResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (areaRequest.AreaPoints.Count < 3
                || areaRequest.AreaPoints.Any(areaPoint => areaPoint == null)
                || !AreasHelper.IsSelfDiscountingPolygon(areaRequest.AreaPoints))
            {
                // 400 | Содержит менее трёх точек, null значения вместо точек
                // или нельзя создать самонепересекающейся многоугольник
                return new Response<AreaResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Все зоны из базы данных (кроме обновляющейся, так как с ней будет гарантированное пересечение)
            var allAreas = await _areaRepository
                .GetAll()
                .Where(area => area.Id != areaToUpdate.Id)
                .ToListAsync();
            // Создаём многоугольник по точкам
            var polygonFromRequestPoints = new Polygon(areaRequest.AreaPoints);

            //Проверяется наличие пересечения со всеми другими зонами в базе данных
            foreach (var area in allAreas)
            {
                // Строим многоульник зоны по точкам
                var areaPoints = _areaPointRepository
                            .GetAll()
                            .Where(areaPoint => areaPoint.AreaId == area.Id)
                            .OrderBy(areaPoint => areaPoint.Id)
                            .ToList();
                var polygon = new Polygon(areaPoints);

                if (AreasHelper.PolygonCollision(polygon, polygonFromRequestPoints))
                {
                    // 400 | Новая зона пересекается с существующей
                    return new Response<AreaResponse>
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }

            // Меняем имя зоны на новое
            areaToUpdate.Name = areaRequest.Name;
            areaToUpdate = await _areaRepository.Update(areaToUpdate);


            // Удаляем старые точки зоны из базы данных

            List<AreaPoint> areaToUpdatePoints = await _areaPointRepository
                .GetAll()
                .Where(areaPoint => areaPoint.AreaId == id)
                .OrderBy(areaPoint => areaPoint.Id)
                .ToListAsync();

            foreach (var point in areaToUpdatePoints)
            {
                await _areaPointRepository.Delete(point);
            }

            // Добавляем новые точки зоны в базу данных и response
            var newAreaPoints = new List<AreaPoint>();
            foreach (var areaPoint in areaRequest.AreaPoints)
            {
                var newAreaPoint = new AreaPoint
                {
                    Latitude = areaPoint.Latitude,
                    Longitude = areaPoint.Longitude,
                    AreaId = id
                };
                await _areaPointRepository.Create(newAreaPoint);
                newAreaPoints.Add(newAreaPoint);
            }

            var areaResponse = new AreaResponse(areaToUpdate, newAreaPoints);

            // Возврат обновлённой зоны
            return new Response<AreaResponse>
            {
                Data = areaResponse,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<AreaAnalyticsResponse>> GetAreaAnalytics(long id, DateTime startDate, DateTime endDate)
        {
            var areaForAnalytics = await _areaRepository
                .GetAll()
                .FirstOrDefaultAsync(area => area.Id == id);

            if (areaForAnalytics == null)
            {
                // 404 | Зона не найдена
                return new Response<AreaAnalyticsResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Все точки зоны с id
            var areaPoints = await _areaPointRepository
                .GetAll()
                .Where(areaPoint => areaPoint.AreaId == id)
                .ToListAsync();

            // Конвертированная дата с Kind = DateTimeKind.Utc (требование Postgresql)
            var startDateUtc = new DateTime(startDate.Ticks, DateTimeKind.Utc);
            var endDateUtc = new DateTime(endDate.Ticks, DateTimeKind.Utc);

            // Находим всех животных, которых чипировали в этой зоне
            var animalsChippedInArea = (await _animalRepository
                .GetAll()
                .Include(animal => animal.ChippingLocation)
                .ToListAsync())
                .FindAll(animal => AreasHelper.IsPointInPolygon(animal.ChippingLocation, areaPoints));
            // Находим всех животных, которые когда-либо посещали зону или были в ней чипированы
            var animalsRelatedToArea = (await _animalVisitedLocationRepository
                .GetAll()
                .Include(visitedLocationPoint => visitedLocationPoint.LocationPoint)
                .ToListAsync())
                .FindAll(visitedLocationPoint => AreasHelper.IsPointInPolygon(visitedLocationPoint.LocationPoint, areaPoints))
                .Select(visitedLocationPoint => visitedLocationPoint.Animal)
                .Union(animalsChippedInArea)
                .DistinctBy(animal => animal.Id);

            // Счётчики уходов, приходов и находящихся в зоне животных
            long amountOfAnimalsGoneAway = 0;
            long amountOfAnimalsStayed = 0;
            long amountOfAnimalsArrived = 0;

            // Аналитика по типам для response
            var areaTypesAnalytics = new List<AreaTypeAnalytics>();

            // Итерируемся по каждому животному, как-то связанному с зоной
            foreach (var animal in animalsRelatedToArea)
            {
                // Уход и приход у одного животного может засчитываться один раз,
                // поэтому счётчики типа bool
                bool hasGoneAway = false;
                bool hasStayed = false;
                bool hasArrived = false;

                // Передвижения животного в указанном временном промежутке
                var animalMovingsInDate = await _animalVisitedLocationRepository
                    .GetAll()
                    // Сортировка по Id == сортировка по дате (только для AnimalVisitedLocation)
                    .OrderBy(visitedLocation => visitedLocation.Id)
                    // Выбираем все перемещения, удовлетворяющие ограничениям по дате
                    .Where(visitedLocation => 
                        visitedLocation.DateTimeOfVisitLocationPoint <= endDateUtc
                        && visitedLocation.DateTimeOfVisitLocationPoint >= startDateUtc
                        && visitedLocation.AnimalId == animal.Id)
                    // Берём из посещённых локаций только информацию о самих локациях
                    .Select(visitedLocation => visitedLocation.LocationPoint)
                    .ToListAsync();

                // Если вдруг животное было чипировано в указанный промежуток времени,
                // то точку чипирования добавляем в передвижения животного
                if (animal.ChippingDateTime >= startDateUtc && animal.ChippingDateTime <= endDateUtc)
                {
                    animalMovingsInDate = animalMovingsInDate
                        // Так как лист уже отсортирован по дате,
                        // то вставляем точку чипирования в начало
                        .Prepend(animal.ChippingLocation)
                        .ToList();
                }

                // Конвертируем в массив
                var animalMovingsInDateArray = animalMovingsInDate.ToArray();

                // Находим последнее перемещение животного до указанного временного промежутка
                // Нужно, чтобы узнать чем является первое перемещение в промежутке или отсутствие перемещения
                // относительно положения животного.
                var lastAnimalMovingBeforeDate = _animalVisitedLocationRepository
                    .GetAll()
                    // Сортировка по Id == сортировка по дате (только для AnimalVisitedLocation)
                    .OrderBy(visitedLocation => visitedLocation.Id)
                    // Берём последнее перемещение, которое не входит во временной промежуток
                    .LastOrDefault(visitedLocation => visitedLocation.DateTimeOfVisitLocationPoint < startDateUtc && visitedLocation.AnimalId == animal.Id)?
                    // Если такого элемента нет, то если животное чипировали до временного промежутка,
                    // точку чипирования примем за последнее передвижение до него.
                    // Иначе null (животное не существовало до промежутка)
                    .LocationPoint ?? (animal.ChippingDateTime < startDateUtc ? animal.ChippingLocation : null);
                // Является ли последняя точка посещения до начала промежутка точкой зоны?
                bool? lastAnimalMovingBeforeDateInArea = lastAnimalMovingBeforeDate != null ? AreasHelper.IsPointInPolygon(lastAnimalMovingBeforeDate, areaPoints) : null;
                // Является ли первая точка посещения в промежутке точкой зоны?
                bool? firstAnimalMovingInDateInArea = animalMovingsInDateArray.Length > 0 ? AreasHelper.IsPointInPolygon(animalMovingsInDateArray[0], areaPoints) : null;

                // Есть ли точка перемещения до временного промежутка и во временном промежутке
                if (firstAnimalMovingInDateInArea.HasValue && lastAnimalMovingBeforeDateInArea.HasValue)
                {
                    // Животное было в зоне до временного промежутка, но вышло из него
                    if (lastAnimalMovingBeforeDateInArea.Value && !firstAnimalMovingInDateInArea.Value)
                    {
                        hasGoneAway = true;
                    }
                    // Животное не было в зоне до временного промежутка, но пришло в него
                    else if (!lastAnimalMovingBeforeDateInArea.Value && firstAnimalMovingInDateInArea.Value)
                    {
                        hasArrived = true;
                    }
                }
                // Если нет перемещений во временном промежутке, но есть перемещение до,
                // то тогда если животное было в зоне, то оно считается находящимся в ней
                else if (lastAnimalMovingBeforeDateInArea.HasValue)
                {
                    hasStayed = lastAnimalMovingBeforeDateInArea.Value;
                }

                // Если животное передвигалось (либо чипировалось) во временном промежутке
                if (animalMovingsInDateArray != null && animalMovingsInDateArray.Length != 0)
                {
                    // Проверяем последнюю точку перемещения в промежутке.
                    // Если она в зоне, то животное в ней находится
                    if (AreasHelper.IsPointInPolygon(animalMovingsInDateArray[animalMovingsInDateArray.Length - 1], areaPoints))
                    {
                        hasStayed = true;
                    }

                    for (int i = 1; i < animalMovingsInDateArray.Length; i++)
                    {
                        // Вся информация уже получена
                        if (hasArrived && hasGoneAway)
                            break;
                        // Если животное не было в зоне, но пришло в неё
                        if (!hasArrived && !AreasHelper.IsPointInPolygon(animalMovingsInDateArray[i - 1], areaPoints) 
                            && AreasHelper.IsPointInPolygon(animalMovingsInDateArray[i], areaPoints))
                        {
                            hasArrived = true;
                        }
                        // Если животное было в зоне, но вышло из неё
                        else if (!hasGoneAway && AreasHelper.IsPointInPolygon(animalMovingsInDateArray[i - 1], areaPoints) 
                            && !AreasHelper.IsPointInPolygon(animalMovingsInDateArray[i], areaPoints))
                        {
                            hasGoneAway = true;
                        }
                    }
                }

                // Увеличиваем счётчики
                amountOfAnimalsGoneAway += hasGoneAway ? 1 : 0;
                amountOfAnimalsArrived += hasArrived ? 1 : 0;
                amountOfAnimalsStayed += hasStayed ? 1 : 0;

                // Находим все типы этого животного и берём их названия
                var animalTypes = _animalTypeRelationRepository
                    .GetAll()
                    .Where(typeRelation => typeRelation.AnimalId == animal.Id)
                    .Include(typeRelation => typeRelation.AnimalType)
                    .Select(typeRelation => typeRelation.AnimalType)
                    .ToList();
                // Итерируемся по каждому типу
                foreach(var animalType in animalTypes)
                {
                    var sameAreaType = areaTypesAnalytics
                        .FirstOrDefault(analytics => analytics.AnimalType == animalType.Type);
                    // Если тип уже добавлен в конечный response, просто увеличиваем счётчики.
                    if (sameAreaType != null)
                    {
                        sameAreaType.AnimalsArrived += hasArrived ? 1 : 0;
                        sameAreaType.AnimalsGone += hasGoneAway ? 1 : 0;
                        sameAreaType.QuantityAnimals += hasStayed ? 1 : 0;
                    }
                    // Иначе добавляем и делаем счётчики равными нынешнему животному
                    else
                    {
                        areaTypesAnalytics.Add(
                            new AreaTypeAnalytics
                            {
                                AnimalType = animalType.Type,
                                AnimalTypeId = animalType.Id,
                                AnimalsArrived = hasArrived ? 1 : 0,
                                AnimalsGone = hasGoneAway ? 1 : 0,
                                QuantityAnimals = hasStayed ? 1 : 0
                            }    
                        );
                    }
                }
            }

            var areaAnalyticsResponse = new AreaAnalyticsResponse
            {
                TotalAnimalsArrived = amountOfAnimalsArrived,
                TotalAnimalsGone = amountOfAnimalsGoneAway,
                TotalQuantityAnimals = amountOfAnimalsStayed,
                AnimalsAnalytics = areaTypesAnalytics
            };

            return new Response<AreaAnalyticsResponse>
            {
                Data = areaAnalyticsResponse,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
