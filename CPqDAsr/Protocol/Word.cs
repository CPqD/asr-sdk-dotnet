using Newtonsoft.Json;

namespace CPqDASR.Protocol
{
    public class Word
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("start_time")]
        public float StartTime { get; set; }

        [JsonProperty("end_time")]
        public float EndTime { get; set; }
    }
}
