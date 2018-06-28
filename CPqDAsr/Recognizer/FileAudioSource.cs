/*******************************************************************************
 * Copyright 2017 CPqD. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License.  You may obtain a copy
 * of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
 * License for the specific language governing permissions and limitations under
 * the License.
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace CPqDASR.Recognizer
{
    public class FileAudioSource : IAudioSource
    {
        private Queue<byte[]> Buffer { get; set; }

        internal FileAudioSource()
        {
            Buffer = new Queue<byte[]>();
        }

        public FileAudioSource(string Path)
            : this()
        {
            byte[] fileByte = File.ReadAllBytes(Path);

            int incomingOffset = 0;

            while (incomingOffset < fileByte.Length)
            {
                byte[] chunck = new byte[4096];

                int length = Math.Min(chunck.Length, fileByte.Length - incomingOffset);

                System.Buffer.BlockCopy(fileByte, incomingOffset, chunck, 0, length);

                incomingOffset += length;

                // Transmit outbound buffer
                Buffer.Enqueue(chunck);
            }
        }

        public FileAudioSource(byte[] Buffer)
            : this()
        {
            this.Buffer.Enqueue(Buffer);
        }

        public byte[] Read()
        {
            if (Buffer.Count > 0)
                return Buffer.Dequeue();
            return new byte[0];
        }

        public void Close()
        {
            Buffer = null;
        }

        public void Finish()
        {
            Close();
        }
    }
}
