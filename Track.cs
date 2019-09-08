/*
    Copyright 2019 Tamas Bolner
    
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
    
      http://www.apache.org/licenses/LICENSE-2.0
    
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/
using System;
using System.Collections.Generic;

namespace Synth {
    public class Track {
        /// <summary>
        /// Buffer for accumulating the sound samples.
        /// </summary>
        private double[] soundBufferLeft = null;
        private double[] soundBufferRight = null;

        /// <summary>
        /// The sample rate of the underlying buffer.
        /// </summary>
        public SampleRateValue SampleRate { get; }

        /// <summary>
        /// Container for the note sequences.
        /// </summary>
        private List<Sequence> sequences = new List<Sequence>();

        /// <summary>
        /// Supported sample rates.
        /// </summary>
        public enum SampleRateValue : int {
            R_48000_Hz = 48000,
            R_44100_Hz = 44100,
            R_22050_Hz = 22050
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sampleRate"></param>
        public Track(SampleRateValue sampleRate) {
            this.SampleRate = sampleRate;
        }

        /// <summary>
        /// Record the content of the sequnces to the internal buffer.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="length"></param>
        public void Render(double delay, double length) {
            int samples = (int)Math.Ceiling((length + delay) * (int)SampleRate);
            double r1 = 1D / (double)SampleRate;
            soundBufferLeft = new double[samples];
            soundBufferRight = new double[samples];
            
            foreach (var seq in sequences) {
                foreach(var note in seq.Notes) {
                    double start = delay + note.StartTime;
                    double noteLength = seq.Instrument.MinimalNoteLength + note.SustainLength;
                    int sampleCount = (int)Math.Ceiling(noteLength / r1);
                    int startPos = (int)(start / r1);

                    for (int i = 0; i < sampleCount; i++) {
                        if (i + startPos >= soundBufferLeft.Length) {
                            break;
                        }

                        var value = seq.Instrument.Play(i * r1, note.Frequency, note.SustainLength) * note.Velocity * seq.VolumeChange;
                        var pan = (seq.StereoPan + 1) * 0.5d;

                        soundBufferLeft[i + startPos] += value * (1d - pan);
                        soundBufferRight[i + startPos] += value * pan;
                    }
                }
            }
        }

        /// <summary>
        /// Save the content of the sound buffer as a WAV file.
        /// </summary>
        /// <param name="path">Destination file path</param>
        public void SaveAsWavFile(string path) {
            if (soundBufferLeft == null || soundBufferRight == null) {
                throw new Exception("Track.SaveAsWavFile(): Empty buffer");
            }

            Normalize(soundBufferLeft);
            Normalize(soundBufferRight);

            WavFile.saveFile(
                path,
                toIntBuffer(soundBufferLeft),
                toIntBuffer(soundBufferRight),
                SampleRate
            );
        }

        private Int16[] toIntBuffer(double[] samples) {
            var buffer = new Int16[samples.Length];

            for (int i = 0; i < samples.Length; i++) {
                buffer[i] = (Int16)(32256d * samples[i]);
            }

            return buffer;
        }

        /// <summary>
        /// Finds the maximum and minimum values of this track, to
        /// normalize its output.
        /// </summary>
        public void Normalize(double[] soundBuffer) {
            if (soundBuffer == null) {
                return;
            }

            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;

            for (int i = 0; i < soundBuffer.Length; i++) {
                if (soundBuffer[i] < min) {
                    min = soundBuffer[i];
                }

                if (soundBuffer[i] > max) {
                    max = soundBuffer[i];
                }
            }

            double scale = 0.95 / Math.Max(max, Math.Abs(min));

            for (int i = 0; i < soundBuffer.Length; i++) {
                soundBuffer[i] = soundBuffer[i] * scale;
            }
        }

        /// <summary>
        /// Add a new sequence of notes.
        /// </summary>
        /// <param name="seq">A sequence of notes.</param>
        public void AddSequence(Sequence seq) {
            sequences.Add(seq);
        }
    }
}
