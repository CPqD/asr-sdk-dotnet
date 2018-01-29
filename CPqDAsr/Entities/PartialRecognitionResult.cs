using CPqDASR;
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
