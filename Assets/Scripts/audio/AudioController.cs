using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {
  public const int sampleCount = 64;
  public const int visualizerLineCount = 8;

  public VisualizerController visualizer;
  public MidiController midi;

  private RealTimePlayback playback;
  private float[] fftData;

  private void Start () {
    playback = new RealTimePlayback (sampleCount);
    fftData = new float[sampleCount];
    playback.Start ();
    StartCoroutine (FFTUpdate ());
  }

  private void OnDestroy () {
    StopAllCoroutines ();
    playback.Stop ();
    playback.Dispose ();
  }

  private IEnumerator FFTUpdate () {
    for (;;) {
      DrawFFT ();
      yield return new WaitForSecondsRealtime (0.25f);
    }
  }

  private void DrawFFT () {
    if (playback.GetFFTData (fftData)) {
      visualizer.SetFFTData (fftData);
      midi.DrawFFT (fftData);
    }
  }
}
