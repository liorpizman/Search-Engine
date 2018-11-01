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

        public Document(string m_DOCNO, string m_DATE1, string m_TI, string m_TEXT)
        {
            this.m_DOCNO = m_DOCNO;
            this.m_DATE1 = m_DATE1;
            this.m_TI = m_TI;
            this.m_TEXT = m_TEXT;
        }

    }
}
