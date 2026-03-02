using System;
using Microsoft.AspNetCore.Authentication;

namespace Nerdolando.Bff.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IAuthOptionsConfigureHandler<in TOptions>
        where TOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionsType"></param>
        /// <returns></returns>
        bool CanHandle(Type optionsType);

        
        /// <summary>
        /// Handles the specified options and scheme name to perform the necessary operations.
        /// </summary>
        /// <param name="options">The options to configure the handling process, which may include various settings that influence the
        /// behavior of the operation.</param>
        /// <param name="schemeName">The name of the scheme to be applied during the handling process, which determines the context or method of
        /// operation.</param>
        void Handle(TOptions options, string schemeName);
    }
}

