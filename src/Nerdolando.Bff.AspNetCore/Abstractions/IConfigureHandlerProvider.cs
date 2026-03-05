using Microsoft.AspNetCore.Authentication;
using Nerdolando.Bff.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Abstractions
{
    internal interface IConfigureHandlerProvider
    {
        IAuthOptionsConfigureHandler<TOptions>? GetHandlerForOptions<TOptions>()
            where TOptions : AuthenticationSchemeOptions;
    }
}