using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class DocumentTerms
    {
        public string m_valueOfTerm;
        public int m_tfc;
        public Dictionary<string, Term> m_Terms;

        public DocumentTerms(string value)
        {
            this.m_valueOfTerm = value;
            this.m_tfc = 0;
            this.m_Terms = new Dictionary<string, Term>();
        }

        public void addToDocumentDictionary(Term term)
        {
            this.m_Terms.Add(term.m_value, term);
            this.m_tfc++;
        }


        public StringBuilder writeToPostingFile()
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, Term> pair in m_Terms)
            {
                pair.Value.writeDocumentToPostingFile();
            }
            return data;
        }
    }
}
