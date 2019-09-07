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
using System.IO;

/// <summary>
/// Wav file specification:
/// http://soundfile.sapp.org/doc/WaveFormat/
/// </summary>
namespace Synth {
    public class WavFile {
        /// <summary>
        /// Create the binary representation of a WAV file in memory.
        /// </summary>
        /// <param name="data">Sound samples</param>
        /// <param name="sampleRate">Sample rate, e.g. 44100 Hz</param>
        /// <returns>WAV file binary content</returns>
        public static byte[] createFile(Int16[] data, Track.SampleRateValue sampleRate) {
            using (Stream stream = new MemoryStream())
            {
                writeToStream(stream, data, sampleRate);
                var buffer = new byte[stream.Length];

                stream.Read(buffer, 0, (int)stream.Length);

                return buffer;
            }
        }

        /// <summary>
        /// Save the sample data as a WAV file to the filesystem.
        /// </summary>
        /// <param name="path">Destination file path</param>
        /// <param name="data">Sound samples</param>
        /// <param name="sampleRate">Sample rate, e.g. 44100 Hz</param>
        public static void saveFile(string path, Int16[] data, Track.SampleRateValue sampleRate) {
            using(FileStream fileStream = new FileStream(path, FileMode.Create)) {
                writeToStream(fileStream, data, sampleRate);
            }
        }

        /// <summary>
        /// Output the binary representation of the WAV file to a stream.
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="data">Sound samples</param>
        /// <param name="sampleRate">Sample rate, e.g. 44100 Hz</param>
        public static void writeToStream(Stream stream, Int16[] data, Track.SampleRateValue sampleRate) {
            var subChunk2Size = (UInt32)data.Length * 2;
            var chunkSize = subChunk2Size + 36;
            var sr = (UInt32)sampleRate;
            var byteRate = sr * 2;

            UInt32[] header = new UInt32[] {
                0x46464952, chunkSize, 0x45564157, 0x20746d66, 16, 0x00010001, sr, byteRate,
                0x00100002, 0x61746164, subChunk2Size
            };

            using (BinaryWriter writer = new BinaryWriter(stream)) {
                foreach(var quad in header) {
                    writer.Write(quad);
                }

                foreach(var sample in data) {
                    writer.Write(sample);
                }
            }
        }
    }
}
