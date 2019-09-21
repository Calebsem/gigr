using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public sealed class TrackRenderer : PostProcessEffectRenderer<TrackRenderSettings> {

  public static RenderTexture texture;
  public static bool showTrack;

  public override void Init () {
    if (texture != null)
      RenderTexture.ReleaseTemporary (texture);
    texture = RenderTexture.GetTemporary (
      Screen.width, Screen.height, 24,
      RenderTextureFormat.Default,
      RenderTextureReadWrite.Default,
      4
    );
  }

  public override void Render (PostProcessRenderContext context) {
    var sheet = context.propertySheets.Get (Shader.Find ("Hidden/GigR/Render"));
    sheet.properties.SetInt ("_showTrack", showTrack ? 1 : 0);
    if (showTrack) {
      sheet.properties.SetTexture ("_TrackTex", texture);
    }
    context.command.BlitFullscreenTriangle (context.source, context.destination, sheet, 0);
  }

  public override void Release () {
    RenderTexture.ReleaseTemporary (texture);
  }
}
