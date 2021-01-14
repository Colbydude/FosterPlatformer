# FosterPlatformer
C# port of [tiny_link](https://github.com/NoelFB/tiny_link) for the purpose of practice and testing out the [Foster](https://github.com/NoelFB/Foster) framework. All original artwork and source code belong to [Noel Berry](https://twitter.com/NoelFB).

**NOTE:** Currently this uses my own fork of Foster to add more Time utilities (`PauseFor` and `OnTime`), ported from [blah](https://github.com/NoelFB/blah).

## Running:
- **Requirements:** [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0) and [C# 8.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8)
- Clone the repo.
- `dotnet run -p src`

## Known Issues:
- Reloading using F9 appears to be busted ...for no reason?
- SpriteFont drawing had issues scaling, or at least doesn't behave how I'd expect it to. Tweaks were made to get things looking good.
- Dying and falling out of the map will move the player back into starting position, but fall again. After falling again, the room will reload like normal.
- Dying in 3-0 or 8-0 will cause you to fall down to 3-1 or 8-1, respectively, because of the above issue. This may potentially cause you to get stuck until you reload the room.
- Virtual inputs currently lack a `ClearPressedBuffer()` (from blah)(or is named something else and I missed it), so it may behave differently. Had to adjust some numbers a bit from the source.
