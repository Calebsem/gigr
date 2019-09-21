// shamelessly copied and adapted from stackoverflow
// https://stackoverflow.com/questions/13398802/spectrum-analyzer-with-naudio-wpfsoundvisualizationlib
using System;
using NAudio.Dsp;
using NAudio.Wave;

public class RealTimePlayback : IDisposable {
  private WaveIn _capture;
  private object _lock;
  private int _fftPos;
  private int _fftLength;
  private Complex[] _fftBuffer;
  private float[] _lastFftBuffer;
  private bool _fftBufferAvailable;
  private int _m;

  public RealTimePlayback (int length) {
    this._lock = new object ();

    // Wasapi being pretty unstable on my setup
    // I went for the standard WaveIn defaulting
    // to whatever input comes first
    this._capture = new WaveIn ();
    this._capture.DataAvailable += this.DataAvailable;

    this._m = (int) Math.Log (this._fftLength, 2.0);
    this._fftLength = length; // 44.1kHz.
    this._fftBuffer = new Complex[this._fftLength];
    this._lastFftBuffer = new float[this._fftLength];
  }

  public WaveFormat Format {
    get {
      return this._capture.WaveFormat;
    }
  }

  private float[] ConvertByteToFloat (byte[] array, int length) {
    int samplesNeeded = length / 4;
    float[] floatArr = new float[samplesNeeded];

    for (int i = 0; i < samplesNeeded; i++) {
      floatArr[i] = BitConverter.ToSingle (array, i * 4);
    }

    return floatArr;
  }

  private void DataAvailable (object sender, WaveInEventArgs e) {
    // Convert byte[] to float[].
    float[] data = ConvertByteToFloat (e.Buffer, e.BytesRecorded);

    // For all data. Skip right channel on stereo (i += this.Format.Channels).
    for (int i = 0; i < data.Length; i += this.Format.Channels) {
      this._fftBuffer[_fftPos].X = (float) (data[i] * FastFourierTransform.HannWindow (_fftPos, _fftLength));
      this._fftBuffer[_fftPos].Y = 0;
      this._fftPos++;

      if (this._fftPos >= this._fftLength) {
        this._fftPos = 0;

        // NAudio FFT implementation.
        FastFourierTransform.FFT (true, this._m, this._fftBuffer);

        // Copy to buffer.
        lock (this._lock) {
          for (int c = 0; c < this._fftLength; c++) {
            float amplitude = (float) Math.Sqrt (this._fftBuffer[c].X * this._fftBuffer[c].X + this._fftBuffer[c].Y * this._fftBuffer[c].Y);
            this._lastFftBuffer[c] = amplitude;
          }

          this._fftBufferAvailable = true;
        }
      }
    }
  }

  public void Start () {
    this._capture.StartRecording ();
  }

  public void Stop () {
    this._capture.StopRecording ();
  }

  public bool GetFFTData (float[] fftDataBuffer) {
    lock (this._lock) {
      // Use last available buffer.
      if (this._fftBufferAvailable) {
        this._lastFftBuffer.CopyTo (fftDataBuffer, 0);
        this._fftBufferAvailable = false;
        return true;
      } else {
        return false;
      }
    }
  }

  public int GetFFTFrequencyIndex (int frequency) {
    int index = (int) (frequency / (this.Format.SampleRate / this._fftLength / this.Format.Channels));
    return index;
  }

  public void Dispose () {
    this._capture.Dispose ();
  }
}
