/*******************************************************************************
 * Copyright 2017 CPqD. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License.  You may obtain a copy
 * of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
 * License for the specific language governing permissions and limitations under
 * the License.
 ******************************************************************************/

namespace CPqDASR.Config
{
    public class RecognitionConfig
    {
        #region Propriedades

        #region Headers

        internal const string HeaderDecoderStartInputTimers = "decoder.startInputTimers";

        internal const string HeaderDecoderMaxSentences = "decoder.maxSentences";

        internal const string HeaderHeadMarginMilliseconds = "endpointer.headMargin";

        internal const string HeaderTailMarginMilliseconds = "endpointer.tailMargin";

        internal const string HeaderWaitEndMilliseconds = "endpointer.waitEnd";

        internal const string HeaderEndpointerLevelThreshold = "endpointer.levelThreshold";

        internal const string HeaderEndpointerAutoLevelLen = "endpointer.autoLevelLen";

        internal const string HeaderEndpointerLevelMode = "endpointer.levelMode";

        internal const string HeaderNoInputTimeoutEnabled = "noInputTimeout.enabled";

        internal const string HeaderNoInputTimeoutMilliseconds = "noInputTimeout.value";

        internal const string HeaderRecognitionTimeoutEnabled = "recognitionTimeout.enabled";

        internal const string HeaderRecognitionTimeoutMilliseconds = "recognitionTimeout.value";

        internal const string HeaderContinuousMode = "decoder.continuousMode";

        internal const string HeaderConfidenceThreshold = "decoder.confidenceThreshold";

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
        public float? EndpointerLevelThreshold { get; set; }

        /// <summary>
        /// Define se o modo de reconhecimento será continuo
        /// </summary>
        public bool? ContinuousMode { get; set; } = false;

        #endregion

        #endregion
    }
}
