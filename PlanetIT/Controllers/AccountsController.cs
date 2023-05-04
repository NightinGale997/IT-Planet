using Microsoft.AspNetCore.Mvc;
using PlanetIT.Attributes;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // accounts/{id} [GET]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountResponse>> Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _accountService.GetAccountById(id);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // accounts/search [GET]
        [Authorize(Role.ADMIN)]
        [HttpGet("search")]
        public async Task<ActionResult<List<AccountResponse>>> Search(string? firstName, string? lastName, string? email, int from = 0, int size = 10)
        {
            if (size <= 0 || from < 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _accountService.GetAccountsBySearch(firstName, lastName, email, from, size);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // accounts [POST]
        [Authorize(Role.ADMIN)]
        [HttpPost]
        public async Task<ActionResult<AccountResponse>> Post([FromBody] AccountRequest accountRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var response = await _accountService.CreateAccount(accountRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
        // accounts/{id} [PUT]
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<AccountResponse>> Put(int id, [FromBody] AccountRequest accountRequest)
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
                var response = await _accountService.UpdateAccount(id, accountRequest);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        // accounts/{id} [DELETE]
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            try
            {
                var response = await _accountService.DeleteAccount(id);
                return StatusCode(response.StatusCode, response.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }

}
