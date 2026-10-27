# Setup guide — Windows

About 5 minutes, once.

## 1. Start the app on your PC

1. Download `iPhoneCamRemote-win-x64.zip` from
   [Releases](https://github.com/jameellzx/iphone-cam-remote/releases/latest) and unzip it.
2. Double-click **`iPhoneCamRemote.exe`**.
   - Windows may show *"Windows protected your PC"* because the app isn't code-signed.
     Click **More info → Run anyway**.
3. Make sure Bluetooth is **on** (Settings → Bluetooth & devices).
4. The top line of the app says **Waiting for iPhone — pair with "YOUR-PC-NAME"**.
   - If it says **Bluetooth failed: ... peripheral role**, your PC's Bluetooth radio can't act as a
     keyboard. A USB Bluetooth 5.0+ dongle with a named chipset (e.g. ASUS USB-BT500) fixes that.

## 2. Turn on the iPhone pointer

On the iPhone: **Settings → Accessibility → Touch → AssistiveTouch → ON**.

This gives the iPhone a mouse pointer, which the remote uses to press the pause button.
Tip: in the same screen set **Idle Opacity** to 15% so the floating button barely shows.

## 3. Pair the iPhone with the PC

On the iPhone: **Settings → Bluetooth** → under *Other Devices* tap your **PC's name** → **Pair**.

The top line in the app turns green: **Connected: iPhone**.

> If your PC's name doesn't show up: turn iPhone Bluetooth off and on, and keep the app open.
> If it paired before and stopped working: tap ⓘ next to the PC → **Forget This Device**, then pair again.

## 4. Show it where the pause button is

1. Open the **Camera** app, switch to **Video**, and hold/mount the phone the way you film.
2. **Right click** in the big box → the phone starts recording (the box turns red).
3. Click **Show spot on phone** → a grey circle (the pointer) appears on the iPhone screen.
4. Change the **Right** and **Down** numbers, then click **Show spot on phone** again.
   Repeat until the circle sits on the **pause button**.
5. Click **Test tap** → the recording pauses. **Left click** in the box resumes it.
6. **Right click** to stop.

The spot is saved. You only redo this if you film in a different orientation (portrait vs landscape).

## 5. Film

| In the big box | What happens |
| --- | --- |
| **Right click** | Start recording / stop recording |
| **Left click** | Pause / resume |

- Clicks only count **inside the app window**, so you can still use your mouse normally elsewhere.
- A click that doesn't make sense (e.g. left click while stopped) just beeps — it never starts or stops
  a take by accident.
- The first pause after you start recording may take ~1 second while the pointer travels to the
  button. Every pause/resume after that is instant.
- If you tap the phone yourself and the box shows the wrong state, click **Phone is stopped (reset)**.

## Troubleshooting

| Problem | Fix |
| --- | --- |
| Right click does nothing on the phone | Make sure the Camera app is open and on **Video**. Check the top line says *Connected*. |
| Pause tap misses the button | Redo step 4. Keep the phone in the same orientation. |
| No pointer on the iPhone | AssistiveTouch is off — see step 2. |
| Phone won't reconnect after restarting the app | iPhone Settings → Bluetooth → tap the PC name to connect. If that fails, Forget and pair again. |
| "peripheral role" error | Your Bluetooth radio can't do this; use a Bluetooth 5.0+ USB dongle. |
