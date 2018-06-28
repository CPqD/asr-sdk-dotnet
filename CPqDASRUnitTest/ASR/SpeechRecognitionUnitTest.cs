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
        /// <summary>
        /// Testa se o "SpeechRecognizer.Builder" dispara exception quando a URL de conexão com servidor ASR é nula
        /// </summary>
        [TestMethod]
        public void UrlNull()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault(), null);
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);
            List<RecognitionResult> results = null;

            try
            {           
                lModelLst.AddFromUri(TestsReferences.GramPizzaHttp);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.GetType(), typeof(ArgumentNullException));
                return;
            }

            throw new AssertFailedException("A null server Url doesn't throw an ArgumentNullException!");
        }

        /// <summary>
        /// Testa se o "SpeechRecognizer.Builder" dispara exception quando a URL de conexão com servidor ASR é inválida
        /// </summary>
        [TestMethod]
        public void UrlInvalid()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault(), "ws:invalid_uri");
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);
            List<RecognitionResult> results = null;

            try
            {
                lModelLst.AddFromUri(TestsReferences.GramPizzaHttp);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.GetType(), typeof(UriFormatException));
                return;
            }

            throw new AssertFailedException("A invalid server Url doesn't throw an UriFormatException!");
        }

        /// <summary>
        /// Testa se o "SpeechRecognizer.Builder" de fato aceita uma credencial de acesso válida
        /// </summary>
        [TestMethod]
        public void CredentialValid()
        {
            var clientConfig = CreateClientWithCredentials(CreateConfigDefault(), TestsReferences.DefaultASRURL, "estevan", "Thect195");
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);
            List<RecognitionResult> results = null;

            try
            {
                lModelLst.AddFromUri(TestsReferences.GramPizzaHttp);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                throw new InternalTestFailureException(ex.Message);
            }

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0);
        }

        /// <summary>
        /// Testa se o "SpeechRecognizer.Builder" dispara exception quando a credencial de acesso é inválida
        /// </summary>
        [TestMethod]
        public void CredentialInvalid()
        {
            var clientConfig = CreateClientWithCredentials(CreateConfigDefault(), TestsReferences.DefaultASRURL, "invalid", "invalid");
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);

            try
            {
                lModelLst.AddFromUri(TestsReferences.GramPizzaHttp);
                ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(IOException));
                return;
            }

            throw new AssertFailedException();
        }

        /// <summary>
        /// Testa se o "SpeechRecognizer.Builder" dispara exception quando a credencial de acesso é nula
        /// </summary>
        [TestMethod]
        public void CredentialNull()
        {
            var clientConfig = CreateClientWithCredentials(CreateConfigDefault(), TestsReferences.DefaultASRURL, null, null);
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);

            try
            {
                lModelLst.AddFromUri(TestsReferences.GramPizzaHttp);
                ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NullReferenceException));
                return;
            }

            throw new AssertFailedException();
        }

        /// <summary>
        /// Testa se o "SpeechRecognizer.Builder" repassa a configuração de parâmetros solicitada
        /// </summary>
        [TestMethod]
        public void RecogConfig()
        {
            var recogConfig = new RecognitionConfig
            {
                ConfidenceThreshold = 100,
                ContinuousMode = false,
                EndpointerAutoLevelLen = 350,
                EndpointerLevelMode = 0,
                EndpointerLevelThreshold = 4,
                HeadMarginMilliseconds = 250,
                MaxSentences = 3,
                NoInputTimeoutEnabled = false,
                NoInputTimeoutMilliseconds = 2000,
                RecognitionTimeoutEnabled = false,
                RecognitionTimeoutMilliseconds = 65000,
                StartInputTimers = false,
                TailMarginMilliseconds = 450,
                WaitEndMilliseconds = 900
            };

            var clientConfig = CreateClientConfigDefault(recogConfig);
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioCpf);
            List<RecognitionResult> results = null;

            try
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                throw new InternalTestFailureException(ex.Message);
            }

            Assert.AreEqual(results[0].Alternatives[0].Confidence == recogConfig.ConfidenceThreshold ?
                CPqDASR.RecognitionResultCode.RECOGNIZED : CPqDASR.RecognitionResultCode.NO_MATCH, results[0].ResultCode);
        }

        /// <summary>
        /// Testa se o "SpeechRecognizer.Builder" está repassando os eventos para todos os listeners
        /// </summary>
        [TestMethod]
        public async Task MultipleListeners()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                lModelLst.AddFromUri(TestsReferences.GramPizzaHttp);

                Events = new EventsPassed();
                speechRecognizer.OnListening += SpeechRecognizer_OnListening;
                speechRecognizer.OnPartialRecognitionResult += SpeechRecognizer_OnPartialRecognitionResult;
                speechRecognizer.OnRecognitionResult += SpeechRecognizer_OnRecognitionResult;
                speechRecognizer.OnSpeechStart += SpeechRecognizer_OnSpeechStart;
                speechRecognizer.OnSpeechStop += SpeechRecognizer_OnSpeechStop;

                speechRecognizer.Recognize(audioSource, lModelLst);

                Task<bool> checkEventsPassed = CheckIfEventsHasPassed();

                bool result = await checkEventsPassed;

                Assert.IsTrue(result);
            }
            Events = null;
        }

        /// <summary>
        /// Testa se o "SpeechRecognizer.Builder" respeita o valor de timeout de espera de resposta definido
        /// </summary>
        [TestMethod]
        public void MaxWaitSettings()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);

            //Just initializing variables with same value
            DateTime initWait = DateTime.Now;
            DateTime endWait = new DateTime(initWait.Ticks);

            double stampInMilliseconds;
            const int timeToWait = 1000;

            SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig);
            try
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);

                speechRecognizer.Recognize(audioSource, lModelLst);
                initWait = DateTime.Now;
                speechRecognizer.WaitRecognitionResult(timeToWait);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("Response timeout"))
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

            stampInMilliseconds = (endWait - initWait).TotalMilliseconds;

            //Asserts if stamp was correctly calculated and is lower than timeToWait 
            //with an increment of 200 milis that considering the natural processing delay
            Assert.IsTrue(stampInMilliseconds > 0 && stampInMilliseconds <= (timeToWait + 200));
        }
        #endregion

        #region Methods of SpeechRecognition
        /// <summary>
        /// Checa se um reconhecimento básico com gramática retorna texto, interpretação e score adequados
        /// </summary>
        [TestMethod]
        public void BasicGrammar()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);
            List<RecognitionResult> results = null;

            try
            {
                lModelLst.AddFromUri(TestsReferences.GramPizzaHttp);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                throw new InternalTestFailureException(ex.Message);
            }

            var score = results?.
                Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).
                FirstOrDefault().Alternatives.
                Where(a => a.Confidence >= 90).FirstOrDefault()?.Confidence;
            Assert.IsNotNull(score);

            var textFromFirstAlternative = results[0].Alternatives[0].Text.ToString();
            Assert.AreEqual(TestsReferences.TextPizzaVeg, textFromFirstAlternative);
            var firstInterpFromFirstAlt = results[0].Alternatives[0].Interpretations[0].InterpretationJson.ToString();
            Assert.AreEqual(TestsReferences.InterpPizzaVeg, firstInterpFromFirstAlt);
        }

        /// <summary>
        /// Checa se um reconhecimento básico com Modelo de Fala Livre retorna texto, interpretação e score adequados
        /// </summary>
        [TestMethod]
        public void BasicSLM()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);
            List<RecognitionResult> results = null;

            try
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                throw new InternalTestFailureException(ex.Message);
            }

            var score = results?.
                Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).
                FirstOrDefault().Alternatives.
                Where(a => a.Confidence >= 90).FirstOrDefault()?.Confidence;
            Assert.IsNotNull(score);

            var textFromFirstAlternative = results[0].Alternatives[0].Text.ToString();
            Assert.AreEqual(TestsReferences.TextPizzaVeg, textFromFirstAlternative);
        }

        /// <summary>
        /// Checar se o estado NO_SPEECH é retornado quando não há detecção de fala e a flag que indica último pacote é verdadeira
        /// </summary>
        [TestMethod]
        public void NoSpeech()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioSilence);
            List<RecognitionResult> results = null;

            try
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                throw new InternalTestFailureException(ex.Message);
            }

            Assert.IsTrue(results != null && results.Count > 0);
            Assert.AreEqual(CPqDASR.RecognitionResultCode.NO_SPEECH, results[0].ResultCode);
        }

        /// <summary>
        /// Checar se o estado NO_INPUT_TIMEOUT é retornado quando não ocorre START_OF_SPEECH antes do timer estourar
        /// </summary>
        [TestMethod]
        public void NoInputTimeOut()
        {
            var recogConfig = new RecognitionConfig
            {
                NoInputTimeoutMilliseconds = 2000,
                NoInputTimeoutEnabled = true
            };

            var clientConfig = CreateClientConfigDefault(recogConfig);
            var lModelLst = new LanguageModelList();
            // O método "BufferAudioSource" envia áudio simulando tempo real
            // Aqui o arquivo de silêncio deve ser maior que o NoInputTimeoutMilliseconds
            var audioSource = new BufferAudioSource(File.ReadAllBytes(TestsReferences.AudioSilence));
            List<RecognitionResult> results = null;

            try
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                throw new InternalTestFailureException(ex.Message);
            }

            Assert.IsTrue(results != null && results.Count > 0);
            Assert.AreEqual(CPqDASR.RecognitionResultCode.NO_INPUT_TIMEOUT, results[0].ResultCode);
        }

        /// <summary>
        /// Checa se o reconhecimento ocorre normalmente com áudio enviado simulando streaming
        /// </summary>
        [TestMethod]
        public void RecognizeBufferAudioSource()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new BufferAudioSource(File.ReadAllBytes(TestsReferences.AudioPizzaVeg));
            List<RecognitionResult> results = null;

            try
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                results = ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                throw new InternalTestFailureException(ex.Message);
            }

            var score = results?.
                Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).
                FirstOrDefault().Alternatives.
                Where(a => a.Confidence >= 90).FirstOrDefault()?.Confidence;
            Assert.IsNotNull(score);

            var textFromFirstAlternative = results[0].Alternatives[0].Text.ToString();
            Assert.AreEqual(TestsReferences.TextPizzaVeg, textFromFirstAlternative);
        }

        /// <summary>
        /// Checa se o timer de timeout na thread de resposta é respeitado
        /// </summary>
        [TestMethod]
        public void RecognizeMaxWaitSeconds()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioCpf);

            //Set 2 seconds to max wait time
            clientConfig.MaxWaitSeconds = 2000;
            try
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                ExecuteRecognition(clientConfig, lModelLst, audioSource);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(RecognitionException));
            }
        }

        /// <summary>
        /// Checa se a execução do Close() durante o reconhecimento retorna resultado vazio
        /// </summary>
        [TestMethod]
        public void CloseWhileRecognize()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new BufferAudioSource(File.ReadAllBytes(TestsReferences.AudioCpf));
            List<RecognitionResult> results = null;

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                speechRecognizer.Recognize(audioSource, lModelLst);

                Thread.Sleep(1000);

                speechRecognizer.Close();
                results = speechRecognizer.WaitRecognitionResult();
            }

            Assert.AreEqual(0, results.Count);
        }

        /// <summary>
        /// Checa se a execução do Close() sem reconhecimento não gera erros
        /// </summary>
        [TestMethod]
        public void CloseWithoutRecognize()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                speechRecognizer.Close();
            }
        }

        /// <summary>
        /// Checa se a execução do CancelRecognition() durante o reconhecimento retorna resultado vazio
        /// </summary>
        [TestMethod]
        public void CancelWhileRecognize()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new BufferAudioSource(File.ReadAllBytes(TestsReferences.AudioCpf));
            List<RecognitionResult> results = null;

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                speechRecognizer.Recognize(audioSource, lModelLst);

                Thread.Sleep(1000);

                speechRecognizer.CancelRecognition();
                results = speechRecognizer.WaitRecognitionResult();
            }

            Assert.AreEqual(0, results.Count);
        }

        /// <summary>
        /// Checa se a execução do CancelRecognition() sem reconhecimento não gera erros
        /// </summary>
        [TestMethod]
        public void CancelNoRecognize()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                speechRecognizer.CancelRecognition();
            }
        }

        /// <summary>
        /// Checa se a execução do método WaitRecognitionResult() sem reconhecimento não retorna erros
        /// </summary>
        [TestMethod]
        public void WaitNoRecognize()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            List<RecognitionResult> results = null;

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                results = speechRecognizer.WaitRecognitionResult();
            }

            Assert.AreEqual(0, results.Count);
        }

        /// <summary>
        /// Checa se uma segunda chamada do WaitRecognitionResult() retorna resultado vazio e sem atraso
        /// </summary>
        [TestMethod]
        public void WaitRecognitionResultDuplicate()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var lModelLst = new LanguageModelList();
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);

            //Just initializing variables with same value
            DateTime initWait = DateTime.Now;
            DateTime endWait = new DateTime(initWait.Ticks);
            double stampInMilliseconds;

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                speechRecognizer.Recognize(audioSource, lModelLst);

                var firstResult = speechRecognizer.WaitRecognitionResult();
                Assert.AreEqual(CPqDASR.RecognitionResultCode.RECOGNIZED, firstResult[0].ResultCode);

                initWait = DateTime.Now;
                var duplicatedResult = speechRecognizer.WaitRecognitionResult();
                endWait = DateTime.Now;

                stampInMilliseconds = (endWait - initWait).TotalMilliseconds;

                Assert.AreEqual(0, duplicatedResult.Count);
                Assert.IsTrue(0 < stampInMilliseconds && stampInMilliseconds < 5);
            }
        }

        /// <summary>
        /// Checa se a chamada de um segundo Recognize() gera exceção, mas não afeta o resultado do primeiro
        /// </summary>
        [TestMethod]
        public void DuplicateRecognize()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);
            var lModelLst = new LanguageModelList();
            List<RecognitionResult> results = null;

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                speechRecognizer.Recognize(audioSource, lModelLst);

                Thread.Sleep(500);
                try
                {
                    speechRecognizer.Recognize(audioSource, lModelLst);
                }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(RecognitionException));
                }
                results = speechRecognizer.WaitRecognitionResult();
            }

            Assert.AreEqual(CPqDASR.RecognitionResultCode.RECOGNIZED, results[0].ResultCode);

        }

        /// <summary>
        /// Checa estabilidade com múltiplos reconhecimentos mantendo conexões abertas desde o início ao fim da interação com o servidor
        /// </summary>
        [TestMethod]
        public void MultipleRecognize()
        {
            var config = CreateClientConfigDefault(CreateConfigDefault());

            config.ConnectOnRecognize = false; // Valor padrão
            config.AutoClose = false; // Valor padrão

            ExecuteMultiplesRecognitions(config, 4);
        }

        /// <summary>
        /// Checa estabilidade com múltiplos reconhecimentos mantendo conexões abertas desde o início do reconhecimento
        /// ao fim da interação com o servidor
        /// </summary>
        [TestMethod]
        public void MultiplesConnectOnRecognize()
        {
            var config = CreateClientConfigDefault(CreateConfigDefault());

            config.ConnectOnRecognize = true;
            config.AutoClose = false;

            ExecuteMultiplesRecognitions(config, 4);
        }

        /// <summary>
        /// Checa estabilidade com múltiplos reconhecimentos mantendo conexões abertas desde o início da interação com o servidor
        /// e fechando ao término do reconhecimento
        /// </summary>
        [TestMethod]
        public void MultiplesAutoClose()
        {
            var config = CreateClientConfigDefault(CreateConfigDefault());

            config.ConnectOnRecognize = false;
            config.AutoClose = true;

            ExecuteMultiplesRecognitions(config, 4);
        }

        /// <summary>
        /// Checa se o resultado é recuperado normalmente após ocorrer timeout de sessão por falta de uso
        /// </summary>
        [TestMethod]
        public void SessionTimeout()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);
            var lModelLst = new LanguageModelList();
            List<RecognitionResult> results = null;

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                speechRecognizer.Recognize(audioSource, lModelLst);

                Thread.Sleep(65000);

                results = speechRecognizer.WaitRecognitionResult();
            }

            var score = results?.
                Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).
                FirstOrDefault().Alternatives.
                Where(a => a.Confidence >= 90).FirstOrDefault()?.Confidence;
            Assert.IsNotNull(score);

            var textFromFirstAlternative = results[0].Alternatives[0].Text.ToString();
            Assert.AreEqual(TestsReferences.TextPizzaVeg, textFromFirstAlternative);
        }

        /// <summary>
        /// Checa se é possível fazer um reconhecimento após ocorrer timeout de sessão por falta de uso
        /// </summary>
        [TestMethod]
        public void RecogAfterSessionTimeout()
        {
            var clientConfig = CreateClientConfigDefault(CreateConfigDefault());
            var audioSource = new FileAudioSource(TestsReferences.AudioPizzaVeg);
            var lModelLst = new LanguageModelList();
            List<RecognitionResult> results = null;

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                Thread.Sleep(65000);

                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                speechRecognizer.Recognize(audioSource, lModelLst);
                results = speechRecognizer.WaitRecognitionResult();
            }

            var score = results?.
                Where(r => r.ResultCode == CPqDASR.RecognitionResultCode.RECOGNIZED).
                FirstOrDefault().Alternatives.
                Where(a => a.Confidence >= 90).FirstOrDefault()?.Confidence;
            Assert.IsNotNull(score);

            var textFromFirstAlternative = results[0].Alternatives[0].Text.ToString();
            Assert.AreEqual(TestsReferences.TextPizzaVeg, textFromFirstAlternative);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void BasicContinuousModeOn()
        {
            var recogConfig = new RecognitionConfig
            {
                ContinuousMode = true
            };

            var clientConfig = CreateClientConfigDefault(recogConfig);
            var lModelLst = new LanguageModelList();
            List<RecognitionResult> results = null;
            int i = 0;
            List<string> segmentsText = new List<string>(new string[] {
                TestsReferences.TextContinuousModeSeg1,
                TestsReferences.TextContinuousModeSeg2,
                TestsReferences.TextContinuousModeSeg3 });

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                var audioSource = new FileAudioSource(File.ReadAllBytes(TestsReferences.AudioContinuosMode));
                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                speechRecognizer.Recognize(audioSource, lModelLst);

                results = speechRecognizer.WaitRecognitionResult();
                Assert.IsTrue(segmentsText.Count() + 1 == results.Count());

                for (i = 0; i < segmentsText.Count(); i++)
                {
                    Assert.AreEqual(CPqDASR.RecognitionResultCode.RECOGNIZED, results[i].ResultCode);
                    var textFromFirstAlternative = results[i].Alternatives[0].Text.ToString();
                    Assert.AreEqual(segmentsText[i], textFromFirstAlternative);
                }
                Assert.AreEqual(CPqDASR.RecognitionResultCode.NO_SPEECH, results[i].ResultCode);
            }

            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(clientConfig))
            {
                var audioSource = new BufferAudioSource(File.ReadAllBytes(TestsReferences.AudioContinuosMode));

                lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                speechRecognizer.Recognize(audioSource, lModelLst);

                results = speechRecognizer.WaitRecognitionResult();
                Assert.IsTrue(segmentsText.Count() + 1 == results.Count());

                for (i = 0; i < segmentsText.Count(); i++)
                {
                    Assert.AreEqual(CPqDASR.RecognitionResultCode.RECOGNIZED, results[i].ResultCode);
                    var textFromFirstAlternative = results[i].Alternatives[0].Text.ToString();
                    Assert.AreEqual(segmentsText[i], textFromFirstAlternative);
                }
                Assert.AreEqual(CPqDASR.RecognitionResultCode.NO_INPUT_TIMEOUT, results[i].ResultCode);
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

        private void ExecuteMultiplesRecognitions(ClientConfig config, int recogs, bool useEndSleep = true)
        {
            using (SpeechRecognizer speechRecognizer = SpeechRecognizer.Create(config))
            {
                for (int i = 0; i < recogs; i++)
                {
                    var audioSource = new FileAudioSource(TestsReferences.AudioCpf);
                    var lModelLst = new LanguageModelList();
                    lModelLst.AddFromUri(TestsReferences.FreeLanguageModel);
                    speechRecognizer.Recognize(audioSource, lModelLst);
                    var result = speechRecognizer.WaitRecognitionResult();

                    Assert.AreEqual(CPqDASR.RecognitionResultCode.RECOGNIZED, result[0].ResultCode);

                    if (i < recogs - 1)
                    {
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        Thread.Sleep(6000);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private RecognitionConfig CreateConfigDefault()
        {
            return new RecognitionConfig(){};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recogConfig"></param>
        /// <returns></returns>
        private ClientConfig CreateClientConfigDefault(RecognitionConfig recogConfig)
        {
            return CreateClientConfigDefault(recogConfig, TestsReferences.InternalASRURL);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recogConfig"></param>
        /// <returns></returns>
        private ClientConfig CreateClientConfigDefault(RecognitionConfig recogConfig, string serverUrl)
        {
            return new ClientConfig()
            {
                ServerUrl = serverUrl,
                RecogConfig = recogConfig,
                UserAgent = "desktop=x64; os=Some Crazy Windows; client=.NET Core; app=CPqDASRUnitTest",
                MaxWaitSeconds = 20000,
                ConnectOnRecognize = false
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recogConfig"></param>
        /// <param name="serverUri"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private ClientConfig CreateClientWithCredentials(RecognitionConfig recogConfig, string serverUri, string user, string password)
        {
            var credentials = new Credentials()
            {
                UserName = user,
                Password = password
            };

            var clientConfig = CreateClientConfigDefault(recogConfig, serverUri);
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

            while (!Events.IsAllEventsPassed() && !timeout)
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
            timeout = true;
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
