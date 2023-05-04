using Microsoft.Extensions.Configuration;
using Npgsql;
using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Helpers;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public AuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context, IBaseRepository<Account> accountRepository)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ");

            //Проверяет есть ли информацию внутри хэдера, если есть,
            //то проверяет токен, если нет или токен не Basic,
            //то сразу ставит контекст аккаунта как null
            if (token != null && token.First() != null && token.Last() != null && token.First().ToLower() == "basic")
            {
                AttachAccountToContext(context, accountRepository, token.Last());
            }
            else
            {
                SetAccountInContext(context, null);
            }
            //Проверяет что в хэдере точно есть значение, и если в контекст не добавлен аккаунт,
            //то значит авторизационные данные неверны и возвращает response 401
            if (token != null && token.First() != "" && context.Items["Account"] == null)
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await _next(context);
        }
        public void AttachAccountToContext(HttpContext context, IBaseRepository<Account> accountRepository, string token)
        {
            try
            {
                (string email, string password) = TokenAuthorizationHelper.Decode(token);
                var authAccount = accountRepository.GetAll().FirstOrDefault(account => account.Email == email && account.Password == password);
                SetAccountInContext(context, authAccount);
            }
            catch
            {

            }
        }
        //Принимает контекст и аккаунт, если аккаунт не null, то добавляет его в контекст, если null, то удаляет из контекста
        public void SetAccountInContext(HttpContext context, Account? account)
        {
            context.Items["Account"] = account;
        }
    }
}
