using System;

[Serializable]
public class Config {
  public string[] bundles;
  public Track[] tracks;
}

[Serializable]
public class Track {
  public int index;
  public int color;
  public string bundle;
  public string prefab;
}
