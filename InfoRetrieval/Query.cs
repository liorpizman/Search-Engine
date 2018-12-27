using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents new query of the user
    /// </summary>
    public class Query
    {
        /// <summary>
        /// fields of Query
        /// </summary>
        public static int ID = 700;
        public string m_ID { get; private set; } // id of query

        public Dictionary<string, MethodScore> m_docsRanks { get; set; }

        private string m_content;
        public string content
        {
            get
            {
                return m_content;
            }
            set
            {
                m_content = value;
            }
        }

        /// <summary>
        /// constructor of Query
        /// </summary>
        /// <param name="content">content of query</param>
        public Query(string content)
        {
            this.m_content = content;
            m_ID = "" + ID++;
            m_docsRanks = new Dictionary<string, MethodScore>();
        }

        /// <summary>
        /// constructor of Query
        /// </summary>
        /// <param name="content">content of query</param>
        /// <param name="queryID">id of query</param>
        public Query(string content, string queryID)
        {
            this.m_content = content;
            m_ID = queryID;
            m_docsRanks = new Dictionary<string, MethodScore>();
        }

        /// <summary>
        /// method to get all data of the query
        /// </summary>
        /// <returns>the data of the query</returns>
        public StringBuilder GetQueryData()
        {
            Dictionary<string, double> DocResults = new Dictionary<string, double>();
            int counter = 50, i = 1;
            StringBuilder data = new StringBuilder();
            foreach (string key in m_docsRanks.Keys)
            {
                DocResults.Add(key, m_docsRanks[key].GetTotalScore());
            }
            DocResults = DocResults.OrderByDescending(j => j.Value).ToDictionary(p => p.Key, p => p.Value);
            foreach (string docno in DocResults.Keys)
            {
                if (i > 50)
                {
                    break;
                }
                data.AppendLine(m_ID + " 0 " + docno + " 1 " + (counter--) + " mt");
                i++;
            }
            return data;
        }

    }
}
