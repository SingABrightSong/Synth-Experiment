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

            var instrument1 = new Instrument(
                new Tuning(Tuning.Scale.Ptolemyc, 440),
                attack: 0.04, decay: 0.2, sustainLevel: 0.6, release: 0.3,
                (double time, double frequency) => Waveform.Sine(time, frequency)
            );

            var track = new Track(Track.SampleRateValue.R_44100_Hz);

            track.AddSequence(
                instrument1,
                "C0:0:0.7, D0:1:0.7, E0:2:0.7, F0:3:0.7, G0:4:0.7, A0:5:0.7, B0:6:0.7, C1:7:0.7"
            );

            track.Render(0.7, 10);
            track.SaveAsWavFile("sine_test.wav");
        }
    }
}
