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

using Newtonsoft.Json;
using System.Collections.Generic;

namespace CPqDASR.Protocol
{
    public class Sentences
    {
        [JsonProperty("alternatives")]
        public List<Alternatives> Alternatives { get; set; }

        [JsonProperty("last_segment")]
        public bool LastSegment { get; set; }

        [JsonProperty("segment_index")]
        public int SegmentIndex { get; set; }

        [JsonProperty("final_result")]
        public bool FinalResult { get; set; }

        [JsonProperty("start_time")]
        public float StartTime { get; set; }

        [JsonProperty("end_time")]
        public float EndTime { get; set; }

        [JsonProperty("result_status")]
        public string ResultStatus { get; set; }
    }
}
