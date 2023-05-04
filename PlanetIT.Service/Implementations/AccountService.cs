using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlanetIT.DAL.Interfaces;
using PlanetIT.DAL.Repositories;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Domain.Models.Request;
using PlanetIT.Domain.Models.Response;
using PlanetIT.Domain.Response;
using PlanetIT.Service.Interfaces;

namespace PlanetIT.Service.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IBaseRepository<Account> _accountRepository;
        private readonly IBaseRepository<Animal> _animalRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(IHttpContextAccessor httpContextAccessor, IBaseRepository<Account> accountRepository, IBaseRepository<Animal> animalRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _accountRepository = accountRepository;
            _animalRepository = animalRepository;
        }

        public async Task<Response<AccountResponse>> RegisterAccount(RegistrationRequest registrationRequest)
        {
            var accountWithSameEmail = await _accountRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Email == registrationRequest.Email);
            
            if (accountWithSameEmail != null)
            {
                // 409 | Конфликт, пользователь с таким email уже существует
                return new Response<AccountResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }
            // Создаём новый аккаунт и добавляем в базу данных
            var newAccount = new Account
            {
                Email = registrationRequest.Email,
                FirstName = registrationRequest.FirstName,
                LastName = registrationRequest.LastName,
                Password = registrationRequest.Password,
                Role = Role.USER
            };
            await _accountRepository.Create(newAccount);

            // Возвращаем данные о нём
            return new Response<AccountResponse>
            {
                Data = new AccountResponse(newAccount),
                StatusCode = StatusCodes.Status201Created
            };
        }
        public async Task<Response<AccountResponse>> CreateAccount(AccountRequest accountRequest)
        {
            var accountWithSameEmail = await _accountRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Email == accountRequest.Email);
            
            if (accountWithSameEmail != null)
            {
                // 409 | Конфликт, пользователь с таким email уже существует
                return new Response<AccountResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Создаём аккаунт и сохраняем в базу данных, возвращаем его данные
            var newAccount = new Account
            {
                Email = accountRequest.Email,
                FirstName = accountRequest.FirstName,
                LastName = accountRequest.LastName,
                Password = accountRequest.Password,
                Role = accountRequest.Role
            };
            await _accountRepository.Create(newAccount);

            return new Response<AccountResponse>
            {
                Data = new AccountResponse(newAccount),
                StatusCode = StatusCodes.Status201Created
            };
        }
        public async Task<Response<AccountResponse>> GetAccountById(int id)
        {
            var accountFromDb = await _accountRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Id == id);
            
            // Получаем данные об аккаунте запрашивающего
            var userAccount = (Account?)_httpContextAccessor.HttpContext.Items["Account"];

            // Если запрашивает админ, но аккаунта не найдено, то 404.
            // Если User или Chipper, то 403 если запрашивают данные не к своему аккаунту
            switch (userAccount?.Role)
            {
                case Role.USER:
                case Role.CHIPPER:
                    if (accountFromDb == null || userAccount != accountFromDb)
                    {
                        return new Response<AccountResponse>
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                    }
                    break;
                case Role.ADMIN:
                    if (accountFromDb == null)
                    {
                        return new Response<AccountResponse>
                        {
                            StatusCode = StatusCodes.Status404NotFound
                        };
                    }
                    break;
            }

            return new Response<AccountResponse>
            {
                Data = new AccountResponse(accountFromDb),
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<List<AccountResponse>>> GetAccountsBySearch(string? firstName, string? lastName, string? email, int from = 0, int size = 10)
        {
            // Создаём query, к которой будем добавлять условия поиска
            var accountsQuery = _accountRepository.GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(firstName))
                accountsQuery = accountsQuery.Where(account => EF.Functions.ILike(account.FirstName, $"%{firstName}%"));
            if (!string.IsNullOrWhiteSpace(lastName))
                accountsQuery = accountsQuery.Where(account => EF.Functions.ILike(account.LastName, $"%{lastName}%"));
            if (!string.IsNullOrWhiteSpace(email))
                accountsQuery = accountsQuery.Where(account => EF.Functions.ILike(account.Email, $"%{email}%"));

            // Находим результаты поиска и возвращаем их
            var accounts = await accountsQuery
                .OrderBy(account => account.Id)
                .Skip(from)
                .Take(size)
                .ToListAsync();

            var searchResult = new List<AccountResponse>();
            foreach (var account in accounts)
            {
                searchResult.Add(new AccountResponse(account));
            }

            return new Response<List<AccountResponse>>
            {
                Data = searchResult,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<Response<AccountResponse>> UpdateAccount(int id, AccountRequest accountRequest)
        {
            var accountToUpdate = await _accountRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Id == id);

            // Получаем данные об аккаунте запрашивающего
            var userAccount = (Account?)_httpContextAccessor.HttpContext.Items["Account"];

            // Если запрашивает админ, но аккаунта не найдено, то 404.
            // Если User или Chipper, то 403 если пытаются изменить данные не к своему аккаунту
            switch (userAccount?.Role)
            {
                case Role.USER:
                case Role.CHIPPER:
                    if (accountToUpdate == null || userAccount != accountToUpdate)
                    {
                        return new Response<AccountResponse>
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                    }
                    break;
                case Role.ADMIN:
                    if (accountToUpdate == null)
                    {
                        return new Response<AccountResponse>
                        {
                            StatusCode = StatusCodes.Status404NotFound
                        };
                    }
                    break;
            }

            var accountWithSameEmail = await _accountRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Email == accountRequest.Email);
            if (accountWithSameEmail != accountToUpdate && accountWithSameEmail != null)
            {
                // 409 | Конфликт, пользователь с таким email уже существует
                return new Response<AccountResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }

            // Обновляем поля аккаунта, сохраняем в базу данных и возвращаем его данные
            accountToUpdate.Email = accountRequest.Email;
            accountToUpdate.Password = accountRequest.Password;
            accountToUpdate.FirstName = accountRequest.FirstName;
            accountToUpdate.LastName = accountRequest.LastName;
            accountToUpdate.Role = accountRequest.Role;
            accountToUpdate = await _accountRepository.Update(accountToUpdate);
            return new Response<AccountResponse>
            {
                Data = new AccountResponse(accountToUpdate),
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<Response<bool>> DeleteAccount(int id)
        {
            var accountToDelete = await _accountRepository
                .GetAll()
                .FirstOrDefaultAsync(account => account.Id == id);

            // Получаем данные об аккаунте запрашивающего
            var userAccount = (Account?)_httpContextAccessor.HttpContext.Items["Account"];

            // Если запрашивает админ, но аккаунта не найдено, то 404.
            // Если User или Chipper, то 403 если пытаются удалить не свой аккаунт
            switch (userAccount?.Role)
            {
                case Role.USER:
                case Role.CHIPPER:
                    if (accountToDelete == null || userAccount != accountToDelete)
                    {
                        return new Response<bool>
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                    }
                    break;
                case Role.ADMIN:
                    if (accountToDelete == null)
                    {
                        return new Response<bool>
                        {
                            StatusCode = StatusCodes.Status404NotFound
                        };
                    }
                    break;
            }

            // Находим зависимое от пользователя животное
            var dependentAnimal = await _animalRepository
                .GetAll()
                .FirstOrDefaultAsync(animal => animal.ChipperId == id);
            
            if (dependentAnimal != null)
            {
                // 400 | Нельзя удалить аккаунт, к которому привязаны животные
                return new Response<bool>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Удаляем аккаунт
            await _accountRepository.Delete(accountToDelete);
            return new Response<bool>
            {
                Data = true,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
