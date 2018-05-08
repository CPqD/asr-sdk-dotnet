using CPqDAsr.ASR;
using CPqDAsr.Entities;
using CPqDASR.Config;
using CPqDASR.Entities;
using System;
using System.Collections.Generic;

namespace CPqDASR.ASR
{
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

        public static SpeechRecognizer Create(ClientConfig objClientConfig)
        {
            return new SpeechRecognizer(objClientConfig);
        }

        #endregion

        #region Metodos Publicos

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
