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
    public class Alternatives
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("interpretations")]
        public List<object> Interpretations { get; set; }

        [JsonProperty("words")]
        public List<Word> Words { get; set; }

        [JsonProperty("interpretation_scores")]
        public List<int> InterpretationScores { get; set; }


        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("lm")]
        public string LanguageModel { get; set; }
    }
}
