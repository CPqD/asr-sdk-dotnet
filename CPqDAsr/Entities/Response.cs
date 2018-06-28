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

using CPqDASR.Extensions;
using CPqDASR.Config;

namespace CPqDASR.Entities
{
    public class Response
    {
        public string Handle { get; set; }
        public WS_COMMANDS Method { get; set; }
        public string Result { get; set; }
        public string SessionStatus { get; set; }
        public string Message { get; set; }
        public int Expires { get; set; }
        public string ErrorCode { get; set; }
        public RecognitionConfig RecognitionConfig { get; set; }

        public Response()
        {
            RecognitionConfig = new RecognitionConfig();
        }

        /// <summary>
        /// Verifica se o valor retornado foi com sucesso
        /// </summary>
        public bool IsSuccess
        {
            get
            {
                return this.Result.ToEnum<RESPONSE_RESULTS>() == RESPONSE_RESULTS.SUCCESS;
            }
        }
    }
}
