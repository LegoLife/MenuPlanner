# Menu Planner

Generates the family dinner plan: pick a start date and a number of days, and it picks a main for each night plus
the right number of sides that go with it. Lock the nights you like, re-roll the rest, and keep a list of mains and sides.
Blazor Server (.NET 10), MudBlazor, EF Core + SQLite, single-password login; runs in Docker on a Raspberry Pi.

## Live demo

**[legolife.github.io/MenuPlanner](https://legolife.github.io/MenuPlanner/)**: the real pages and planning code running entirely in your
browser (Blazor WebAssembly with SQLite compiled to WebAssembly) on the sample dishes; it resets when you reload.
The demo project is `MenuPlanner.Demo`; building it needs `dotnet workload install wasm-tools`.

## Run locally

```
dotnet run --project MenuPlanner
```

The development password is in `appsettings.Development.json`. For a real deployment, set `MenuPlanner__PasswordHash`
(generate one with `dotnet run --project MenuPlanner -- hash-password`); see `deploy/pi/`.
