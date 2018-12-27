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
        public string m_DOCNO { get; private set; }
        public StringBuilder m_DATE1 { get; private set; }
        public StringBuilder m_TI { get; private set; }
        public StringBuilder m_CITY { get; private set; }
        public StringBuilder m_language { get; private set; }
        public string m_TEXT { get; private set; }
        public int m_uniqueCounter { get; set; }
        public int m_maxTF { get; set; }
        public int m_length { get; set; }
        public Dictionary<string, double> m_Entities { get; set; }
        public StringBuilder m_KFirstWords { get; set; }
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
            this.m_length = 0;
            this.m_Entities = new Dictionary<string, double>();
            this.m_KFirstWords = new StringBuilder();
        }

        /// <summary>
        /// method to get all entities of current document
        /// </summary>
        /// <returns>a string of the entities</returns>
        private string GetEntities()
        {
            StringBuilder sol = new StringBuilder();
            double normalizeByLength = 0;
            normalizeByLength += m_length;
            foreach (KeyValuePair<string, double> Entity in m_Entities)
            {
                sol.Append(" [#] " + Entity.Key + "[*]" + Math.Round((Entity.Value / normalizeByLength), 5)); // normallize by the length of the doc
            }
            return sol.ToString();
        }

        /// <summary>
        /// method to add new word of the first k first words
        /// </summary>
        /// <param name="nextWord">the word to add</param>
        public void AddKWord(string nextWord)
        {
            m_KFirstWords.Append(nextWord + " ");
        }

        /// <summary>
        /// method to write the document to index file
        /// </summary>
        /// <returns>the string to write</returns>
        public StringBuilder WriteDocumentToIndexFile()
        {
            string title = m_TI.ToString();
            if (title.Contains('\n'))
            {
                title = title.Split('\n')[0];
            }
            StringBuilder data = new StringBuilder(m_DOCNO + " (#)" + "TI: " + title + " (#)" + "Kwords:" + m_KFirstWords.ToString() +
                " (#)" + "unique words: " + m_uniqueCounter + " (#)" + "maxTF: " + m_maxTF + " (#)" + "Entities: " + GetEntities() +
                " (#)" + "length: " + m_length);
            if (!m_CITY.ToString().Equals(""))
            {
                string[] city = m_CITY.ToString().Split(' ');
                if (city.Length > 1)
                {
                    data.Append(" (#)city: " + city[0].ToUpper());
                    return data;
                }
                else
                {
                    data.Append(" (#)city: " + m_CITY);
                    return data;
                }
            }
            else
            {
                data.Append(" (#)city:---- ");
                return data;
            }
        }
    }
}