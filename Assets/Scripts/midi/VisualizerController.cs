using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizerController : MonoBehaviour {
  public Material BarMaterial;
  public float RotationSpeed = 25f;

  private Transform[] Children;

  void Start () {
    Children = new Transform[MidiController.sampleCount];
    for (int x = 0; x < MidiController.visualizerLineCount; x++) {
      for (int y = 0; y < MidiController.visualizerLineCount; y++) {
        var bar = GameObject.CreatePrimitive (PrimitiveType.Cube);
        bar.GetComponent<MeshRenderer> ().material = BarMaterial;
        bar.transform.SetParent (transform);
        bar.transform.localPosition = new Vector3 (
          x - MidiController.visualizerLineCount / 2f + .5f,
          0,
          y - MidiController.visualizerLineCount / 2f + .5f
        );
        Children[x * MidiController.visualizerLineCount + y] = bar.transform;
      }
    }
  }

  private void Update () {
    transform.Rotate (0, RotationSpeed * Time.deltaTime, 0);
    foreach (var child in Children) {
      if (child.localScale.y > 1) {
        var scale = child.localScale.y - Time.deltaTime;
        child.localScale = new Vector3 (1, scale, 1);
        child.localPosition = new Vector3 (child.localPosition.x, scale / 2f, child.localPosition.z);
      } else {
        child.localScale = Vector3.one;
        child.localPosition = new Vector3 (child.localPosition.x, 0, child.localPosition.z);
      }
    }
  }

  public void SetFFTData (float[] fftData) {
    for (int i = 0; i < MidiController.sampleCount; i++) {
      var amplitude = fftData[i];
      var scale = amplitude / MidiController.fftCeiling;
      var child = Children[i];
      if (!float.IsNaN (scale)) {
        child.localScale = new Vector3 (1, 1 + scale, 1);
        child.localPosition = new Vector3 (child.localPosition.x, scale / 2f, child.localPosition.z);
      } else {
        child.localScale = Vector3.one;
        child.localPosition = new Vector3 (child.localPosition.x, 0, child.localPosition.z);
      }
    }
  }
}
