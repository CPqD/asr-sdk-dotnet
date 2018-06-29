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

using CPqDASR.Recognizer;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CPqDASR.Recognizer
{
    /// <summary>
    /// 
    /// </summary>
    public class BufferAudioSource : IAudioSource
    {
        private Queue<byte[]> buffer;

        private bool finished;
        private object locker = new object();

        public BufferAudioSource()
        {
            buffer = new Queue<byte[]>();
        }

        public BufferAudioSource(byte[] bytes) : this()
        {
            Write(bytes);
        }

        public bool Write(byte[] bytes)
        {
            if (!finished && bytes != null)
            {
                int incomingOffset = 0;

                while (incomingOffset < bytes.Length)
                {
                    lock (locker)
                    {
                        try
                        {
                            byte[] chunck = new byte[4096];
                            int length = Math.Min(chunck.Length, bytes.Length - incomingOffset);

                            Buffer.BlockCopy(bytes, incomingOffset, chunck, 0, length);
                            incomingOffset += length;

                            // Transmit outbound buffer
                            buffer.Enqueue(chunck);
                        }
                        finally
                        {
                            Monitor.PulseAll(locker);
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] Read()
        {
            byte[] bytes = null;

            if (buffer?.Count > 0 || !finished)
            {
                lock (locker)
                {
                    while (buffer?.Count == 0)
                    {
                        Monitor.Wait(locker);
                    }
                    bytes = buffer != null ? buffer.Dequeue() : new byte[0];
                }
            }
            else
            {
                bytes = new byte[0];
            }
            return bytes;
        }

        public void Close()
        {
            buffer = null;
            lock (locker)
            {
                Monitor.PulseAll(locker);
            }

        }

        public void Finish()
        {
            finished = true;

            lock (locker)
            {
                Monitor.PulseAll(locker);
            }
        }
    }
}
