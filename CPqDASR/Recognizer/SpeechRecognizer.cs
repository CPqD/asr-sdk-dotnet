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

using CPqDAsr.Recognizer;
using CPqDAsr.Entities;
using CPqDASR.Config;
using CPqDASR.Entities;
using System;
using System.Collections.Generic;

namespace CPqDASR.Recognizer
{
    /// <summary>
    /// The SpeechRecognizer.
    /// </summary>
    public class SpeechRecognizer : IDisposable
    {
        #region Delegates

        public delegate void SpeechHandler(int Time);
        public delegate void RecognitionResultHandler(RecognitionResult Result);
        public delegate void PartialRecognitionResultHandler(PartialRecognitionResult Result);
        public delegate void ListeningHandler();
        public delegate void ErrorHandler();

        #endregion

        #region Eventos

        public event SpeechHandler OnSpeechStart;
        public event SpeechHandler OnSpeechStop;
        public event RecognitionResultHandler OnRecognitionResult;
        public event PartialRecognitionResultHandler OnPartialRecognitionResult;
        public event ListeningHandler OnListening;
        public event ErrorHandler OnError;

        #endregion

        #region Propriedades

        private ASRClient Client { get; set; }

        /// <summary>
        /// Flag indicating that was disposed
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Construtores
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objClientConfig">Setup of speech recognizer</param>
        internal SpeechRecognizer(ClientConfig objClientConfig)
        {
            Client = new ASRClient(objClientConfig);

            Client.OnStartOfSpeak += ObjASR_OnStartOfSpeak;
            Client.OnEndOfSpeak += ObjASR_OnEndOfSpeak;
            Client.OnListening += ObjASR_OnListening;
            Client.OnPartialResult += ObjASR_OnPartialResult;
            Client.OnFinalResult += ObjASR_OnFinalResult;
            Client.OnError += ObjASR_OnError;

        }

        #endregion

        #region Eventos

        private void ObjASR_OnStartOfSpeak(object sender)
        {
            OnSpeechStart?.Invoke(0);
        }

        private void ObjASR_OnEndOfSpeak(object sender)
        {
            OnSpeechStop?.Invoke(0);
        }

        private void ObjASR_OnListening(object sender)
        {
            OnListening?.Invoke();
        }

        private void ObjASR_OnPartialResult(object sender, PartialRecognitionResult e)
        {
            OnPartialRecognitionResult?.Invoke(e);
        }

        private void ObjASR_OnFinalResult(object sender, RecognitionResult e)
        {
            OnRecognitionResult?.Invoke(e);
        }

        private void ObjASR_OnError(object sender, Events.CPqDErrorEventArgs e)
        {
            OnError?.Invoke();
        }

        #endregion

        #region Metodos Staticos
        /// <summary>
        /// Factory that create instances of SpeechRecognizer
        /// </summary>
        /// <param name="objClientConfig"></param>
        /// <returns></returns>
        public static SpeechRecognizer Create(ClientConfig objClientConfig)
        {
            return new SpeechRecognizer(objClientConfig);
        }

        #endregion

        #region Metodos Publicos
        /// <summary>
        /// Recognizes an audio source.The recognition session with the server must
        /// be created previously.The recognition result will be notified in the
        /// registered AsrListener callbacks.The audio source is automatically
        /// closed after the end of the recognition process.
        /// </summary>
        /// <param name="objAudioSource">Object that implements IAudioSource</param>
        /// <param name="languageModel">Language model list</param>
        public void Recognize(IAudioSource objAudioSource, LanguageModelList languageModel)
        {
            if (!isDisposed)
            {
                LanguageModel lModel = new LanguageModel();

                if (languageModel?.UrlList.Count > 0)
                {
                    lModel.Uri = languageModel.UrlList[0];
                }
                else if (languageModel?.GrammarList.Count > 0)
                {
                    string[] grammar = languageModel.GrammarList[0];
                    lModel.Id = grammar[0];
                    lModel.Definition = grammar[1];
                }

                Client.StartRecognition(lModel, objAudioSource);
            }
            else
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        /// <summary>
        /// Cancel recognition
        /// </summary>
        public void CancelRecognition()
        {
            if (!isDisposed)
            {
                if (Client.IsConnected)
                {
                    Client.CancelRecognition();
                }
            }
            else
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        /// <summary>
        /// Returns the recognition result. If audio packets are still being sent to
        /// the server, the method blocks and waits for the end of the recognition
        /// process.
        /// </summary>
        /// <returns>the recognition result or null if there is no result available.</returns>
        public List<RecognitionResult> WaitRecognitionResult()
        {
            if (!isDisposed)
            {
                return Client.WaitRecognitionResult();
            }
            else
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

        }

        /// <summary>
        /// Returns the recognition result. If audio packets are still being sent to
        /// the server, the method blocks and waits for the end of the recognition
        /// </summary>
        /// <param name="seconds">the max wait time for a recognition result (in seconds). The timer 
        /// is started after the last audio packet is sent.</param>
        /// <returns>the recognition result or null if there is no result available.</returns>
        public List<RecognitionResult> WaitRecognitionResult(int seconds)
        {
            if (!isDisposed)
            {
                return Client.WaitRecognitionResult(seconds);
            }
            else
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

        }

        public void Close()
        {
            Client.Close();
        }

        public void Dispose()
        {
            Close();
            Client = null;
            isDisposed = true;
        }

        #endregion
    }
}