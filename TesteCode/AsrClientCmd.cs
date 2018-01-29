using CPqDASR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TesteCode
{
    public class AsrClientCmd
    {
        public class AudioFile
        {
            public AudioFile(string strName, string strText)
            {
                this.Name = strName;
                this.Text = strText;
            }

            public string Name { get; set; }
            public string Text { get; set; }
        }

        /// <summary>
        /// Propriedades de execução
        /// </summary>
        private Propeties objProps;

        /// <summary>
        /// Buffer a ser utilizado
        /// </summary>
        //private byte[] AudioBuffer;

        private List<String> counters;

        /// <summary>
        /// Quantidade de erros ocorridos nos testes
        /// </summary>
        private int intErrors = 0;

        /// <summary>
        /// Quantidade de resultados obtidos
        /// </summary>
        private int intResults = 0;

        private long TotalMilli = 0;
        private long QtdTotalSessions = 0;

        Random random = new Random();

        private Dictionary<int, AudioFile> dicAudios;

        public AsrClientCmd(String propsFile)
        {
            objProps = new Propeties();
            counters = new List<string>();

            Dictionary<string, string> props = new Dictionary<string, string>();
            string[] properties = File.ReadAllLines(propsFile);


            foreach (string value in properties)
            {
                if (value.IndexOf("#") == -1 && value.IndexOf("=") != -1)
                {
                    string[] item = value.Split('=');
                    props.Add(item[0].Trim(), item[1].Trim());
                }
            }

            objProps.AsrApiUrl = GetProperty(props, "servercmd.apiurl", null);
            if (objProps.AsrApiUrl == null)
            {
                Console.WriteLine("servercmd.apiurl not informed");
                return;
            }

            objProps.AudioPath = GetProperty(props, "servercmd.audiopath", null);
            if (objProps.AudioPath == null)
            {
                Console.WriteLine("servercmd.audiopath not informed");
                return;
            }

            objProps.LMUri = GetProperty(props, "servercmd.lmuri", null);
            if (objProps.LMUri == null)
            {
                Console.WriteLine("servercmd.lmuri not informed");
                return;
            }
            objProps.UserName = GetProperty(props, "servercmd.username", "");
            objProps.Password = GetProperty(props, "servercmd.password", "");


            objProps.PacketSizeInBytes = int.Parse(GetProperty(props, "servercmd.audio.packetsize", "0"));
            objProps.PacketDelayInMillis = int.Parse(GetProperty(props, "servercmd.audio.packetdelay", "0"));
            objProps.NumSessionStart = int.Parse(GetProperty(props, "servercmd.startsessions", "1"));
            objProps.NumSessionEnd = int.Parse(GetProperty(props, "servercmd.endsessions", "1"));
            objProps.NumRecogs = int.Parse(GetProperty(props, "servercmd.recognitions", "1"));
            objProps.NumExecutions = int.Parse(GetProperty(props, "servercmd.executions", "1"));


            dicAudios = new Dictionary<int, AudioFile>();
            dicAudios.Add(1, new AudioFile("pizza-queijo-8k.wav", "quero pizza de queijo"));
            dicAudios.Add(2, new AudioFile("pizza-margherita-8k.wav", "quero pizza marguerita"));
            dicAudios.Add(3, new AudioFile("pizza-veg-8k.wav", "eu quero uma pizza vegetariana por favor"));
            dicAudios.Add(4, new AudioFile("pizza-pedra-8k.wav", "eu quero uma pizza de madri por favor"));

        }

        /// <summary>
        /// Verifica se o valor passado é vazio, caso seja é retornado o valor defaul informado e caso contrario é retornado o próprio valor
        /// </summary>
        /// <param name="dic">Dicionário a ser procurado os valores</param>
        /// <param name="value">Valor a ser validado</param>
        /// <param name="defaultValue">Valor default</param>
        /// <returns></returns>
        private string GetProperty(Dictionary<string, string> dic, string value, string defaultValue)
        {
            if (dic.ContainsKey(value))
            {
                if (dic[value] == null || value == string.Empty)
                    return defaultValue;
                else
                    return dic[value];
            }
            else
                return defaultValue;
        }

        public void runTest()
        {

            Console.WriteLine("Starting test...");

            Console.WriteLine("API URL: " + objProps.AsrApiUrl);
            Console.WriteLine("Audio path: " + objProps.AudioPath);
            Console.WriteLine("Number of sessions (start): " + objProps.NumSessionStart);
            Console.WriteLine("Number of sessions (end): " + objProps.NumSessionEnd);
            Console.WriteLine("Recognitions per session: " + objProps.NumRecogs);
            Console.WriteLine("Executions: " + objProps.NumExecutions);

            //AudioBuffer = File.ReadAllBytes(objProps.AudioPath);

            for (int numSess = objProps.NumSessionStart; numSess <= objProps.NumSessionEnd; numSess++)
            {
                ClearCounters();
                for (long r = 0; objProps.NumExecutions == 0 || r < objProps.NumExecutions; r++)
                {
                    Thread[] threads = new Thread[numSess];
                    for (int s = 0; s < numSess; s++)
                    {
                        //SessionTask obj = new SessionTask("session " + (s + 1) + "/" + (numSess) + " execution " + (r + 1));
                        threads[s] = new Thread(StartRecognized);
                        threads[s].Name = s.ToString();
                        threads[s].Start();
                    }
                    for (int s = 0; s < numSess; s++)
                    {
                        try
                        {
                            threads[s].Join();
                        }
                        catch (ThreadInterruptedException e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
                Console.WriteLine("######## AVERAGE TIME (sessions=" + numSess + ")");


                //foreach (string key in counters)
                //{
                //    Console.WriteLine("> " + key + " (ms): " + getAverageTime(key));
                //}
                Console.WriteLine();
                //Console.Write("Counters: ");
                //foreach (string key in counters)
                //{
                //    Console.Write(key + " ");
                //}
                //Console.WriteLine();
                //Console.Write("Values: ");
                //foreach (String key in counters)
                //{
                //    Console.Write(getAverageTime(key) + " ");
                //}
                Console.WriteLine("AVERAGE (ms): " + new TimeSpan(TotalMilli / QtdTotalSessions).TotalMilliseconds);
                Console.WriteLine();
                Console.Write("Erros: ");
                Console.Write(intErrors.ToString());
                Console.WriteLine();
                Console.Write("Resultados: ");
                Console.Write(intResults.ToString());
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private int intTestando = 0;
        int ID;
        private void StartRecognized()
        {
            intTestando += 1;
            Trace.WriteLine("Iniciando: " + intTestando.ToString());
            SincronoASR obj = new SincronoASR(objProps.AsrApiUrl, objProps.UserName, objProps.Password);
            obj.OnError += obj_OnError;
            obj.OnResult += obj_OnResult;


            bool bolRetorno = obj.Open(objProps.AsrApiUrl, objProps.UserName, objProps.Password);

            if (bolRetorno)
            {
                for (int NumSession = 0; NumSession < objProps.NumRecogs; NumSession++)
                {
                    QtdTotalSessions += 1;
                    DateTime dtInicio = DateTime.Now;
                    long lngStart = dtInicio.Ticks;

                    AudioFile objAudio = dicAudios[random.Next(1, dicAudios.Count)];
                    byte[] b = File.ReadAllBytes(string.Concat(objProps.AudioPath, objAudio.Name));
                    obj.StartRecognition(objProps.LMUri, b, objAudio.Text);
                    DateTime dtFim = DateTime.Now;
                    long lngFinal = dtFim.Ticks;
                    long Total = lngFinal - lngStart;
                    Console.WriteLine(string.Format("Hora Inicio: {0}, Hora Fim: {1} Inicio: {2} - Final: {3} - Total: {4} - Total (ms): {5}", dtInicio.ToString("HH:mm:ss.ffff"), dtFim.ToString("HH:mm:ss.ffff"), lngStart, lngFinal, Total, new TimeSpan(Total).TotalMilliseconds));
                    TotalMilli += Total;
                }
                obj.Close();
                intTestando -= 1;
                Trace.WriteLine("Fechando: " + intTestando.ToString());
            }
            else
                Console.WriteLine("Problema na inicialização do servidor");
        }

        private void obj_OnResult(object sender, string Message)
        {
            intResults += 1;
        }

        private void obj_OnError(object sender, string Message)
        {
            intErrors += 1;
        }

        public void ClearCounters()
        {
            //    totalCountMap.clear();
            //    totalTimeMap.clear();
            counters.Clear();
        }

        //public long getAverageTime(String key) {
        //    return totalTimeMap.get(key).get() / totalCountMap.get(key).get();
        //}

    }
}
