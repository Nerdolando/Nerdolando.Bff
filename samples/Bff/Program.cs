using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Nerdolando.Bff.AspNetCore.Extensions;
using Nerdolando.Bff.Storage.Sqlite.Extensions;

namespace Bff
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddAuth0WebAppAuthentication(o =>
            {
                builder.Configuration.GetSection("Auth0").Bind(o);
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.CallbackPath = new PathString("/auth-callback");
                o.ResponseType = OpenIdConnectResponseType.Code;
                o.Scope = "openid profile email offline_access";
                o.SkipCookieMiddleware = false;
                

                o.OpenIdConnectEvents = new OpenIdConnectEvents();
                o.OpenIdConnectEvents.OnRedirectToIdentityProvider = context =>
                {
                    return Task.CompletedTask;
                };

                o.OpenIdConnectEvents.OnTokenValidated = context =>
                {
                    return Task.CompletedTask;
                };
            }).WithAccessToken(o =>
            {
                o.Audience = "api://weather";
            });

            builder.Services.Configure<OpenIdConnectOptions>(o =>
            {
                o.TokenValidationParameters.NameClaimType = "name";
            });

            builder.Services.AddBff(options =>
            {
                options.FrontUrls = new Dictionary<string, Uri>
                {
                    { "local", new Uri("https://localhost:7230") }
                };
                options.Endpoints.ChallengeAuthenticationScheme = Auth0Constants.AuthenticationScheme;
                options.Endpoints.TargetApiBaseUrl = new Uri("https://localhost:7136");
                options.Endpoints.TargetApiPath = "/api";
            }).UseSqliteTokenStorage();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("https://localhost:7230")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapBff();

            app.Run();
        }
    }
}
