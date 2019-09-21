repo for testing Novation Launchpad Mk2 controlling with Unity3d. Eventually
meant to be a bridge between visuals and audio for my personal use. Feel free to
use it for your needs.

[License](License.md)

## Software

- Untested outside of Windows 10, might work since I'm using NAudio net standard
  version, but no promises here
- **Unity3d 2019.2.6f1 using LWRP**

## How to use

While the source code is totally free to use, assets are a different thing. The
point is to handle visuals and audio at the same time. So to "protect" these
assets, I chose to use Unity3d AssetBundles. This way you can share your code
while keeping your assets to yourself. An obvious advantage is that you can
build bundles for later reuse, and extend your library in time with new assets
by just making new bundles and loading them.

The philosophical point here is that I plan to use this system for unreleased
material that I'm not ready to share publicly, because they're either demo stuff
or just not meant to be free :)

### Config

Everything is handled in a `config.json` file. Its path is
`Application.dataPath/Config/config.json`, which means
`Assets/Config/config.json` in the editor, but also
[some open access paths](https://docs.unity3d.com/ScriptReference/Application-dataPath.html)
once you have a build. There is a sample in `Assets/Config/config.json.sample`,
you can just rename it to use it as a base.

The config file is meant to describe how and what to load :

- `bundles` : a list of bundle names to load for your set. These are built and
  stored in `Application.dataPath/Config/bundles`, more on that later
- `tracks` : an array of objects describing each track that will have a trigger
  on the launchpad's top row (working on having more rows)
  - `index`: the button that will trigger this track, starting from the
    launchpad's top left button as index 0
  - `color`: the color for the button, based on
    [the Launchpad Mk2's midi specs](https://d2xhy469pqj8rc.cloudfront.net/sites/default/files/novation/downloads/10529/launchpad-mk2-programmers-reference-guide-v1-02.pdf)
  - `bundle`: which bundle contains the main prefab to load. This string must
    also be in the `bundles` array above
  - `prefab`: name of the prefab to load from the bundle

On pressing the button on your controller when running, the prefab will
instantiate in the scene. _WIP because no standard yet_

### Building bundles

- You need the Editor to build a bundle.
- make a base prefab with a known name
- assign a bundle to it in the bottom part of your inspector while selecting it
- in the top menu, `GigR/Build AssetBundles`
- the bundle files are stored in `Application.dataPath/Config/bundles` for
  whatever good this info gives you
- your bundle is ready to use, just add its name to the `bundles` array in your
  config file

Loaded bundles are done in this dir, modify `Orchestrator.cs` if you wish to
change that. This folder is git ignored by default so that you don't commit
stuff accidentally :)

## Known issues

First run will work out of the box, but subsequent runs in Unity will trigger a
WinForms error popup from somewhere in NAudio. It doesn't prevent NAudio from
working but requires explicit user action to launch the game. Haven't looked
into it yet.
