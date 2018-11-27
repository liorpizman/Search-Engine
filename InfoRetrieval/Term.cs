using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Term
    {
        public string m_value;
        public int m_tf;  // num of instances of m_value in current m_DOCNO
        public string m_DOCNO;
        public StringBuilder m_positions; //maybe list

        public Term(string m_value, string docno, int newPOS)
        {
            this.m_value = m_value;
            this.m_tf = 1;
            this.m_positions = new StringBuilder();
            //add poisition index add in the constructor for the first step
            this.m_DOCNO = docno;
        }

        public void AddNewIndex(int newPos)
        {
            m_positions.Append(newPos + " ");
            this.m_tf++;
        }

        public StringBuilder WriteDocumentToPostingFileTerm()
        {
            return new StringBuilder("\t" + m_DOCNO + "\t" + m_tf + "\t" + m_positions);
            //document num of current term 
            //tf - term frequency in current doc
            //all the indexes of the current term
        }

    }
}
