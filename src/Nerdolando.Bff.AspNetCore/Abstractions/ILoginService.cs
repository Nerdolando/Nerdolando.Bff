using Microsoft.AspNetCore.Http;

namespace Nerdolando.Bff.AspNetCore.Abstractions
{
    internal interface ILoginService
    {
        IResult Login(string front, string? returnUrl);
    }
}