using CPqDASR;
using System;

namespace CPqDASR.Exceptions
{
    public class RecognitionException : Exception
    {
        public RecognitionErrorCode ErrorCode { get; set; }

        public RecognitionException(RecognitionErrorCode errorCode, string Message)
            : base(Message)
        {
            ErrorCode = errorCode;
        }

        public RecognitionException(RecognitionErrorCode errorCode, string Message, Exception ex)
            : base(Message, ex)
        {
            ErrorCode = errorCode;
        }
    }
}
