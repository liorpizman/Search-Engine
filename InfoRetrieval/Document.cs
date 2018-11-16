using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Document
    {
        public string m_DOCNO;
        public string m_DATE1;
        public string m_TI;
        public string m_TEXT;
        public Dictionary<string, int> m_termsInDictionary; // all terms (strings) in document with (int) counter of instances

        public Document(string m_DOCNO, string m_DATE1, string m_TI, string m_TEXT)
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
