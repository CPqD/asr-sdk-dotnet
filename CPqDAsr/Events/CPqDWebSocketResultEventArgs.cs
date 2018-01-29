using System;

namespace CPqDASR.Events
{
    public class CPqDWebSocketResultEventArgs : EventArgs
    {
        public bool IsBinary { get; set; }
        public bool IsPing { get; set; }
        public bool IsText { get; set; }
        public string Data { get; set; }
        public byte[] RawData { get; set; }
    }
}
