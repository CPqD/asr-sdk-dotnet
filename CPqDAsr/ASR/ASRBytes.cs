using CPqDASR.Entities;
using CPqDASR.Protocol;
using CPqDASR.Communication;
using CPqDASR.Config;
using CPqDASR.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CPqDASR.ASR
{
    public abstract class ASRBytes : IDisposable
    {
        #region Eventos

        /// <summary>
        /// Cliente estabeleceu sessão com servidor ASR e agora o método StartRecognition pode ser chamado.
        /// </summary>
        public event Main.ReadyHandler OnOpen;

        /// <summary>
        /// Evento com os reconhecimentos parciais do audio informado (Este texto ainda pode ser alterado)
        /// </summary>
        public event Main.PartialRecognitionResultHandler OnPartialResult;

        /// <summary>
        /// Evento de termino de reconhecimento
        /// </summary>
        public event Main.RecognitionResultHandler OnFinalResult;

        /// <summary>
        /// Evento disparados quando ocorre erro no WebSocket
        /// </summary>
        public event Main.CPqDAsrErrorHandler OnError;

        /// <summary>
        /// Ocorreu um fechamento da sessão por parte do servidor ASR.
        /// </summary>
        public event Main.CloseHandler OnClose;

        /// <summary>
        /// Retorna os parametros configurados para a sessão
        /// </summary>
        public event Main.ResponseHandler OnGetParameter;

        #endregion

        #region Variaveis

        protected ASRClient objASR = null;

        #endregion

        #region Propriedades

        public string Handle
        {
            get { return objASR.Handle; }
        }

        /// <summary>
        /// Indica se o certificado SSL deve ser validado
        /// </summary>
        public bool CheckCertificateRevocation
        {
            get
            {
                return objASR.CheckCertificateRevocation;
            }
            set
            {
                objASR.CheckCertificateRevocation = value;
            }
        }

        /// <summary>
        /// Quantidade de sentenças retornadas, de 1 a 5
        /// </summary>
        public short MaxSentence
        {
            get { return objASR.MaxSentence; }
            set { objASR.MaxSentence = value; }
        }

        /// <summary>
        /// Verifica se a conexão com o servidor ASR está aberta
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return objASR.IsConnected;
            }
        }

        /// <summary>
        /// Versão da biblioteca CPqD ASR Framework corrente
        /// </summary>
        public string Version
        {
            get
            {
                return objASR.Version;
            }
        }

        #endregion

        #region Construtores

        internal ASRBytes(ClientConfig objSpeechRecognizer)
        {
            objASR = new ASRClient(objSpeechRecognizer);
            objASR.OnOpen += objASR_OnOpen;
            objASR.OnPartialResult += objASR_OnPartialResult;
            objASR.OnFinalResult += objASR_OnFinalResult;
            objASR.OnError += objASR_OnError;
            objASR.OnClose += objASR_OnClose;
            objASR.OnGetParameter += objASR_OnGetParameter;
        }

        #endregion

        #region Eventos Server ASR

        void objASR_OnOpen(object sender)
        {
            OnOpen?.Invoke(this);
        }

        void objASR_OnPartialResult(object sender, PartialRecognitionResult e)
        {
            OnPartialResult?.Invoke(this, e);
        }

        void objASR_OnFinalResult(object sender, RecognitionResult e)
        {
            OnFinalResult?.Invoke(this, e);
        }

        void objASR_OnError(object sender, CPqDErrorEventArgs e)
        {
            if (OnError != null && !(e.Method == WS_COMMANDS.SEND_AUDIO && e.Result == "INVALID_ACTION"))
                OnError(this, e);

            if (e.Method == WS_COMMANDS.SEND_AUDIO && e.Result == "INVALID_ACTION")
                Debug.WriteLine("Pacote de audio descartado, servidor já parou de receber.");
        }

        void objASR_OnClose(object sender, CPqDCloseEventArgs e)
        {
            OnClose?.Invoke(this, e);
        }

        void objASR_OnGetParameter(object sender, Response e)
        {
            OnGetParameter?.Invoke(sender, e);
        }

        #endregion

        #region Metodos Publicos

        /// <summary>
        /// Abre conexão com o servidor ASR
        /// </summary>
        /// <param name="ServerURL">Endereço do servidor ASR</param>
        /// <param name="Credentials">Credenciais para acesso ao servidor ASR</param>
        public void Open(string ServerURL, Credentials Credentials)
        {
            objASR.Open(ServerURL, Credentials);
        }

        /// <summary>
        /// Configuração dos parametros de reconhecimento para a sessão corrente
        /// </summary>
        /// <param name="Parameters">Dicionário de parametros a serem setados</param>
        public void SetParameter(RecognitionConfig objRecognitionConfig)
        {
            objASR.SetParameters(objRecognitionConfig);
        }

        /// <summary>
        /// Solicita todas as configurações dos parameros do ASR
        /// </summary>
        public void GetParameter()
        {
            GetParameter(null);
        }

        /// <summary>
        /// Solicita as configurações dos parametros passados ao ASR
        /// </summary>
        /// <param name="Parameters">Dicionário de parametros a serem setados</param>
        public void GetParameter(List<RecognitionParameters> Parameters)
        {
            objASR.GetParameters(Parameters);
        }

        /// <summary>
        /// Cancela o reconhecimento que está sendo realizado
        /// </summary>
        public void CancelRecognition()
        {
            objASR.CancelRecognition();
        }

        /// <summary>
        /// Fecha a conexão com o servidor ASR
        /// </summary>
        public void Close()
        {
            if (objASR != null)
                objASR.Close();
        }

        /// <summary>
        /// Escreve no log
        /// </summary>
        /// <param name="value"></param>
        public void WriteLog(string value)
        {
            objASR.WriteLog(value);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (objASR != null)
                objASR.Dispose();
        }

        #endregion
    }
}
