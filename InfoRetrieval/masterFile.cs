using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    public class masterFile
    {
        public Dictionary<string, Document> m_documents;
        public string m_fileName;
        public string m_path;
        public int m_docAmount;

        public masterFile(string m_fileName, string m_path)
        {
            this.m_documents = new Dictionary<string, Document>();
            this.m_fileName = m_fileName;
            this.m_path = m_path;
            this.m_docAmount = 0;
        }
    }
}
