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
    public class DocumentsTerm
    {
        /// <summary>
        /// fields of DocumentTerms
        /// </summary>
        public string m_valueOfTerm { get; set; }
        public Dictionary<string, Term> m_Terms { get; private set; } // df- m_Terms.length
        public int line { get; set; }
        public int postNum { get; private set; }
        public static Hashtable m_postingNums = new Hashtable()
        {
            {'a', 1 }, {'b', 2 }, {'c', 3 },{'d', 4 }, //{ "", "0" },
            { 'e', 5 }, {'f', 6 }, {'g', 7 }, {'h', 8 },{'i', 9 },
            { 'j', 10 }, {'k', 11 }, {'l', 12 }, {'m', 13 }, {'n', 14 },
            {'o', 15 }, {'p', 16 },{'q', 17 },{'r', 18 }, {'s', 19 },
            {'t', 20 }, {'u', 21 },{'v', 22 },{'w', 23 }, {'x', 24 },
            { 'y', 25 } ,{'z', 26 }
        };

        /// <summary>
        /// constructor of DocumentTerms
        /// </summary>
        /// <param name="value">the vale of the term</param>
        public DocumentsTerm(string value)
        {
            this.m_valueOfTerm = value;
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
            }
            foreach (KeyValuePair<string, Term> pair in m_Terms)
            {
                data.Append("[#]" + pair.Value.WriteDocumentToPostingFileTerm());
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
                this.postNum = 0;
                return;
            }
            char c = char.ToLower(m_valueOfTerm[0]);
            if (char.IsLetter(c))
            {
                this.postNum = (int)m_postingNums[c];
            }
            else
            {
                this.postNum = 0;
            }
        }

        /// <summary>
        /// method to get the total Frequency of a term
        /// </summary>
        /// <returns></returns>
        public int countTotalFrequency()
        {
            int sol = 0;
            foreach (Term term in m_Terms.Values)
            {
                sol += term.m_tf;
            }
            return sol;
        }
    }
}
