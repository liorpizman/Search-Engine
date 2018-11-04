using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class DocumentTerms
    {
        public string DOCNO;
        public int m_totalAmount;
        public Dictionary<string, Term> m_Terms;

        public DocumentTerms(string docno)
        {
            this.DOCNO = docno;
            this.m_totalAmount = 0;
            this.m_Terms = new Dictionary<string, Term>();
        }

        public void increaseTotalAmount()
        {
            this.m_totalAmount++;
        }

        public void addToDocumentDictionary(Term term)
        {
            this.m_Terms.Add(term.m_value, term);
        }

    }
}
