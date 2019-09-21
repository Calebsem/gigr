using System;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess (typeof (TrackRenderer), PostProcessEvent.AfterStack, "GigR/Renderer")]
public sealed class TrackRenderSettings : PostProcessEffectSettings {
  // [Range (0f, 1f), Tooltip ("Grayscale effect intensity.")]
  // public FloatParameter blend = new FloatParameter { value = 0.5f };
}
