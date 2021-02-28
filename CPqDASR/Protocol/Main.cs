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
using CPqDASR.Extensions;
using CPqDASR.Communication;
using CPqDASR.Config;
using CPqDASR.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CPqDAsr.Entities;

namespace CPqDASR.Protocol
{
    public abstract class Main : IDisposable
    {
        #region Delegates

        public delegate void ReadyHandler(object sender);
        public delegate void ResponseHandler(object sender, Response e);
        public delegate void CPqDAsrErrorHandler(object sender, CPqDErrorEventArgs e);
        public delegate void RecognitionResultHandler(object sender, RecognitionResult e);
        public delegate void PartialRecognitionResultHandler(object sender, PartialRecognitionResult e);
        public delegate void CloseHandler(object sender, CPqDCloseEventArgs e);

        #endregion

        #region Eventos

        /// <summary>
        /// Cliente estabeleceu sessão com servidor ASR e agora o método StartRecognition pode ser chamado.
        /// </summary>
        public event ReadyHandler OnOpen;

        /// <summary>
        /// Ocorreu um fechamento da sessão por parte do servidor ASR.
        /// </summary>
        public event CloseHandler OnClose;

        /// <summary>
        /// Servidor está pronto para começar a receber o audio
        /// </summary>
        public event ReadyHandler OnListening;

        /// <summary>
        /// Servidor começou a reconhecer fala
        /// </summary>
        public event ReadyHandler OnStartOfSpeak;

        /// <summary>
        /// Evento de aviso que o servidor interrompeu o recebimento de audio
        /// </summary>
        public event ReadyHandler OnEndOfSpeak;

        /// <summary>
        /// Evento com os reconhecimentos parciais do audio informado (Este texto ainda pode ser alterado)
        /// </summary>
        public event PartialRecognitionResultHandler OnPartialResult;

        /// <summary>
        /// Evento de termino de reconhecimento
        /// </summary>
        public event RecognitionResultHandler OnFinalResult;

        /// <summary>
        /// Evento disparados quando ocorre erro no WebSocket
        /// </summary>
        public event CPqDAsrErrorHandler OnError;

        /// <summary>
        /// Evento disparado após a sessão ter sido criada
        /// </summary>
        public event ResponseHandler OnCreatedSession;

        /// <summary>
        /// Retorna os parametros configurados para a sessão
        /// </summary>
        public event ResponseHandler OnGetParameter;

        #endregion

        #region Constantes

        /// <summary>
        /// Identificação do produto
        /// </summary>
        private const string PRODUCT = "ASR";

        /// <summary>
        /// Versão do produto utilizado
        /// </summary>
        private const string VERSION = "2.3";

        /// <summary>
        /// 
        /// </summary>
        private const string APPLICATION_SRGS = "application/srgs";

        /// <summary>
        /// 
        /// </summary>
        private const string TEXT_URI_LIST = "text/uri-list";

        #endregion

        #region Variaveis

        /// <summary>
        /// Conexões abertas com o CPqDASR
        /// </summary>
        private static int intConnectionOpen = 0;

        private WebSocketManager objWebSocket;

        //Variaveis default
        private CHANNEL_TYPE enuChannel = CHANNEL_TYPE.MONO;

        private SAMPLE_RATE enuSampleRate = SAMPLE_RATE.SAMPLE_RATE_16;

        private int shoMaxSentence = 5;
        
        /// <summary>
        /// Armazena a sessão de referencia no ASR
        /// </summary>
        private string strHandle = "";

        #endregion

        #region Propriedades Privadas

        /// <summary>
        /// Número de canais a serem utilizados (1 - Mono / 2 - Estereo)
        /// </summary>
        private CHANNEL_TYPE Channels
        {
            get
            {
                return enuChannel;
            }
            set
            {
                enuChannel = value;
            }
        }

        #endregion

        #region Propriedades Publicas

        public string Handle
        {
            get { return this.strHandle; }
        }

        /// <summary>
        /// Indica se o certificado SSL deve ser validado
        /// </summary>
        public bool CheckCertificateRevocation
        {
            get
            {
                return objWebSocket.CheckCertificateRevocation;
            }
            set
            {
                objWebSocket.CheckCertificateRevocation = value;
            }
        }

        /// <summary>
        /// Número de Sample por segundos (Default: 16)
        /// </summary>
        private SAMPLE_RATE SampleRate
        {
            get { return enuSampleRate; }
            set { enuSampleRate = value; }
        }

        /// <summary>
        /// Quantidade de sentenças retornadas, de 1 a 5
        /// </summary>
        public int MaxSentence
        {
            get { return shoMaxSentence; }
            set { shoMaxSentence = value; }
        }

