using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.AuthenticationEvents;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nerdolando.Bff.AspNetCore.Tests.AuthenticationEvents
{
    public class OAuthEventsProxyTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ExecuteBffCreatingTicket_WhenAccessTokenIsNull_DoesntCallTokenReceived(string? accessToken)
        {
            //Arrange
            var tokenResponse = CreateTokenResponse(accessToken);
            var oAuthTokenResponse = OAuthTokenResponse.Success(tokenResponse);
            var httpContext = new DefaultHttpContext();
            var context = new OAuthCreatingTicketContext(
                principal: new ClaimsPrincipal(),
                properties: new AuthenticationProperties(),
                context: httpContext,
                scheme: new AuthenticationScheme("Test scheme", "", typeof(RemoteAuthenticationHandler<OAuthOptions>)),
                options: new OAuthOptions(),
                backchannel: new HttpClient(),
                tokens: oAuthTokenResponse,
                user: default);

            var authEventsProvider = Substitute.For<IAuthenticationEventsProvider<OAuthEvents>>();
            var originalOAuthEvents = Substitute.For<OAuthEvents>();
            originalOAuthEvents.CreatingTicket(Arg.Any<OAuthCreatingTicketContext>())
                .Returns(Task.CompletedTask);

            authEventsProvider.GetOriginalEvents(Arg.Any<BaseContext<OAuthOptions>>()).Returns(originalOAuthEvents);
            var eventsProxy = new OAuthEventsProxy(authEventsProvider);

            //Act
            var ex = await Record.ExceptionAsync(async () => await eventsProxy.CreatingTicket(context));

            //Assert
            Assert.Null(ex);
            await originalOAuthEvents.Received(1).CreatingTicket(Arg.Any<OAuthCreatingTicketContext>());
        }

        [Fact]
        public async Task ExecuteBffCreatingTicket_WhenIdTokenExists_AddsIdTokenToTokenResponse()
        {
            //Arrange
            var tokenResponse = CreateTokenResponse("accessTokenValue", "idTokenValue");
            var oAuthTokenResponse = OAuthTokenResponse.Success(tokenResponse);

            var tokenReceivedService = Substitute.For<ITokenReceivedService>();
            tokenReceivedService.HandleAsync(Arg.Any<TokenResponse>()).Returns(Task.CompletedTask);

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(ITokenReceivedService)).Returns(tokenReceivedService);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var context = new OAuthCreatingTicketContext(
                principal: new ClaimsPrincipal(),
                properties: new AuthenticationProperties(),
                context: httpContext,
                scheme: new AuthenticationScheme("Test scheme", "", typeof(RemoteAuthenticationHandler<OAuthOptions>)),
                options: new OAuthOptions(),
                backchannel: new HttpClient(),
                tokens: oAuthTokenResponse,
                user: default);

            var authEventsProvider = Substitute.For<IAuthenticationEventsProvider<OAuthEvents>>();
            var originalOAuthEvents = Substitute.For<OAuthEvents>();
            originalOAuthEvents.CreatingTicket(Arg.Any<OAuthCreatingTicketContext>())
                .Returns(Task.CompletedTask);

            authEventsProvider.GetOriginalEvents(Arg.Any<BaseContext<OAuthOptions>>()).Returns(originalOAuthEvents);
            var eventsProxy = new OAuthEventsProxy(authEventsProvider);

            //Act
            var ex = await Record.ExceptionAsync(async () => await eventsProxy.CreatingTicket(context));

            //Assert
            Assert.Null(ex);
            await originalOAuthEvents.Received(1).CreatingTicket(Arg.Any<OAuthCreatingTicketContext>());
            await tokenReceivedService.Received(1).HandleAsync(Arg.Is<TokenResponse>(tr =>
                tr.AccessToken == "accessTokenValue" &&
                tr.IdToken == "idTokenValue"));
        }

        [Fact]
        public async Task ExecuteBffCreatingTicket_WhenNoIdToken_DoesNotThrow()
        {
            //Arrange
            var tokenResponse = CreateTokenResponse("accessTokenValue");
            var oAuthTokenResponse = OAuthTokenResponse.Success(tokenResponse);

            var tokenReceivedService = Substitute.For<ITokenReceivedService>();
            tokenReceivedService.HandleAsync(Arg.Any<TokenResponse>()).Returns(Task.CompletedTask);

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(ITokenReceivedService)).Returns(tokenReceivedService);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var context = new OAuthCreatingTicketContext(
                principal: new ClaimsPrincipal(),
                properties: new AuthenticationProperties(),
                context: httpContext,
                scheme: new AuthenticationScheme("Test scheme", "", typeof(RemoteAuthenticationHandler<OAuthOptions>)),
                options: new OAuthOptions(),
                backchannel: new HttpClient(),
                tokens: oAuthTokenResponse,
                user: default);

            var authEventsProvider = Substitute.For<IAuthenticationEventsProvider<OAuthEvents>>();
            var originalOAuthEvents = Substitute.For<OAuthEvents>();
            originalOAuthEvents.CreatingTicket(Arg.Any<OAuthCreatingTicketContext>())
                .Returns(Task.CompletedTask);

            authEventsProvider.GetOriginalEvents(Arg.Any<BaseContext<OAuthOptions>>()).Returns(originalOAuthEvents);
            var eventsProxy = new OAuthEventsProxy(authEventsProvider);

            //Act
            var ex = await Record.ExceptionAsync(async () => await eventsProxy.CreatingTicket(context));

            //Assert
            Assert.Null(ex);
            await originalOAuthEvents.Received(1).CreatingTicket(Arg.Any<OAuthCreatingTicketContext>());
            await tokenReceivedService.Received(1).HandleAsync(Arg.Is<TokenResponse>(tr =>
                tr.AccessToken == "accessTokenValue" &&
                tr.IdToken == null));
        }

        private JsonDocument CreateTokenResponse(string? accessToken, string? idToken = null)
        {
            var accessTokenPart = accessToken is null ? "" : $"\"access_token\": \"{accessToken}\",";
            var idTokenPart = idToken is null ? "" : $"\"id_token\": \"{idToken}\",";

            var json = new StringBuilder();
            json.Append("{");
            json.Append(accessTokenPart);
            json.Append(idTokenPart);
            json.Append("\"token_type\": \"bearer\",");
            json.Append("\"expires_in\": \"3600\",");
            json.Append("\"refresh_token\": \"refresh-token-value\"");
            json.Append("}");

            return JsonDocument.Parse(json.ToString());
        }
    }
}
