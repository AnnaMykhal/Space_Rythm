using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Entities;

namespace SpaceRythm.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Отримання користувача з HttpContext
        var user = (User)context.HttpContext.Items["User"];

        // Перевірка наявності користувача та його прав адміністратора
        if (user == null || !user.IsAdmin)
        {
            // Встановлення результату як Unauthorized якщо користувач не є адміністратором
            context.Result = new JsonResult(new { message = "Unauthorized" })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}

