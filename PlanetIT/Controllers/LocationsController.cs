using Microsoft.AspNetCore.Mvc;
using PlanetIT.Attributes;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Service.Interfaces;
using PlanetIT.Domain.Enums;

namespace PlanetIT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationPointService _locationPointService;

        public LocationsController(ILocationPointService locationPointService)
        {
            _locationPointService = locationPointService;
        }

        // locations/{id} [GET]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationPoint>> Get(long id)
        {
            if (id <= 0)
                return BadRequest();
            try
            {
                var response = await _locationPointService.GetLocationPointById(id);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // locations [GET]
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<LocationPoint>> Get(double latitude, double longitude)
        {
            if (Math.Abs(latitude) > 90.0 || Math.Abs(longitude) > 180)
                return BadRequest();
            try
            {
                var response = await _locationPointService.GetId(latitude, longitude);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // locations/geohash [GET]
        [Authorize]
        [HttpGet("geohash")]
        public async Task<ActionResult<LocationPoint>> GetGeoHash(double latitude, double longitude)
        {
            if (Math.Abs(latitude) > 90.0 || Math.Abs(longitude) > 180)
                return BadRequest();
            try
            {
                var response = await _locationPointService.GetGeoHash(latitude, longitude, 1);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // locations/geohash [GET]
        [Authorize]
        [HttpGet("geohashv2")]
        public async Task<ActionResult<LocationPoint>> GetGeoHashV2(double latitude, double longitude)
        {
            if (Math.Abs(latitude) > 90.0 || Math.Abs(longitude) > 180)
                return BadRequest();
            try
            {
                var response = await _locationPointService.GetGeoHash(latitude, longitude, 2);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // locations/geohash [GET]
        [Authorize]
        [HttpGet("geohashv3")]
        public async Task<ActionResult<LocationPoint>> GetGeoHashV3(double latitude, double longitude)
        {
            if (Math.Abs(latitude) > 90.0 || Math.Abs(longitude) > 180)
                return BadRequest();
            try
            {
                var response = await _locationPointService.GetGeoHash(latitude, longitude, 2);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // locations [POST]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPost]
        public async Task<ActionResult<LocationPoint>> Post([FromBody] LocationPointRequest locationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _locationPointService.CreateLocationPoint(locationRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // locations/{id} [PUT]
        [Authorize(Role.ADMIN, Role.CHIPPER)]
        [HttpPut("{id}")]
        public async Task<ActionResult<LocationPoint>> Put(long id, [FromBody] LocationPointRequest locationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var response = await _locationPointService.UpdateLocationPoint(id, locationRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // locations/{id} [DELETE]
        [Authorize(Role.ADMIN)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            if (id <= 0)
                return BadRequest();
            try
            {
                var response = await _locationPointService.DeleteLocationPoint(id);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
