[![Build and test](https://github.com/Nerdolando/Nerdolando.Bff/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Nerdolando/Nerdolando.Bff/actions/workflows/dotnet.yml)
[![Deploy](https://github.com/Nerdolando/Nerdolando.Bff/actions/workflows/deploy.yml/badge.svg)](https://github.com/Nerdolando/Nerdolando.Bff/actions/workflows/deploy.yml)
[![License](https://img.shields.io/github/license/Nerdolando/Nerdolando.Bff.svg)](LICENSE)

# Problem - WHY
SPA Front applications should not keep access tokens in JS. They should manage authZ/authN using concept called "Backend For Frontend". But, manually creating BFF app with YARP + OpenIdConnect it's a cumbersome work full of pitfalls.
Solution: `Nerdolando.Bff` gives you ready to use BFF library that hosts cookie, proxies to your API, and stores tokens in SAFE storage. It also gives you refresh token flow out of the box.
For whom: for .NET developers creating project with SPA apps who want good, secure and FREE solution out of the box without reading tones of articles :)

## Typical scenarios
- SPA application in any front framework (REACT, VUE, Angular, Blazor, ...) with OAuth + WebApi
- Blazor WASM with OAuth + WebApi
- Multifront app (SPA + admin panel)

# About
`Nerdolando.Bff` is a library that provides a set of tools and components to help developers build Backend for Frontend (BFF) applications using ASP.NET Core. It aims to simplify the process of creating BFF layers by offering pre-built functionalities and best practices.
This package is built on top of .NETCore and YARP and leverages Microsoft's OpenIdConnect library.

It's been built also with best security practices in mind.

If you are not aware of BFF architecture, you can read more about it here: https://masterbranch.pl/jak-trzymac-tokeny-w-spa-czyli-backend-for-frontend/ (polish article) or here: https://auth0.com/blog/the-backend-for-frontend-pattern-bff/

## Front applications

`If you are using Blazor as your front app, you can use ready package: Nerdolando.Bff.Components to integrate with Bff seamlessly.`

`Nerdolando.Bff.AspNetCore` is for backend part of BFF architecture. It is not a front-end library, so you will need to create your own front application (SPA, Blazor, etc.) that will communicate with the BFF.
It has a ready services for Blazor though in separate package - `Nerdolando.Bff.Components`.

# Usage - creating Backend For Frontend.
To use Nerdolando.Bff.AspNetCore in your ASP.NET Core project, follow these steps:
1. Create a new ASP.NET Core project if you don't have one already. It may be simple `WebApi` or `Blazor Server`.

1. Install the NuGet package:
   ```
   dotnet add package Nerdolando.Bff.AspNetCore
   dotnet add package Nerdolando.Bff.Storage.Sqlite
   ```

1. Configure the BFF services in your `Program.cs`:
   ```csharp
    builder.Services.AddBff(o =>
    {
        o.FrontUrls = new Dictionary<string, Uri>
        {
            { "my-spa-app", new Uri("https://localhost:5001") } //your frontend URL
        };
           
        o.Endpoints.ChallengeAuthenticationScheme = yourChallengeAuthScheme;
        o.Endpoints.TargetApiBaseUrl = new Uri("https://api.yourservice.com");
        o.Endpoints.TargetApiPath = "/api";
    }).UseSqliteTokenStorage(); //from Nerdolando.Bff.Storage.Sqlite - see below
   ```

1. Use the BFF middleware:
   ```csharp
   app.MapBff(); //must be after UseAuthentication and UseAuthorization
   ```
1. You need to configure CORS policy to allow your front application to communicate with the BFF. The simplest policy may look like this:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("CorsPolicy", policy =>
       {
           policy.WithOrigins("https://localhost:5001") // Your front app URL
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials(); // this is important
       });
   });
   app.UseCors("CorsPolicy"); //must be after UseRouting, but before UseAuthorization.
   ```
    If you are not aware of CORS, you can read more about it here: https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS
  
1. You need to configure remote authentication in your ASP.NET Core project. 
`Nerdolando.Bff` does not impose any specific authentication method, so you can choose the one that best fits your needs. 
But must be compatible with OpenIdConnect (Google, Facebook, Auth0, generally OAuth and OIDC).

## Configuration
`Nerdolando.Bff.AspNetCore` provides several configuration options to customize its behavior according to your needs.
Configuration is done in `AddBff` method or by configuring `BffConfig` class:

### FrontUrls
Keeps list of Front URLs that are allowed to access the BFF. Key is front alias of your choice, that you will need to pass to BFF during signing in.

So simply speaking, you need to "alias" your front application with some friendly name like "front", "spa" or whatever you like.
Alias must point to URI where your front application is hosted. You can treat it as a whitelist of allowed front applications.
This is important for security reasons, to prevent authentication cookies from being sent to unauthorized front applications.
It is recommended to use HTTPS in production environments to ensure secure communication between the front application and the BFF.

This is the only source of thruth, BFF will NOT send authentication cookie to URL that is not listed here.
BFF will check alias of the request and then send cookie to URL that matches the alias.

### Endpoints
Endpoints configuration allows you to define how the BFF interacts with the target API and handles authentication challenges.

- `ChallengeAuthenticationScheme`: Specifies the authentication scheme to be used when a challenge is required. This is typically set to the scheme used for user authentication (e.g., cookies, JWT, etc.). This is the same name that you use while configuring your Authentication mechanism.
- `TargetApiBaseUrl`: This is the base URL of the target API where requests will be forwarded to.
- `TargetApiPath`: Each request starting with this path will be routed to the target API. For example: if request sent to Bff is https://bff.example.com/api/weather and the TargetApiPath is `/api`, this request will be routed to https://your-api.com/api/weather.
- `BffLoginPath`: The path that the BFF will use for login requests. This endpoint will initiate the authentication process. It happens on BFF side, so no routing to the API is done.
- `BffLogoutPath`: The path that the BFF will use for logout requests. This endpoint will handle the logout process. It happens on BFF side, so no routing to the API is done.
- `BffUserInfoPath`: The path that the BFF will use to return logged user information based on the current authentication cookie. It happens on BFF side, so no routing to the API is done.
- `RefreshTokenEndpoint`: Endpoint used to refresh the access token using the refresh token. If not set, the refresh token endpoint will be read from OpenID Connect metadata (well known configuration). It's used by default service implemeting ITokenRefresherHandler.

# Token storage
For security reason Bff will not store tokens (access tokens, refresh tokens) inside cookie, but WILL NOT prevent you from doing so.
Bff will store those tokens in some other place. For now only Sqlite storage is implemented. That's why you must install `Nerdolando.Bff.Storage.Sqlite` and call it while configuring Bff: `UseSqliteTokenStorage()`

# Details
## Sign in from front application
To sign in from your front application you need to redirect user to that address on Bff Server:
`/auth{BffLoginPath}?front={frontAlias}&returnUrl=/`

For example if you are using BFF's default settings, BffLoginPath is `/login`, so the URL may look like that:
`https://bff.example.com/auth/login?front=my-spa&returnUrl=/oidc-signin`

When Bff gets this request it starts authentication process (Challenge) - remember that you need to configure it in the normal way as you would configure Authentication. 
After valid login process, Bff will store tokens in storage and redirect to your front app.
First it will look for base URL for your front alias and then add the returnUrl part to it.

So if you have configured Bff like that:

```
builder.Services.AddBff(o =>
{
    o.FrontUrls = new Dictionary<string, Uri>
    {
        { "my-spa", new Uri("https://localhost:5001") }
    };
       
    //endpoints configuration...
}).UseSqliteTokenStorage();
```

it will redirect you to `https://localhost:5001/oidc-signin` adding credential cookie to the request.
Credential cookie is encrypted by Bff, HttpOnly and secured. So it cannot be read by front applications.

## Requesting WebApi
To request your Api you need to send each request to BFF endpoint at specified path (TargetApiPath).
You do it using ordinary http client. It may be axios, fetch or any other library that sends HTTP requests.
The important thing is that you MUST configure this client to include credential cookie. It's done differently depending on library of your choice. For example:

- axios - you need to add property `withCredentials: true`, like: `axios.get(BASE_URL + '/todos', { withCredentials: true });`
- fetch - `fetch(resourceURL, {credentials:"include"})`
- Blazor - just use my library: `Nerdolando.Bff.Components`

If you are using any other http client, it should have property like the above - just read documentation "how to include credential cookie"

When you send request to Bff on configured path (ie. `/api`), Bff reads the credential cookie, then reads token for that session that has been stored during login, adds this token to Authorization header (as Bearer) and forwards this request to your Api.
Bff first checks if access token needs to be refreshed. If yes, it tries to refresh the access token first and then adds Authorization header with new access token.

For example, if your WebApi is at: `https://api.example.com` and you want to get data from: `https://api.example.com/api/weather`, you need to send this request to Bff: `https://my-bff.example.com/api/weather` including credential cookie.

## Signing out
To sign out you need to POST request (including credential cookie) to Bff at:
`/auth{BffLogoutPath}?front={frontAlias}&returnUrl=/`

For example if you are using BFF's default settings, BffLogoutPath is `/logout`, so the URL may look like that:
`https://bff.example.com/auth/logout?front=my-spa&redirectUrl=/signout`

It will work the same way as while signing in process.


