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

namespace CPqDAsr.Entities
{
    /// <summary>
    /// Class to an internal processing error
    /// </summary>
    internal class InternalProcessingError
    {
        internal InternalProcessingError ()
        {

        }

        /// <summary>
        /// Construct to load properties
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <param name="errorCode">Error code</param>
        internal InternalProcessingError (string errorMessage, int errorCode)
        {
            this.ErrorMessage = errorMessage;
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Error message
        /// </summary>
        internal string ErrorMessage;

        /// <summary>
        /// Error code
        /// </summary>
        internal int ErrorCode;
    }
}
