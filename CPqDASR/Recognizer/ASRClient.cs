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

using CPqDASR.Entities;
using CPqDASR.Protocol;
using CPqDASR.Config;
using CPqDASR.Events;
using CPqDASR.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CPqDAsr.Entities;
using System.IO;
using System.Net.Mime;

namespace CPqDASR.Recognizer
{
    /// <summary>
    /// The ASR Client
    /// </summary>
    public class ASRClient : Main
    {
        #region Variaveis

        /// <summary>
        /// Thread de envio de audio ao servidor ASR
        /// </summary>
        private Thread thrSendAudio;

        /// <summary>
        /// Armazena os reconhecimentos realizados
        /// </summary>
        private List<RecognitionResult> objResult;

        /// <summary>
        /// Armazena a mensagem de erro ocorrido em uma conexão
        /// </summary>
        private InternalProcessingError connectionError;

        /// <summary>
        /// 
        /// </summary>
        private bool isOpening = false;

        /// <summary>
        /// 
        /// </summary>
        private string contentType;
        
        #endregion

        #region AutoResetEvent

        /// <summary>
        /// Controla a abertura de conexão com o servidor ASR
        /// </summary>
        AutoResetEvent wtOpen = new AutoResetEvent(false);

        /// <summary>
        /// Controla o termino do reconhecimento
        /// </summary>
        AutoResetEvent wtResult = null;

        #endregion

        #region Propriedades

        /// <summary>
        /// Armazena as configurações da API realizadas pelo usuário
        /// </summary>
        protected ClientConfig ClientConfig { get; set; }

        private IAudioSource AudioSource { get; set; }

        #endregion

        #region Construtor

        /// <summary>
        /// Inicializa a classe de comunicação com o ASR
        /// </summary>
        /// <param name="objClientConfig">Objeto de configuração do servidor ASR</param>
        public ASRClient(ClientConfig objClientConfig)
            : base()
        {
            ClientConfig = objClientConfig;

            if (!ClientConfig.ConnectOnRecognize)
            {
                wtOpen.Reset();
                isOpening = true;
                Open(objClientConfig.ServerUrl, objClientConfig.Credentials);
                wtOpen.WaitOne();
                isOpening = false;
                ValidConnection();
            }
        }

        #endregion

        #region Override

        /// <summary>
        /// Quando o WebSocket é aberto, solicita a criação da Sessão
        /// </summary>
        internal override void SendOnOpen()
        {
            StartSession();
        }

        /// <summary>
        /// Quando a sessão é aberta, avisa que o ASR está aberto
        /// </summary>
        internal override void SendOnCreatedSession(Response objResponse)
        {

            //Faz a configuração definida pelo usuário na inicialização
            SetParameters(ClientConfig.RecogConfig);

            //Avisa os metodos sincronos que o ASR já está pronto para receber um inicio de fala
            wtOpen.Set();
            base.SendOnOpen();
        }

        /// <summary>
        /// Avisa quando o servidor ASR está pronto para receber mensagens
        /// </summary>
        internal override void SendOnListening()
        {
            Debug.WriteLine("Iniciando a thread de envio de audio");
            thrSendAudio = new Thread(SendAudio);
            thrSendAudio.Start();
            base.SendOnListening();
        }

