using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.Abstractions;
using Nerdolando.Bff.AspNetCore.Services;
using NSubstitute;
namespace Nerdolando.Bff.AspNetCore.Tests
{
    public class AuthRefresherTests
    {
        [Fact]
        public async Task GetOrRefreshAuth_NoToken_ReturnsEmptyToken() 
        {
            // Arrange
            var sessionIdProviderMock = Substitute.For<ISessionIdProvider>();
            sessionIdProviderMock.GetSessionIdFromCurrentContextAsync()
                .Returns(Guid.NewGuid());

            var userTokenStorageMock = Substitute.For<IUserTokenStorage>();
            userTokenStorageMock.GetTokenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<UserToken?>(null));
            
            var authRefresher = new AuthRefresher(sessionIdProviderMock, 
                userTokenStorageMock, 
                null!);

            // Act
            var result = await authRefresher.GetOrRefreshAuthAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, result.SessionId);
            Assert.Equal(string.Empty, result.AccessToken);
            Assert.Equal(string.Empty, result.RefreshToken);
            Assert.Equal(default, result.ExpiresAt);
        }

        [Fact]
        public async Task GetOrRefreshAuth_NoSessionId_ReturnsEmptyToken()
        {
            // Arrange
            var sessionIdProviderMock = Substitute.For<ISessionIdProvider>();
            sessionIdProviderMock.GetSessionIdFromCurrentContextAsync()
                .Returns(Task.FromResult<Guid?>(null));

            var userTokenStorageMock = Substitute.For<IUserTokenStorage>();
            userTokenStorageMock.GetTokenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<UserToken?>(null));

            var authRefresher = new AuthRefresher(sessionIdProviderMock,
                userTokenStorageMock,
                null!);

            // Act
            var result = await authRefresher.GetOrRefreshAuthAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Guid.Empty, result.SessionId);
            Assert.Equal(string.Empty, result.AccessToken);
            Assert.Equal(string.Empty, result.RefreshToken);
            Assert.Equal(default, result.ExpiresAt);
        }

        [Fact]
        public async Task GetOrRefreshAuth_TokenNotExpired_DoesNotRefresh()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var sessionIdProviderMock = Substitute.For<ISessionIdProvider>();
            sessionIdProviderMock.GetSessionIdFromCurrentContextAsync()
                .Returns(sessionId);

            var userToken = new UserToken
            {
                AccessToken = "at",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                RefreshToken = "rt",
                SessionId = sessionId
            };

            var userTokenStorageMock = Substitute.For<IUserTokenStorage>();
            userTokenStorageMock.GetTokenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(userToken);

            var tokenRefresherMock = Substitute.For<ITokenRefreshHandler>();
            var authRefresher = new AuthRefresher(sessionIdProviderMock, 
                userTokenStorageMock, 
                tokenRefresherMock);

            // Act
            var newUserToken = await authRefresher.GetOrRefreshAuthAsync();
            
            // Assert
            Assert.NotNull(newUserToken);            
            var record = await Record.ExceptionAsync(async () => await tokenRefresherMock.DidNotReceiveWithAnyArgs().HandleAsync(default!));
            Assert.Null(record);
            Assert.Equivalent(newUserToken, userToken);
        }

        [Fact]
        public async Task GetOrRefreshAuth_TokenIsExpired_Refreshes()
        {

            // Arrange
            var sessionId = Guid.NewGuid();
            var sessionIdProviderMock = Substitute.For<ISessionIdProvider>();
            sessionIdProviderMock.GetSessionIdFromCurrentContextAsync()
                .Returns(sessionId);

            var userToken = new UserToken
            {
                AccessToken = "at",
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(10),
                RefreshToken = "rt",
                SessionId = sessionId
            };

            var newExpiration = DateTimeOffset.UtcNow.AddMinutes(60);
            var newUserToken = new UserToken
            {
                AccessToken = "at2",
                ExpiresAt = newExpiration,
                RefreshToken = "rt2",
                SessionId = sessionId
            };

            var userTokenStorageMock = Substitute.For<IUserTokenStorage>();
            userTokenStorageMock.GetTokenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(userToken);

            var tokenRefresherMock = Substitute.For<ITokenRefreshHandler>();
            tokenRefresherMock.HandleAsync(userToken)
                .Returns(newUserToken);

            var authRefresher = new AuthRefresher(sessionIdProviderMock, 
                userTokenStorageMock, 
                tokenRefresherMock);

            // Act
            var refreshedUserToken = await authRefresher.GetOrRefreshAuthAsync();

            // Assert
            Assert.NotNull(refreshedUserToken);
            Assert.Equal(refreshedUserToken.SessionId, userToken.SessionId);
            Assert.NotEqual(refreshedUserToken.AccessToken, userToken.AccessToken);
            Assert.NotEqual(refreshedUserToken.RefreshToken, userToken.RefreshToken);
            Assert.Equal(newExpiration, refreshedUserToken.ExpiresAt);
        }
    }
}
