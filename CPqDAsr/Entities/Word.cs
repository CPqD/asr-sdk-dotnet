using System;
using System.Collections.Generic;
using System.Text;

namespace CPqDAsr.Entities
{
    /// <summary>
    /// Represents the result of recognition result of a word, confidence and recognition time
    /// </summary>
    public class Word
    {
        
        /// <summary>
        /// Text recognized
        /// </summary>
        public string Text { get; set; }

        public int Confidence { get; set; }

        /// <summary>
        /// Start time of recognition
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        /// End time of recognition
        /// </summary>
        public float EndTime { get; set; }

    }
}
