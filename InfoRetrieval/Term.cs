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
        public int m_amount;  // num of instances of m_value in current m_DOCNO
        public string m_DOCNO;
        public StringBuilder m_positions;

        public Term(string m_value, string docno)
        {
            this.m_value = m_value;
            this.m_amount = 1;     
            this.m_positions = new StringBuilder();
            //add poisition index add in the constructor for the first step
            this.m_DOCNO = docno;
        }

        public void addNewIndex(string newPos)
        {
            m_positions.Append(newPos);
            this.m_amount++;
        }

        public StringBuilder writeDocumentToPostingFile()
        {
            StringBuilder data = new StringBuilder();
            data.Append(m_value);                      //term value
            data.Append("\t");
            data.Append(m_DOCNO);                      //document num of current term 
            data.Append("\t");
            data.Append(m_amount);                     // tf - term frequency in current doc
            data.Append("\t");
            data.Append(m_positions);                  //all the indexes of the current term
            return data;
        }
    }
}
