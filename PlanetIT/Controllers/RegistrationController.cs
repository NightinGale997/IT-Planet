using Microsoft.AspNetCore.Mvc;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public RegistrationController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // registration [POST]
        [HttpPost]
        public async Task<ActionResult<LocationPoint>> Post([FromBody] RegistrationRequest registrationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (HttpContext.Items["Account"] != null)
            {
                return StatusCode(403);
            }
            try
            {
                var response = await _accountService.RegisterAccount(registrationRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
