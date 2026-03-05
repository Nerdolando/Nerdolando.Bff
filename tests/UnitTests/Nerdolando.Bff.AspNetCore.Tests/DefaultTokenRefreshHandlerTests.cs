using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Nerdolando.Bff.AspNetCore.Models;
using Nerdolando.Bff.AspNetCore.Services;
using Nerdolando.Bff.Common;
using Nerdolando.Bff.TestingUtils;
using NSubstitute;
using System.Net;

namespace Nerdolando.Bff.AspNetCore.Tests
{
    public class DefaultTokenRefreshHandlerTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("clientId", "")]
        [InlineData("", "clientSecret")]
        [InlineData(null, null)]
        [InlineData("clientId", null)]
        [InlineData(null, "clientSecret")]
        public async Task HandleAsync_WhenNoClientIdOrClientSecret_ThrowsException(string? clientId, string? clientSecret)
        {
            // Arrange
            var oidcOptions = new OpenIdConnectOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            var bffConfig = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "default", new Uri("https://localhost:5001") }
                },
                UseIdTokenAsAccessToken = false,
                Endpoints = new BffEndpointConfig
                {
                    ChallengeAuthenticationScheme = "TestScheme"
                }
            };

            var bffOptionsMonitor = Microsoft.Extensions.Options.Options.Create(bffConfig);

            var oidcOptionsMonitor = Substitute.For<IOptionsMonitor<OpenIdConnectOptions>>();
            oidcOptionsMonitor.CurrentValue.Returns(oidcOptions);
            oidcOptionsMonitor.Get(Arg.Any<string>()).Returns(oidcOptions);

            var tokenRefreshHandler = new DefaultTokenRefreshHandler(oidcOptionsMonitor, bffOptionsMonitor);

            var existingToken = new UserToken
            {
                AccessToken = "existing_access_token",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                IdToken = "existing_id_token",
                RefreshToken = "existing_refresh_token",
                SessionId = Guid.NewGuid()
            };


            // Act
            var ex = await Record.ExceptionAsync(async () => await tokenRefreshHandler.HandleAsync(existingToken));

            //Assert
            Assert.NotNull(ex);
            Assert.IsType<InvalidOperationException>(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task HandleAsync_WhenNoRefreshToken_DoesNotRefreshes(string? refreshToken)
        {
            // Arrange
            var oidcOptions = new OpenIdConnectOptions
            {
                ClientId = "clientId",
                ClientSecret = "clientSecret"
            };

            var bffConfig = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "default", new Uri("https://localhost:5001") }
                },
                UseIdTokenAsAccessToken = false,
                Endpoints = new BffEndpointConfig
                {
                    ChallengeAuthenticationScheme = "TestScheme"
                }
            };

            var bffOptionsMonitor = Microsoft.Extensions.Options.Options.Create(bffConfig);

            var oidcOptionsMonitor = Substitute.For<IOptionsMonitor<OpenIdConnectOptions>>();
            oidcOptionsMonitor.CurrentValue.Returns(oidcOptions);
            oidcOptionsMonitor.Get(Arg.Any<string>()).Returns(oidcOptions);

            var tokenRefreshHandler = new DefaultTokenRefreshHandler(oidcOptionsMonitor, bffOptionsMonitor);

            var existingToken = new UserToken
            {
                AccessToken = "existing_access_token",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                IdToken = "existing_id_token",
                RefreshToken = refreshToken,
                SessionId = Guid.NewGuid()
            };


            // Act
            UserToken? resultToken = null;
            var ex = await Record.ExceptionAsync(async () => resultToken = await tokenRefreshHandler.HandleAsync(existingToken));

            //Assert
            Assert.Null(ex);
            Assert.Null(resultToken);
        }

        [Fact]
        public async Task HandleAsync_WhenRefreshEndpointSetInOidcConfig_Refreshes()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"new_access_token\",\"expires_in\":3600,\"id_token\":\"new_id_token\",\"refresh_token\":\"new_refresh_token\"}")
            };

            var backchannel = new HttpClient(new FakeMessageHandler(response));

            var configManager = Substitute.For<IConfigurationManager<OpenIdConnectConfiguration>>();
            var oidcConfig = new OpenIdConnectConfiguration
            {
                TokenEndpoint = "https://localhost:5001/connect/token"
            };

            configManager.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(oidcConfig);
            var oidcOptions = new OpenIdConnectOptions
            {
                ClientId = "clientId",
                ClientSecret = "clientSecret",
                Backchannel = backchannel,
                ConfigurationManager = configManager
            };

            var bffConfig = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "default", new Uri("https://localhost:5001") }
                },
                UseIdTokenAsAccessToken = false,
                Endpoints = new BffEndpointConfig
                {
                    ChallengeAuthenticationScheme = "TestScheme"
                }
            };

            var bffOptionsMonitor = Microsoft.Extensions.Options.Options.Create(bffConfig);

            var oidcOptionsMonitor = Substitute.For<IOptionsMonitor<OpenIdConnectOptions>>();
            oidcOptionsMonitor.CurrentValue.Returns(oidcOptions);
            oidcOptionsMonitor.Get(Arg.Any<string>()).Returns(oidcOptions);

            var tokenRefreshHandler = new DefaultTokenRefreshHandler(oidcOptionsMonitor, bffOptionsMonitor);

            var existingToken = new UserToken
            {
                AccessToken = "existing_access_token",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                IdToken = "existing_id_token",
                RefreshToken = "refreshToken",
                SessionId = Guid.NewGuid()
            };

            // Act
            UserToken? resultToken = null;
            var ex = await Record.ExceptionAsync(async () => resultToken = await tokenRefreshHandler.HandleAsync(existingToken));

            //Assert
            Assert.Null(ex);
            Assert.NotNull(resultToken);
            Assert.Equal("new_access_token", resultToken!.AccessToken);
            Assert.Equal("new_id_token", resultToken.IdToken);
            Assert.Equal("new_refresh_token", resultToken.RefreshToken);
        }

        [Fact]
        public async Task HandleAsync_WhenNoRefreshEndpoint_Throws()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"new_access_token\",\"expires_in\":3600,\"id_token\":\"new_id_token\",\"refresh_token\":\"new_refresh_token\"}")
            };

            var backchannel = new HttpClient(new FakeMessageHandler(response));

            var configManager = Substitute.For<IConfigurationManager<OpenIdConnectConfiguration>>();
            configManager.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(new OpenIdConnectConfiguration());
            var oidcOptions = new OpenIdConnectOptions
            {
                ClientId = "clientId",
                ClientSecret = "clientSecret",
                Backchannel = backchannel,
                ConfigurationManager = configManager
            };

            var bffConfig = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "default", new Uri("https://localhost:5001") }
                },
                UseIdTokenAsAccessToken = false,
                Endpoints = new BffEndpointConfig
                {
                    ChallengeAuthenticationScheme = "TestScheme"
                }
            };

            var bffOptionsMonitor = Microsoft.Extensions.Options.Options.Create(bffConfig);

            var oidcOptionsMonitor = Substitute.For<IOptionsMonitor<OpenIdConnectOptions>>();
            oidcOptionsMonitor.CurrentValue.Returns(oidcOptions);
            oidcOptionsMonitor.Get(Arg.Any<string>()).Returns(oidcOptions);

            var tokenRefreshHandler = new DefaultTokenRefreshHandler(oidcOptionsMonitor, bffOptionsMonitor);

            var existingToken = new UserToken
            {
                AccessToken = "existing_access_token",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                IdToken = "existing_id_token",
                RefreshToken = "refreshToken",
                SessionId = Guid.NewGuid()
            };

            // Act
            UserToken? resultToken = null;
            var ex = await Record.ExceptionAsync(async () => resultToken = await tokenRefreshHandler.HandleAsync(existingToken));

            //Assert
            Assert.NotNull(ex);
            Assert.IsType<InvalidOperationException>(ex);
        }

        [Fact]
        public async Task HandleAsync_WhenNotSuccessRefreshResponse_DoesNotRefresh()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            var backchannel = new HttpClient(new FakeMessageHandler(response));

            var oidcOptions = new OpenIdConnectOptions
            {
                ClientId = "clientId",
                ClientSecret = "clientSecret",
                Backchannel = backchannel
            };

            var bffConfig = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "default", new Uri("https://localhost:5001") }
                },
                UseIdTokenAsAccessToken = false,
                Endpoints = new BffEndpointConfig
                {
                    ChallengeAuthenticationScheme = "TestScheme",
                    RefreshTokenEndpoint = new Uri("https://localhost:5001/connect/token")
                }
            };

            var bffOptionsMonitor = Microsoft.Extensions.Options.Options.Create(bffConfig);

            var oidcOptionsMonitor = Substitute.For<IOptionsMonitor<OpenIdConnectOptions>>();
            oidcOptionsMonitor.CurrentValue.Returns(oidcOptions);
            oidcOptionsMonitor.Get(Arg.Any<string>()).Returns(oidcOptions);

            var tokenRefreshHandler = new DefaultTokenRefreshHandler(oidcOptionsMonitor, bffOptionsMonitor);

            var existingToken = new UserToken
            {
                AccessToken = "existing_access_token",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                IdToken = "existing_id_token",
                RefreshToken = "refreshToken",
                SessionId = Guid.NewGuid()
            };

            // Act
            UserToken? resultToken = null;
            var ex = await Record.ExceptionAsync(async () => resultToken = await tokenRefreshHandler.HandleAsync(existingToken));

            //Assert
            Assert.Null(ex);
            Assert.Null(resultToken);
        }

        [Fact]
        public async Task HandleAsync_WhenValidRequest_Refreshes()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"new_access_token\",\"expires_in\":3600,\"id_token\":\"new_id_token\",\"refresh_token\":\"new_refresh_token\"}")
            };

            var backchannel = new HttpClient(new FakeMessageHandler(response));

            var oidcOptions = new OpenIdConnectOptions
            {
                ClientId = "clientId",
                ClientSecret = "clientSecret",
                Backchannel = backchannel
            };

            var bffConfig = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "default", new Uri("https://localhost:5001") }
                },
                UseIdTokenAsAccessToken = false,
                Endpoints = new BffEndpointConfig
                {
                    ChallengeAuthenticationScheme = "TestScheme",
                    RefreshTokenEndpoint = new Uri("https://localhost:5001/connect/token")
                }
            };

            var bffOptionsMonitor = Microsoft.Extensions.Options.Options.Create(bffConfig);

            var oidcOptionsMonitor = Substitute.For<IOptionsMonitor<OpenIdConnectOptions>>();
            oidcOptionsMonitor.CurrentValue.Returns(oidcOptions);
            oidcOptionsMonitor.Get(Arg.Any<string>()).Returns(oidcOptions);

            var tokenRefreshHandler = new DefaultTokenRefreshHandler(oidcOptionsMonitor, bffOptionsMonitor);

            var existingToken = new UserToken
            {
                AccessToken = "existing_access_token",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                IdToken = "existing_id_token",
                RefreshToken = "refreshToken",
                SessionId = Guid.NewGuid()
            };

            // Act
            UserToken? resultToken = null;
            var ex = await Record.ExceptionAsync(async () => resultToken = await tokenRefreshHandler.HandleAsync(existingToken));

            //Assert
            Assert.Null(ex);
            Assert.NotNull(resultToken);
            Assert.Equal("new_access_token", resultToken!.AccessToken);
            Assert.Equal("new_id_token", resultToken.IdToken);
            Assert.Equal("new_refresh_token", resultToken.RefreshToken);
        }
    }
}
