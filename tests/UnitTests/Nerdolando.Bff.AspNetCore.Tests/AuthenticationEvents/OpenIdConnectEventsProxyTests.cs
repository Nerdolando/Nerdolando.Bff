using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.AuthenticationEvents;
using Nerdolando.Bff.Common;
using NSubstitute;
using System.Collections.Specialized;
using System.Security.Claims;

namespace Nerdolando.Bff.AspNetCore.Tests.AuthenticationEvents
{
    public class OpenIdConnectEventsProxyTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ExecuteBffTokenValidated_WhenAccessTokenIsNull_DoesntCallTokenReceived(string? accessToken)
        {
            //Arrange
            var oidcMessage = CreateOidcMessage(accessToken);
            var httpContext = new DefaultHttpContext();

            var tokenValidatedContext = new TokenValidatedContext(
                context: httpContext,
                scheme: new AuthenticationScheme("Test scheme", "", typeof(RemoteAuthenticationHandler<OpenIdConnectOptions>)),
                options: new OpenIdConnectOptions(),
                principal: new ClaimsPrincipal(),
                properties: new AuthenticationProperties());

            tokenValidatedContext.TokenEndpointResponse = oidcMessage;

            var eventsProvider = Substitute.For<IAuthenticationEventsProvider<OpenIdConnectEvents>>();
            var originalEvents = Substitute.For<OpenIdConnectEvents>();
            originalEvents.TokenValidated(Arg.Any<TokenValidatedContext>())
                .Returns(Task.CompletedTask);

            eventsProvider.GetOriginalEvents(Arg.Any<BaseContext<OpenIdConnectOptions>>()).Returns(originalEvents);
            var eventsProxy = new OpenIdConnectEventsProxy(eventsProvider);

            //Act
            var ex = await Record.ExceptionAsync(async () => await eventsProxy.TokenValidated(tokenValidatedContext));

            //Assert
            Assert.Null(ex);
            await originalEvents.Received(1).TokenValidated(Arg.Any<TokenValidatedContext>());
        }

        [Fact]
        public async Task ExecuteBffTokenValidated_WhenIdTokenExists_AddsIdTokenToTokenResponse()
        {
            //Arrange
            var oidcMessage = CreateOidcMessage("accessTokenValue", "idTokenValue");

            var tokenReceivedService = Substitute.For<ITokenReceivedService>();
            tokenReceivedService.HandleAsync(Arg.Any<TokenResponse>()).Returns(Task.CompletedTask);

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(ITokenReceivedService)).Returns(tokenReceivedService);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var tokenValidatedContext = new TokenValidatedContext(
                context: httpContext,
                scheme: new AuthenticationScheme("Test scheme", "", typeof(RemoteAuthenticationHandler<OpenIdConnectOptions>)),
                options: new OpenIdConnectOptions(),
                principal: new ClaimsPrincipal(),
                properties: new AuthenticationProperties());

            tokenValidatedContext.TokenEndpointResponse = oidcMessage;

            var eventsProvider = Substitute.For<IAuthenticationEventsProvider<OpenIdConnectEvents>>();
            var originalEvents = Substitute.For<OpenIdConnectEvents>();
            originalEvents.TokenValidated(Arg.Any<TokenValidatedContext>())
                .Returns(Task.CompletedTask);

            eventsProvider.GetOriginalEvents(Arg.Any<BaseContext<OpenIdConnectOptions>>()).Returns(originalEvents);
            var eventsProxy = new OpenIdConnectEventsProxy(eventsProvider);

            //Act
            var ex = await Record.ExceptionAsync(async () => await eventsProxy.TokenValidated(tokenValidatedContext));

            //Assert
            Assert.Null(ex);
            await originalEvents.Received(1).TokenValidated(Arg.Any<TokenValidatedContext>());
            await tokenReceivedService.Received(1).HandleAsync(Arg.Is<TokenResponse>(tr =>
                tr.AccessToken == "accessTokenValue" &&
                tr.IdToken == "idTokenValue"));
        }

        [Fact]
        public async Task ExecuteBffTokenValidated_WhenNoIdToken_DoesNotThrow()
        {
            //Arrange
            var oidcMessage = CreateOidcMessage("accessTokenValue");

            var tokenReceivedService = Substitute.For<ITokenReceivedService>();
            tokenReceivedService.HandleAsync(Arg.Any<TokenResponse>()).Returns(Task.CompletedTask);

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(ITokenReceivedService)).Returns(tokenReceivedService);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var tokenValidatedContext = new TokenValidatedContext(
                context: httpContext,
                scheme: new AuthenticationScheme("Test scheme", "", typeof(RemoteAuthenticationHandler<OpenIdConnectOptions>)),
                options: new OpenIdConnectOptions(),
                principal: new ClaimsPrincipal(),
                properties: new AuthenticationProperties());

            tokenValidatedContext.TokenEndpointResponse = oidcMessage;

            var eventsProvider = Substitute.For<IAuthenticationEventsProvider<OpenIdConnectEvents>>();
            var originalEvents = Substitute.For<OpenIdConnectEvents>();
            originalEvents.TokenValidated(Arg.Any<TokenValidatedContext>())
                .Returns(Task.CompletedTask);

            eventsProvider.GetOriginalEvents(Arg.Any<BaseContext<OpenIdConnectOptions>>()).Returns(originalEvents);
            var eventsProxy = new OpenIdConnectEventsProxy(eventsProvider);

            //Act
            var ex = await Record.ExceptionAsync(async () => await eventsProxy.TokenValidated(tokenValidatedContext));

            //Assert
            Assert.Null(ex);
            await originalEvents.Received(1).TokenValidated(Arg.Any<TokenValidatedContext>());
            await tokenReceivedService.Received(1).HandleAsync(Arg.Is<TokenResponse>(tr =>
                tr.AccessToken == "accessTokenValue" &&
                tr.IdToken == null));
        }

        private OpenIdConnectMessage CreateOidcMessage(string? accessToken, string? idToken = null)
        {
            var data = new NameValueCollection();
            if (accessToken != null)
                data["access_token"] = accessToken;

            if (idToken != null)
                data["id_token"] = idToken;

            data["token_type"] = "bearer";
            data["expires_in"] = "3600";
            data["refresh_token"] = "refresh-token-value";

            return new OpenIdConnectMessage(data);

        }
    }
}
