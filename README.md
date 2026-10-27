# iPhone Cam Remote

Control your iPhone's **built-in Camera app** from your Windows PC — over your PC's own Bluetooth.
No extra hardware, no app on the phone.

| Mouse button (inside the remote window) | iPhone Camera |
| --- | --- |
| **Right click** | Start recording / stop recording |
| **Left click** | Pause / resume |

Pause while you think, resume when you talk — the dead space never gets recorded, and you still
end up with **one clip** in your Camera Roll.

> **Status: early release.** The PC side is tested; real-world iPhone testing is ongoing.
> Please open an issue if something doesn't work on your phone.

## Download

1. Go to **[Releases](https://github.com/jameellzx/iphone-cam-remote/releases/latest)** and download
   `iPhoneCamRemote-win-x64.zip`.
2. Unzip it anywhere and double-click **`iPhoneCamRemote.exe`**. No install, no .NET needed.
3. Follow the setup guide for your computer:
   - **[Windows setup guide](docs/SETUP-WINDOWS.md)** ← start here
   - **[Mac](docs/SETUP-MAC.md)** — not supported yet, read why

## What you need

- A **Windows 10 (2004+) or Windows 11** PC with Bluetooth that supports the "LE peripheral role".
  Most built-in Intel, Realtek and laptop radios do; cheap no-name dongles often don't.
  The app tells you straight away if yours can't.
- An **iPhone on iOS 18 or newer** (the Camera app got its pause button in iOS 18).

## How it works

Your PC pretends to be a Bluetooth keyboard + mouse that the iPhone pairs with like any accessory.

- **Record / stop** = the PC presses **Volume Up**. The Camera app treats Volume Up as its shutter
  button — the same trick cheap Bluetooth selfie remotes use.
- **Pause / resume** = there is no hardware key for pause, so the PC moves the iPhone's
  AssistiveTouch pointer onto the Camera's pause button and clicks it. You show it where the button
  is once; it remembers.

## Build it yourself

Want to change it or build from source? See **[docs/BUILD.md](docs/BUILD.md)**.

## Credits

Built on **[windows-ble-hid](https://github.com/abhishek-raj/windows-ble-hid)** by Abhishek Raj (MIT),
which does the hard part: turning a Windows PC into a Bluetooth LE keyboard and mouse using only the
in-box Windows Bluetooth stack. This project adds a Volume-Up (consumer control) report and the
camera remote app in `src/CamRemote`. The original project's README is kept at
[docs/BLEHID-README.md](docs/BLEHID-README.md).

MIT licensed — see [LICENSE](LICENSE).
