using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Document
    {
        public Dictionary<string, int> m_termsInDictionary; // all terms (strings) in document with (int) counter of instances
        public string m_DOCNO { get; private set; }
        public StringBuilder m_DATE1 { get; private set; }
        public StringBuilder m_TI { get; private set; }
        public StringBuilder m_CITY { get; private set; }
        public string m_TEXT { get; private set; }
        public int m_uniqueCounter { get; set; }
        public int m_maxTF { get; private set; }


        public Document(string m_DOCNO, StringBuilder m_DATE1, StringBuilder m_TI, string m_TEXT, StringBuilder m_CITY)
        {
            this.m_DOCNO = m_DOCNO;
            this.m_DATE1 = m_DATE1;
            this.m_TI = m_TI;
            this.m_TEXT = m_TEXT;
            this.m_CITY = m_CITY;
            this.m_uniqueCounter = 0;
            this.m_maxTF = 0;
            this.m_termsInDictionary = new Dictionary<string, int>();               // we should check if we need delete this??
        }



        public StringBuilder WriteDocumentToIndexFile()
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
