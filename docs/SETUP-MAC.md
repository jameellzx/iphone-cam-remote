# Mac — not supported yet

**iPhone Cam Remote currently runs on Windows only.** There is no Mac download.

## Why

The app works by making the computer pretend to be a Bluetooth keyboard and mouse.
Windows lets normal apps do that. **macOS doesn't**: Apple's Bluetooth framework for apps
(CoreBluetooth) refuses to publish the keyboard/mouse (HID) service, so the iPhone never sees the
Mac as a keyboard. Porting the code isn't enough — it needs a completely different approach on Mac.

## What Mac users can do today

- **Use a Windows PC** (any Windows 10/11 laptop with Bluetooth) and follow the
  [Windows setup guide](SETUP-WINDOWS.md).
- **Use a cheap Bluetooth camera remote** (the ~$5 "selfie shutter" buttons). They send Volume Up,
  so they can start and stop recording — but they can't pause.
- **Use an Apple Watch**: the Camera Remote app on the watch can start and stop recording.

## Want to help?

A Mac version would need either a small Bluetooth board (like an ESP32) plugged into the Mac to act
as the keyboard, or macOS's lower-level Bluetooth APIs. If you'd like to build it, open an issue.
