using Nerdolando.Bff.AspNetCore.Models;
using Nerdolando.Bff.AspNetCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerdolando.Bff.AspNetCore.Tests
{
    public class UriUtilsTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        [InlineData(null)]
        public void BuildRedirectUri_WhenFrontTypeEmpty_ReturnsNull(string? frontType)
        {
            // Arrange
            var options = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "web", new Uri("https://example.com") }
                },
                Endpoints = new BffEndpointConfig(),
                UseIdTokenAsAccessToken = false
            };


            // Act
            var result = UriUtils.BuildRedirectUri(frontType!, "/home", options);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        [InlineData(null)]
        public void BuildRedirectUri_WhenReturnUrlEmpty_ReturnsBaseUri(string? returnUrl)
        {
            // Arrange
            var options = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "web", new Uri("https://example.com") }
                },
                Endpoints = new BffEndpointConfig(),
                UseIdTokenAsAccessToken = false
            };
            // Act
            var result = UriUtils.BuildRedirectUri("web", returnUrl, options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://example.com/", result!.ToString());
        }

        [Fact]
        public void BuildRedirectUri_WhenQueryParameters_ReturnsUriWithQuery()
        {
            // Arrange
            var options = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "web", new Uri("https://example.com") }
                },
                Endpoints = new BffEndpointConfig(),
                UseIdTokenAsAccessToken = false
            };
            // Act
            var result = UriUtils.BuildRedirectUri("web", "/home?param=value", options);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://example.com/home?param=value", result!.ToString());
        }

        [Fact]
        public void BuildRedirectUri_WhenMultipleQueryParameters_ReturnsUriWithQuery()
        {
            // Arrange
            var options = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "web", new Uri("https://example.com") }
                },
                Endpoints = new BffEndpointConfig(),
                UseIdTokenAsAccessToken = false
            };
            // Act
            var result = UriUtils.BuildRedirectUri("web", "/home?param=value&other=1", options);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://example.com/home?param=value&other=1", result!.ToString());
        }

        [Fact]
        public void BuildRedirectUri_WhenNoQueryParameters_ReturnsUri()
        {
            // Arrange
            var options = new BffConfig
            {
                FrontUrls = new Dictionary<string, Uri>
                {
                    { "web", new Uri("https://example.com") }
                },
                Endpoints = new BffEndpointConfig(),
                UseIdTokenAsAccessToken = false
            };
            // Act
            var result = UriUtils.BuildRedirectUri("web", "/home", options);
            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://example.com/home", result!.ToString());
        }
    }
}
