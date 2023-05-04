using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlanetIT.Attributes;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Service.Implementations;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AreasController : ControllerBase
    {
        private readonly IAreaService _areaService;

        public AreasController(IAreaService areaService)
        {
            _areaService = areaService;
        }
        // areas/{id} [GET]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(long id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _areaService.GetAreaById(id);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // areas [POST]
        [Authorize(Role.ADMIN)]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AreaRequest areaRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var response = await _areaService.CreateArea(areaRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
        // areas/{id} [DELETE]
        [Authorize(Role.ADMIN)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Post(long id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _areaService.DeleteArea(id);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // areas/{id} [PUT]
        [Authorize(Role.ADMIN)]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(long id, [FromBody] AreaRequest areaRequest)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _areaService.UpdateArea(id, areaRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // areas/{id}/analytics [GET]
        [Authorize]
        [HttpGet("{id}/analytics")]
        public async Task<ActionResult> Get(long id, string startDate, string endDate)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            // В правильном ли формате даты ("yyyy-MM-dd")
            if (!DateTime.TryParseExact(startDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime startDateDateTime))
            {
                return BadRequest();
            }
            if (!DateTime.TryParseExact(endDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime endDateDateTime))
            {
                return BadRequest();
            }
            // Правильно ли указан временной промежуток
            if (startDateDateTime > endDateDateTime)
            {
                return BadRequest();
            }
            try
            {
                var response = await _areaService.GetAreaAnalytics(id, startDateDateTime, endDateDateTime);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
    }
}