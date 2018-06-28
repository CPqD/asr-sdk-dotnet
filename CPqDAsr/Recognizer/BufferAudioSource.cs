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
using System.Collections.Generic;


namespace CPqDAsr.Recognizer
{
    /// <summary>
    /// 
    /// </summary>
    public class BufferAudioSource : IAudioSource
    {
        private Queue<byte[]> buffer;

        private bool finished;

        public BufferAudioSource()
        {
            buffer = new Queue<byte[]>(); 
        }

        public BufferAudioSource(byte[] bytes) : this()
        {
            buffer.Enqueue(bytes);
        }

        public bool Write(byte[] bytes)
        {
            if (!finished)
            {
                buffer.Enqueue(bytes);
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] Read()
        {
            if (buffer.Count > 0)
            {
                return buffer.Dequeue();
            }

            return null;
        }

        public void Close()
        {
            buffer = null;
        }

        public void Finish()
        {
            finished = true;
            Close();
        }
    }
}
