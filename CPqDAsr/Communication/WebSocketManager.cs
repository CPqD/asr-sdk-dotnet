using CPqDASR.Events;
using CPqDWebSocketStandard;
using System;
using System.Threading;

namespace CPqDASR.Communication
{
    public class WebSocketManager
    {
        #region Delegates

        public delegate void WebSocketHandler(WebSocketManager sender);
        public delegate void WebSocketCloseHandler(WebSocketManager sender, CPqDCloseEventArgs Reason);
        public delegate void WebSocketErrorHandler(WebSocketManager sender, CPqDErrorEventArgs e);
        public delegate void WebSocketResultHandler(WebSocketManager sender, CPqDWebSocketResultEventArgs e);

        #endregion

        #region Eventos

        /// <summary>
        /// Disparado quando o WebSocket inicia a conexão com o servidor
        /// </summary>
        public event WebSocketHandler OnOpening;

        /// <summary>
        /// Disparado após o WebSocket estabelecer conexão com o servidor
        /// </summary>
        public event WebSocketHandler OnOpened;

        /// <summary>
        /// Disparado quando o WebSocket é fechado
        /// </summary>
        public event WebSocketCloseHandler OnClosed;

        /// <summary>
        /// Disparado quando ocorre erros no WebSocket
        /// </summary>
        public event WebSocketErrorHandler OnError;

        /// <summary>
        /// Disparado ao receber uma mensagem no WebSocket
        /// </summary>
        public event WebSocketResultHandler OnMessage;

        #endregion

        #region Variaveis

        private WebSocket objWebSocket = null;

        #endregion

        #region Propriedades

        /// <summary>
        /// Indica se o status do WebSocket está aberto
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (objWebSocket == null)
                    return false;
                //return objWebSocket.IsAlive;
                return objWebSocket.ReadyState == WebSocketState.Open;
            }
        }

        /// <summary>
        /// Indica se o certificado SSL deve ser validado
        /// </summary>
        public bool CheckCertificateRevocation
        {
            get
            {
                return objWebSocket.SslConfiguration.CheckCertificateRevocation;
            }
            set
            {
                objWebSocket.SslConfiguration.CheckCertificateRevocation = value;
            }
        }

        #endregion

        #region Metodos Publicos

        /// <summary>
        /// Abre a conexão WebSocket com a informações passadas
        /// </summary>
        /// <param name="ServerURL">Servidor a ser realizada a conexão WebSocket</param>
        /// <param name="Credentials">Credenciais a serem utilizadas</param>
        public void Open(string ServerURL, Credentials Credentials)
        {
            //Criando objeto de conexão socket
            objWebSocket = new WebSocket(ServerURL, LogLevel.Trace);
            objWebSocket.OnOpen += objWebSocket_OnOpen;
            objWebSocket.OnClose += objWebSocket_OnClose;
            objWebSocket.OnError += objWebSocket_OnError;
            objWebSocket.OnMessage += objWebSocket_OnMessage;

            if (Credentials != null)
                objWebSocket.SetCredentials(Credentials.UserName, Credentials.Password, false);

            //Abrindo a conexão socket
            try
            {
                objWebSocket.Connect();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Disparando evento de conectando
            OnOpening?.Invoke(this);

            Thread thrConnect = new Thread(ValidConncetion);
            thrConnect.Start();
        }

        /// <summary>
        /// Envia array de bytes para o WebSocket
        /// </summary>
        /// <param name="Value">Array de bytes com o conteudo a ser enviado</param>
        public void Send(byte[] Value)
        {
            if (objWebSocket != null)
                try
                {
                    objWebSocket.Send(Value);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
        }

        /// <summary>
        /// Envia string para o WebSocket
        /// </summary>
        /// <param name="Value">String a ser enviada ao WebSocket</param>
        public void Send(string Value)
        {
            Send(System.Text.Encoding.UTF8.GetBytes(Value));
        }

        /// <summary>
        /// Fecha a conexão WebSocket
        /// </summary>
        public void Close()
        {
            if (objWebSocket != null)
                objWebSocket.Close();
        }

        #endregion

        #region Eventos WebSocket

        /// <summary>
        /// Recebe mensagem do WebSocket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void objWebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            CPqDWebSocketResultEventArgs obj = new CPqDWebSocketResultEventArgs();
            obj.Data = e.Data;
            obj.IsBinary = e.IsBinary;
            obj.IsPing = e.IsPing;
            obj.IsText = e.IsText;
            obj.RawData = e.RawData;

            OnMessage?.Invoke(this, obj);
        }

        /// <summary>
        /// Recebe erro ocorridos no WebSocket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void objWebSocket_OnError(object sender, ErrorEventArgs e)
        {
            //if (!e.Message.Equals("An exception has occurred while sending data."))
            //{
            CPqDErrorEventArgs obj = new CPqDErrorEventArgs();
            obj.Exception = e.Exception;
            obj.Message = e.Message;
            OnError?.Invoke(this, obj);
            //}
        }

        /// <summary>
        /// Disparado quando a conexão com o WebSocket é ecerrada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void objWebSocket_OnClose(object sender, CloseEventArgs e)
        {
            if (OnClosed != null)
            {
                CPqDCloseEventArgs obj = new CPqDCloseEventArgs();
                obj.Code = e.Code;
                obj.Reason = e.Reason;
                OnClosed(this, obj);
            }
        }

        /// <summary>
        /// Disparado ao estabelecer uma conexão com o servidor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void objWebSocket_OnOpen(object sender, EventArgs e)
        {
            OnOpened?.Invoke(this);
        }

        #endregion

        #region Metodos Privados

        /// <summary>
        /// Verifica se a conexão foi aberta corretamente
        /// </summary>
        private void ValidConncetion()
        {
            //Aguarda o inicio do WebSocket, ou gera um timeout após 60 segundos
            DateTime dtStart = DateTime.Now;
            DateTime dtNow = DateTime.Now;

            while (!objWebSocket.IsAlive && dtStart.AddSeconds(60) >= DateTime.Now)
            {
                Thread.Sleep(100);
            }

            if (dtStart.AddSeconds(60) <= DateTime.Now)
            {
                objWebSocket.Close();
                CPqDErrorEventArgs obj = new CPqDErrorEventArgs();
                obj.Exception = new Exception("It isn't possible to establish a connection with ASR server");
                obj.Message = "It isn't possible to establish a connection with ASR server";
                obj.ErrorCode = "4021";
                OnError?.Invoke(this, obj);
            }
        }

        #endregion
    }
}
