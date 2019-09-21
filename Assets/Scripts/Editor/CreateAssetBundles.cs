using System.IO;
using UnityEditor;

public class CreateAssetBundles {
  [MenuItem ("GigR/Build AssetBundles")]
  static void BuildAllAssetBundles () {
    string assetBundleDirectory = "Assets/Config/bundles";
    if (!Directory.Exists (assetBundleDirectory)) {
      Directory.CreateDirectory (assetBundleDirectory);
    }
    BuildPipeline.BuildAssetBundles (assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
  }
}
