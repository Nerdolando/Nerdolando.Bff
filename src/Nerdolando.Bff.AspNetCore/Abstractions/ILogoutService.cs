using Microsoft.AspNetCore.Http;

namespace Nerdolando.Bff.AspNetCore.Abstractions
{
    internal interface ILogoutService
    {
        Task<IResult> LogoutAsync(string front, string? returnUrl);
    }
}