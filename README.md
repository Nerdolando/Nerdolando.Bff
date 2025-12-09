[![Build and test](https://github.com/Nerdolando/Nerdolando.Bff/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Nerdolando/Nerdolando.Bff/actions/workflows/dotnet.yml)
[![Deploy](https://github.com/Nerdolando/Nerdolando.Bff/actions/workflows/deploy.yml/badge.svg)](https://github.com/Nerdolando/Nerdolando.Bff/actions/workflows/deploy.yml)
[![License](https://img.shields.io/github/license/Nerdolando/Nerdolando.Bff.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Nerdolando.Bff.AspNetCore.svg)](https://www.nuget.org/packages/Nerdolando.Bff.AspNetCore/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Nerdolando.Bff.AspNetCore.svg)](https://www.nuget.org/packages/Nerdolando.Bff.AspNetCore/)


# Nerdolando.Bff
**Secure Backend-for-Frontend (BFF) for .NET** – built on ASP.NET Core, YARP and OpenIdConnect, with safe token storage and refresh-token flow out of the box.

---

## Why (Problem & Solution)

**Problem**  
SPA / front-end applications (React, Vue, Angular, Blazor, ...) should **not** keep access tokens in JavaScript. Handling tokens in the browser leads to security pitfalls (XSS, token leakage, complicated flows). A recommended pattern is the **Backend for Frontend (BFF)**: a dedicated backend that:

- owns the auth cookie,
- talks to the Identity Provider (IdP),
- calls downstream APIs on behalf of the SPA.

But manually building such a BFF on top of YARP + OpenIdConnect in ASP.NET Core is cumbersome and full of pitfalls.

**Solution – Nerdolando.Bff**  
`Nerdolando.Bff` gives you a ready-to-use BFF library that:

- hosts the authentication cookie on the server,
- proxies calls from your front-end to your API,
- stores access/refresh tokens in **safe server-side storage** (not in JS),
- automatically handles **refresh token flow**.

**For whom**  
For .NET developers building SPA / Blazor applications who want a **good, secure and free BFF** without spending days reading tons of articles and wiring everything manually.

---

## Typical scenarios

- SPA application (React, Vue, Angular, Blazor, etc.) using OAuth / OpenID Connect + backend Web API.
- Blazor WebAssembly app calling a protected Web API.
- Multi-front setup (e.g. main SPA + admin panel) sharing the same BFF.

---

## About the library

`Nerdolando.Bff` is a library that provides tools and components to build **Backend for Frontend** layers on ASP.NET Core. It:

- is built on top of **ASP.NET Core**, **YARP** and **Microsoft’s OpenIdConnect handler**,
- follows **BFF best practices** with security in mind,
- separates **front-end** code (SPA/Blazor) from sensitive auth logic.

If you are new to the BFF architecture, you can read more here:

- (PL) https://masterbranch.pl/jak-trzymac-tokeny-w-spa-czyli-backend-for-frontend/
- (EN) https://auth0.com/blog/the-backend-for-frontend-pattern-bff/

---

## Front-end applications

- `Nerdolando.Bff.AspNetCore` – the **backend/BFF** package. It does not provide any UI. You use it in your ASP.NET Core host that acts as the BFF.
- `Nerdolando.Bff.Components` – optional package with **ready services for Blazor** front-end to integrate with the BFF seamlessly.

You are free to use **any front-end framework** (React, Vue, Angular, Blazor, etc.) as long as it can:

- send HTTP requests,
- include the authentication cookie (credentials) in those requests (even simple javascript's `fetch`).

## Quick start
### 1. Create a ASP.NET Core project that will act as your BFF. It may be simple `WebApi` or `Blazor Server`.
### 1. Install the NuGet packages:
   ```
   dotnet add package Nerdolando.Bff.AspNetCore 
   dotnet add package Nerdolando.Bff.Storage.Sqlite
   ```
### 1. Configure the BFF services (`Program.cs`):
   ```csharp
    builder.Services.AddBff(o =>
    {
        o.FrontUrls = new Dictionary<string, Uri>
        {
            { "my-spa-app", new Uri("https://localhost:5001") } //front alias -> front URL
        };
           
        o.Endpoints.ChallengeAuthenticationScheme = yourChallengeAuthScheme;
        o.Endpoints.TargetApiBaseUrl = new Uri("https://api.yourservice.com"); //your api base URL
        o.Endpoints.TargetApiPath = "/api"; //path on BFF that will be forwarded to the API
    }).UseSqliteTokenStorage(); //from Nerdolando.Bff.Storage.Sqlite - see below
   ```
yourChallengeAuthScheme should be the name of your authentication scheme (e.g. the cookie/OIDC scheme you configure with AddAuthentication / AddOpenIdConnect).

### 1. Configure CORS
Your front-end must be allowed to talk to the BFF (including cookies). Add a CORS policy:

   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("CorsPolicy", policy =>
       {
           policy.WithOrigins("https://localhost:5001") // replace it with your front app URL
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials(); // this is important
       });
   });

   var app = builder.Build();
   
   app.UseRouting();

   app.UseCors("CorsPolicy"); // after UseRouting, before UseAuthorization

   app.UseAuthentication();
   app.UseAuthorization();

   app.MapBff(); // must be AFTER UseAuthentication and UseAuthorization, REMEMBER!
   ```
If you are not aware of CORS, you can read more about it here: https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS
    
### 1. REMEMBER about BFF middleware:
   ```csharp
   app.MapBff(); //must be AFTER UseAuthentication and UseAuthorization
   ```   
### 1. Configure authentication
You must configure remote authentication (e.g. OpenID Connect) in your ASP.NET Core BFF host. You do this as you normally would in any other webapp with OIDC/OAuth

`Nerdolando.Bff` does not enforce a specific IdP. You can use any provider compatible with OpenID Connect (Google, Auth0, Microsoft Entra ID, etc.) as long as you configure it via standard ASP.NET Core authentication:

```csharp
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        // configure your OIDC provider here
    });

```

Make sure ChallengeAuthenticationScheme in AddBff matches the scheme you want to use for user sign-in (e.g. "oidc").

So **the whole BFF project** could look like that:

```csharp
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        // configure your OIDC provider here
    });

 builder.Services.AddBff(o =>
 {
     o.FrontUrls = new Dictionary<string, Uri>
     {
         { "my-spa-app", new Uri("https://localhost:5001") } 
     };
        
     o.Endpoints.ChallengeAuthenticationScheme = "oidc"; // your challenge scheme that you use in AddAuthentication
     o.Endpoints.TargetApiBaseUrl = new Uri("https://api.yourservice.com"); //your api base URL
     o.Endpoints.TargetApiPath = "/api"; //path on BFF that will be forwarded to the API
 }).UseSqliteTokenStorage(); //from Nerdolando.Bff.Storage.Sqlite - see below

builder.Services.AddCors(options =>
   {
       options.AddPolicy("CorsPolicy", policy =>
       {
           policy.WithOrigins("https://localhost:5001") // replace it with your front app URL
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials(); // this is important
       });
   });

   var app = builder.Build();
   
   app.UseRouting();

   app.UseCors("CorsPolicy"); // after UseRouting, before UseAuthorization

   app.UseAuthentication();
   app.UseAuthorization();

   app.MapBff(); // must be AFTER UseAuthentication and UseAuthorization, REMEMBER!
```

Actually that's it. You don't need any other stuff in your BFF project.

## Configuration
`Nerdolando.Bff.AspNetCore` exposes via AddBff method (and underlying BffConfig class):

### FrontUrls
A dictionary of whitelisted front-end applications:
- Key – front alias of your choice (e.g. "front", "spa", "admin")
- Value – base URI where your front application is hosted.

Example:
```csharp
o.FrontUrls = new Dictionary<string, Uri>
{
    { "front", new Uri("https://localhost:5001") },
    { "admin", new Uri("https://localhost:6001") }
};
```

This acts as a whitelist:
BFF will only send the authentication cookie to URLs that are listed here.

>This is crucial for security: it prevents cookies from being issued/attached to unauthorized front-ends. Use HTTPS in production.

### Endpoints
Controls how the BFF talks to the target API and exposes its own endpoints:

- `ChallengeAuthenticationScheme`: Authentication scheme used when a challenge is required. Typically the scheme you use for user sign-in registered in `AddAuthentication` method
- `TargetApiBaseUrl`: The base URL of the target API where requests will be forwarded to, e.g. `https://api.yourapi.com`
- `TargetApiPath`: Path on the BFF that will be forwarded to the API. Example: if TargetApiPath = "/api" and client calls `https://bff.example.com/api/weather`, BFF forwards the request to `https://api.yourservice.com/api/weather`.
- `BffLoginPath`: Path on the BFF used to initiate login, e.g. `/login`. No forwarding to the API here – this is handled by the BFF itself.
- `BffLogoutPath`: Path on the BFF used to log the user out, e.g. /logout. No forwarding to the API here – this is handled by the BFF itself.
- `BffUserInfoPath`: Path on the BFF used to retrieve user information based on the current authentication cookie, e.g. `/user`. No forwarding to the API here – this is handled by the BFF itself.
- `RefreshTokenEndpoint`: Endpoint used to refresh the access token using the refresh token. If not set, it is read from the OpenID Connect metadata (well-known configuration) and used by the default `ITokenRefresherHandler` implementation.

## Token storage
For security reason Bff **does not** store tokens (access tokens, refresh tokens) inside cookie, but WILL NOT prevent you from doing so. Instead:
- authentication cookie is encrypted, HttpOnly, Secure,
- tokens are stored in server-side storage.
  
Currently, the implemented storage is Sqlite via `Nerdolando.Bff.Storage.Sqlite`:
```csharp
builder.Services
    .AddBff(o => { /* options */ })
    .UseSqliteTokenStorage();
```

This storage keeps tokens per session/server-side, so the browser only holds the credential cookie.
You are not prevented from storing tokens elsewhere if you really need to, but the recommended approach is to keep tokens out of JavaScript.

# How it works (flow details)
## Sign in from the front application
To sign in from your front-end, redirect the user to the BFF:
`/auth{BffLoginPath}?front={frontAlias}&returnUrl=/`

Example with default BffLoginPath = "/login":
`https://bff.example.com/auth/login?front=my-spa&returnUrl=/oidc-signin`

Flow:
1. BFF receives the request at /auth/login
1. BFF starts the authentication challenge using the configured `ChallengeAuthenticationScheme`
1. After a successful login at the IdP (Identity Provider):
   - BFF stores tokens (access + refresh) in token storage,
   - BFF looks up the base URL for the front alias,
   - BFF redirects the user to `{frontBaseUrl}{returnUrl}`, e.g. `https://localhost:5001/oidc-signin`
   - BFF sets the credential cookie on that redirect.

The credential cookie:
- is encrypted by the BFF,
- is HttpOnly and Secure,
- cannot be accessed from JavaScript on the front-end.

## Calling the Web API via BFF
Your front-end never calls the API directly. Instead, it calls the BFF.
1. Front-end sends requests to the BFF at `TargetApiPath`, e.g.: `https://bff.example.com/api/weather`
1. The HTTP client must include credentials (cookies):
   - axios:
   ```javascript
   axios.get(`${BASE_URL}/api/todos`, { withCredentials: true });
   ```
   - fetch:
   ```javascript
   fetch(`${BASE_URL}/api/todos`, { credentials: "include" });
   ```
   - Blazor:
   just use `Nerdolando.Bff.Components` which abstracts this for you.
1. BFF:
   - reads the credential cookie,
   - loads tokens for the current session from storage,
   - checks if the access token needs to be refreshed:
      - if yes – attempts to refresh it using the refresh token,
   - attaches the (possibly refreshed) access token in the `Authorization: Bearer` header,
   - forwards the request to the configured TargetApiBaseUrl + path.
  
Example:
- API: `https://api.example.com`
- Protected endpoint: `https://api.example.com/api/weather`
- BFF is configured with:
   - TargetApiBaseUrl = "https://api.example.com"
   - TargetApiPath = "/api"

The front-end calls:
`https://my-bff.example.com/api/weather`
BFF forwards it to:
`https://api.example.com/api/weather`
with the appropriate `Authorization` header.

## Signing out
To sign out, send a POST request (including the credential cookie) to:
`/auth{BffLogoutPath}?front={frontAlias}&returnUrl={relativeReturnUrl}`

Example with default BffLogoutPath = "/logout":
`https://bff.example.com/auth/logout?front=my-spa&returnUrl=/signout`

Flow:
1. BFF receives the logout request.
1. BFF performs its logout logic:
   - clears tokens from storage for the session,
   - deletes/invalidates the credential cookie,
   - optionally calls sign-out on the IdP if configured.
1. BFF redirects back to the front URL constructed from the alias + returnUrl, e.g.: `https://localhost:5001/signout`

# Further reading
(PL) BFF overview: https://masterbranch.pl/jak-trzymac-tokeny-w-spa-czyli-backend-for-frontend/
(EN) BFF pattern explained: https://auth0.com/blog/the-backend-for-frontend-pattern-bff/

Pull requests, issues and feedback are very welcome 😊


