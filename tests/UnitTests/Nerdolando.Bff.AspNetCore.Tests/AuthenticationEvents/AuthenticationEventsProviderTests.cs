using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.TestingUtils;
using NSubstitute;


namespace Nerdolando.Bff.AspNetCore.AuthenticationEvents.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="AuthenticationEventsProvider{TEventType}"/>.
    /// </summary>
    public class AuthenticationEventsProviderTests
    {
        /// <summary>
        /// Tests that GetOriginalEvents returns a cached event instance when the cache is already populated.
        /// This verifies the caching optimization that prevents redundant registry lookups.
        /// </summary>
        [Fact]
        public void GetOriginalEvents_WhenCachedEventsExists_ReturnsCachedEvents()
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var options = new RemoteAuthenticationOptions();
            var context = CreateContext(options);

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = new RemoteAuthenticationEvents();
                    return true;
                });

            // Act - First call to populate cache
            var firstResult = provider.GetOriginalEvents(context);

            // Clear the mock to verify it's not called again
            eventsRegistry.ClearReceivedCalls();

            // Act - Second call should return cached value
            var secondResult = provider.GetOriginalEvents(context);

            // Assert
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.Same(firstResult, secondResult);
            eventsRegistry.DidNotReceiveWithAnyArgs().TryGet(default!, default!, out _);
        }

        /// <summary>
        /// Tests that GetOriginalEvents returns a new event instance when the registry does not contain
        /// any events for the specified scheme. This represents the fallback behavior when no custom
        /// events are registered.
        /// </summary>
        [Fact]
        public void GetOriginalEvents_WhenRegistryReturnsFalse_ReturnsNewEventInstance()
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var options = new RemoteAuthenticationOptions();
            var context = CreateContext(options);

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = null;
                    return false;
                });

            // Act
            var result = provider.GetOriginalEvents(context);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RemoteAuthenticationEvents>(result);
            eventsRegistry.Received(1).TryGet(options.GetType(), "TestScheme", out _);
        }

        /// <summary>
        /// Tests that GetOriginalEvents creates an instance using ActivatorUtilities when the registry
        /// returns a Type object. This scenario occurs when events are registered as a type to be
        /// instantiated with dependency injection.
        /// </summary>
        [Fact]
        public void GetOriginalEvents_WhenRegistryReturnsTypeObject_CreatesInstanceUsingActivator()
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var options = new RemoteAuthenticationOptions();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var context = CreateContext(options, serviceProvider: serviceProvider);

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = typeof(RemoteAuthenticationEvents);
                    return true;
                });

            // Act
            var result = provider.GetOriginalEvents(context);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RemoteAuthenticationEvents>(result);
            eventsRegistry.Received(1).TryGet(options.GetType(), "TestScheme", out _);
        }

        /// <summary>
        /// Tests that GetOriginalEvents returns the event instance directly when the registry contains
        /// an already-instantiated event object. This is the most straightforward scenario where events
        /// are registered as singleton instances.
        /// </summary>
        [Fact]
        public void GetOriginalEvents_WhenRegistryReturnsEventInstance_ReturnsEventInstance()
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var options = new RemoteAuthenticationOptions();
            var context = CreateContext(options);
            var expectedEvents = new RemoteAuthenticationEvents();

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = expectedEvents;
                    return true;
                });

            // Act
            var result = provider.GetOriginalEvents(context);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedEvents, result);
            eventsRegistry.Received(1).TryGet(options.GetType(), "TestScheme", out _);
        }

        /// <summary>
        /// Tests that GetOriginalEvents returns a new event instance when the registry returns
        /// an unexpected object type that doesn't match TEventType. This handles edge cases where
        /// the registry contains incorrectly typed data.
        /// </summary>
        [Fact]
        public void GetOriginalEvents_WhenRegistryReturnsUnknownObject_ReturnsNewEventInstance()
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var options = new RemoteAuthenticationOptions();
            var context = CreateContext(options);

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = "unexpected string object";
                    return true;
                });

            // Act
            var result = provider.GetOriginalEvents(context);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RemoteAuthenticationEvents>(result);
            eventsRegistry.Received(1).TryGet(options.GetType(), "TestScheme", out _);
        }

        /// <summary>
        /// Tests that GetOriginalEvents caches the result after the first call and returns the same
        /// instance on subsequent calls, verifying that the registry is only queried once.
        /// This ensures efficient performance for repeated accesses.
        /// </summary>
        [Fact]
        public void GetOriginalEvents_WhenCalledMultipleTimes_CachesResultAfterFirstCall()
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var options = new RemoteAuthenticationOptions();
            var context = CreateContext(options);
            var expectedEvents = new RemoteAuthenticationEvents();

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = expectedEvents;
                    return true;
                });

            // Act
            var firstResult = provider.GetOriginalEvents(context);
            var secondResult = provider.GetOriginalEvents(context);
            var thirdResult = provider.GetOriginalEvents(context);

            // Assert
            Assert.NotNull(firstResult);
            Assert.Same(expectedEvents, firstResult);
            Assert.Same(firstResult, secondResult);
            Assert.Same(secondResult, thirdResult);
            eventsRegistry.Received(1).TryGet(options.GetType(), "TestScheme", out _);
        }

        /// <summary>
        /// Tests that GetOriginalEvents correctly uses the scheme name from the context
        /// when querying the registry. This verifies proper parameter passing.
        /// </summary>
        [Theory]
        [InlineData("Scheme1")]
        [InlineData("Scheme2")]
        [InlineData("CustomScheme")]
        [InlineData("")]
        public void GetOriginalEvents_WithDifferentSchemeNames_PassesCorrectSchemeToRegistry(string schemeName)
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var options = new RemoteAuthenticationOptions();
            var context = CreateContext(options, schemeName);

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = null;
                    return false;
                });

            // Act
            provider.GetOriginalEvents(context);

            // Assert
            eventsRegistry.Received(1).TryGet(options.GetType(), schemeName, out _);
        }

        /// <summary>
        /// Tests that GetOriginalEvents uses the runtime options type when querying the registry.
        /// </summary>
        [Fact]
        public void GetOriginalEvents_WhenOptionsAreDerived_UsesDerivedOptionsType()
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var options = new RemoteAuthenticationOptions();
            var context = CreateContext(options);

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = null;
                    return false;
                });

            // Act
            provider.GetOriginalEvents(context);

            // Assert
            eventsRegistry.Received(1).TryGet(typeof(RemoteAuthenticationOptions), "TestScheme", out _);
        }

        /// <summary>
        /// Tests that GetOriginalEvents resolves registered event types through the request service provider.
        /// </summary>
        [Fact]
        public void GetOriginalEvents_WhenTypeRequiresDependencies_ResolvesUsingServiceProvider()
        {
            // Arrange
            var eventsRegistry = Substitute.For<IAuthenticationEventsRegistry>();
            var provider = new AuthenticationEventsProvider<RemoteAuthenticationEvents>(eventsRegistry);

            var services = new ServiceCollection()
                .BuildServiceProvider();
            var options = new RemoteAuthenticationOptions();
            var context = CreateContext(options, serviceProvider: services);

            eventsRegistry.TryGet(Arg.Any<Type>(), Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x =>
                {
                    x[2] = typeof(RemoteAuthenticationEvents);
                    return true;
                });

            // Act
            var result = provider.GetOriginalEvents(context);

            // Assert
            var typedResult = Assert.IsType<RemoteAuthenticationEvents>(result);
            eventsRegistry.Received(1).TryGet(options.GetType(), "TestScheme", out _);
        }

        private static FakeAuthenticationContext<RemoteAuthenticationOptions> CreateContext(RemoteAuthenticationOptions options, string schemeName = "TestScheme", IServiceProvider? serviceProvider = null)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider ?? new ServiceCollection().BuildServiceProvider()
            };

            var scheme = new AuthenticationScheme(schemeName, schemeName, typeof(RemoteAuthenticationHandler<RemoteAuthenticationOptions>));
            return new FakeAuthenticationContext<RemoteAuthenticationOptions>(httpContext, scheme, options);
        }
    }
}