        /// <summary>
        /// Servidor ASR avisa quando o audio parou de ser reconhecido
        /// </summary>
        internal override void SendOnEndOfSpeak()
        {

            base.SendOnEndOfSpeak();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objResult"></param>
        internal override void SendOnFinalResult(RecognitionResult objResult)
        {
            
            //When that is called at first time, the objResult is null and needed be initialized
            if (this.objResult == null)
            {
                this.objResult = new List<RecognitionResult>();
            }

            this.objResult.Add(objResult);

            base.SendOnFinalResult(objResult);
            
            if ((objResult?.LastSpeechSegment ?? false) && (objResult?.FinalResult ?? false))
            {
                wtResult?.Set();
            }

            if (ClientConfig.AutoClose && (objResult?.LastSpeechSegment ?? false) && (objResult?.FinalResult ?? false))
            {
                Close();
            }

        }

        internal override void SendOnClose(CPqDCloseEventArgs e)
        {
            if (isOpening)
            {
                connectionError = new InternalProcessingError(e.Reason, e.Code);
                wtOpen.Set();
            }
            else
                base.SendOnClose(e);
        }

        #endregion

        #region Metodos Publicos

        /// <summary>
        /// Prepara o servidor ASR para o inicio do recebimento de audio
        /// </summary>
        /// <param name="lModel"></param>
        public void StartRecognition(LanguageModel lModel, IAudioSource objAudioSource)
        {
            AudioSource = objAudioSource;
            objResult = null;
            wtResult = new AutoResetEvent(false);
            
            contentType = objAudioSource.getContentType();

            if (!IsConnected)
            {
                wtOpen.Reset();
                isOpening = true;
                Open(ClientConfig.ServerUrl, ClientConfig.Credentials);
                wtOpen.WaitOne();
                isOpening = false;
                ValidConnection();
            }

            base.StartRecognition(lModel, ClientConfig.RecogConfig);
        }

        /// <summary>
        /// Fica a espera do termino do reconhecimento e o retorna
        /// </summary>
        /// <returns>Objeto com os dados do reconhecimento</returns>
        public List<RecognitionResult> WaitRecognitionResult()
        {
            return WaitRecognitionResult(ClientConfig.MaxWaitSeconds);
        }

        /// <summary>
        /// Fica a espera do término do reconhecimento e o retorna
        /// </summary>
        /// <param name="timeout">Timeout for elapse wait</param>
        /// <returns>Objeto com os dados do reconhecimento</returns>
        public List<RecognitionResult> WaitRecognitionResult(double timeout)
        {
            if (wtResult != null)
            {
                //Configura o timer para o tempo de espera configurado
                System.Timers.Timer tm = new System.Timers.Timer(timeout);
                tm.Elapsed += Tm_Elapsed;
                tm.Start();

                //Aguarda o termino do reconhecimento
                wtResult.WaitOne();
                tm.Close();
                
                if (objResult == null)
                {
                    throw new RecognitionException(RecognitionErrorCode.FAILURE, "Response timeout");
                }
              
            }

            //Reset wtResult to future interations
            wtResult = null;
            RecognitionResult[] results = null;

            if (objResult != null && objResult.Count > 0)
            {
                results = new RecognitionResult[objResult.Count];
                objResult.CopyTo(results);
                objResult = null;
            }

            return results != null ? new List<RecognitionResult>(results) : new List<RecognitionResult>();
        }

        public override void CancelRecognition()
        {
            base.CancelRecognition();
            wtResult?.Set();
            wtResult = null;
            objResult = new List<RecognitionResult>();
        }

        /// <summary>
        /// Caso a resposta do ASR não ocorra no tempo configurado, é disparado o evento de Timeout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            wtResult?.Set();
        }

        /// <summary>
        /// Fecha a conexão com o servidor ASR
        /// </summary>
        public new void Close()
        {
            ReleaseSession();
            objResult = new List<RecognitionResult>();

        }

        #endregion

        #region Metodos Privados

        /// <summary>
        /// Envia o audio ao servidor ASR
        /// </summary>
        /// <param name="obj"></param>
        private void SendAudio(object obj)
        {
            try
            {
                Debug.WriteLine("Envio de audio iniciado");

                byte[] buffer = null;

                do
                {
                    buffer = AudioSource.Read();
                    if (IsListening)
                    {
#if(DEBUG)
                        Debug.WriteLine("\n\nENVIA AUDIO\n\n");
#endif
                        //Envia um array de bytes do tamanho que está no buffer
                        SendAudio(buffer, buffer.Length == 0, contentType);
                    }
                } while (IsListening && buffer.Length > 0);

                Debug.WriteLine("Envio de audio finalizado");
            }
            catch (Exception ex)
            {
                WriteLog(String.Concat("Erro nos envios de pacote: " + ex.Message));
            }
            finally
            {
                AudioSource.Close();
            }
        }

        internal override void ReleaseSession()
        {
            base.ReleaseSession();
            wtResult?.Set();
        }

        /// <summary>
        /// Fecha possíveis threads abertas indevidamente
        /// </summary>
        private void StopSendAudio()
        {
            //this.thrSendAudio.Abort();
        }

        private void ValidConnection()
        {
            if (connectionError != null)
            {
                string strMessage = connectionError.ErrorMessage;
                if (connectionError.ErrorCode == 1016)
                {
                    connectionError = null;
                    throw new IOException(strMessage);
                }
                else if (connectionError.ErrorCode == 1017)
                {
                    connectionError = null;
                    throw new NullReferenceException(strMessage);
                }
                else
                {
                    connectionError = null;
                    throw new Exception(strMessage);
                }
            }
        }
        #endregion
    }
}
