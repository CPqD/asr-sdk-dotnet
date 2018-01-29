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
