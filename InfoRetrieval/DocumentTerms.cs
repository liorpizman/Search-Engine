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
        public int line;
        public int postNum;


        public DocumentTerms(string value)
        {
            this.m_valueOfTerm = value;
            this.m_tfc = 0;
            this.m_Terms = new Dictionary<string, Term>();
            initPostNumer();
            line = Int32.MaxValue;
        }

        public void AddToDocumentDictionary(Term term)
        {
            this.m_Terms.Add(term.m_DOCNO, term); //this.m_Terms.Add(term.m_value, term);
            this.m_tfc++;
        }

        public StringBuilder WriteToPostingFileDocDocTerm(bool printed)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, Term> pair in m_Terms)
            {
                if (!printed)
                {
                    data.Append(m_valueOfTerm);
                }
                data.Append("\t");
                data.Append(pair.Value.WriteDocumentToPostingFileTerm());
            }
            return data;
        }

        public void initPostNumer()
        {
            if (m_valueOfTerm == "")
            {
                postNum = 26;
            }
            else
            {
                char c = m_valueOfTerm.ToLower()[0];
                if (c == 'a')
                    postNum = 0;
                else if (c == 'b')
                    postNum = 1;
                else if (c == 'c')
                    postNum = 2;
                else if (c == 'd')
                    postNum = 3;
                else if (c == 'e')
                    postNum = 4;
                else if (c == 'f')
                    postNum = 5;
                else if (c == 'g')
                    postNum = 6;
                else if (c == 'h')
                    postNum = 7;
                else if (c == 'i')
                    postNum = 8;
                else if (c == 'j')
                    postNum = 9;
                else if (c == 'k')
                    postNum = 10;
                else if (c == 'l')
                    postNum = 11;
                else if (c == 'm')
                    postNum = 12;
                else if (c == 'n')
                    postNum = 13;
                else if (c == 'o')
                    postNum = 14;
                else if (c == 'p')
                    postNum = 15;
                else if (c == 'q')
                    postNum = 16;
                else if (c == 'r')
                    postNum = 17;
                else if (c == 's')
                    postNum = 18;
                else if (c == 't')
                    postNum = 19;
                else if (c == 'u')
                    postNum = 20;
                else if (c == 'v')
                    postNum = 21;
                else if (c == 'w')
                    postNum = 22;
                else if (c == 'x')
                    postNum = 23;
                else if (c == 'y')
                    postNum = 24;
                else if (c == 'z')
                    postNum = 25;
                else
                {
                    postNum = 26;
                }
            }

        }
    }
}
