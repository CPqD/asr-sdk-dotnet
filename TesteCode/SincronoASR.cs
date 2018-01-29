using CPqDASR;
using CPqDASR.ASR;
using CPqDAsrStandard.Entities;
using CPqDAsrStandard.Events;
using System;
using System.Diagnostics;
using System.Threading;

namespace TesteCode
{
    public class SincronoASR
    {
        private static int intID = 0;
        AutoResetEvent wtOpen = new AutoResetEvent(false);
        AutoResetEvent wtRecognized = new AutoResetEvent(false);
        FileASRClient objAsr = null;

        public delegate void Returned(object sender, string Message);

        public event Returned OnError;
        public event Returned OnResult;

        private static int intQtd = 0;

        string Text;

        private int ID;

        public SincronoASR(string Uri, string UserName, string Password)
        {
            //lock (this)
            //{
            //    intID += 1;
            //    ID = intID;
            //}
            //objAsr = ASRClientFactory.CreateFileClient(Uri, null);
            //objAsr.OnOpen += objAsr_OnOpen;
            //objAsr.OnError += objAsr_OnError;
            //objAsr.OnFinalResult += objAsr_OnFinalResult;
            //objAsr.OnClose += objAsr_OnClose;
        }

        void objAsr_OnClose(object sender, CPqDCloseEventArgs e)
        {
            wtOpen.Set();
            wtRecognized.Set();
        }

        void objAsr_OnOpen(object sender)
        {
            wtOpen.Set();
        }

        public bool Open(string Uri, string UserName, string Password)
        {
            wtOpen.Reset();
            objAsr.Open(Uri, null);

            wtOpen.WaitOne();

            return objAsr.IsConnected;
        }

        byte[] b;
        public void StartRecognition(string LanguageModel, byte[] buffer, string strText)
        {
            wtRecognized.Reset();
            this.Text = strText;

            b = buffer;

            WriteLog("Iniciando o reconhecimento");
            objAsr.StartRecognition(LanguageModel, buffer);

            wtRecognized.WaitOne();
            WriteLog("Reconhecimento foi finalizado");
            intQtd -= 1;
        }

        public void Close()
        {
            wtOpen.Set();
            wtRecognized.Set();
            objAsr.Close();
        }

        private void objAsr_OnFinalResult(object sender, RecognitionResult e)
        {
            if (e.Alternatives == null)
            {
                OnError(sender, "As alternativas não foram retornadas: " + e.ResultCode.ToString());
                Console.WriteLine("###########################################################");
                Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss-ffff") + "As alternativas não foram retornadas: " + e.ResultCode.ToString());
                Console.WriteLine("###########################################################");
            }
            else
            {
                if (this.Text == e.Alternatives[0].Text)
                {
                    if (OnResult != null)
                        OnResult(sender, e.Alternatives[0].Text);
                }
                else
                {
                    if (OnError != null)
                        OnError(sender, "A frase não foi reconhecida corretamente: ");
                    Console.WriteLine("###########################################################");
                    Console.WriteLine(DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss-ffff") + " Frase correta: \"" + this.Text + "\" Frase retornada: \"" + e.Alternatives[0].Text + "\"");
                    Console.WriteLine("###########################################################");
                }
            }
            wtRecognized.Set();
        }

        private void objAsr_OnError(object sender, CPqDErrorEventArgs e)
        {
            Trace.WriteLine("ERROR: " + e.Message);
            wtOpen.Set();
            wtRecognized.Set();
            if (OnError != null)
                OnError(sender, e.Message);
        }

        public void WriteLog(string strValue)
        {
            string str = String.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.ffff"), " AplicativoTeste - ", strValue);
            Trace.WriteLine(str);
        }
    }
}
