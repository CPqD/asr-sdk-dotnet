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

namespace CPqDASR.Recognizer
{
    /// <summary>
    /// Represents an audio input source for the recognition process.
    /// </summary>
    public interface IAudioSource
    {
        /// <summary>
        /// Reads data from the source into an array of bytes. The number of bytes
        /// actually read is returned as an integer.The method blocks until at least
        /// 1 byte of input is available, end of the stream has been detected
        /// </summary>
        /// <returns>the total number of bytes read into the buffer</returns>
        byte[] Read();

        /// <summary>
        /// Closes the source and releases any system resources associated.
        /// </summary>
        void Close();

        /// <summary>
        /// Informs that the audio is finished. Forces any buffered output bytes to
        /// be written out.
        /// </summary>
        void Finish();


        string getContentType();
    }
}
