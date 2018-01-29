using System;
using System.Collections.Generic;
using System.Text;

namespace CPqDAsr.Entities
{
    /// <summary>
    /// Represents a Language Model.
    /// </summary>
    public class LanguageModel
    {

        /// <summary>
        /// URI for the LM file location.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// LM definition, in XML or BNF format.
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// The LM definition format type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The LM identification string.
        /// </summary>
        public string Id { get; set; }

        public LanguageModel()
        {

        }

        public LanguageModel(string uri) : this()
        {
            this.Uri = uri;
        }
    }
}
