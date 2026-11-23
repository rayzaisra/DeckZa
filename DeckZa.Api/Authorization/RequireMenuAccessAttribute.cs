using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using DeckZa.Api.Helpers;
using DeckZa.Application.Common;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireMenuAccessAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly PermissionAction _action;
    private readonly string[] _menuUniques;

    public RequireMenuAccessAttribute(PermissionAction action, params string[] menuUniques)
    {
        _action = action;
        _menuUniques = menuUniques;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissions = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        if (await permissions.HasAccessAsync(_action, _menuUniques))
        {
            return;
        }

        context.Result = new ObjectResult(
            ApiResponseFactory.Error<string>("You do not have permission to perform this action."))
        {
            StatusCode = StatusCodes.Status403Forbidden,
        };
    }
}
