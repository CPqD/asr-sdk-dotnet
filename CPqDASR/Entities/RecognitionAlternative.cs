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

using CPqDAsr.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CPqDASR.Entities
{
    public class RecognitionAlternative
    {
        /** the languagem model considered in the recognition. */
        [JsonProperty("lm")]
        public string LanguageModel { get; set; }

        /** the recognized text. */
        [JsonProperty("text")]
        public string Text { get; set; }

        /** the recognition confidence score. */
        [JsonProperty("score")]
        public int Confidence { get; set; }

        /** the interpretations result. */
        public List<Interpretation> Interpretations { get; set; } = new List<Interpretation>();

        /** the word confidence score. */
        public List<Word> Words { get; set; } = new List<Word>();
    }
}
