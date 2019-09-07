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
    /// <summary>
    /// An Instrument is a kind of container encompassing all properties
    /// that determine the generation of composite waveforms out of notes.
    /// </summary>
    public class Instrument {
        /// <summary>
        /// The selected tuning for this instrument.
        /// </summary>
        public Tuning Tuning { get; }

        /// <summary>
        /// Attack time, in seconds.
        /// </summary>
        public double Attack { get; set; }

        /// <summary>
        /// Decay time, in secods.
        /// </summary>
        public double Decay { get; set; }

        /// <summary>
        /// The strength of the signal during the sustain phase. Value between 0 and 1.
        /// </summary>
        public double SustainLevel { get; set; }

        /// <summary>
        /// Release time in secods.
        /// </summary>
        public double Release { get; set; }

        /// <summary>
        /// The minimal length of a note, when sustain = 0. It is: attack + decay + release
        /// </summary>
        public double MinimalNoteLength { get; }

        /// <summary>
        /// This parameter is used for normalizing the output amplitude.
        /// </summary>
        private double normalizeScale = 1;

        /// <summary>
        /// A function that determines the air pressure for a specific time and frequency.
        /// </summary>
        private Func<double, double, double> generator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tuning">The selected tuning for this instrument.</param>
        /// <param name="attack">Attack time, in seconds.</param>
        /// <param name="decay">Decay time, in secods.</param>
        /// <param name="sustainLevel">The strength of the signal during the sustain phase. Value between 0 and 1.</param>
        /// <param name="release">Release time in secods.</param>
        /// <param name="generator">A function that determines the air pressure for a specific time and frequency.</param>
        public Instrument(Tuning tuning, double attack, double decay, double sustainLevel, double release,
                Func<double, double, double> generator) {

            this.Tuning = tuning;
            this.Attack = attack;
            this.Decay = decay;
            this.SustainLevel = sustainLevel;
            this.Release = release;
            this.generator = generator;
            this.MinimalNoteLength = attack + decay + release;

            analyse();
        }

        /// <summary>
        /// Since composite waveforms can get weird shapes and amplitude alterations,
        /// it simplifies things to normalize the amplitude.
        /// </summary>
        private void analyse() {
            double sampleRate = 65536d;
            double timeInterval = MinimalNoteLength + 0.5;
            int sampleCount = (int)Math.Ceiling(timeInterval * sampleRate);
            double timeStep = 1d / sampleRate;

            double sample;
            double max = double.NegativeInfinity;
            double min = double.PositiveInfinity;
            normalizeScale = 1;

            for (int i = 0; i < sampleCount; i++) {
                double time = ((double)i) * timeStep;
                sample = Play(time, 440d, 0.5);

                if (sample < min) {
                    min = sample;
                }

                if (sample > max) {
                    max = sample;
                }
            }

            normalizeScale = 1d / Math.Max(max, Math.Abs(min));
        }

        /// <summary>
        /// Get a sound sample for a specific moment.
        /// </summary>
        /// <param name="time">Number of seconds passed since the note started.</param>
        /// <param name="frequency">Frequency of the noe, in Hz</param>
        /// <param name="sustain">The sustain length for the ADSR envelope</param>
        /// <returns>Sound sample</returns>
        public double Play(double time, double frequency, double sustain) {
            return generator(time, frequency)
                * Waveform.ADSR(time, Attack, Decay, sustain, SustainLevel, Release)
                * normalizeScale;
        }
    }
}
