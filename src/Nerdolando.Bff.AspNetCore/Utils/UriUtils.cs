using Nerdolando.Bff.AspNetCore.Models;

namespace Nerdolando.Bff.AspNetCore.Utils
{
    internal static class UriUtils
    {
        internal static Uri? BuildRedirectUri(string frontType, string? returnUrl, BffConfig options)
        {
            if (string.IsNullOrWhiteSpace(frontType))
                return null;

            if (!options.FrontUrls.TryGetValue(frontType, out Uri? value))
                return null;

            if (string.IsNullOrWhiteSpace(returnUrl))
                returnUrl = "/";

            if (returnUrl.IndexOfAny(['\\', '\r', '\n']) >= 0)
                return null;

            var pathParts = returnUrl.Split('?');

            var ub = new UriBuilder(value!);
            ub.Path = pathParts[0];
            if (pathParts.Length > 1)
                ub.Query = pathParts[1];
            return ub.Uri;
        }
    }
}
