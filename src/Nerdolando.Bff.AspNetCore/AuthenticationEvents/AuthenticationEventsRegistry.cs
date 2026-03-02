using Nerdolando.Bff.Abstractions;
using System.Collections.Concurrent;

namespace Nerdolando.Bff.AspNetCore.AuthenticationEvents
{
    internal sealed class AuthenticationEventsRegistry: IAuthenticationEventsRegistry
    {
        internal readonly record struct AuthenticationSchemeInfo(string OptionsTypeName, string SchemeName);
        private readonly ConcurrentDictionary<AuthenticationSchemeInfo, object> _originalEventTypesDict;
        

        public AuthenticationEventsRegistry()
        {
            _originalEventTypesDict = new();
        }

        public void Set(Type originalOptionsType, string authScheme, object originalEvents)
        {
            var key = CreateDictionaryKey(originalOptionsType, authScheme);

            _originalEventTypesDict[key] = originalEvents;
        }

        public bool TryGet(Type originalOptionsType, string authScheme, out object result)
        {
            var key = CreateDictionaryKey(originalOptionsType, authScheme);
            return _originalEventTypesDict.TryGetValue(key, out result!);
        }

        private static AuthenticationSchemeInfo CreateDictionaryKey(Type originalOptionsType, string authScheme)
        {
            return new AuthenticationSchemeInfo(originalOptionsType.FullName!, authScheme);
        }
    }
}
