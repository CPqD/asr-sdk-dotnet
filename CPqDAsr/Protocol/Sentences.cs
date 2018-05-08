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
