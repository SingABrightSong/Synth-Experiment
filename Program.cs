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
using System.Threading;
using System.Globalization;

namespace Synth {
    public class Program {
        public static void Main() {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var tuning = new Tuning(Tuning.Scale.Ptolemaic, 280, 53);

            var lead = new Instrument(
                tuning,
                attack: 0.05, decay: 0.1, sustainLevel: 0.5, release: 0.4,
                (double time, double frequency) =>
                    (
                        3d * Waveform.Triangle(time, frequency) +
                        2d * Waveform.Triangle(time, frequency + 1.8d) +
                        0.8d * Waveform.Triangle(time, frequency * 2d + 0.8d) +
                        0.5d * Waveform.Triangle(time, frequency + 3.8d)
                    ) +
                    0.02d * Waveform.Noise()
            );

            var crystal = new Instrument(
                tuning,
                attack: 0.05, decay: 0.1, sustainLevel: 0.5, release: 0.4,
                (double time, double frequency) =>
                    (
                        3d * Waveform.SemiSine(time, frequency) +
                        2d * Waveform.SemiSine(time, frequency + 0.8d) +
                        1d * Waveform.SemiSine(time, frequency + 1.6d) +
                        0.5d * Waveform.SemiSine(time, frequency + 2.1d)
                    ) +
                    0.01d * Waveform.Noise()
            );

            var synthBass = new Instrument(
                tuning,
                attack: 0.05, decay: 0.1, sustainLevel: 0.5, release: 0.4,
                (double time, double frequency) =>
                    (
                        3d * Waveform.Triangle(time, frequency - 0.2d) +
                        2d * Waveform.Triangle(time, frequency + 0.7d) +
                        1d * Waveform.Triangle(time, frequency + 1.5d) +
                        0.5d * Waveform.Triangle(time, frequency + 2.2d)
                    ) * (frequency / tuning.BaseFrequency) +
                    (
                        3d * Waveform.Sawtooth(time, frequency - 0.2d) +
                        2d * Waveform.Sawtooth(time, frequency + 0.9d) +
                        1d * Waveform.Sawtooth(time, frequency + 1.8d) +
                        0.5d * Waveform.Sawtooth(time, frequency + 2.7d)
                    ) * (30d / frequency) +
                    0.08d * Waveform.Noise()
            );

            var track = new Track(Track.SampleRateValue.R_44100_Hz);

            var Superius = new Sequence(lead, "data/Pavane_dAngleterre.mid", filterChannel: 0, steroPan: -0.4,
                tempoChange: 0.95, lengthChange: 0.7, volumeChange: 0.8, octaveShift: 0);
            var Tenor = new Sequence(crystal, "data/Pavane_dAngleterre.mid", filterChannel: 1, steroPan: 0.4,
                tempoChange: 0.95, lengthChange: 0.7, volumeChange: 1.4, octaveShift: 0);
            var Bass = new Sequence(synthBass, "data/Pavane_dAngleterre.mid", filterChannel: 2, steroPan: 0.2,
                tempoChange: 0.95, lengthChange: 0.7, volumeChange: 2.5, octaveShift: 0);

            track.AddSequence(Superius);
            track.AddSequence(Tenor);
            track.AddSequence(Bass);

            track.Render(delay: 0.7, Superius.TotalLength);
            track.SaveAsWavFile("data/Pavane_dAngleterre.wav");
        }
    }
}
