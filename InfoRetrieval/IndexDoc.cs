using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    ///  Class which represents the data of a document for indexing
    /// </summary>
    class IndexDoc
    {
        /// <summary>
        /// fields of a IndexDoc
        /// </summary>
        public int m_mxTF { get; private set; }
        public int m_uniqueCounter { get; private set; }
        public StringBuilder m_City { get; private set; }

        /// <summary>
        /// constructor of IndexDoc
        /// </summary>
        /// <param name="m_mxTF">the max instances of unique term in a current document</param>
        /// <param name="m_uniqueCounter">the number of unique terms in a current document</param>
        /// <param name="m_City">the city which represents the document</param>
        public IndexDoc(int m_mxTF, int m_uniqueCounter, StringBuilder m_City)
        {
            this.m_mxTF = m_mxTF;
            this.m_uniqueCounter = m_uniqueCounter;
            this.m_City = m_City;
        }

        /// <summary>
        /// method for writing to cities index file
        /// </summary>
        /// <returns>stringbuilder for writing to cities index file</returns>
        public StringBuilder PrintDocumentData()
        {
            return new StringBuilder("(#)" + "mxTF: " + m_mxTF + "(#)" + "unique: " + m_uniqueCounter + "(#)" + "city: " + m_City);
        }
    }
}