        /// <summary>
        /// Verifica se a conexão com o servidor ASR está aberta
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (objWebSocket != null)
                    return objWebSocket.IsOpen;
                return false;
            }
        }

        /// <summary>
        /// Indica se o sitema está pronto para receber audio ou não
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Versão da biblioteca CPqD ASR Framework corrente
        /// </summary>
        public string Version
        {
            get
            {
                return String.Concat(PRODUCT, " ", VERSION);
            }
        }

        #endregion

        #region Construtores

        public Main()
        {
            objWebSocket = new WebSocketManager();
            objWebSocket.OnOpened += ObjWebSocket_OnOpened;
            objWebSocket.OnOpening += ObjWebSocket_OnOpening;
            objWebSocket.OnMessage += ObjWebSocket_OnMessage;
            objWebSocket.OnClosed += ObjWebSocket_OnClosed;
            objWebSocket.OnError += ObjWebSocket_OnError;
        }

        #endregion

        #region Eventos WebSocket

        void ObjWebSocket_OnOpening(WebSocketManager sender)
        {
            SendOnOpening();
        }

        private void ObjWebSocket_OnOpened(WebSocketManager sender)
        {
            intConnectionOpen += 1;
            WriteLog(string.Concat("Abriu conexão - quantidades: ", intConnectionOpen.ToString()));
            SendOnOpen();
        }

        void ObjWebSocket_OnMessage(WebSocketManager sender, CPqDWebSocketResultEventArgs e)
        {
            string result = "<None>";
            try
            {
                result = System.Text.Encoding.UTF8.GetString(e.RawData);
                WriteLog(result.Trim().Replace("\r\n", ", "));
            }
            catch (Exception ex)
            {
                WriteLog("ERROR: Retorno - " + result);
                WriteLog("ERROR: " + ex.Message);
                SendOnError("Erro na conversão das informações do servidor", "4025");
            }

            try
            {
                IdentifyResponse(result);
            }
            catch (Exception ex)
            {
                WriteLog("ERROR: Retorno - " + result);
                WriteLog("ERROR: " + ex.Message);
                SendOnError("Erro na interpretação dos dados do servidor", "4026");
            }
        }

        void ObjWebSocket_OnClosed(WebSocketManager sender, CPqDCloseEventArgs e)
        {
            intConnectionOpen -= 1;
            if (intConnectionOpen < 0)
            {
                intConnectionOpen = 0;
                WriteLog("Problema na contabilização de conexões");
            }
            WriteLog(string.Concat("Fechou conexão - quantidades: ", intConnectionOpen.ToString(), " - Motivo: ", e.Reason, " - Code: ", e.Code));
            strHandle = "";
            SendOnClose(e);
        }

        void ObjWebSocket_OnError(WebSocketManager sender, CPqDErrorEventArgs e)
        {
            SendOnError(e);
        }

        #endregion

        #region Metodos Internal

        /// <summary>
        /// Cria a sessão para acessar o ASR
        /// </summary>
        internal void StartSession()
        {
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(string.Format("{0} {1} {2}", PRODUCT, VERSION, ASR_Command.CREATE_SESSION));
            strHandle = "";

            WriteLog("Create Session");
            SendCommand(strCommand.ToString());
        }

        /// <summary>
        /// Libera a sessão no servidor ASR
        /// </summary>
        internal virtual void ReleaseSession()
        {
            WriteLog("Release Session");
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(string.Format("{0} {1} {2}", PRODUCT, VERSION, ASR_Command.RELEASE_SESSION));
            SendCommand(strCommand.ToString());
            objWebSocket?.Close();
        }

        /// <summary>
        /// Envia trecho de audio para o servidor seguindo protocolo
        /// </summary>
        /// <param name="audio">Conteudo do audio</param>
        /// <param name="lastPackage">Indica se está sendo enviado o último pacote para o servidor</param>
        internal void SendAudio(byte[] audio, bool bolLastPacket,string contentType)
        {
            try
            {
                StringBuilder strCommand = new StringBuilder();
                strCommand.AppendLine(string.Format("{0} {1} {2}", PRODUCT, VERSION, ASR_Command.SEND_AUDIO));
                strCommand.AppendLine(string.Concat("LastPacket: ", bolLastPacket));
                strCommand.AppendLine(string.Concat("Content-Length: ", audio.Length));
                strCommand.AppendLine(string.Concat("Content-Type: ", contentType));
                strCommand.AppendLine();

                WriteLog(strCommand.ToString().Trim().Replace("\r\n", ", "));

                var bytes = System.Text.Encoding.UTF8.GetBytes(strCommand.ToString());
                byte[] rv = new byte[bytes.Length + audio.Length];
                System.Buffer.BlockCopy(bytes, 0, rv, 0, bytes.Length);
                System.Buffer.BlockCopy(audio, 0, rv, bytes.Length, audio.Length);

                try
                {
                    if (IsListening && objWebSocket.IsOpen)
                    {
                        if (bolLastPacket)
                            IsListening = false;
                        WriteLog("Enviando dados ao WebSocket");
                        objWebSocket.Send(rv);
                        WriteLog("Dados enviados ao WebSocket");

#if (DEBUG)
                        Debug.WriteLine(strCommand.ToString());
#endif
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("Não foi possivel enviar o pacote: " + ex.Message);
                    //throw ex;
                }
            }
            catch (ThreadAbortException)
            {
                WriteLog("Thread de envio de audio foi finalizada");
            }
            catch (Exception ex)
            {
                WriteLog("Não foi possivel Montar o pacote" + ex.Message);
                throw;
            }
        }

        #endregion

        #region Metodos Publicos

        /// <summary>
        /// Abre a conexão com o WebSocket do servidor ASR
        /// </summary>
        /// <param name="ServerUrl">Endreço de acesso do servidor ASR</param>
        /// <param name="Credentials">Credenciais para acesso ao servidor ASR</param>
        public void Open(string ServerUrl, Credentials Credentials)
        {
            if (objWebSocket.IsOpen)
            {
                Trace.WriteLine("Conexão será encerrada com o servidor ASR");
                //Caso a conexão de websocket esteja aberta, a mesma é encerrada
                ReleaseSession();
            }
            Trace.WriteLine("Conexão com o servidor ASR está sendo aberta");
            //Efetua a conexão do websocket com o servidor ASR
            objWebSocket.Open(ServerUrl, Credentials);
        }

        /// <summary>
        /// Informa o servidor ASR de que se deseja realizar o reconhecimento de um buffer
        /// </summary>
        /// <param name="lModel">Modelo de linguagem a ser utilizado</param>
        /// <param name="objRecognitionConfig">Parametro a serem setados apenas para este reconhecimento</param>
        /// <param name="dicParameters">Parametro a serem setados apenas para este reconhecimento</param>
        public void StartRecognition(LanguageModel lModel, RecognitionConfig objRecognitionConfig)
        {
            WriteLog(string.Concat("Thread Id: ", Thread.CurrentThread.ManagedThreadId));
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(string.Format("{0} {1} {2}", PRODUCT, VERSION, ASR_Command.START_RECOGNITION));

            //Insere os valores dos parametros configurados
            if (objRecognitionConfig != null)
            {
                strCommand.AppendLine(GetCommand(objRecognitionConfig).TrimEnd('\r','\n'));
            }

            if (lModel?.Uri != null)
            {
                strCommand.AppendLine(string.Concat("Content-Length: ", lModel.Uri.Length + 8));
                strCommand.AppendLine("Content-Type: " + TEXT_URI_LIST);
                strCommand.AppendLine();
                strCommand.AppendLine(lModel.Uri);
            }
            else if (lModel?.Definition != null)
            {
                strCommand.AppendLine("Content-ID: " + lModel.Id);
                strCommand.AppendLine(string.Concat("Content-Length: ", lModel.Definition.Length + 8));
                strCommand.AppendLine("Content-Type: " + APPLICATION_SRGS);
                strCommand.AppendLine();
                strCommand.AppendLine(lModel.Definition);
            }

            WriteLog("Start Recognition");
            SendCommand(strCommand.ToString());
        }

        /// <summary>
        /// Cancela o reconhecimento que está sendo realizado
        /// </summary>
        public virtual void CancelRecognition()
        {
            WriteLog("Cancel Recognition");
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(string.Format("{0} {1} {2}", PRODUCT, VERSION, ASR_Command.CANCEL_RECOGNITION));
            SendCommand(strCommand.ToString());
        }

        /// <summary>
        /// Configuração dos parametros de reconhecimento para a sessão corrente
        /// </summary>
        /// <param name="Parameters">Dicionário de parametros a serem setados</param>
        public void SetParameters(RecognitionConfig objRecognitionConfig)
        {
            if (objRecognitionConfig != null)
            {
                StringBuilder strCommand = new StringBuilder();
                strCommand.AppendLine(string.Format("{0} {1} {2}", PRODUCT, VERSION, ASR_Command.SET_PARAMETERS));

                strCommand.AppendLine(GetCommand(objRecognitionConfig));

                WriteLog("Set Parameters");
                SendCommand(strCommand.ToString());
            }
        }

        /// <summary>
        /// Solicita todas as configurações dos parameros do ASR
        /// </summary>
        public void GetParameters()
        {
            GetParameters(null);
        }

        /// <summary>
        /// Solicita as configurações dos parametros passados ao ASR
        /// </summary>
        /// <param name="Parameters">Dicionário de parametros a serem setados</param>
        public void GetParameters(List<RecognitionParameters> Parameters)
        {
            StringBuilder strCommand = new StringBuilder();
            strCommand.AppendLine(string.Format("{0} {1} {2}", PRODUCT, VERSION, ASR_Command.GET_PARAMETERS));

            if (Parameters != null)
                foreach (RecognitionParameters item in Parameters)
                    strCommand.AppendLine(string.Format("{0}:", ConvertRecognitionParameter(item)));

            WriteLog("Get Parameters");
            SendCommand(strCommand.ToString());
        }

        #endregion

        #region Metodos Privados

        /// <summary>
        /// Envia mensagens para o WebSocket
        /// </summary>
        /// <param name="strValue"></param>
        private void SendCommand(string strValue)
        {
            var a = Encoding.UTF8.GetBytes(strValue);
            if (objWebSocket != null && objWebSocket.IsOpen)
            {
                //WriteLog(strValue);
                objWebSocket.Send(a);
                //WriteLog("Comando enviado");
            }
        }

        /// <summary>
        /// Identifica uma resposta do servidor seguindo especificação do protocolo de comunicação estabelecido
        /// </summary>
        /// <param name="strValue"></param>
        private void IdentifyResponse(string strValue)
        {
#if (DEBUG)
            Debug.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Debug.WriteLine(strValue);
            Debug.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
#endif
            try
            {
                strValue = strValue.Replace("\r\n", "\r");
                var arrValues = strValue.Split('\r');
                var strResponse = GetResponse(arrValues);
                IsListening = arrValues.First(p => p.IndexOf("Session-Status: ") != -1).Replace("Session-Status: ", "") == "LISTENING";
#if (DEBUG)
                Debug.WriteLine("IsListening: " + IsListening);
#endif
                switch (strResponse.ToEnum<WS_RESPONSES>())
                {
                    case WS_RESPONSES.RESPONSE: //respostas gerais do servidor
                        ValidateResponse(arrValues);
                        break;
                    case WS_RESPONSES.RECOGNITION_RESULT: //resposta de reconhecimento de fala
                        ValidateRecognitionResult(arrValues);
                        break;
                    case WS_RESPONSES.START_OF_SPEECH: //servidor apontando que iniciou o reconhecimento de voz do audio enviado
                        ValidateStartOfSpeech(arrValues);
                        break;
                    case WS_RESPONSES.END_OF_SPEECH: //resposta para apontar que foi identificado silencio e que portanto parou-se de realizar o reconhecimento
                        ValidateEndOfSpeech(arrValues);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Despreza o cabeçalho presente nas mensagens vindas do servidor e retorna o restante
        /// </summary>
        /// <param name="arrValues"></param>
        /// <returns></returns>
        private string GetResponse(string[] arrValues)
        {
            string value = arrValues[0];
            return value.Substring(value.LastIndexOf(" ") + 1);
        }

        /// <summary>
        /// trata um objeto Response vindo do servidor
        /// </summary>
        /// <param name="arrValues"></param>
        private void ValidateResponse(string[] arrValues)
        {
            var objResponse = PrepareResponse(arrValues);
            if (objResponse.IsSuccess)
            {
                switch (objResponse.Method)
                {
                    case WS_COMMANDS.CREATE_SESSION:
                        strHandle = objResponse.Handle;
                        SendOnCreatedSession(objResponse);
                        break;
                    case WS_COMMANDS.START_RECOGNITION:
                        SendOnListening();
                        break;
                    case WS_COMMANDS.STOP_RECOGNITION:
                    case WS_COMMANDS.RELEASE_SESSION:
                        strHandle = "";
                        Debug.WriteLine("Recebeu pedido de Release Session");
                        break;
                    case WS_COMMANDS.SEND_AUDIO:
                    case WS_COMMANDS.GET_SESSION_STATUS:
                        break;
                    case WS_COMMANDS.SET_PARAMETERS:
                    case WS_COMMANDS.GET_PARAMETERS:
                        SendOnGetParameters(objResponse);
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Resposta não esperada");
                if (objResponse != null && objResponse.Message != String.Empty)
                {
                    SendOnError(objResponse);
                }
                else
                    SendOnError("It wasn't possible to recognize the sent data", "4022");
            }
        }

        /// <summary>
        /// Trata um objeto RecognitionResult vindo do servidor
        /// </summary>
        /// <param name="arrValue"></param>
        private void ValidateRecognitionResult(string[] arrValue)
        {
            if (GetSessionStatus(arrValue) == SESSION_STATUS.IDLE || IsFinalResult(arrValue))
            {
                var objRecognitionResult = PrepareRecognitionResult(arrValue);
                SendOnFinalResult(objRecognitionResult);
            }
            else
            {
                var objPartialRecognitionResult = PreparePartialRecognitionResult(arrValue);
                SendOnPartialResult(objPartialRecognitionResult);
            }
        }

        /// <summary>
        /// Trata um objeto validateStartOfSpeech vindo do servidor
        /// </summary>
        /// <param name="arrValue"></param>
        private void ValidateStartOfSpeech(string[] arrValue)
        {
            SendOnStartOfSpeak();
        }

        /// <summary>
        /// Trata um objeto validateEndOfSpeech vindo do servidor
        /// </summary>
        /// <param name="arrValue"></param>
        private void ValidateEndOfSpeech(string[] arrValue)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Parou de receber audio");
                //É necessário chamar este item, para validar as informações que chegou
                OfSpeech objOfSpeech = PrepareOfSpeech(arrValue);
                SendOnEndOfSpeak();
            }
            catch (Exception)
            {
                SendOnError("Incompatible ASR server state", "4024");
            }
        }

        public void WriteLog(string strValue)
        {
            string str = String.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.ffff"), " Handle: ", strHandle, " ", strValue);
            Trace.WriteLine(str);
#if (DEBUG)
            Debug.WriteLine(str);
#endif
        }

        #region Prepara informações recebidas em objetos

        /// <summary>
        /// Converte o array fornecido em um objeto to Tipo Response, que é um dos tipos possíveis de reposta do servidor descrita pelo Protocolo
        /// </summary>
        /// <param name="arrValues"></param>
        private Response PrepareResponse(string[] arrValues)
        {
            Response objResponse = new Response();
            RecognitionConfig objRecognitionConfig = new RecognitionConfig();
            foreach (string value in arrValues)
            {
                if (value.IndexOf(string.Format("{0} {1}", PRODUCT, VERSION)) == -1)
                {
                    if (value.IndexOf("Result: ") != -1)
                        objResponse.Result = value.Replace("Result: ", "");
                    else if (value.IndexOf("Handle: ") != -1)
                        objResponse.Handle = value.Replace("Handle: ", "");
                    else if (value.IndexOf("Session-Status: ") != -1)
                        objResponse.SessionStatus = value.Replace("Session-Status: ", "");
                    else if (value.IndexOf("Method: ") != -1)
                        objResponse.Method = value.Replace("Method: ", "").ToEnum<WS_COMMANDS>();
                    else if (value.IndexOf("Expires: ") != -1)
                        objResponse.Expires = int.Parse(value.Replace("Expires: ", "").Trim());
                    else if (value.IndexOf("Message: ") != -1)
                        objResponse.Message = value.Replace("Message: ", "");
                    else if (value.IndexOf("Error-Code: ") != -1)
                        objResponse.ErrorCode = value.Replace("Error-Code: ", "");
                    else
                    {
                        var arr = value.Split(':');
                        if (arr.Length == 2)
                        {
                            SetParametersSettings(ref objRecognitionConfig, arr[0], arr[1]);
                        }
                    }
                }
            }
            objResponse.RecognitionConfig = objRecognitionConfig;

            return objResponse;
        }

        /// <summary>
        /// Method that get session status of WebSocket Response
        /// </summary>
        /// <param name="arrValues">Web Socket Response</param>
        /// <returns>Session status</returns>
        private SESSION_STATUS? GetSessionStatus(string[] arrValues)
        {
            foreach (string value in arrValues)
            {
                if (value.IndexOf("Session-Status: ") != -1)
                { //especificamente no retorno de resultado de reconhecimento, o status do servidor vem na tag RESULT e não na tag STATUS
                    return value.Replace("Session-Status: ", "").ToEnum<SESSION_STATUS>();
                }
            };
            return null;
        }

        /// <summary>
        /// Method that check if the segment received is a final result
        /// </summary>
        /// <param name="values">Segment received</param>
        /// <returns>Flag indicating if that segment is a final result</returns>
        private bool IsFinalResult(string[] values)
        {
            Dictionary<string, object> json =
                JsonConvert.DeserializeObject<Dictionary<string, object>>((
                from s in values
                where s.IndexOf("{") != -1
                select s).FirstOrDefault());

            bool isFinalResult = (((bool?)json?["final_result"]) ?? false);

            return isFinalResult;
        }

        /// <summary>
        /// Converte o array fornecido em um objeto do Tipo PartialRecognitionResult, que é um dos tipos possíveis de reposta do servidor descrita pelo Protocolo
        /// </summary>
        /// <param name="arrValue"></param>
        /// <returns></returns>
        private PartialRecognitionResult PreparePartialRecognitionResult(string[] arrValues)
        {
            PartialRecognitionResult objPartialRecognitionResult = new PartialRecognitionResult();
            RecognitionResultCode? ResultCode = null;
            string Handle = "";

            foreach (string value in arrValues)
            {
                if (value.IndexOf("Result-Status: ") != -1 && (value.IndexOf("PROCESSING") == -1))
                { //especificamente no retorno de resultado de reconhecimento, o status do servidor vem na tag RESULT e não na tag STATUS
                    ResultCode = value.Replace("Result-Status: ", "").ToEnum<RecognitionResultCode>();
                }
                if (value.IndexOf("Handle: ") != -1)
                { //especificamente no retorno de resultado de reconhecimento, o status do servidor vem na tag RESULT e não na tag STATUS
                    Handle = value.Replace("Handle: ", "").ToString();
                }
                if (value.IndexOf("[{") != -1)
                { //json contendo as sentencas identificadas
                    objPartialRecognitionResult = JsonConvert.DeserializeObject<PartialRecognitionResult>(value);
                }
            };

            if (objPartialRecognitionResult != null)
            {
                objPartialRecognitionResult.Handle = Handle;
                objPartialRecognitionResult.ResultCode = ResultCode;
            }

            return objPartialRecognitionResult;
        }

        /// <summary>
        /// Converte o array fornecido em um objeto do Tipo RecognitionResult, que é um dos tipos possíveis de reposta do servidor descrita pelo Protocolo
        /// </summary>
        /// <param name="arrValues"></param>
        /// <returns></returns>
        private RecognitionResult PrepareRecognitionResult(string[] arrValues)
        {
            RecognitionResult objRecognitionResult = null;
            RecognitionResultCode? ResultCode = null;
            string Handle = "";
            foreach (string value in arrValues)
            {
                if (value.IndexOf("Result-Status: ") != -1 && (value.IndexOf("PROCESSING") == -1))
                {
                    //especificamente no retorno de resultado de reconhecimento, o status do servidor vem na tag RESULT e não na tag STATUS
                    ResultCode = value.Replace("Result-Status: ", "").ToEnum<RecognitionResultCode>();
                }
                if (value.IndexOf("Handle: ") != -1)
                {
                    //especificamente no retorno de resultado de reconhecimento, o status do servidor vem na tag RESULT e não na tag STATUS
                    Handle = value.Replace("Handle: ", "").ToString();
                }
                if (value.IndexOf("[{") != -1 || value.IndexOf("{") != -1)
                {
                    objRecognitionResult = JsonConvert.DeserializeObject<RecognitionResult>(value);
                }
            };

            if (objRecognitionResult != null)
            {
                objRecognitionResult.Handle = Handle;
                objRecognitionResult.ResultCode = ResultCode;
            }

            return objRecognitionResult;
        }

        /// <summary>
        /// Set parameters for RecognitionConfiguration
        /// </summary>
        /// <param name="objRecognitionConfig">Reference for an instance of RecognitionConfig</param>
        /// <param name="key">Key to be set</param>
        /// <param name="value">Value to be set</param>
        private void SetParametersSettings(ref RecognitionConfig objRecognitionConfig, string key, string value)
        {
            value = value.Trim();

            if (key?.Equals(RecognitionConfig.HeaderDecoderStartInputTimers) ?? false)
            {
                objRecognitionConfig.StartInputTimers = bool.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderDecoderMaxSentences) ?? false)
            {
                objRecognitionConfig.MaxSentences = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderHeadMarginMilliseconds) ?? false)
            {
                objRecognitionConfig.HeadMarginMilliseconds = int.Parse(value.ToString());
            }
            else if (key?.Equals(RecognitionConfig.HeaderTailMarginMilliseconds) ?? false)
            {
                objRecognitionConfig.TailMarginMilliseconds = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderWaitEndMilliseconds) ?? false)
            {
                objRecognitionConfig.WaitEndMilliseconds = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderEndpointerLevelThreshold) ?? false)
            {
                value = value.Replace(".", ",");
                objRecognitionConfig.EndpointerLevelThreshold = float.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderEndpointerAutoLevelLen) ?? false)
            {
                objRecognitionConfig.EndpointerAutoLevelLen = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderEndpointerLevelMode) ?? false)
            {
                objRecognitionConfig.EndpointerLevelMode = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderNoInputTimeoutEnabled) ?? false)
            {
                objRecognitionConfig.NoInputTimeoutEnabled = bool.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderNoInputTimeoutMilliseconds) ?? false)
            {
                objRecognitionConfig.NoInputTimeoutMilliseconds = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderRecognitionTimeoutEnabled) ?? false)
            {
                objRecognitionConfig.RecognitionTimeoutEnabled = bool.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderRecognitionTimeoutMilliseconds) ?? false)
            {
                objRecognitionConfig.RecognitionTimeoutMilliseconds = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderConfidenceThreshold) ?? false)
            {
                objRecognitionConfig.ConfidenceThreshold = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderContinuousMode) ?? false)
            {
                objRecognitionConfig.ContinuousMode = bool.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderTextify) ?? false)
            {
                objRecognitionConfig.Textify = bool.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderEndpointerUseToneDetectors) ?? false)
            {
                objRecognitionConfig.EndpointerUseToneDetectors = bool.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderWordDetails) ?? false)
            {
                objRecognitionConfig.WordDetails = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderMaxSegmentDuration) ?? false)
            {
                objRecognitionConfig.MaxSegmentDuration = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderSegmentOverlapTime) ?? false)
            {
                objRecognitionConfig.SegmentOverlapTime = int.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderHintsWords) ?? false)
            {
                objRecognitionConfig.HintsWords = value;
            }
            else if (key?.Equals(RecognitionConfig.HeaderFormattingEnabled) ?? false)
            {
                objRecognitionConfig.FormattingEnabled = bool.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderTextifyFormattingRules) ?? false)
            {
                objRecognitionConfig.TextifyFormattingRules = value;
            }
            else if (key?.Equals(RecognitionConfig.HeaderLoggingTag) ?? false)
            {
                objRecognitionConfig.LoggingTag = value;
            }
            else if (key?.Equals(RecognitionConfig.HeaderPartialResultEnabled) ?? false)
            {
                objRecognitionConfig.PartialResultEnabled = bool.Parse(value);
            }
            else if (key?.Equals(RecognitionConfig.HeaderPartialResultInterval) ?? false)
            {
                objRecognitionConfig.PartialResultInterval = int.Parse(value);
            }


        }

        /// <summary>
        /// Converte o array fornecido em um objeto to Tipo OfSpeech(start ou end), que é um dos tipos possíveis de reposta do servidor descrita pelo Protocolo
        /// </summary>
        /// <param name="arrValues"></param>
        /// <returns></returns>
        private OfSpeech PrepareOfSpeech(string[] arrValues)
        {
            var objOfSpeech = new OfSpeech();
            foreach (string value in arrValues)
            {
                if (value.IndexOf("Handle: ") != -1)
                    objOfSpeech.Handle = value.Replace("Handle: ", "");
                if (value.IndexOf("Session-Status: ") != -1)
                {
                    objOfSpeech.SessionStatus = value.Replace("Session-Status: ", "").ToEnum<SESSION_STATUS>();
                }
            };
            return objOfSpeech;
        }

        /// <summary>
        /// Monta a string a ser passada ao servidor ASR com as configurações alteradas
        /// </summary>
        /// <param name="objParameters"></param>
        /// <returns></returns>
        private string GetCommand(RecognitionConfig objRecognitionConfig)
        {
            StringBuilder str = new StringBuilder();

            if (objRecognitionConfig.StartInputTimers != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderDecoderStartInputTimers, objRecognitionConfig.StartInputTimers.ToString().ToLower()));

            if (objRecognitionConfig.MaxSentences != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderDecoderMaxSentences, objRecognitionConfig.MaxSentences));

            if (objRecognitionConfig.HeadMarginMilliseconds != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderHeadMarginMilliseconds, objRecognitionConfig.HeadMarginMilliseconds));

            if (objRecognitionConfig.TailMarginMilliseconds != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderTailMarginMilliseconds, objRecognitionConfig.TailMarginMilliseconds));

            if (objRecognitionConfig.WaitEndMilliseconds != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderWaitEndMilliseconds, objRecognitionConfig.WaitEndMilliseconds));

            if (objRecognitionConfig.EndpointerLevelThreshold != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderEndpointerLevelThreshold, objRecognitionConfig.EndpointerLevelThreshold));

            if (objRecognitionConfig.EndpointerAutoLevelLen != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderEndpointerAutoLevelLen, objRecognitionConfig.EndpointerAutoLevelLen));

            if (objRecognitionConfig.EndpointerLevelMode != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderEndpointerLevelMode, objRecognitionConfig.EndpointerLevelMode));

            if (objRecognitionConfig.NoInputTimeoutEnabled != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderNoInputTimeoutEnabled, objRecognitionConfig.NoInputTimeoutEnabled.ToString().ToLower()));

            if (objRecognitionConfig.NoInputTimeoutMilliseconds != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderNoInputTimeoutMilliseconds, objRecognitionConfig.NoInputTimeoutMilliseconds));

            if (objRecognitionConfig.RecognitionTimeoutEnabled != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderRecognitionTimeoutEnabled, objRecognitionConfig.RecognitionTimeoutEnabled.ToString().ToLower()));

            if (objRecognitionConfig.RecognitionTimeoutMilliseconds != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderRecognitionTimeoutMilliseconds, objRecognitionConfig.RecognitionTimeoutMilliseconds));

            if (objRecognitionConfig.ContinuousMode != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderContinuousMode, objRecognitionConfig.ContinuousMode.ToString().ToLower()));

            if (objRecognitionConfig.ConfidenceThreshold != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderConfidenceThreshold, objRecognitionConfig.ConfidenceThreshold));
            
            if (objRecognitionConfig.EndpointerUseToneDetectors != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderEndpointerUseToneDetectors, objRecognitionConfig.EndpointerUseToneDetectors.ToString().ToLower()));
            
            if (objRecognitionConfig.WordDetails != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderWordDetails, objRecognitionConfig.WordDetails));
            
            if (objRecognitionConfig.MaxSegmentDuration != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderMaxSegmentDuration, objRecognitionConfig.MaxSegmentDuration));
            
            if (objRecognitionConfig.SegmentOverlapTime != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderSegmentOverlapTime, objRecognitionConfig.SegmentOverlapTime));
            
            if (objRecognitionConfig.HintsWords != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderHintsWords, objRecognitionConfig.HintsWords));
            
            if(objRecognitionConfig.Textify != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderTextify, objRecognitionConfig.Textify.ToString().ToLower()));

            if(objRecognitionConfig.FormattingEnabled  != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderFormattingEnabled, objRecognitionConfig.FormattingEnabled.ToString().ToLower()));
            
            if(objRecognitionConfig.TextifyFormattingRules  != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderTextifyFormattingRules, objRecognitionConfig.TextifyFormattingRules));
            
            if(objRecognitionConfig.LoggingTag  != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderLoggingTag, objRecognitionConfig.LoggingTag));
            
            if(objRecognitionConfig.PartialResultEnabled  != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderPartialResultEnabled, objRecognitionConfig.PartialResultEnabled.ToString().ToLower()));
            
            if(objRecognitionConfig.PartialResultInterval  != null)
                str.AppendLine(string.Format("{0}: {1}", RecognitionConfig.HeaderPartialResultInterval, objRecognitionConfig.PartialResultInterval));
            
            return str.ToString();
        }

        /// <summary>
        /// Retorna o valor a ser enviado para o servidor, de acordo com o passado na estrutura
        /// </summary>
        /// <param name="enu"></param>
        /// <returns></returns>
        private string ConvertRecognitionParameter(RecognitionParameters enu)
        {
            string strValue = "";
            switch (enu)
            {
                case RecognitionParameters.DecoderStartInputTimers:
                    strValue = RecognitionConfig.HeaderDecoderStartInputTimers;
                    break;
                case RecognitionParameters.DecoderMaxSentences:
                    strValue = RecognitionConfig.HeaderDecoderMaxSentences;
                    break;
                case RecognitionParameters.EndPointerHeadMargin:
                    strValue = RecognitionConfig.HeaderHeadMarginMilliseconds;
                    break;
                case RecognitionParameters.EndPointerTailMargin:
                    strValue = RecognitionConfig.HeaderTailMarginMilliseconds;
                    break;
                case RecognitionParameters.EndPointerWaitEnd:
                    strValue = RecognitionConfig.HeaderWaitEndMilliseconds;
                    break;
                case RecognitionParameters.EndPointerLevelThreshold:
                    strValue = RecognitionConfig.HeaderEndpointerLevelThreshold;
                    break;
                case RecognitionParameters.EndPointerAutoLevelLen:
                    strValue = RecognitionConfig.HeaderEndpointerAutoLevelLen;
                    break;
                case RecognitionParameters.EndPointerLevelMode:
                    strValue = RecognitionConfig.HeaderEndpointerLevelMode;
                    break;
                case RecognitionParameters.NoInputTimeoutEnabled:
                    strValue = RecognitionConfig.HeaderNoInputTimeoutEnabled;
                    break;
                case RecognitionParameters.NoInputTimeoutValue:
                    strValue = RecognitionConfig.HeaderNoInputTimeoutMilliseconds;
                    break;
                case RecognitionParameters.RecognitionTimeoutEnabled:
                    strValue = RecognitionConfig.HeaderRecognitionTimeoutEnabled;
                    break;
                case RecognitionParameters.RecognitionTimeoutValue:
                    strValue = RecognitionConfig.HeaderRecognitionTimeoutMilliseconds;
                    break;
                case RecognitionParameters.ConfidenceThreshold:
                    strValue = RecognitionConfig.HeaderConfidenceThreshold;
                    break;
            }

            return strValue;
        }

        /// <summary>
        /// Converte um valor em enumerado de Recognition Parameters
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        private RecognitionParameters ConvertRecognitionParameter(string strValue)
        {
            RecognitionParameters enuParameter;
            switch (strValue)
            {
                case RecognitionConfig.HeaderDecoderStartInputTimers:
                    enuParameter = RecognitionParameters.DecoderStartInputTimers;
                    break;
                case RecognitionConfig.HeaderDecoderMaxSentences:
                    enuParameter = RecognitionParameters.DecoderMaxSentences;
                    break;
                case RecognitionConfig.HeaderHeadMarginMilliseconds:
                    enuParameter = RecognitionParameters.EndPointerHeadMargin;
                    break;
                case RecognitionConfig.HeaderTailMarginMilliseconds:
                    enuParameter = RecognitionParameters.EndPointerTailMargin;
                    break;
                case RecognitionConfig.HeaderWaitEndMilliseconds:
                    enuParameter = RecognitionParameters.EndPointerWaitEnd;
                    break;
                case RecognitionConfig.HeaderEndpointerLevelThreshold:
                    enuParameter = RecognitionParameters.EndPointerLevelThreshold;
                    break;
                case RecognitionConfig.HeaderEndpointerAutoLevelLen:
                    enuParameter = RecognitionParameters.EndPointerAutoLevelLen;
                    break;
                case RecognitionConfig.HeaderEndpointerLevelMode:
                    enuParameter = RecognitionParameters.EndPointerLevelMode;
                    break;
                case RecognitionConfig.HeaderNoInputTimeoutEnabled:
                    enuParameter = RecognitionParameters.NoInputTimeoutEnabled;
                    break;
                case RecognitionConfig.HeaderNoInputTimeoutMilliseconds:
                    enuParameter = RecognitionParameters.NoInputTimeoutValue;
                    break;
                case RecognitionConfig.HeaderRecognitionTimeoutEnabled:
                    enuParameter = RecognitionParameters.RecognitionTimeoutEnabled;
                    break;
                case RecognitionConfig.HeaderRecognitionTimeoutMilliseconds:
                    enuParameter = RecognitionParameters.RecognitionTimeoutValue;
                    break;
                case RecognitionConfig.HeaderConfidenceThreshold:
                    enuParameter = RecognitionParameters.ConfidenceThreshold;
                    break;
                default:
                    enuParameter = RecognitionParameters.DecoderMaxSentences;
                    break;
            }

            return enuParameter;
        }

        #endregion

        #region Disparador de Eventos

        /// <summary>
        /// Informa que o WebSocket está abrindo
        /// </summary>
        internal virtual void SendOnOpening()
        {
            //if (OnOpening != null)
            //    OnOpening(this);
        }

        /// <summary>
        /// Informa a abertura do WebSocket
        /// </summary>
        internal virtual void SendOnOpen()
        {
            new Thread(() =>
            {
                OnOpen?.Invoke(this);
            }).Start();
        }

        /// <summary>
        /// Informa quando a sessão do ASR foi criada
        /// </summary>
        internal virtual void SendOnCreatedSession(Response objResponse)
        {
            new Thread(() =>
            {
                OnCreatedSession?.Invoke(this, objResponse);
            }).Start();
        }

        /// <summary>
        /// Informa que o WebSocket foi fechado
        /// </summary>
        internal virtual void SendOnClose(CPqDCloseEventArgs e)
        {
            new Thread(() =>
            {
                OnClose?.Invoke(this, e);
            }).Start();
        }

        /// <summary>
        /// Informa que o servidor ASR está pronto para receber audio
        /// </summary>
        internal virtual void SendOnListening()
        {
            new Thread(() =>
            {
                OnListening?.Invoke(this);
            }).Start();
        }

        /// <summary>
        /// Informa que o ASR começou o reconhecer fala
        /// </summary>
        internal virtual void SendOnStartOfSpeak()
        {
            new Thread(() =>
            {
                OnStartOfSpeak?.Invoke(this);
            }).Start();
        }

        /// <summary>
        /// Informa que o ASR terminou o reconhecimento da fala
        /// </summary>
        internal virtual void SendOnEndOfSpeak()
        {
            new Thread(() =>
            {
                OnEndOfSpeak?.Invoke(this);
            }).Start();
        }

        /// <summary>
        /// Dispara evento de configuração dos parametros
        /// </summary>
        private void SendOnGetParameters(Response objResponse)
        {
            new Thread(() =>
            {
                OnGetParameter?.Invoke(this, objResponse);
            }).Start();
        }

        /// <summary>
        /// Dispara os eventos de reconhecimento parcial
        /// </summary>
        internal virtual void SendOnPartialResult(PartialRecognitionResult objResult)
        {
            new Thread(() =>
            {
                OnPartialResult?.Invoke(this, objResult);
            }).Start();
        }

        /// <summary>
        /// Dispara o evento de finalização do reconhecimento
        /// </summary>
        internal virtual void SendOnFinalResult(RecognitionResult objResult)
        {
            new Thread(() =>
            {
                OnFinalResult?.Invoke(this, objResult);
            }).Start();
        }

        internal void SendOnError(Response objResponse)
        {
            var obj = new CPqDErrorEventArgs()
            {
                Exception = new Exception(objResponse.Message),
                Message = objResponse.Message,
                ErrorCode = objResponse.ErrorCode,
                Result = objResponse.Result,
                Method = objResponse.Method
            };

            SendOnError(obj);
        }

        internal void SendOnError(string Message, string ErrorCode)
        {
            var obj = new CPqDErrorEventArgs()
            {
                Exception = new Exception(Message),
                Message = Message,
                ErrorCode = ErrorCode
            };

            SendOnError(obj);
        }

        /// <summary>
        /// Dispara o evento de finalização do reconhecimento
        /// </summary>
        internal virtual void SendOnError(CPqDErrorEventArgs e)
        {
            WriteLog(e.Message);
            new Thread(() =>
            {
                OnError?.Invoke(this, e);
            }).Start();
        }

        #endregion

        #endregion

        #region Metodos Protected

        /// <summary>
        /// Chama um metodo em thread
        /// </summary>
        /// <param name="method"></param>
        protected void StartThread(ThreadStart method)
        {
            Thread thr = new Thread(method);
            thr.Start();
        }

        /// <summary>
        /// Start a thread
        /// </summary>
        /// <param name="method"></param>
        protected void StartThread(ParameterizedThreadStart method)
        {
            Thread thr = new Thread(method);
            thr.Start();
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Close websocket connection
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            objWebSocket?.Close();
        }
        #endregion
    }
}
