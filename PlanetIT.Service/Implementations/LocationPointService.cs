using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;
using Geohash;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Service.Implementations
{
    public class LocationPointService : ILocationPointService
    {
        private readonly IBaseRepository<LocationPoint> _locationPointRepository;
        private readonly IBaseRepository<AnimalVisitedLocation> _animalVisitedLocationRepository;
        private readonly IBaseRepository<Animal> _animalRepository;

        public LocationPointService(IBaseRepository<Animal> animalRepository,
            IBaseRepository<AnimalVisitedLocation> animalVisitedLocationRepository,
            IBaseRepository<LocationPoint> locationPointRepository)
        {
            _locationPointRepository = locationPointRepository;
            _animalRepository = animalRepository;
            _animalVisitedLocationRepository = animalVisitedLocationRepository;
        }

        public async Task<Response<LocationPoint>> CreateLocationPoint(LocationPointRequest locationPointRequest)
        {
            var sameLocationPoint = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint => locationPoint.Longitude == locationPointRequest.Longitude
                && locationPoint.Latitude == locationPointRequest.Latitude);
            if (sameLocationPoint != null)
            {
                // 409 | Конфликт, локация с такими координатами уже существует
                return new Response<LocationPoint>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Создание новой локации и добавление в базу

            var newLocationPoint = new LocationPoint { 
                Latitude = locationPointRequest.Latitude,
                Longitude = locationPointRequest.Longitude };

            await _locationPointRepository.Create(newLocationPoint);

            // Возвращаем созданную локацию
            return new Response<LocationPoint>
            {
                Data = newLocationPoint,
                StatusCode = StatusCodes.Status201Created
            };
        }



        public async Task<Response<LocationPoint>> GetLocationPointById(long id)
        {
            var locationPointFromDb = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint => locationPoint.Id == id);
            if (locationPointFromDb == null)
            {
                // 404 | Точка локации не найдена
                return new Response<LocationPoint>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            // Возвращаем локацию
            return new Response<LocationPoint>
            {
                Data = locationPointFromDb,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<LocationPoint>> UpdateLocationPoint(long id, LocationPointRequest locationPointRequest)
        {
            var locationPointToUpdate = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint => locationPoint.Id == id);
            if (locationPointToUpdate == null)
            {
                // 404 | Локация не найдена
                return new Response<LocationPoint>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var sameLocationPoint = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint => locationPoint.Longitude == locationPointRequest.Longitude && locationPoint.Latitude == locationPointRequest.Latitude);
            if (sameLocationPoint != null)
            {
                // 409 | Конфликт, локация с такими координатами уже существует
                return new Response<LocationPoint>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Меняем значения старой локации на новые и сохраняем в базе данных
            locationPointToUpdate.Longitude = locationPointRequest.Longitude;
            locationPointToUpdate.Latitude = locationPointRequest.Latitude;
            locationPointToUpdate = await _locationPointRepository.Update(locationPointToUpdate);

            // Возвращаем значение
            return new Response<LocationPoint>
            {
                Data = locationPointToUpdate,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<bool>> DeleteLocationPoint(long id)
        {
            var locationPointToDelete = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint => locationPoint.Id == id);

            if (locationPointToDelete == null)
            {
                // 404 | Локация не найдена
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Есть ли животное, чипированное в этой локации?
            var dependentAnimal = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.ChippingLocationId == locationPointToDelete.Id);

            if (dependentAnimal != null)
            {
                // 400 | Есть животное, зависящее от этой локации
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Была ли эта локация посещена животным
            var dependentVisitedLocationPoint = await _animalVisitedLocationRepository
                .GetAll()
                .FirstOrDefaultAsync(visitedLocationPoint => visitedLocationPoint.LocationPointId == locationPointToDelete.Id);

            if (dependentVisitedLocationPoint != null)
            {
                // 400 | Локацию посещало животное
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Беспоследственно удаляем локацию и возвращаем статус

            await _locationPointRepository.Delete(locationPointToDelete);

            return new Response<bool>
            {
                Data = true,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<long>> GetId(double latitude, double longitude)
        {
            var locationPointFromDb = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint =>
                    locationPoint.Latitude == latitude
                    && locationPoint.Longitude == longitude);

            if (locationPointFromDb == null)
            {
                // 404 | Локация не найдена
                return new Response<long>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Возвращаем id локации с этими координатами
            return new Response<long>
            {
                Data = locationPointFromDb.Id,
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<string>> GetGeoHash(double latitude, double longitude, int v)
        {
            var locationPointFromDb = await _locationPointRepository
                .GetAll()
                .FirstOrDefaultAsync(locationPoint =>
                    locationPoint.Latitude == latitude
                    && locationPoint.Longitude == longitude);

            if (locationPointFromDb == null)
            {
                // 404 | Локация не найдена
                return new Response<string>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Получение GeoHash
            var geoHasher = new Geohasher();
            var result = geoHasher.Encode(latitude, longitude, 12);

            switch (v)
            {
                // Конвертация в base64 для v = 2
                case 2:
                    result = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(result));
                    break;
            }

            return new Response<string>
            {
                Data = result,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
