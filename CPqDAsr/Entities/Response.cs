using CPqDASR.Extensions;
using CPqDASR.Config;

namespace CPqDASR.Entities
{
    public class Response
    {
        public string Handle { get; set; }
        public WS_COMMANDS Method { get; set; }
        public string Result { get; set; }
        public string SessionStatus { get; set; }
        public string Message { get; set; }
        public int Expires { get; set; }
        public string ErrorCode { get; set; }
        public RecognitionConfig RecognitionConfig { get; set; }

        public Response()
        {
            RecognitionConfig = new RecognitionConfig();
        }

        /// <summary>
        /// Verifica se o valor retornado foi com sucesso
        /// </summary>
        public bool IsSuccess
        {
            get
            {
                return this.Result.ToEnum<RESPONSE_RESULTS>() == RESPONSE_RESULTS.SUCCESS;
            }
        }
    }
}
