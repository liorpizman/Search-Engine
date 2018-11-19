using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Document
    {
        public StringBuilder m_DOCNO;
        public StringBuilder m_DATE1;
        public StringBuilder m_TI;
        public StringBuilder m_TEXT;
        public Dictionary<string, int> m_termsInDictionary; // all terms (strings) in document with (int) counter of instances

        public Document(StringBuilder m_DOCNO, StringBuilder m_DATE1, StringBuilder m_TI, StringBuilder m_TEXT)
        {
            this.m_DOCNO = m_DOCNO;
            this.m_DATE1 = m_DATE1;
            this.m_TI = m_TI;
            this.m_TEXT = m_TEXT;
            this.m_termsInDictionary = new Dictionary<string, int>();
        }


        public StringBuilder writeDocumentToIndexFile()
        {
            StringBuilder data = new StringBuilder();
            string pair = "";
            data.Append(m_DOCNO);
            data.Append("\t");
            data.Append(m_DATE1);
            data.Append("\t");
            data.Append(m_TI);
            //data.Append("\t");
            data.Append(Environment.NewLine);
            foreach (string key in m_termsInDictionary.Keys)
            {
                pair = key + "\t" + m_termsInDictionary[key];// + "\t";
                data.Append(pair);
                data.Append(Environment.NewLine);
            }
            return data;
        }

    }
}
