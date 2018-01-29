using CPqDASR;
using System;

namespace CPqDASR.Events
{
    public class CPqDErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string Result { get; set; }
        public WS_COMMANDS Method { get; set; }
    }
}
