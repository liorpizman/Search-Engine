using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents a Document in a File
    /// </summary>
    public class Document
    {
        /// <summary>
        /// fields of a Document
        /// </summary>
        //public Dictionary<string, int> m_termsInDictionary { get; private set; }// all terms (strings) in document with (int) counter of instances
        public string m_DOCNO { get; private set; }
        public StringBuilder m_DATE1 { get; private set; }
        public StringBuilder m_TI { get; private set; }
        public StringBuilder m_CITY { get; private set; }
        public StringBuilder m_language { get; private set; }
        public string m_TEXT { get; private set; }
        public int m_uniqueCounter { get; set; }
        public int m_maxTF { get; set; }

        /// <summary>
        /// constructor of a Document
        /// </summary>
        /// <param name="m_DOCNO">document's id</param>
        /// <param name="m_DATE1">document's date</param>
        /// <param name="m_TI">document's title</param>
        /// <param name="m_TEXT">document's text</param>
        /// <param name="m_CITY">document's city</param>
        public Document(string DOCNO, StringBuilder DATE1, StringBuilder TI, string TEXT, StringBuilder CITY, StringBuilder language)
        {
            this.m_DOCNO = DOCNO;
            this.m_DATE1 = DATE1;
            this.m_TI = TI;
            this.m_TEXT = TEXT;
            this.m_CITY = CITY;
            this.m_language = language;
            this.m_uniqueCounter = 0;
            this.m_maxTF = 0;
            //this.m_termsInDictionary = new Dictionary<string, int>();               // we should check if we need delete this??
        }
        public StringBuilder WriteDocumentToIndexFile()
        {
            string title = m_TI.ToString();
            if (title.Contains('\n'))
            {
                title = title.Split('\n')[0];
            }
            StringBuilder data = new StringBuilder(m_DOCNO + " (#)" + "TI: " + title + " (#)" + "unique words: " + m_uniqueCounter + " (#)" + "maxTF: " + m_maxTF);
            if (!m_CITY.ToString().Equals(""))
            {
                string[] city = m_CITY.ToString().Split(' ');
                if (city.Length > 1)
                {
                    data.Append(" city: " + city[0].ToUpper());
                    return data;
                }
                else
                {
                    data.Append(" city: " + m_CITY);
                    return data;
                }
            }
            else
            {
                data.Append(" city:---- ");
                return data;
            }
        }
    }
}