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

using CPqDASR.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CPqDASR.Entities
{
    public class RecognitionResult
    {

        /// <summary>
        /// Identificação da sessão
        /// </summary>
        internal string Handle { get; set; }

        /// <summary>
        /// the recognition result code.
        /// </summary>
        public RecognitionResultCode? ResultCode { get; set; }

        /// <summary>
        /// the speech segment index when operating in continuous mode.
        /// </summary>
        [JsonProperty("segment_index")]
        public int SpeechSegmentIndex { get; set; }

        /// <summary>
        /// indicates if this is the last recognized segment, when operating in
        /// continuous mode.
        /// </summary>
        [JsonProperty("last_segment")]
        public bool LastSpeechSegment { get; set; }

        /// <summary>
        /// Flag indicating that is the final
 
        /// result
        /// </summary>
        [JsonProperty("final_result")]
        public bool FinalResult { get; set; }

        /// <summary>
        /// the audio position when the speech start was detected (in milis).
        /// </summary>
        public int SentenceStartTimeMilliseconds { get; set; }

        /// <summary>
        /// the audio position when the speech stop was detected (in milis).
        /// </summary>
        public int SentenceEndTimeMilliseconds { get; set; }

        /// <summary>
        /// the list of recognition result alternative sentences.
        /// </summary>
        public List<RecognitionAlternative> Alternatives { get; set; } = new List<RecognitionAlternative>();

        [JsonProperty("alternatives")]
        private List<Protocol.Alternatives> AltervativeJson
        {
            set
            {
                Alternatives = getAlternatives(value);
            }
        }

        /// <summary>
        /// Recebe o tempo de inicio da API em float e converte para inteiro
        /// </summary>
        [JsonProperty("start_time")]
        private float StartTime
        {
            set
            {
                SentenceStartTimeMilliseconds = (int)(value * 1000);
            }
        }

        /// <summary>
        /// Recebe o tempo de inicio da API em float e converte para inteiro
        /// </summary>
        [JsonProperty("end_time")]
        private float EndTime
        {
            set
            {
                SentenceEndTimeMilliseconds = (int)(value * 1000);
            }
        }


        /// <summary>
        /// Formata a lista retronada pelo servidor ASR em objetos conhecidos pela API
        /// </summary>
        /// <param name="lstAlternatives"></param>
        /// <returns></returns>
        private List<RecognitionAlternative> getAlternatives(List<Alternatives> lstAlternatives)
        {
            List<RecognitionAlternative> lstRecognitionAlternative = new List<RecognitionAlternative>();
            try
            {
                foreach (Alternatives obj in lstAlternatives)
                {
                    RecognitionAlternative objRecognitionAlternative = new RecognitionAlternative()
                    {
                        Confidence = obj.Score,
                        LanguageModel = obj.LanguageModel,
                        Text = obj.Text
                    };

                    //Monta o objeto de interpretação
                    if (obj.Interpretations != null)
                        for (int i = 0; i < obj.Interpretations.Count; i++)
                        {
                            Interpretation objInterpretation = new Interpretation()
                            {
                                InterpretationJson = obj.Interpretations[i].ToString(),
                                InterpretationConfidence = obj.InterpretationScores[i]
                            };
                            objRecognitionAlternative.Interpretations.Add(objInterpretation);
                        }

                    //Monta o objeto de palavras
                    if (obj.Words != null)
                        foreach (Word word in obj.Words)
                        {
                            CPqDAsr.Entities.Word objWord = new CPqDAsr.Entities.Word()
                            {
                                Text = word.Text,
                                Confidence = word.Score,
                                StartTime = word.StartTime,
                                EndTime = word.EndTime
                                
                            };

                            objRecognitionAlternative.Words.Add(objWord);

                        }

                    lstRecognitionAlternative.Add(objRecognitionAlternative);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lstRecognitionAlternative;
        }
    }
}
