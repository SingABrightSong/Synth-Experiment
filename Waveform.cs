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
    public class Waveform {
        /// <summary>
        /// The higher this value the more steep the curves in ADSR become.
        /// </summary>
        private const double adsrExp = 9d;

        private static Random RND = new Random();

        /// <summary>
        /// Attack-Decay-Sustain-Release Envelope
        /// </summary>
        /// <param name="time">Time elapsed (in seconds) from the beginning of the note.</param>
        /// <param name="attack">Attack time, in seconds.</param>
        /// <param name="decay">Decay time, in secods.</param>
        /// <param name="sustain">Sustain time, in seconds.</param>
        /// <param name="sustainLevel">The strength of the signal during the sustain phase. Value between 0 and 1.</param>
        /// <param name="release">Release time in secods.</param>
        /// <returns>Returns a value between 0 and 1.</returns>
        public static double ADSR(double time, double attack, double decay, double sustain, double sustainLevel, double release) {
            double toDecay = attack + decay;
            double toSustain = toDecay + sustain;
            double toRelease = toSustain + release;

            if (time < 0d) {
                return 0d;
            } else if (time < attack) {
                return ExpIncrease(time, attack, 1d);
            } else if (time < toDecay) {
                return sustainLevel + ExpDecrease(time - attack, decay, 1d - sustainLevel);
            } else if (time < toSustain) {
                return sustainLevel;
            } else if (time < toRelease) {
                return ExpDecrease(time - toSustain, release, sustainLevel);
            }

            return 0d;
        }

        /// <summary>
        /// Exponentionally decreasing function. (Hyperbolic)
        /// </summary>
        /// <param name="time">Time between 0 - width.</param>
        /// <param name="width">The domain of the function.</param>
        /// <param name="height">Maximal value. (Minimal is 0)</param>
        /// <returns>Double precision value between 0 - height.</returns>
        private static double ExpDecrease(double time, double width, double height) {
            return (
                (adsrExp / ((adsrExp - 1d) * Math.Pow(adsrExp, time / width)))
                - (1d / adsrExp)
            ) * height;
        }

        /// <summary>
        /// Exponentionally increasing function. (Hyperbolic)
        /// </summary>
        /// <param name="time">Time between 0 - width.</param>
        /// <param name="width">The domain of the function.</param>
        /// <param name="height">Maximal value. (Minimal is 0)</param>
        /// <returns>Double precision value between 0 - height.</returns>
        private static double ExpIncrease(double time, double width, double height) {
            return (
                (1d - Math.Pow(adsrExp, - time / width))
                * (adsrExp / (adsrExp - 1d))
            ) * height;
        }

        /// <summary>
        /// Sine wave
        /// </summary>
        /// <param name="time">Time elapsed (in seconds) from the beginning of the note.</param>
        /// <param name="frequency">The wave's frequency.</param>
        /// <returns>The air pressure value (between -1 and 1) at the specified time.</returns>
        public static double Sine(double time, double frequency) {
            return Math.Sin(time * frequency * Math.PI * 2d);
        }

        public static double SemiSine(double time, double frequency) {
            return Math.Abs(Math.Sin(time * frequency * Math.PI)) * 2d - 1d;
        }

        public static double Square(double time, double frequency) {
            return ((int)(time * frequency) % 2) * 2 - 1;
        }

        public static double Sawtooth(double time, double frequency) {
            var periods = time * frequency;
            var fractional = periods - Math.Floor(periods);

            return fractional * 2 - 1;
        }

        public static double Noise() {
            return RND.NextDouble() * 2 - 1;
        }

        public static double Triangle(double time, double frequency) {
            var phase = (int)(time * frequency) % 2;
            var periods = time * frequency;
            var fractional = periods - Math.Floor(periods);

            if (phase == 0) {
                return fractional * 2 - 1;
            }

            return (1 - fractional) * 2 - 1;
        }
    }
}
