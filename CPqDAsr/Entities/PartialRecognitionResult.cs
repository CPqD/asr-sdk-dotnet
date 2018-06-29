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

using CPqDASR.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CPqDASR.Entities
{
    public class PartialRecognitionResult
    {
        internal RecognitionResultCode? ResultCode { get; set; }
        internal string Handle { get; set; }

        /// <summary>
        /// the speech segment index when operating in continuous mode.
        /// </summary>
        [JsonProperty("segment_index")]
        public int SpeechSegmentIndex { get; set; }

        /// <summary>
        /// the recognized text.
        /// </summary>
        public String Text { get; set; }

        [JsonProperty("alternatives")]
        private List<Alternatives> AltervativeJson
        {
            set
            {
                if (value != null && value.Count > 0)
                    Text = value[0].Text;
            }
        }
    }
}
