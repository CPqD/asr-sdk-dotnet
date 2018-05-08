using System;
using System.Collections.Generic;
using System.Text;

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
