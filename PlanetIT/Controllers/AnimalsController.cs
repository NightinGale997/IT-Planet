using Azure;
using Microsoft.AspNetCore.Mvc;
using PlanetIT.Attributes;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnimalsController : ControllerBase
    {
        private readonly IAnimalTypeService _animalTypeService;
        private readonly IAnimalVisitedLocationPointService _animalVisitedLocationService;
        private readonly IAnimalService _animalService;

        public AnimalsController(IAnimalService animalService, IAnimalTypeService animalTypeService, IAnimalVisitedLocationPointService animalVisitedLocationService)
        {
            _animalTypeService = animalTypeService;
            _animalVisitedLocationService = animalVisitedLocationService;
            _animalService = animalService;
        }
        // Логика с AnimalService
        // -------------------------------------------------------------------------------------
        // animals/{animalId} [GET]
        [HttpGet("{animalId}")]
        public async Task<ActionResult<AnimalResponse>> GetAnimalById(long animalId)
        {
            if (animalId <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalService.GetAnimalById(animalId);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/search [GET]
        [HttpGet("search")]
        public async Task<ActionResult<List<AnimalResponse>>> SearchAnimals(DateTime? startDateTime, DateTime? endDateTime, int? chipperId, long? chippingLocationId, LifeStatus? lifeStatus, Gender? gender, int from = 0, int size = 10)
        {
            if (chipperId <= 0 || chippingLocationId <= 0 || from < 0 || size <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalService.GetAnimalsBySearch(startDateTime, endDateTime, chipperId, chippingLocationId, lifeStatus, gender, from, size);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // animals/{animalId} [POST]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPost]
        public async Task<ActionResult<AnimalResponse>> AddAnimal([FromBody] NewAnimalRequest animalRequest)
        {
            if (animalRequest.AnimalTypes.Where(tId => tId <= 0).Count() != 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalService.CreateAnimal(animalRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/{animalId} [PUT]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPut("{animalId}")]
        public async Task<ActionResult<AnimalResponse>> UpdateAnimal(long animalId, [FromBody] AnimalRequest animalRequest)
        {
            try
            {
                var response = await _animalService.UpdateAnimal(animalId, animalRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/{animalId} [DELETE]
        [Authorize(Role.ADMIN)]
        [HttpDelete("{animalId}")]
        public async Task<ActionResult> DeleteAnimal(long animalId)
        {
            if (animalId <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalService.DeleteAnimal(animalId);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/{animalId}/types/{typeId} [POST]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPost("{animalId}/types/{typeId}")]
        public async Task<ActionResult<AnimalResponse>> AddTypeToAnimal(long animalId, long typeId)
        {
            if (animalId <= 0 || typeId <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalService.AddTypeToAnimal(animalId, typeId);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/{animalId}/types/{typeId} [DELETE]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpDelete("{animalId}/types/{typeId}")]
        public async Task<ActionResult<AnimalResponse>> DeleteTypeOfAnimal(long animalId, long typeId)
        {
            if (animalId <= 0 || typeId <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalService.RemoveTypeFromAnimal(animalId, typeId);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/{animalId}/types [PUT]
        [Authorize]
        [HttpPut("{animalId}/types")]
        public async Task<ActionResult<AnimalResponse>> UpdateTypeOfAnimal(long animalId, [FromBody] UpdateTypeOfAnimalRequest request)
        {
            if (animalId <= 0 || !ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalService.UpdateTypeOfAnimal(animalId, request);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // Логика с AnimalVisitedLocationService
        // -------------------------------------------------------------------------------------
        // animals/{animalId}/locations [GET]
        [Authorize]
        [HttpGet("{animalId}/locations")]
        public async Task<ActionResult<List<AnimalVisitedLocationPointResponse>>> SearchAnimalVisitedLocations(long animalId, DateTime? startDateTime, DateTime? endDateTime, int from = 0, int size = 10)
        {
            if (from < 0 || size <= 0)
            {
                return BadRequest();
            }

            try
            {
                var response = await _animalVisitedLocationService.GetLocationPointsBySearch(animalId, startDateTime, endDateTime, from, size);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/{animalId}/locations/{pointId} [POST]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPost("{animalId}/locations/{pointId}")]
        public async Task<ActionResult<AnimalVisitedLocationPointResponse>> AddLocationToAnimal(long animalId, long pointId)
        {
            if (animalId <= 0 || pointId <= 0)
            {
                return BadRequest();
            }

            try
            {
                var response = await _animalVisitedLocationService.SetLocationPointAsVisited(animalId, pointId);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/{animalId}/locations [PUT]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPut("{animalId}/locations")]
        public async Task<ActionResult<AnimalVisitedLocationPointResponse>> UpdateVisitedLocation(long animalId, [FromBody] AnimalVisitedLocationPointRequest animalVisitedLocationRequest)
        {
            if (animalId <= 0 || animalVisitedLocationRequest.LocationPointId <= 0 || animalVisitedLocationRequest.VisitedLocationPointId <= 0)
            {
                return BadRequest();
            }

            try
            {
                var response = await _animalVisitedLocationService.UpdateLocationPoint(animalId, animalVisitedLocationRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/{animalId}/locations/{visitedPointId} [DELETE]
        [Authorize(Role.ADMIN)]
        [HttpDelete("{animalId}/locations/{visitedPointId}")]
        public async Task<ActionResult> DeleteLocationFromAnimal(long animalId, long visitedPointId)
        {
            if (animalId <= 0 || visitedPointId <= 0)
            {
                return BadRequest();
            }

            try
            {
                var response = await _animalVisitedLocationService.RemoveLocationPointFromVisited(animalId, visitedPointId);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // Логика с AnimalTypeService
        // -------------------------------------------------------------------------------------
        // animals/types/{id} [GET]
        [HttpGet("types/{id}")]
        public async Task<ActionResult<AnimalType>> GetAnimalTypeById(long id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalTypeService.GetAnimalTypeById(id);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/types [POST]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPost("types")]
        public async Task<ActionResult<AnimalType>> CreateAnimalType([FromBody] AnimalTypeRequest animalTypeRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var response = await _animalTypeService.CreateAnimalType(animalTypeRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/types/{id} [PUT]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPut("types/{id}")]
        public async Task<ActionResult<AnimalType>> UpdateAnimalType(long id, [FromBody] AnimalTypeRequest animalTypeRequest)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            else if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var response = await _animalTypeService.UpdateAnimalType(id, animalTypeRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // animals/types/{id} [DELETE]
        [Authorize(Role.ADMIN)]
        [HttpDelete("types/{id}")]
        public async Task<ActionResult> DeleteAnimalType(long id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _animalTypeService.DeleteAnimalType(id);
                return StatusCode(response.StatusCode);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }

}
