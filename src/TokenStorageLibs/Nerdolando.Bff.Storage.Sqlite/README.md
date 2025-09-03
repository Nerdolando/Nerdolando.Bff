# About
`Nerdolando.Bff.StorageSqlite` is a library that provides storage for authorization tokens in `Nerdolando.Bff.AspNetCore` library.

This package uses Sqlite database to store tokens securely. It does not require any additional setup, as it creates and manages the database file automatically.


# Usage - creating Backend For Frontend.
To use this package and make `Nerdolando.Bff.AspNetCore` to store authorization tokens inside Sqlite database, just add `UseSqliteTokenStorage()` method while configuring `Nerdolando.Bff.AspNetCore`:

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

# Configuration
You can configure the database file path by setting the `SqliteStorageOptions` when adding the service:
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
}).UseSqliteTokenStorage(options =>
{
    options.FilePath = "path/to/your/database/file.db";
});
```

Default database file path is `bff_storage.db` in the current directory.