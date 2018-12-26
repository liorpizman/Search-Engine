using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    public class DocInfo
    {
        private string m_docNo;
        private double m_docLength;
        private string m_docTitle;
        private string m_city;
        public Dictionary<string, double> m_Entities { get; set; }
        public string m_KWords { get; set; }

        public DocInfo(string docNo, double docLength, string docTitle, string city, string kWords, bool doStemming, string stopWordsPath)
        {
            this.m_docNo = docNo;
            this.m_docLength = docLength;
            this.m_docTitle = docTitle;
            this.m_city = city;
            this.m_Entities = new Dictionary<string, double>();
            this.m_KWords = "";
            SetKFirstWords(doStemming, stopWordsPath, kWords);
        }

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

        public void SetEntite(string entitiy, double frequency)
        {
            m_Entities.Add(entitiy, frequency);
        }
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
