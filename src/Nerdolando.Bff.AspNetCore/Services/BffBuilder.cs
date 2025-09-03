using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nerdolando.Bff.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Services
{
    internal sealed class BffBuilder(IServiceCollection _services) : IBffBuilder
    {
        public IServiceCollection Services => _services;

        public IBffBuilder UseCustomTokenStorage<T>() where T : class, IUserTokenStorage
        {
            _services.AddScoped<IUserTokenStorage, T>();
            return this;
        }

        IBffBuilder IBffBuilder.UseCustomTokenRefresher<T>()
        {
            _services.RemoveAll<ITokenRefreshHandler>();
            _services.AddScoped<ITokenRefreshHandler, T>();
            return this;
        }
    }
}
