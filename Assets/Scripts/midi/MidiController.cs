using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAudio.Midi;
using UnityEngine;

// using official Novation Launchpad MK2 refs for messages
// https://d2xhy469pqj8rc.cloudfront.net/sites/default/files/novation/downloads/10529/launchpad-mk2-programmers-reference-guide-v1-02.pdf
public class MidiController : MonoBehaviour {
  MidiIn controllerIn;
  MidiOut controllerOut;
  RealTimePlayback playback;

  public const int UP = 43;
  public const int DOWN = 23;
  public const int LEFT = 32;
  public const int RIGHT = 34;
  public const int A = 36;
  public const int B = 37;
  public const int C = 46;
  public const int D = 47;
  public const int sampleCount = 64;
  public const int visualizerLineCount = 8;
  public const int launchpadColumnCount = 8;
  public const int cellCount = 4;
  public const int cellOffset = 40;
  public const float fftCeiling = float.MaxValue / 4f;

  public Transform cube;
  public VisualizerController visualizer;
  public Vector2 translation;

  private float[] fftData;
  private Vector3 originalPos;

  void Start () {
    originalPos = Camera.main.transform.localPosition;
    for (int device = 0; device < MidiIn.NumberOfDevices; device++) {
      if (MidiIn.DeviceInfo (device).ProductName.Contains ("Launchpad")) {
        controllerIn = new MidiIn (device);
        controllerIn.MessageReceived += controller_MessageReceived;
        controllerIn.ErrorReceived += controller_ErrorReceived;
        controllerIn.Start ();

        break;
      }
    }
    for (int device = 0; device < MidiOut.NumberOfDevices; device++) {
      if (MidiOut.DeviceInfo (device).ProductName.Contains ("Launchpad")) {
        controllerOut = new MidiOut (device);
        break;
      }
    }
    playback = new RealTimePlayback (sampleCount);
    fftData = new float[sampleCount];
    playback.Start ();
  }

  private void Update () {
    // test stuff, plox ignore
    if (cube && translation.sqrMagnitude > 0) {
      var move = Camera.main.transform.forward * translation.x + Camera.main.transform.right * translation.y;
      move.Normalize ();
      cube.Translate (move);
      if (translation.x > 0)
        translation.x -= Time.deltaTime * 2;
      else
        translation.x += Time.deltaTime * 2;
      if (translation.y > 0)
        translation.y -= Time.deltaTime * 2;
      else
        translation.y += Time.deltaTime * 2;
      if (Mathf.Abs (translation.x) < 0.25f) {
        translation.x = 0;
      }
      if (Mathf.Abs (translation.y) < 0.25f) {
        translation.y = 0;
      }
    }
    DrawFFT ();
    DrawControls ();
  }

  private void LateUpdate () {
    var avgAmplitude = fftData.Where (value => !float.IsNaN (value)).Average () / fftCeiling;
    if (!float.IsNaN (avgAmplitude) && !float.IsInfinity (avgAmplitude))
      Camera.main.transform.localPosition = originalPos + UnityEngine.Random.insideUnitSphere * avgAmplitude;
  }

  private void OnDestroy () {
    for (int i = 11; i < 90; i++) {
      SendMessage (i, 0);
    }
    controllerIn.Stop ();
    controllerIn.Dispose ();
    controllerOut.Dispose ();
    playback.Stop ();
    playback.Dispose ();
  }

  private void DrawFFT () {
    if (playback.GetFFTData (fftData)) {
      visualizer.SetFFTData (fftData);
      for (int i = 0; i < sampleCount; i++) {
        var amplitude = fftData[i];
        if (i % launchpadColumnCount == 0) {
          var launchpadIndex = i / launchpadColumnCount;
          if (amplitude > 1) {
            for (int cell = 1; cell <= cellCount; cell++) {
              if (amplitude / (cell * (fftCeiling / cellCount)) >= 1)
                SendMessage (cellOffset + 10 * cell + 1 + launchpadIndex, 72 + launchpadIndex, 1, true);
            }
          } else {
            for (int cell = 1; cell <= cellCount; cell++) {
              SendMessage (cellOffset + 10 * cell + 1 + launchpadIndex, 0, 1, true);
            }
          }
        }
      }
    }
  }

  private void DrawControls () {
    SendMessage (UP, 56);
    SendMessage (DOWN, 56);
    SendMessage (LEFT, 56);
    SendMessage (RIGHT, 56);
    SendMessage (A, 8);
    SendMessage (B, 16);
    SendMessage (C, 24);
    SendMessage (D, 32);
  }

  private bool IsControl (int note) {
    return note == UP || note == DOWN || note == LEFT || note == RIGHT || note == A || note == B || note == C || note == D;
  }

  private void SendMessage (int note, int velocity, int channel = 1, bool ignoreControls = false) {
    if (ignoreControls && IsControl (note)) return;
    var noteOnEvent = new NoteOnEvent (0, channel, note, velocity, 1);
    controllerOut.Send (noteOnEvent.GetAsShortMessage ());
  }

  private void controller_ErrorReceived (object sender, MidiInMessageEventArgs e) {
    Debug.LogError (String.Format ("Time {0} Message 0x{1:X8} Event {2}",
      e.Timestamp, e.RawMessage, e.MidiEvent));
  }

  private void controller_MessageReceived (object sender, MidiInMessageEventArgs e) {
    var noteOn = e.MidiEvent as NoteOnEvent;
    if (noteOn != null) {
      switch (noteOn.NoteNumber) {
        case UP:
          translation.x = 1;
          break;
        case DOWN:
          translation.x = -1;
          break;
        case LEFT:
          translation.y = -1;
          break;
        case RIGHT:
          translation.y = 1;
          break;
      }
    }
  }
}
