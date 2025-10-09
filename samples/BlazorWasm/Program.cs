using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nerdolando.Bff.Components.Extensions;

namespace BlazorWasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddBffServices(o =>
            {
                o.BffBaseAddress = new Uri("https://localhost:7133");
                o.FrontAlias = "local";
            });

            builder.Services.AddHttpClient("BffApi", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7133");
            }).AddCredentialCookie();


            await builder.Build().RunAsync();
        }
    }
}
