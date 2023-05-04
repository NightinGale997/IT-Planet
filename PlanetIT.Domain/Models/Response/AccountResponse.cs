using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.Domain.Models.Response
{
    public class AccountResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }

        public AccountResponse(Account account)
        {
            Id = account.Id;
            FirstName = account.FirstName;
            LastName = account.LastName;
            Email = account.Email;
            Role = account.Role;
        }
    }
}
