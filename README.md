# Min-OS

MonoGame version 0 architecture scaffold targeting the installed .NET 10 SDK.

## Build

```powershell
dotnet restore Min-OS.sln
dotnet build Min-OS.sln
dotnet test Min-OS.sln
```

The repository uses the newer MonoGame Content Builder task. If restore cannot resolve `MonoGame.Framework.DesktopGL` or `MonoGame.Content.Builder.Task`, install/confirm the local MonoGame template and update the pinned versions in `src/Min-OS.Game/Min-OS.Game.csproj`.

The repository also pins MGCB as a local .NET tool. Run `dotnet tool restore` from the solution directory before building; this makes the build's `dotnet mgcb` command resolve consistently across machines.

The gameplay contract and state machine are in `src/Min-OS.Game/Architecture/STATE-MACHINE.md`; the implementation boundary is documented in `ENGINEER-HANDOFF.md`.

Bits are stored as `decimal` and should be displayed with one fractional digit. HP regeneration is limited to one event per level, even with Regen upgrades.
