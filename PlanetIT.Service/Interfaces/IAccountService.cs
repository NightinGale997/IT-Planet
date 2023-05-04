using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;

namespace PlanetIT.Service.Interfaces
{
    public interface IAccountService
    {
        Task<Response<AccountResponse>> RegisterAccount(RegistrationRequest registrationRequest);
        Task<Response<AccountResponse>> CreateAccount(AccountRequest accountRequest);
        Task<Response<AccountResponse>> GetAccountById(int id);
        Task<Response<List<AccountResponse>>> GetAccountsBySearch(string? firstName, string? lastName, string? email, int from, int size);
        Task<Response<AccountResponse>> UpdateAccount(int id, AccountRequest accountRequest);
        Task<Response<bool>> DeleteAccount(int id);
    }
}
