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

        public DocInfo(string docNo, double docLength, string docTitle, string city)
        {
            m_docNo = docNo;
            m_docLength = docLength;
            m_docTitle = docTitle;
            m_city = city;
            m_Entities = new Dictionary<string, double>();
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
