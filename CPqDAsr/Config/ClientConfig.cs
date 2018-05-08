using CPqDASR.Communication;

namespace CPqDASR.Config
{
    public class ClientConfig
    {
        /// <summary>
        /// URL padrão WebSocket. Ex: wss://speech.cpqd.com.br/asr/api2/8k
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Usuário e senha fornecidos
        /// </summary>
        public Credentials Credentials { get; set; }

        /// <summary>
        /// Objeto RecognitionConfig
        /// </summary>
        public RecognitionConfig RecogConfig { get; set; }

        /// <summary>
        /// Campo texto livre
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Número inteiro positivo, correspondente ao tempo em segundos
        /// </summary>
        public int MaxWaitSeconds { get; set; }

        /// <summary>
        /// Valor booleano. Default = false
        /// </summary>
        public bool ConnectOnRecognize { get; set; } = false;

        /// <summary>
        /// Valor booleano. Default = false
        /// </summary>
        public bool AutoClose { get; set; } = false;
    }
}
