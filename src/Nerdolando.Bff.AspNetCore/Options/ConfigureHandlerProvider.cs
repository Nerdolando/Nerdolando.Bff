using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Options
{
    internal sealed class ConfigureHandlerProvider(IServiceProvider _serviceProvider) : IConfigureHandlerProvider
    {
        public IAuthOptionsConfigureHandler<TOptions>? GetHandlerForOptions<TOptions>()
            where TOptions : AuthenticationSchemeOptions
        {
            var requestedType = typeof(TOptions);

            for (var type = requestedType; type != null && typeof(AuthenticationSchemeOptions).IsAssignableFrom(type); type = type.BaseType)
            {
                var serviceType = typeof(IAuthOptionsConfigureHandler<>).MakeGenericType(type);
                var handlers = _serviceProvider.GetServices(serviceType);
                if (handlers == null)
                    continue;

                foreach (var handler in handlers)
                {
                    if (handler is not IAuthOptionsConfigureHandler<TOptions> typedHandler)
                        continue;

                    if (typedHandler.CanHandle(requestedType))
                        return typedHandler;
                }
            }

            return null;
        }
    }
}
