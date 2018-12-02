using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents all instances of a term in a collection of files 
    /// </summary>
    public class DocumentTerms
    {
        /// <summary>
        /// fields of DocumentTerms
        /// </summary>
        public string m_valueOfTerm;
        //public int m_totalFreqCorpus;
        public Dictionary<string, Term> m_Terms;   // df- m_Terms.length
        public int line;
        public int postNum;
        public static Hashtable m_postingNums = new Hashtable()
        {
            {'a', 0 }, {'b', 1 }, {'c', 2 },{'d', 3 }, //{ "", "26" },
            { 'e', 4 }, {'f', 5 }, {'g', 6 }, {'h', 7 },{'i', 8 },
            { 'j', 9 }, {'k', 10 }, {'l', 11 }, {'m', 12 }, {'n', 13 },
            {'o', 14 }, {'p', 15 },{'q', 16 },{'r', 17 }, {'s', 18 },
            {'t', 19 }, {'u', 20 },{'v', 21 },{'w', 22 }, {'x', 23 },
            { 'y', 24 } ,{'z', 25 }
        };

        /// <summary>
        /// constructor of DocumentTerms
        /// </summary>
        /// <param name="value">the vale of the term</param>
        public DocumentTerms(string value)
        {
            this.m_valueOfTerm = value;
            //this.m_totalFreqCorpus = 0;
            this.m_Terms = new Dictionary<string, Term>();
            UpdateCorrectPostNum();
            line = Int32.MaxValue;
        }

        /// <summary>
        /// method to add a new instance of a term in a new document
        /// </summary>
        /// <param name="term">an object which respresents the instance in the new document</param>
        public void AddToDocumentDictionary(Term term)
        {
            this.m_Terms.Add(term.m_DOCNO, term);
        }

        /// <summary>
        /// method for writing to posting file
        /// </summary>
        /// <param name="printed">checks whether the name of the term was printed in the posting</param>
        /// <returns>stringbuilder for writing to posting file</returns>
        public StringBuilder WriteToPostingFileDocDocTerm(bool printed)
        {
            StringBuilder data = new StringBuilder();
            if (!printed)
            {
                data.Append(m_valueOfTerm);
                //printed = true;
            }
            foreach (KeyValuePair<string, Term> pair in m_Terms)
            {
                data.Append("(#)" + pair.Value.WriteDocumentToPostingFileTerm());
            }
            return data;
        }

        public StringBuilder WriteToCitiesIndexFile()
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, Term> pair in m_Terms)
            {
                data.Append("(#)" + pair.Value.WritePositionsToCitiesIndex());
            }
            return data;

        }

        /// <summary>
        /// evaluates the correct post number of the current term
        /// </summary>
        public void UpdateCorrectPostNum()
        {

            if (this.m_valueOfTerm.Equals(""))
            {
                this.postNum = 26;
                return;
            }
            char c = char.ToLower(m_valueOfTerm[0]);
            if (char.IsLetter(c))
            {
                this.postNum = (int)m_postingNums[c];
            }
            else
            {
                this.postNum = 26;
            }
        }

    }
}
