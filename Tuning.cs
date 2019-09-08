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
        public int BaseNote { get; }

        /// <summary>
        /// Internal array of frequency ratios per the 12 notes.
        /// </summary>
        private double[] frequencyRatios = null;

        /// <summary>
        /// Type of the tuning.
        /// </summary>
        public enum Scale : int {
            Ptolemaic = 1,
            Chromatic_12 = 2
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scale">Defines frequency ratios between the notes</param>
        /// <param name="baseFrequency">E.g.null 440 Hz</param>
        /// <param name="baseNote">MIDI note number. This is the note which has the 'baseFrequency'</param>
        public Tuning(Scale scale, double baseFrequency, int baseNote) {
            this.ScaleType = scale;
            this.BaseNote = baseNote;
            this.BaseFrequency = baseFrequency;

            if (scale == Scale.Ptolemaic) {
                frequencyRatios = new double[12] { 1d, 25d/12d, 9d/8d, 0, 5d/4d, 4d/3d, 0, 3d/2d, 25d/16d, 5d/3d, 0, 15d/8d };
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
        /// <param name="note">MIDI note number</param>
        /// <param name="octaveShift">Shift the octave. 0 = no change.</param>
        /// <returns>Frequency of a note in Hz</returns>
        public double getFrequency(int midiNoteNumber, int octaveShift) {
            int note = (midiNoteNumber - BaseNote) % 12;
            int octave = (midiNoteNumber - BaseNote) / 12 + octaveShift;

            if (note < 0) {
                note = 12 + note;
                octave--;
            }

            if (octave == 0) {
                return BaseFrequency * frequencyRatios[note];
            }

            return BaseFrequency * Math.Pow(2, octave) * frequencyRatios[note];
        }
    }
}
