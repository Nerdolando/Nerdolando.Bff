using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.AspNetCore.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Options
{
    internal sealed class BffPostConfigureOptions<TOptions>(IConfigureHandlerProvider _configureHandlerProvider): 
        IPostConfigureOptions<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        public void PostConfigure(string? name, TOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNullOrEmpty(name);
            
            var handler = _configureHandlerProvider.GetHandlerForOptions<TOptions>();
            if (handler == null)
                return;

            handler.Handle(options, name);
        }
    }
}
