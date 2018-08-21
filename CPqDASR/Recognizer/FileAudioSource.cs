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
using System.IO;
using System.Linq;

namespace CPqDASR.Recognizer
{
    public class FileAudioSource : IAudioSource
    {
        private BufferAudioSource bufferAudioSource;

        internal FileAudioSource()
        {
            bufferAudioSource = new BufferAudioSource();
        }

        public FileAudioSource(string path)
            : this()
        {
            try
            {
                byte[] fileByte = File.ReadAllBytes(path);

                if (Path.GetExtension(path).ToLower().Equals(".wav"))
                {
                    byte[] headerMask = fileByte.Skip(16).Take(4).ToArray();

                    int headerSizeCode = BitConverter.ToInt32(headerMask, 0);

                    int headerSize = headerSizeCode + 28;

                    byte[] bufferHeaderless = fileByte.Skip(headerSize).ToArray();

                    bufferAudioSource.Write(bufferHeaderless);
                }
                else
                {
                    bufferAudioSource.Write(fileByte);
                }
            }
            finally
            {
                bufferAudioSource.Finish();
            }
        }

        public FileAudioSource(byte[] buffer)
            : this()
        {
            bufferAudioSource.Write(buffer);
        }

        public byte[] Read()
        {
            return bufferAudioSource.Read();
        }

        public void Close()
        {
            bufferAudioSource.Close();
        }

        public void Finish()
        {
            bufferAudioSource.Finish();
        }
    }
}
