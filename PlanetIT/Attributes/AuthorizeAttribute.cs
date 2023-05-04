using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly Role[]? _expectedRoles;

        public AuthorizeAttribute()
        {
        }

        public AuthorizeAttribute(params Role[] roles)
        {
            _expectedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Получает данные аккаунта из контекста
            var account = (Account?)context.HttpContext.Items["Account"];
            // Если данных в контексте нет, то 401, отсутствие авторизации
            if (account == null)
                context.Result = new JsonResult(new { message = "Unauthorized" })
                { StatusCode = StatusCodes.Status401Unauthorized };
            // Если роль не соответствует требующейся, то 403
            else if (_expectedRoles != null && !_expectedRoles.Contains(account.Role))
                context.Result = new JsonResult(new { message = "Wrong role" })
                { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
