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
using System.Text.RegularExpressions;

namespace Synth {
    public class Sequence {
        public Instrument Instrument { get; }
        public List<Note> Notes { get; } = new List<Note>();

        private Regex noteRegEx = new Regex(@"([a-g]{1}[\#b]?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Regex octaveRegEx = new Regex(@"(\-?[0-9]+$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instrument">Defines the sound and the ADSR envelope.</param>
        /// <param name="notes">Example: "C0:0:0.5, C1:0.5:0.5, D#-1:1:0.5, Gb0:1.5:0.5"</param>
        public Sequence(Instrument instrument, string notes) {
            this.Instrument = instrument;
            var parts = notes.Split(',');
            var lookup = new Dictionary<string, int> {
                {"c", 0}, {"c#", 1}, {"db", 1}, {"d", 2}, {"d#", 3}, {"eb", 3}, {"e", 4}, {"fb", 4}, {"e#", 5}, {"f", 5},
                {"f#", 6}, {"gb", 6}, {"g", 7}, {"g#", 8}, {"ab", 8}, {"a", 9}, {"a#", 10}, {"bb", 10}, {"b", 11}, {"cb", 11}
            };

            foreach(var part in parts) {
                if (part.Trim() == "") {
                    continue;
                }
                
                var props = part.Trim().Split(':');

                if (props.Length != 3) {
                    throw(new Exception($"Invalid note description: {part.Trim()}"));
                }

                var matches = noteRegEx.Matches(props[0]);
                if (matches.Count != 1) {
                    throw(new Exception($"Invalid note description: {part.Trim()}"));
                }

                var noteStr = matches[0].ToString().ToLower();
                if (!lookup.ContainsKey(noteStr)) {
                    throw(new Exception($"Invalid note description: {part.Trim()}"));
                }

                var note = lookup[noteStr];

                matches = octaveRegEx.Matches(props[0]);
                if (matches.Count != 1) {
                    throw(new Exception($"Invalid note description: {part.Trim()}"));
                }

                try {
                    var octave = Int16.Parse(matches[0].ToString());
                    var time = Double.Parse(props[1]);
                    var length = Double.Parse(props[2]);

                    Notes.Add(new Note(instrument.Tuning.getFrequency(octave, note), time, length));
                } catch (Exception) {
                    throw(new Exception($"Invalid note description: {part.Trim()}"));
                }
            }
        }
    }
}
