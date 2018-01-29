using System;
using System.Collections.Generic;
using System.Text;

namespace CPqDAsr.ASR
{
    /// <summary>
    /// Represents the language models used during the speech recognition process.
    /// </summary>
    public class LanguageModelList
    {
        /// <summary>
        /// the language model URI list.
        /// </summary>
        internal List<string> UrlList { get; private set; }

        /// <summary>
        /// the inline grammar body list.
        /// </summary>
        internal List<string[]> GrammarList { get; private set; }

        public LanguageModelList()
        {
            this.UrlList = new List<string>();
            this.GrammarList = new List<string[]>();
        }

        /// <summary>
        /// Adds a new language model from its URI.
        /// </summary>
        /// <param name="uri">the languagem model URI.</param>
        public void AddFromUri(string uri)
        {
            if (uri != null)
            {
                if (this.GrammarList.Count > 0)
                {
                    throw new IndexOutOfRangeException("Only one grammar is supported.");
                }
                this.UrlList.Add(uri);
            }
            else
            {
                throw new ArgumentNullException("The parameter URI cannot be NULL.");
            }
        }

        /// <summary>
        /// Adds a new grammar content.
        /// </summary>
        /// <param name="id">the grammar identification.</param>
        /// <param name="body">the grammar body content.</param>
        public void AddInlineGrammar(string id, string body)
        {
            if (this.GrammarList.Count > 0)
            {
                throw new IndexOutOfRangeException("Only one grammar is supported.");
            }
            this.GrammarList.Add(new string[] { id, body });
        }

    }
}
