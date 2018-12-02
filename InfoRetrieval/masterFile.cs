using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents a collection of files in the corpus
    /// </summary>
    public class masterFile
    {
        /// <summary>
        /// fields of a masterFile
        /// </summary>
        public Dictionary<string, Document> m_documents;
        public string m_fileName;
        public string m_path;
        public int m_docAmount;

        /// <summary>
        /// constructor of  masterFile
        /// </summary>
        /// <param name="m_fileName">first file's name in the collection</param>
        /// <param name="m_path">path of first file in the collection</param>
        public masterFile(string m_fileName, string m_path)
        {
            this.m_documents = new Dictionary<string, Document>();
            this.m_fileName = m_fileName;
            this.m_path = m_path;
            this.m_docAmount = 0;
        }
    }
}
