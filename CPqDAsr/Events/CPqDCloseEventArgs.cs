using System;

namespace CPqDASR.Events
{
    public class CPqDCloseEventArgs : EventArgs
    {
        public int Code { get; set; }
        public string Reason { get; set; }
    }
}
