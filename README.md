repo for testing Novation Launchpad Mk2 controlling with Unity3d. Eventually
meant to be a bridge between visuals and audio for my personal use. Feel free to
use it for your needs.

[License](License.md)

## Software

- Untested outside of Windows 10, might work since I'm using NAudio net standard
  version, but no promises here
- **Unity3d 2019.2.6f1 using LWRP**

## How to use

- clone
- connect your Launchpad
- open `Scenes/main`
- hit play in Unity
- move the cube with the dpad
- play something loud for the spectrum

## Known issues

First run will work out of the box, but subsequent runs in Unity will trigger a
WinForms error popup from somewhere in NAudio. It doesn't prevent NAudio from
working but requires explicit user action to launch the game. Haven't looked
into it yet.
