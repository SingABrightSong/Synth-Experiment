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
    public class Tuning {
        public double BaseFrequency { get; }
        public Scale ScaleType { get; }

        /// <summary>
        /// Internal array of frequency ratios per the 12 notes.
        /// </summary>
        private double[] frequencyRatios = null;

        /// <summary>
        /// Type of the tuning.
        /// </summary>
        public enum Scale : int {
            Ptolemyc = 1,
            Chromatic_12 = 2
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scale">The type of the tuning.</param>
        public Tuning(Scale scale, double baseFrequency) {
            this.ScaleType = scale;
            this.BaseFrequency = baseFrequency;

            if (scale == Scale.Ptolemyc) {
                frequencyRatios = new double[12] { 1d, 0, 9d/8d, 0, 5d/4d, 4d/3d, 0, 3d/2d, 0, 5d/3d, 0, 15d/8d };
            } else {
                /*
                    Chromatic_12
                 */
                frequencyRatios = new double[12];
                var semi = Math.Pow(2d, 1d / 12d);

                frequencyRatios[0] = 1;
                for(int i = 1; i < 12; i++) {
                    frequencyRatios[i] = Math.Pow(semi, (double)i);
                }
            }
        }

        /// <summary>
        /// Returns the frequency of a note
        /// </summary>
        /// <param name="note">Value between 0 and 11, where 0 = Do, 2 = Re, ..., 11 = Ti</param>
        /// <returns>Frequency of a note in Hz</returns>
        public double getFrequency(int octave, int note) {
            if (note < 0 || note > 11) {
                throw(new Exception("Scale::getFrequency(): Invalid note value"));
            }

            if (octave == 0) {
                return BaseFrequency * frequencyRatios[note];
            }

            return BaseFrequency * Math.Pow(2, octave) * frequencyRatios[note];
        }
    }
}
