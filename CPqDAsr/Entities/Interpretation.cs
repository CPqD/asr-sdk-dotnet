namespace CPqDASR.Entities
{
    public class Interpretation
    {
        /// <summary>
        /// the interpretation confidence score.
        /// </summary>
        public int InterpretationConfidence { get; set; }

        /// <summary>
        /// a json representation of the interpretation result.
        /// </summary>
        public string InterpretationJson { get; set; }
    }
}
