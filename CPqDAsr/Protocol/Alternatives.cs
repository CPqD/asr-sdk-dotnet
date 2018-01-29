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
