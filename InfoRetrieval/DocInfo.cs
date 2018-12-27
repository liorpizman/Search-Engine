using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents an information about a document 
    /// </summary>
    public class DocInfo
    {
        /// <summary>
        /// fields of DocInfo
        /// </summary>
        private string m_docNo;
        private double m_docLength;
        private string m_docTitle;
        private string m_city;
        public Dictionary<string, double> m_Entities { get; set; }
        public string m_KWords { get; set; }

        /// <summary>
        /// constructor of DocInfo
        /// </summary>
        /// <param name="docNo">document id</param>
        /// <param name="docLength">length of document</param>
        /// <param name="docTitle">title of document</param>
        /// <param name="city">city of document</param>
        /// <param name="kWords">first k words of document</param>
        /// <param name="doStemming">bool stemming of document</param>
        /// <param name="stopWordsPath">the path of stop words of document</param>
        public DocInfo(string docNo, double docLength, string docTitle, string city, string kWords, bool doStemming, string stopWordsPath)
        {
            this.m_docNo = docNo;
            this.m_docLength = docLength;
            this.m_docTitle = docTitle;
            this.m_city = city;
            this.m_Entities = new Dictionary<string, double>();
            this.m_KWords = "";
        }

        /// <summary>
        /// method to set the k first words of the document
        /// </summary>
        /// <param name="doStemming">bool stemming of document</param>
        /// <param name="stopWordsPath">the path of stop words of document</param>
        /// <param name="_kFirstWords">the k words</param>
        private void SetKFirstWords(bool doStemming, string stopWordsPath, string _kFirstWords)
        {
            Document kWordsDocument = new Document("DOCNO", new StringBuilder("DATE1"), new StringBuilder("TI"), _kFirstWords, new StringBuilder("CITY"), new StringBuilder("language"));
            Parse parse = new Parse(doStemming, stopWordsPath);
            parse.ParseDocuments(kWordsDocument);
            foreach (DocumentsTerm docsOfterm in parse.m_allTerms.Values)
            {
                m_KWords = docsOfterm.m_valueOfTerm + " ";
            }
        }

        /// <summary>
        /// method to set a new entity of the document
        /// </summary>
        /// <param name="entitiy"></param>
        /// <param name="frequency"></param>
        public void SetEntite(string entitiy, double frequency)
        {
            m_Entities.Add(entitiy, frequency);
        }

        /// <summary>
        /// getter and setter of m_docNo
        /// </summary>
        public string docNo
        {
            get
            {
                return m_docNo;
            }
            set
            {
                m_docNo = value;
            }
        }

        /// <summary>
        /// getter and setter of m_docLength
        /// </summary>
        public double docLength
        {
            get
            {
                return m_docLength;
            }
            set
            {
                m_docLength = value;
            }
        }

        /// <summary>
        /// getter and setter of m_docTitle
        /// </summary>
        public string docTitle
        {
            get
            {
                return m_docTitle;
            }
            set
            {
                m_docTitle = value;
            }
        }

        /// <summary>
        /// getter and setter of m_city
        /// </summary>
        public string city
        {
            get
            {
                return m_city;
            }
            set
            {
                m_city = value;
            }
        }


    }
}
