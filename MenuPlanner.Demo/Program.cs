using System.Security.Claims;
using MenuPlanner.Components;
using MenuPlanner.Data;
using MenuPlanner.Planning;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
// SQLite runs inside the page; the file lives in the browser's in-memory file system.
builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseSqlite("Data Source=menuplanner.db"));
builder.Services.AddSingleton(Random.Shared);
builder.Services.AddSingleton<MenuGenerator>();
builder.Services.AddScoped<PlanService>();

// Every visitor is signed in.
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, DemoAuthState>();

var host = builder.Build();
// The sample dishes are model seed data (HasData), so creating the schema adds them.
await using (var db = await host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
{
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
}
await host.RunAsync();

internal sealed class DemoAuthState : AuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> State = Task.FromResult(new AuthenticationState(
        new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Demo")], "demo"))));

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => State;
}
