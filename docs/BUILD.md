# Build from source (Windows)

You only need this if you want to change the code. Everyone else: grab the
[release download](https://github.com/jameellzx/iphone-cam-remote/releases/latest).

## Requirements

- Windows 10 2004+ or Windows 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

## Build and run

```bash
git clone https://github.com/jameellzx/iphone-cam-remote
cd iphone-cam-remote
dotnet run --project src/CamRemote
```

## Make the download zip (no .NET needed on the target PC)

```bash
dotnet publish src/CamRemote/CamRemote.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o publish
```

`publish/iPhoneCamRemote.exe` is the whole app.

## Check your Bluetooth radio

```bash
dotnet run --project src/BleHid.Cli -- --diagnose
```

Look for `Peripheral role : True`. If it says `False`, that radio can't act as a keyboard.

## Run the tests

```bash
dotnet test tests/BleHid.Core.Tests
```

## Where things are

| Path | What |
| --- | --- |
| `src/CamRemote/MainForm.cs` | The remote window: click mapping, record/pause logic, pointer moves |
| `src/BleHid.Core/HidDescriptors.cs` | Bluetooth HID descriptor (keyboard, mouse, Volume Up report) |
| `src/BleHid.Core/BleHidPeripheral.cs` | Publishes the PC as a Bluetooth LE keyboard/mouse |
| `src/BleHid.App`, `src/BleHid.Cli` | The original windows-ble-hid apps (general keyboard/mouse sharing) |

Settings (the pause button spot) are stored in `%APPDATA%\iPhoneCamRemote\settings.json`.
