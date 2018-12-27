using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents all instances of a term in a doument
    /// </summary>
    public class Term
    {
        /// <summary>
        /// fields of a Term
        /// </summary>
        public string m_value { get; private set; }
        public int m_tf { get; private set; }  // num of instances of m_value in current m_DOCNO
        public string m_DOCNO { get; private set; }
        public StringBuilder m_positions { get; private set; }

        /// <summary>
        /// constructor of a Term
        /// </summary>
        /// <param name="m_value">the value of the term</param>
        /// <param name="docno">the id of a document</param>
        /// <param name="newPOS">the postion of the term in the document</param>
        public Term(string m_value, string docno, int newPOS)
        {
            this.m_value = m_value;
            this.m_tf = 1;
            this.m_positions = new StringBuilder("" + newPOS);
            this.m_DOCNO = docno;
        }

        /// <summary>
        /// method to add a new postion of the term
        /// </summary>
        /// <param name="newPos">the new position to add</param>
        public void AddNewIndex(int newPos)
        {
            m_positions.Append(" " + newPos);
            this.m_tf++;
        }

        /// <summary>
        /// method for writing to posting file
        /// </summary>
        /// <returns>stringbuilder for writing to posting file</returns>
        public StringBuilder WriteDocumentToPostingFileTerm()
        {
            return new StringBuilder(m_DOCNO + "(#)" + m_tf + "(#)" + m_positions);
        }

    }
}
