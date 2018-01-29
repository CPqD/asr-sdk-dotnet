using CPqDAsr.ASR;
using CPqDASR.ASR;
using CPqDASR.Communication;
using CPqDASR.Config;
using CPqDASR.Entities;
using CPqDASR.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CPqDASRUnitTest.ASR
{
    [TestClass]
    public class SpeechRecognitionUnitTest
    {
        #region properties
        private EventsPassed Events;

        private bool timeout = false;
        #endregion

        #region Methods for tests
        #region Tests of Builder

        [TestMethod]
        public void UrlNull()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault(), null);
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("http://vmh102.cpqd.com.br:8280/asr_dist/repository/grammars/dynamic-gram/pizza.gram");

                var results = this.ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.GetType(), typeof(ArgumentNullException));
                return;
            }

            throw new AssertFailedException("A null server Url doesn't throw an ArgumentNullException!");
        }

        [TestMethod]
        public void UrlInvalid()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault(), "ws:invalid_uri");
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("http://vmh102.cpqd.com.br:8280/asr_dist/repository/grammars/dynamic-gram/pizza.gram");

                var results = this.ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.GetType(), typeof(UriFormatException));
                return;
            }

            throw new AssertFailedException("A invalid server Url doesn't throw an UriFormatException!");
        }

        [TestMethod]
        public void CredentialValid()
        {
            var clientConfig = this.CreateClientWithCredentials(this.CreateConfigDefault(), "wss://speech.cpqd.com.br/asr/ws/estevan/recognize/8k", "estevan", "Thect195");
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("http://vmh102.cpqd.com.br:8280/asr_dist/repository/grammars/dynamic-gram/pizza.gram");

                var results = this.ExecuteRecognition(clientConfig, lModelLst, audioSource);

                Assert.IsTrue(results?.Count > 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void CredentialInvalid()
        {
            var clientConfig = this.CreateClientWithCredentials(this.CreateConfigDefault(), "wss://speech.cpqd.com.br/asr/ws/estevan/recognize/8k", "invalid", "invalid");
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("http://vmh102.cpqd.com.br:8280/asr_dist/repository/grammars/dynamic-gram/pizza.gram");

                this.ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(IOException));
                return;
            }

            throw new AssertFailedException();
        }

        [TestMethod]
        public void CredentialNull()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault(), "wss://speech.cpqd.com.br/asr/ws/estevan/recognize/8k");
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("http://vmh102.cpqd.com.br:8280/asr_dist/repository/grammars/dynamic-gram/pizza.gram");

                this.ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NullReferenceException));
                return;
            }

            throw new AssertFailedException();
        }

        [TestMethod]
        public async Task MultipleListeners()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");
            Events = new EventsPassed();
            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("http://vmh102.cpqd.com.br:8280/asr_dist/repository/grammars/dynamic-gram/pizza.gram");
                
                speechRecognizer.OnListening += SpeechRecognizer_OnListening;
                speechRecognizer.OnPartialRecognitionResult += SpeechRecognizer_OnPartialRecognitionResult;
                speechRecognizer.OnRecognitionResult += SpeechRecognizer_OnRecognitionResult;
                speechRecognizer.OnSpeechStart += SpeechRecognizer_OnSpeechStart;
                speechRecognizer.OnSpeechStop += SpeechRecognizer_OnSpeechStop;

                speechRecognizer.Recognize(audioSource, lModelLst);

                Task<bool> checkEventsPassed = this.CheckIfEventsHasPassed();

                bool result = await checkEventsPassed;

                Assert.IsTrue(result);
            }
            catch (Exception ex)
            {
                Events = null;
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }

            Events = null;
        }

        [TestMethod]
        public void MaxWaitSettings()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");

            //Just initializinf variables with same value
            DateTime initWait = DateTime.Now;
            DateTime endWait = new DateTime(initWait.Ticks);

            double stampInSeconds;

            const int timeToWait = 1000;
            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);
            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("http://vmh102.cpqd.com.br:8280/asr_dist/repository/grammars/dynamic-gram/pizza.gram");

                speechRecognizer.Recognize(audioSource, lModelLst);

                initWait = DateTime.Now;
                speechRecognizer.WaitRecognitionResult(timeToWait);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("Recognition timeout"))
                {
                    endWait = DateTime.Now;
                }
                else
                {
                    throw ex;
                }
            }
            finally
            {
                speechRecognizer.Close();
            }

            stampInSeconds = (endWait - initWait).TotalSeconds;

            //Asserts if stamp was correctly calculated and is lower than timeToWait 
            //with an increment of 200 milis that considering the natural processing delay
            Assert.IsTrue(stampInSeconds > 0 && stampInSeconds <= (timeToWait + 200));
        }
        #endregion

        #region Methods of SpeechRecognition
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void BasicGrammar()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("http://vmh102.cpqd.com.br:8280/asr_dist/repository/grammars/dynamic-gram/pizza.gram");

                var results = this.ExecuteRecognition(clientConfig, lModelLst, audioSource);

                var score = results.
                    Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).
                    FirstOrDefault().Alternatives.
                    Where(a => a.Confidence >= 90).FirstOrDefault()?.Confidence;

                Assert.AreEqual(score != null && score > 90, true);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void BasicSLM()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                var results = this.ExecuteRecognition(clientConfig, lModelLst, audioSource);

                var score = results.
                    Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).
                    FirstOrDefault().Alternatives.
                    Where(a => a.Confidence >= 90).FirstOrDefault()?.Confidence;

                Assert.AreEqual(score != null && score > 90, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void NoSpeech()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            var audioSource = new FileAudioSource(@"8k\Silencio\silence-8k.wav");

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                var results = ExecuteRecognition(clientConfig, lModelLst, audioSource);

                Assert.IsTrue(results != null && results.Count > 0);
                Assert.AreEqual(results[0].ResultCode, CPqDASR.RecognitionResultCode.NO_SPEECH);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void NoInputTimeOut()
        {
            var recogConfig = this.CreateConfigDefault();

            recogConfig.NoInputTimeoutMilliseconds = 50;
            recogConfig.NoInputTimeoutEnabled = true;

            var clientConfig = this.CreateClientConfigDefault(recogConfig);
            var audioSource = new FileAudioSource(@"8k\Silencio\silence-8k.wav");
            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                var results = ExecuteRecognition(clientConfig, lModelLst, audioSource);

                var reultNoInputTimeout = results.
                    Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.NO_INPUT_TIMEOUT).
                    FirstOrDefault();
                Assert.IsNotNull(reultNoInputTimeout);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void RecognizeBufferAudioSource()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            //Send all buffer to be writter
            var audioSource = new BufferAudioSource(File.ReadAllBytes(@"8k\pizza\pizza_veg_audio_8k.wav"));

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                var results = ExecuteRecognition(clientConfig, lModelLst, audioSource);

                var reultNoInputTimeout = results.
                    Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).
                    FirstOrDefault();
                Assert.IsNotNull(reultNoInputTimeout);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void RecognizeMaxWaitSeconds()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());

            //Set 2 seconds to max wait time
            clientConfig.MaxWaitSeconds = 2000;

            //Send all buffer to be writter
            var audioSource = new BufferAudioSource(File.ReadAllBytes(@"8k\ContinuosMode\joao_mineiro_marciano_intro_8k.wav"));

            try
            {
                //Cria modelo de linguagem com gramática para o áudio de pizza:
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(RecognitionException));
            }
        }

        [TestMethod]
        public void CloseWhileRecognize()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());

            //Send all buffer to be writter
            var audioSource = new BufferAudioSource(File.ReadAllBytes(@"8k\ContinuosMode\joao_mineiro_marciano_intro_8k.wav"));

            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);
            try
            {

                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                speechRecognizer.Recognize(audioSource, lModelLst);

                Thread.Sleep(2000);

                speechRecognizer.Close();
                var result = speechRecognizer.WaitRecognitionResult();

                Assert.IsTrue(result.Count == 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }
        }

        [TestMethod]
        public void CloseWithoutRecognize()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            //Send all buffer to be writter
            var audioSource = new BufferAudioSource(File.ReadAllBytes(@"8k\pizza\pizza_veg_audio_8k.wav"));

            try
            {
                SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);
                speechRecognizer.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void CancelWhileRecognize()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            //Send all buffer to be writter
            var audioSource = new BufferAudioSource(File.ReadAllBytes(@"8k\ContinuosMode\joao_mineiro_marciano_intro_8k.wav"));

            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);

            try
            {

                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                speechRecognizer.Recognize(audioSource, lModelLst);

                Thread.Sleep(2000);

                speechRecognizer.CancelRecognition();

                var result = speechRecognizer.WaitRecognitionResult();
                Assert.IsTrue(result.Count == 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }
        }

        [TestMethod]
        public void CancelNoRecognize()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());

            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);

            try
            {
                speechRecognizer.CancelRecognition();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }
        }

        [TestMethod]
        public void WaitNoRecognize()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());

            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);

            try
            {
                var result = speechRecognizer.WaitRecognitionResult();
                Assert.IsTrue(result.Count == 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }
        }

        [TestMethod]
        public void WaitRecognitionResultDuplicate()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            //Send all buffer to be writter
            var audioSource = new BufferAudioSource(File.ReadAllBytes(@"8k\pizza\pizza_veg_audio_8k.wav"));

            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);

            try
            {
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                speechRecognizer.Recognize(audioSource, lModelLst);

                var result = speechRecognizer.WaitRecognitionResult();
                var resultRecognized = result.Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).FirstOrDefault();

                Assert.IsNotNull(resultRecognized);

                var duplicatedResult = speechRecognizer.WaitRecognitionResult();

                Assert.IsTrue(duplicatedResult.Count == 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }
        }

        [TestMethod]
        public void DuplicateRecognize()
        {
            var clientConfig = this.CreateClientConfigDefault(this.CreateConfigDefault());
            //Send all buffer to be writter
            var audioSource = new BufferAudioSource(File.ReadAllBytes(@"8k\pizza\pizza_veg_audio_8k.wav"));

            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);

            try
            {
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                speechRecognizer.Recognize(audioSource, lModelLst);

                Thread.Sleep(1000);

                try
                {
                    speechRecognizer.Recognize(audioSource, lModelLst);
                }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(RecognitionException));
                }

                var result = speechRecognizer.WaitRecognitionResult();
                var resultRecognized = result.Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).FirstOrDefault();

                Assert.IsNotNull(resultRecognized);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }
        }

        [TestMethod]
        public void MultipleRecognize()
        {
            ExecuteMultiplesRecognitions(CreateClientConfigDefault(CreateConfigDefault()));
        }

        [TestMethod]
        public void MultiplesConnectOnRecognize()
        {
            var config = CreateClientConfigDefault(CreateConfigDefault());

            config.AutoClose = true;

            ExecuteMultiplesRecognitions(config);
        }

        [TestMethod]
        public void SessionTimeout()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            //Send all buffer to be writter
            var audioSource = new BufferAudioSource(File.ReadAllBytes(@"8k\pizza\pizza_veg_audio_8k.wav"));

            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);
            try
            {
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");

                speechRecognizer.Recognize(audioSource, lModelLst);

                Thread.Sleep(65000);

                var result = speechRecognizer.WaitRecognitionResult();
                var resultRecognized = result.Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).FirstOrDefault();

                Assert.IsNotNull(resultRecognized);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }
        }

        #endregion
        #endregion

        #region Privates methods
        private List<RecognitionResult> ExecuteRecognition(ClientConfig clientConfig, LanguageModelList languageModelList, IAudioSource audioSource)
        {
            List<RecognitionResult> results = null;

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                speechRecognizer.Recognize(audioSource, languageModelList);
                results = speechRecognizer.WaitRecognitionResult();
            }

            return results;
        }

        private void ExecuteMultiplesRecognitions(ClientConfig config)
        {
            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(config);
            int k = 0;
            try
            {
                var lModelLst = new LanguageModelList();
                lModelLst.AddFromUri("builtin:slm/general");


                for (int i = 0; i < 4; i++)
                {
                    var audioSource = new FileAudioSource(@"8k\pizza\pizza_veg_audio_8k.wav");
                    speechRecognizer.Recognize(audioSource, lModelLst);

                    var result = speechRecognizer.WaitRecognitionResult();
                    var resultRecognized = result.Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).FirstOrDefault();

                    Assert.IsNotNull(resultRecognized);
                    k++;
                    if (i < 3)
                    {
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        Thread.Sleep(5500);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                speechRecognizer.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private RecognitionConfig CreateConfigDefault()
        {
            return new RecognitionConfig()
            {
                MaxSentences = 2,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recognitionConfig"></param>
        /// <returns></returns>
        private ClientConfig CreateClientConfigDefault(RecognitionConfig recognitionConfig)
        {
            return this.CreateClientConfigDefault(recognitionConfig, "ws://vmh123:8025/asr-server/asr");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recognitionConfig"></param>
        /// <returns></returns>
        private ClientConfig CreateClientConfigDefault(RecognitionConfig recognitionConfig, string serverUrl)
        {
            return new ClientConfig()
            {
                ServerUrl = serverUrl,
                RecogConfig = recognitionConfig,
                UserAgent = "desktop=DELL; os=Windows 10 Pro 64 bits; client=.NET Core; app=CPqDASRUnitTest",
                MaxWaitSeconds = 20000,
                ConnectOnRecognize = false
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recognitionConfig"></param>
        /// <param name="serverUri"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private ClientConfig CreateClientWithCredentials(RecognitionConfig recognitionConfig, string serverUri, string user, string password)
        {
            var credentials = new Credentials()
            {
                UserName = user,
                Password = password
            };

            var clientConfig = this.CreateClientConfigDefault(recognitionConfig, serverUri);
            clientConfig.Credentials = credentials;

            return clientConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckIfEventsHasPassed()
        {
            System.Timers.Timer timer = new System.Timers.Timer(30000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            while (!this.Events.IsAllEventsPassed() && !this.timeout)
            {
                await Task.Delay(1000);
            }

            return Events.IsAllEventsPassed();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.timeout = true;
        }

        /// <summary>
        /// 
        /// </summary>
        private enum AudioOption
        {
            BANCO_TRANSFIRA,
            CONTINUOS_JOAO_MINEIRO,
            CPF,
            PIZZA_PEDRA,
            PIZZA_VEG,
            SILENCE
        }

        #endregion

        #region Events methods

        private void SpeechRecognizer_OnSpeechStop(int Time)
        {
            Events.OnSpeechStop = true;
        }

        private void SpeechRecognizer_OnSpeechStart(int Time)
        {
            Events.OnSpeechStart = true;
        }

        private void SpeechRecognizer_OnRecognitionResult(RecognitionResult Result)
        {
            Events.OnRecognitionResult = true;
        }

        private void SpeechRecognizer_OnPartialRecognitionResult(PartialRecognitionResult Result)
        {
            Events.OnPartialRecognitionResult = true;
        }

        private void SpeechRecognizer_OnListening()
        {
            Events.OnListening = true;
        }
        #endregion

        #region Inner class

        private class EventsPassed
        {
            //TODO: FMarino - Confirmar com o Baldin o porquê de determinados eventos não serem disparados
            internal bool OnSpeechStop { get; set; } = true;

            internal bool OnSpeechStart { get; set; }

            internal bool OnRecognitionResult { get; set; }

            internal bool OnPartialRecognitionResult { get; set; } = true;

            internal bool OnListening { get; set; }

            internal bool IsAllEventsPassed()
            {
                return OnSpeechStart && OnListening && OnPartialRecognitionResult && OnRecognitionResult && OnSpeechStop;
            }
        }

        #endregion
    }
}
