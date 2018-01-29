using System;
using System.Collections.Generic;
using System.Text;

namespace CPqDASR.Config
{
    public class RecognitionConfig
    {
        #region Propriedades

        #region Headers

        internal string HeaderDecoderStartInputTimers
        {
            get
            {
                return "decoder.startInputTimers";
            }
        }

        internal string HeaderDecoderMaxSentences
        {
            get
            {
                return "decoder.maxSentences";
            }
        }

        internal string HeaderHeadMarginMilliseconds
        {
            get
            {
                return "endpointer.headMargin";
            }
        }

        internal string HeaderTailMarginMilliseconds
        {
            get
            {
                return "endpointer.tailMargin";
            }
        }

        internal string HeaderWaitEndMilliseconds
        {
            get
            {
                return "endpointer.waitEnd";
            }
        }

        internal string HeaderEndpointerLevelThreshold
        {
            get
            {
                return "endpointer.levelThreshold";
            }
        }

        internal string HeaderEndpointerAutoLevelLen
        {
            get
            {
                return "endpointer.autoLevelLen";
            }
        }

        internal string HeaderEndpointerLevelMode
        {
            get
            {
                return "endpointer.levelMode";
            }
        }

        internal string HeaderNoInputTimeoutEnabled
        {
            get
            {
                return "noInputTimeout.enabled";
            }
        }

        internal string HeaderNoInputTimeoutMilliseconds
        {
            get
            {
                return "noInputTimeout.value";
            }
        }

        internal string HeaderRecognitionTimeoutEnabled
        {
            get
            {
                return "recognitionTimeout.enabled";
            }
        }

        internal string HeaderRecognitionTimeoutMilliseconds
        {
            get
            {
                return "recognitionTimeout.value";
            }
        }

        internal string HeaderContinuousMode
        {
            get
            {
                return "decoder.continuousMode";
            }
        }

        #endregion

        #region content

        /// <summary>
        /// Valor mínimo da pontuação (score) de confiança do reconhecimento, para que ele seja considerado válido
        /// </summary>
        public int? ConfidenceThreshold { get; set; }

        /// <summary>
        /// Número máximo de resultados prováveis (sentenças alternativas) gerados pelo reconhecimento
        /// </summary>
        public int? MaxSentences { get; set; }
        
        /// <summary>
        /// Tempo de espera pelo início de fala, após a solicitação de reconhecimento
        /// </summary>
        public int? NoInputTimeoutMilliseconds { get; set; }

        /// <summary>
        /// Tempo máximo de espera pelo resultado do reconhecimento, a partir da detecção de fala
        /// </summary>
        public int? RecognitionTimeoutMilliseconds { get; set; }

        /// <summary>
        /// Inicia automaticamente os temporizadores habilitados
        /// </summary>
        public bool? StartInputTimers { get; set; }

        /// <summary>
        /// Habilita o temporizador para esperar pelo início de fala, após a solicitação de reconhecimento
        /// </summary>
        public bool? NoInputTimeoutEnabled { get; set; }

        /// <summary>
        /// Habilita o temporizador para esperar pelo resultado do reconhecimento, a partir da detecção de fala
        /// </summary>
        public bool? RecognitionTimeoutEnabled { get; set; }

        /// <summary>
        /// Tempo de silêncio que será processado no reconhecimento, antes do início da fala
        /// </summary>
        public int? HeadMarginMilliseconds { get; set; }

        /// <summary>
        /// Tempo de silêncio que será processado no reconhecimento, após o fim da fala
        /// </summary>
        public int? TailMarginMilliseconds { get; set; }

        /// <summary>
        /// Duração do silêncio dentro do áudio para a detecção do fim da fala
        /// </summary>
        public int? WaitEndMilliseconds { get; set; }

        /// <summary>
        /// Forma de cálculo do limiar de amplitude que será interpretado como silêncio: 0) Ignora a amplitude; 1) Automático; 2) Manual
        /// </summary>
        public int? EndpointerLevelMode { get; set; }

        /// <summary>
        /// Tempo de áudio para o cálculo do limiar de silêncio. Utilizado se levelMode = 1
        /// </summary>
        public int? EndpointerAutoLevelLen { get; set; }

        /// <summary>
        /// Limiar de amplitude do sinal que será compreendido como silêncio. Utilizado se levelMode = 2
        /// </summary>
        public int? EndpointerLevelThreshold { get; set; }

        /// <summary>
        /// Define se o modo de reconhecimento será continuo
        /// </summary>
        public bool? ContinuousMode { get; set; } = false;

        #endregion

        #endregion
    }
}
