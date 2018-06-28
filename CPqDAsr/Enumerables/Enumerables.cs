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

namespace CPqDASR
{
    /// <summary>
    /// Enumerador com os comandos a serem enviados ao servidor ASR
    /// </summary>
    internal enum ASR_Command
    {
        CREATE_SESSION,
        START_RECOGNITION,
        SEND_AUDIO,
        STOP_RECOGNITION,
        RELEASE_SESSION,
        GET_SESSION_STATUS,
        CANCEL_RECOGNITION,
        SET_PARAMETERS,
        GET_PARAMETERS
    }

    public enum ENCODING_TYPES
    {
        ALAW, //ainda não suportado pelo servidor
        PCM_FLOAT, //ainda não suportado pelo servidor
        PCM_SIGNED,
        PCM_UNSIGNED, //ainda não suportado pelo servidor
        ULAW, // ainda não suportado pelo servidor 
    };

    public enum CHANNEL_TYPE
    {
        MONO = 1,
        ESTEREO = 2,
    }

    /// <summary>
    /// Tipos retornados pelo servidor ASR
    /// </summary>
    internal enum WS_RESPONSES
    {
        RESPONSE,
        RECOGNITION_RESULT,
        START_OF_SPEECH,
        END_OF_SPEECH,
    };

    /// <summary>
    /// Status das respostas do servidor ASR
    /// </summary>
    internal enum SESSION_STATUS
    {
        IDLE,
        LISTENING,
        RECOGNIZING,
    };

    /// <summary>
    /// Resultado dos comandos enviados ao servidor
    /// </summary>
    internal enum RESPONSE_RESULTS
    {
        SUCCESS,
        FAILURE,
        INVALID_ACTION,
    };

    /// <summary>
    /// Comandos existentes no servidor ASR
    /// </summary>
    public enum WS_COMMANDS
    {
        CREATE_SESSION,
        START_RECOGNITION,
        SEND_AUDIO,
        STOP_RECOGNITION,
        RELEASE_SESSION,
        GET_SESSION_STATUS,
        SET_PARAMETERS,
        GET_PARAMETERS,
    };

    /// <summary>
    /// Status do Servidor ASR
    /// </summary>
    public enum RESULT_STATUS
    {
        NONE, //Nenhum resultado disponível
        PROCESSING, //Reconhecimento sendo realizado
        RECOGNIZED, //Reconhecimento já realizado
        NO_MATCH, //Reconhecimento não encontrou resultado
        NO_INPUT_TIMEOUT, //Reconhecimento não encontrou fala no audio enviado
        MAX_SPEECH, //Tempo de fala muito longo
        EARLY_SPEECH, //Fala detectada no início do áudio, sem a margem de silêncio configurada
        RECOGNITION_TIMEOUT, //Reconhecimento demorou mais tempo que o limite configurado
        NO_SPEECH, //Reconhecimento foi finalizado pelo cliente mas nenhuma fala foi detectada no áudio informado
        CANCELED, //Reconhecimento foi cancelado e resultado foi descartado
        FAILURE, //Reconhecimento falhou
    };

    /// <summary>
    /// Sample Rates da biblioteca
    /// </summary>
    public enum SAMPLE_RATE
    {
        SAMPLE_RATE_8 = 8000,
        SAMPLE_RATE_16 = 16000,
    };

    /// <summary>
    /// Enumerador para o Level Mode
    /// </summary>
    public enum LevelMode
    {
        Off = 0,
        Auto = 1,
        Fixed = 2,
    }

    /// <summary>
    /// Parametros para configuração do reconhecimento
    /// </summary>
    public enum RecognitionParameters
    {
        /// <summary>
        /// Habilita os temporizadores noInputTimeout e recognitionTimeout para todos os reconhecimentos. => Boolean (true ou false)
        /// </summary>
        DecoderStartInputTimers,

        /// <summary>
        /// Número de resultados prováveis gerados pelo reconhecimento. => Número inteiro
        /// </summary>
        DecoderMaxSentences,

        /// <summary>
        /// Trecho de silêncio inserido no início do áudio a ser entregue para o reconhecimento (antes da detecção de "início de fala") => Número inteiro (em milis)
        /// </summary>
        EndPointerHeadMargin,

        /// <summary>
        /// Trecho de silêncio inserido no início do áudio a ser entregue para o reconhecimento (antes da detecção de "início de fala") => Número inteiro (em milis)
        /// </summary>
        EndPointerTailMargin,

        /// <summary>
        /// Trecho de silêncio inserido no fim do áudio a ser entregue para o reconhecimento (depois da detecção de "fim de fala"). => Número inteiro (em milis)
        /// </summary>
        EndPointerWaitEnd,

        /// <summary>
        /// Limiar de amplitude do sinal que será compreendido como silêncio. Utilizado se levelMode = 2. => Número inteiro entre 0 e 32767
        /// </summary>
        EndPointerLevelThreshold,

        /// <summary>
        /// Tempo de áudio para o cálculo do limiar de silencia. Utilizado se levelMode = 1 => Número inteiro (em milis)
        /// </summary>
        EndPointerAutoLevelLen,

        /// <summary>
        /// Forma de cálculo do limiar de amplitude que será interpretado como silêncio: 0) Ignora a amplitude; 1) Automático; 2) Manual. => Número (0, 1 ou 2)
        /// </summary>
        EndPointerLevelMode,

        /// <summary>
        /// Habilita automaticamente o temporizador de início de fala, a partir do início do reconhecimento. Se desabilitado, o temporizador será iniciado apenas de forma manual, através da mensagem START_INPUT_TIMERS. => Boolean (true ou false)
        /// </summary>
        NoInputTimeoutEnabled,

        /// <summary>
        /// Define a temporização de início de fala. => Número inteiro (em milis) 
        /// </summary>
        NoInputTimeoutValue,

        /// <summary>
        /// Habilita o temporizador de espera pelo resultado do reconhecimento, a partir do início do reconhecimento. Se desabilitado, o temporizador será iniciado apenas de forma manual, através da mensagem START_INPUT_TIMERS. => Booleano (true ou false)
        /// </summary>
        RecognitionTimeoutEnabled,

        /// <summary>
        /// Define a temporização de espera pelo resultado do reconhecimento. => Numero inteiro (em milis).
        /// </summary>
        RecognitionTimeoutValue,

        ConfidenceThreshold
    }

    public enum RecognitionErrorCode
    {
        SESSION_TIMEOUT,
        CONNECTION_FAILURE,
        FAILURE,
    }

    /// <summary>
    /// Indicates the result of the speech recognition process.
    /// </summary>
    public enum RecognitionResultCode
    {
        RECOGNIZED,
        NO_MATCH,
        NO_INPUT_TIMEOUT,
        MAX_SPEECH,
        NO_SPEECH,
        EARLY_SPEECH,
        RECOGNITION_TIMEOUT,
        FAILURE,
    }
}
