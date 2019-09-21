using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class Orchestrator : MonoBehaviour {
  public MidiController midi;
  public new AudioController audio;
  public GameObject visualizer;

  private Dictionary<string, AssetBundle> bundles;

  private Track currentTrack;
  private Track lastTrack;
  private GameObject currentTrackInstance;

  private void Start () {
    TrackRenderer.showTrack = false;
    var configPath = Path.Combine (Application.dataPath, "Config/config.json");
    var configRaw = File.ReadAllText (configPath);
    var config = JsonConvert.DeserializeObject<Config> (configRaw);
    LoadBundles (config.bundles);
    foreach (var track in config.tracks) {
      midi.SetTrack (track);
    }
  }

  private void Update () {
    if (currentTrack != null && lastTrack != currentTrack) {
      SetupCurrentTrack ();
    }
  }

  private void OnDestroy () {
    TrackRenderer.showTrack = false;
    if (bundles != null) {
      foreach (var bundleKeyVal in bundles) {
        bundleKeyVal.Value.Unload (true);
      }
    }
  }

  private void SetupCurrentTrack () {
    visualizer.SetActive (false);
    var bundle = currentTrack.bundle;
    Debug.Log (String.Format ("Loading {0} from {1}", currentTrack.prefab, bundle));
    lastTrack = currentTrack;
    if (currentTrackInstance != null) {
      Destroy (currentTrackInstance);
    }

    var prefab = bundles[bundle].LoadAsset<GameObject> (currentTrack.prefab);

    currentTrackInstance = Instantiate (prefab, Vector3.zero, Quaternion.identity);

    // find first cam in the hierarchy to assign RT
    var camera = currentTrackInstance.GetComponentInChildren<Camera> ();
    if (camera != null) {
      camera.targetTexture = TrackRenderer.texture;
      TrackRenderer.showTrack = true;
    } else {
      TrackRenderer.showTrack = false;
    }
  }

  private void LoadBundles (string[] bundleNames) {
    if (bundles != null) {
      foreach (var bundleKeyVal in bundles) {
        bundleKeyVal.Value.Unload (true);
      }
      bundles.Clear ();
    } else {
      bundles = new Dictionary<string, AssetBundle> ();
    }

    foreach (var bundleName in bundleNames) {
      var bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "Config/bundles/", bundleName));
      if (bundle == null) {
        Debug.LogError (String.Format ("Failed to load AssetBundle {0}", bundleName));
        return;
      }
      bundles.Add (bundleName, bundle);
    }
  }

  public void ActivateTrack (Track track) {
    if (currentTrack != track) {
      currentTrack = track;
    }
  }
}
