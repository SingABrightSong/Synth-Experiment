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
using Commons.Music.Midi;

namespace Synth {
    public class Sequence {
        public Instrument Instrument { get; }
        public List<Note> Notes { get; } = new List<Note>();

        /// <summary>
        /// The total length of the note sequence in seconds.
        /// </summary>
        public double TotalLength { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instrument">Defines the sound and the ADSR envelope.</param>
        /// <param name="midiFilePath">Path to a MIDI file to read its contents.</param>
        /// <param name="filterChannel">Filter on a specific channel. Use -1 to disable filtering.</param>
        /// <param name="tempoChange">Multiplier for the tempo. 1 = unchanged</param>
        /// <param name="lengthChange">Multiplier for the note lengths. 1 = unchanged</param>
        public Sequence(Instrument instrument, string midiFilePath, int filterChannel, double tempoChange, double lengthChange) {
            this.Instrument = instrument;

            using(var stream = System.IO.File.OpenRead(midiFilePath)) {
                var music = MidiMusic.Read(stream);
                double timeConversion = (((double)music.GetTimePositionInMillisecondsForTick(1000)) / (1000000d)) / tempoChange;

                foreach(var track in music.Tracks) {
                    var tracker = new Dictionary<byte, Note>();
                    double currentTime = 0;

                    foreach(var message in track.Messages) {
                        var type = message.Event.EventType >> 4;

                        if (type != 8 && type != 9) {
                            continue;
                        }

                        if (filterChannel > -1) {
                            if (filterChannel != message.Event.Channel) {
                                continue;
                            }
                        }

                        if (message.Event.Channel == 10) {
                            // Automatically filter the percussion channel
                            continue;
                        }

                        currentTime += ((double)message.DeltaTime) * timeConversion;
                        
                        byte note = message.Event.Msb;
                        byte velocity = message.Event.Lsb;

                        if (type == 8 || (type == 9 && velocity == 0)) {
                            // Note OFF event

                            if (!tracker.ContainsKey(note)) {
                                // Ignore inconsistent data
                                continue;
                            }

                            var noteObj = tracker[note];
                            noteObj.SustainLength = (currentTime - noteObj.StartTime) * lengthChange;
                            Notes.Add(noteObj);
                            tracker.Remove(note);

                            var endTime = noteObj.StartTime + instrument.MinimalNoteLength + noteObj.SustainLength + 1;
                            if (endTime > TotalLength) {
                                TotalLength = endTime;
                            }

                            continue;
                        }

                        if (type == 9) {
                            // Note ON event
                            double frequency = instrument.Tuning.getFrequency(note);
                            tracker[note] = new Note(frequency, currentTime, ((double)velocity) / 127d, 0);

                            continue;
                        }
                    }
                }
            }
        }
    }
}
