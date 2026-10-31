# Min-OS

Min-OS is a keyboard-first 2D MonoGame memory game inspired by Simon Says and roguelike progression. Players memorize rows of colored squares, reproduce the color sequence, earn bits, purchase permanent upgrades, and work toward level 15.

The repository currently contains the version 0 architecture scaffold. It includes the pure game-domain contracts, pattern generation and validation, scoring and health rules, store and upgrade definitions, save-game schema, rendering view models, state-machine documentation, and a minimal MonoGame bootstrap. The full screen flow and playable vertical slices are planned Engineer work.

## Instructions for Build and Use

### Build

Run these commands from the solution directory in PowerShell:

```powershell
Set-Location C:\Users\stock\Min-OS
dotnet tool restore
dotnet restore Min-OS.sln
dotnet build Min-OS.sln --no-restore
dotnet test Min-OS.sln --no-restore
```

`dotnet tool restore` is important because the MonoGame build invokes the repository-local `dotnet mgcb` command. The repository pins MGCB in `.config/dotnet-tools.json`.

If the NuGet source is missing, add it once:

```powershell
dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org
```

If the build reports that `dotnet mgcb` cannot be found, run `dotnet tool restore` again from the directory containing `Min-OS.sln`. The global `mgcb` command and the project-local `dotnet mgcb` command are separate tool resolutions.

### Run

After a successful build, run the current MonoGame scaffold with:

```powershell
dotnet run --project src\Min-OS.Game\Min-OS.Game.csproj --no-restore
```

The current scaffold opens a MonoGame window, clears it to the Min-OS blue background, and exits through the normal window close control or the gamepad Back button. Gameplay screens, input mapping, and the complete run loop are still being implemented.

### Current rules and implementation notes

- Levels contain one set per level, with four colors per set.
- Patterns support deterministic debug seeds and random normal runs.
- The ninth palette color is gray (`#808080`).
- HP regeneration can occur at most once per level.
- Breakpoint time restoration starts at one second and increases linearly with Breakpoint upgrades.
- Store prices use linear progression in version 0.
- Bits are stored as decimal values and should display with one fractional digit.
- Wallpoet may be replaced by Arial or another basic available font if it cannot be bundled.

## Development Environment

To recreate the development environment, install or have access to:

* Windows with PowerShell
* .NET SDK 10.0 or later; the original development machine uses .NET SDK 10.0.402
* MonoGame C# templates, installed with `dotnet new install MonoGame.Templates.CSharp`
* NuGet access to `https://api.nuget.org/v3/index.json`
* MonoGame framework package `MonoGame.Framework.DesktopGL` version `3.8.4.1`
* MonoGame content package `MonoGame.Content.Builder.Task` version `3.8.4.1`
* The repository-local `dotnet-mgcb` tool version `3.8.4.1`, restored with `dotnet tool restore`

The project targets DesktopGL and uses the newer MonoGame content-building workflow. Do not commit `bin` or `obj` output folders.

## Useful Websites to Learn More

I found these websites useful in developing this software:

* [MonoGame Documentation](https://docs.monogame.net/)
* [MonoGame Content Builder](https://docs.monogame.net/articles/getting_started/content_pipeline/content_builder/index.html)
* [MonoGame Content Builder Command-Line Tool](https://docs.monogame.net/articles/getting_started/tools/mgcb.html)
* [Microsoft .NET CLI Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/)
* [NuGet Package Restore](https://learn.microsoft.com/en-us/nuget/consume-packages/package-restore)

## Future Work

The following items are planned for future versions of Min-OS:

* [ ] Fix bugs and complete the leaderboard screen.
* [ ] Balance and tune prices and effects on the store screen.
* [ ] Make the audio manager fully operational, including music and sound effects.
* [ ] Improve UI cleanliness and add more engaging graphics.
* [ ] Add a Cached Memory Mode that does not require memorization of the pattern.

