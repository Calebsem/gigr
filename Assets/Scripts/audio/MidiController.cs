using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NAudio.Midi;
using UnityEngine;

// using official Novation Launchpad MK2 refs for messages
// https://d2xhy469pqj8rc.cloudfront.net/sites/default/files/novation/downloads/10529/launchpad-mk2-programmers-reference-guide-v1-02.pdf
public class MidiController : MonoBehaviour {
  public const int UP = 43;
  public const int DOWN = 23;
  public const int LEFT = 32;
  public const int RIGHT = 34;
  public const int A = 36;
  public const int B = 37;
  public const int C = 46;
  public const int D = 47;
  public const int START_TRACK = 81;
  public const int launchpadColumnCount = 8;
  public const int cellCount = 4;
  public const int cellOffset = 40;
  public const float fftCeiling = float.MaxValue / 4f;

  public Orchestrator orchestrator;

  MidiIn controllerIn;
  MidiOut controllerOut;
  List<Track> tracks;

  void Start () {
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
  }

  private void OnDestroy () {
    StopAllCoroutines ();
    for (int i = 11; i < 90; i++) {
      SendMessage (i, 0);
    }
    controllerIn.Stop ();
    controllerIn.Dispose ();
    controllerOut.Dispose ();
  }

  public void DrawControls () {
    SendMessage (UP, 56);
    SendMessage (DOWN, 56);
    SendMessage (LEFT, 56);
    SendMessage (RIGHT, 56);
    SendMessage (A, 8);
    SendMessage (B, 16);
    SendMessage (C, 24);
    SendMessage (D, 32);

    if (tracks != null)
      foreach (var track in tracks) {
        SendMessage (track.index + START_TRACK, track.color);
      }
  }

  public void DrawFFT (float[] fftData) {
    for (int i = 0; i < AudioController.sampleCount; i++) {
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

  public void SetTrack (Track track) {
    if (tracks == null)
      tracks = new List<Track> ();
    tracks.Add (track);
  }

  private bool IsControl (int note) {
    return note == UP || note == DOWN || note == LEFT || note == RIGHT || note == A || note == B || note == C || note == D ||
      IsTrack (note);
  }

  private bool IsTrack (int note) {
    return tracks.Any (t => t.index + START_TRACK == note);
  }

  private Track GetTrack (int note) {
    if (!IsTrack (note)) return null;
    return tracks.Find (t => t.index == note - START_TRACK);
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
      var track = GetTrack (noteOn.NoteNumber);
      if (track != null) {
        orchestrator.ActivateTrack (track);
      }
      switch (noteOn.NoteNumber) {
        case UP:
          break;
        case DOWN:
          break;
        case LEFT:
          break;
        case RIGHT:
          break;
      }
    }
  }
}
