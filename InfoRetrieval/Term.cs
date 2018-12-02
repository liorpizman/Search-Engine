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
        public string m_value;
        public int m_tf;  // num of instances of m_value in current m_DOCNO
        public string m_DOCNO;
        public StringBuilder m_positions; //maybe list

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
            //add poisition index add in the constructor for the first step
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
            //document num of current term 
            //tf - term frequency in current doc
            //all the indexes of the current term
        }


        public StringBuilder WritePositionsToCitiesIndex()
        {
            return new StringBuilder(m_DOCNO + "(#)" + m_positions);
            //document num of current term 
            //tf - term frequency in current doc
            //all the indexes of the current term
        }
    }
}
