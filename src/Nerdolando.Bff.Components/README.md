# About
`Nerdolando.Bff.Components` is a library that provides integration for Blazor application with `Nerdolando.Bff.AspNetCore` library.


# Usage

1. Install the NuGet package:
   ```
   dotnet add package Nerdolando.Bff.Components
   ```

1. Configure your Blazor application to use BFF services in your `Program.cs`:
   ```csharp
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddAuthorizationCore();
    
    //add BFF services
    builder.Services.AddBffServices(o =>
    {
        o.BffBaseAddress = new Uri("https://localhost:7133"); //your BFF URL
        o.FrontAlias = "my-spa"; //the same alias you used in BFF configuration
    });
    
    //add client to call your API through BFF
    builder.Services.AddHttpClient("BffApi", c =>
    {
        c.BaseAddress = new Uri("https://localhost:7133"); //you must provide your BFF URL here, not Api URL. You define Api path in BFF configuration
    }).AddCredentialCookie(); //you must add this to send auth cookie to BFF
    
   ```

# Components
You can use the following components in your Blazor application:
## RedirectToLogin
This component redirects the user to the login page of the BFF when rendered. You can specify the `ReturnUrl` parameter to indicate where the user should be redirected after a successful login.
Note that this component works as soon as it's rendered. Example usage:

```razor
@page "/authentication/login"
@using Nerdolando.Bff.Components.Components

<RedirectToLogin FrontAlias="my-spa" ReturnUrl="/signed-in" />
```
When you navigate to `/authentication/login`, the component will redirect you to the BFF login page. After a successful login, you will be redirected to `/signed-in`.

# Services
## IAuthenticationStateRefresher
This service allows you to refresh the authentication state of the user.

It has only one method: `RefreshAsync()`, which refreshes the authentication state.

## ILogOutRequest
This service allows you to log out the user from the system (BFF and Identity Provider).
It has only one method: `LogoutAsync(string returnUrl = "/")`, which logs out the user and redirects to the specified return URL.

