using Nerdolando.Bff.AspNetCore.Models;

namespace Nerdolando.Bff.AspNetCore.Utils
{
    internal static class UriUtils
    {
        internal static Uri? BuildRedirectUri(string frontType, string? returnUrl, BffConfig options)
        {
            if (!options.FrontUrls.TryGetValue(frontType, out Uri? value))
                return null;

            if (string.IsNullOrWhiteSpace(returnUrl))
                returnUrl = "/";

            if (returnUrl.IndexOfAny(['\\', '\r', '\n']) >= 0)
                return null;

            var ub = new UriBuilder(value!);
            ub.Path = returnUrl;
            return ub.Uri;
        }
    }
}
