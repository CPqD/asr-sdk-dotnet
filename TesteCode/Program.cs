using CPqDAsr.ASR;
using CPqDASR.ASR;
using CPqDASR.Config;
using CPqDASR.Entities;
using System;
using System.Diagnostics;

namespace TesteCode
{
    class Program
    {
        public static void Main(string[] args)
        {
            Trace.Close();
            TextWriterTraceListener tr1 = new TextWriterTraceListener(System.IO.File.CreateText(string.Format("D:\\Trace_{0}.trace", DateTime.Now.ToString("dd-MM-yyyy_HH-mm"))));
            Trace.Listeners.Add(tr1);
            Trace.AutoFlush = true;

            var objRecognitionConfig = new RecognitionConfig()
            {
                MaxSentences = 2,
                ContinuousMode = true
            };

            var objClientConfig = new ClientConfig()
            {
                ServerUrl = "ws://vmh123:8025/asr-server/asr",
                RecogConfig = objRecognitionConfig,
                UserAgent = "laptop=DELL Latitude E5450; os=Windows 10 Pro 64 bits; client=.NET Core; app=TesteCode",
                MaxWaitSeconds = 20000,
                ConnectOnRecognize = false
            };

            FileAudioSource objAudioSource = new FileAudioSource(@"C:\AudioTestesASR\8K\ContinuosMode\joao_mineiro_marciano_intro_8k.wav");
            try
            {
                SpeechRecognizer obj = SpeechRecognizer.Create(objClientConfig);

                obj.OnSpeechStart += Obj_OnSpeechStart;
                obj.OnSpeechStop += Obj_OnSpeechStop;
                obj.OnListening += Obj_OnListening;
                obj.OnPartialRecognitionResult += Obj_OnPartialRecognitionResult;
                obj.OnRecognitionResult += Obj_OnRecognitionResult;
                obj.OnError += Obj_OnError1;

                LanguageModelList lModelLst = new LanguageModelList();

                lModelLst.AddFromUri("builtin:slm/general");

                obj.Recognize(objAudioSource, lModelLst);
                //var results = obj.WaitRecognitionResult();
                Console.Read();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
        }

        private static void Obj_OnSpeechStart(int Time)
        {
            Console.WriteLine("Inicido da fala");
        }

        private static void Obj_OnSpeechStop(int Time)
        {
            Console.WriteLine("Termino da fala");
        }

        private static void Obj_OnListening()
        {
            Console.WriteLine("Esperando a fala");
        }

        private static void Obj_OnPartialRecognitionResult(PartialRecognitionResult Result)
        {
            Console.WriteLine("Resultado parcial: " + Result.Text);
        }

        private static void Obj_OnRecognitionResult(RecognitionResult Result)
        {
            if ((Result?.ResultCode ?? CPqDASR.RecognitionResultCode.FAILURE) == CPqDASR.RecognitionResultCode.RECOGNIZED)
                Console.WriteLine(Result.Alternatives[0].Text);
            else
            {
                Console.WriteLine("#################################################################################");
                Console.WriteLine((Result?.ResultCode ?? CPqDASR.RecognitionResultCode.FAILURE));
                Console.WriteLine("#################################################################################");
            }
        }

        private static void Obj_OnError1()
        {
            Console.WriteLine("#################################################################################");
            //Console.WriteLine(e.Message + " - " + e.ErrorCode);
            Console.WriteLine("#################################################################################");
        }
        
    }
}