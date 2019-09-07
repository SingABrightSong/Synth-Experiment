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

namespace Synth {
    public class Instrument {
        public Tuning Tuning { get; }
        public double Attack { get; set; }
        public double Decay { get; set; }
        public double SustainLevel { get; set; }
        public double Release { get; set; }
        public double FullExtension { get; }
        private double normalizeTranspose = 0;
        private double normalizeScale = 1;

        private Func<double, double, double> generator;

        public Instrument(Tuning tuning, double attack, double decay, double sustainLevel, double release,
                Func<double, double, double> generator) {

            this.Tuning = tuning;
            this.Attack = attack;
            this.Decay = decay;
            this.SustainLevel = sustainLevel;
            this.Release = release;
            this.generator = generator;
            this.FullExtension = attack + decay + release;

            analyse();
        }

        private void analyse() {
            
        }

        public double Play(double time, double frequency, double sustain) {
            return normalizeTranspose
                + generator(time, frequency) * Waveform.ADSR(time, Attack, Decay, sustain, SustainLevel, Release)
                * normalizeScale;
        }
    }
}
