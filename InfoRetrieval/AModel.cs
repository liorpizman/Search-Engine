using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents abstract model of the search engine to get inverted index 
    /// </summary>
    public abstract class AModel
    {
        /// <summary>
        /// fields of AModel
        /// </summary>
        protected int sizeTasks { get; set; }
        protected int _external { get; set; }
        protected ReadFile readFile { get; set; }
        public Indexer indexer { get; set; }
        public Searcher m_searcher { get; set; }
        protected string inputPath { get; set; }
        protected string outPutPath { get; set; }
        public bool m_doStemming { get; set; }
        public bool lastRunStem { get; set; }
        protected string m_queryFileInputPath { get; set; }
        protected string m_queryFileOutputPath { get; set; }
        protected bool m_doSemantic { get; set; }
        public Dictionary<string, IndexTerm>[] _dictionaries { get; set; }
        protected bool m_saveResults { get; set; }
        protected Dictionary<string, DocInfo> documentsInformation { get; set; }
        protected string[][] m_PostingLines { get; set; }

        /// <summary>
        /// method to set the input path of the user if changed
        /// </summary>
        /// <param name="userInput"> input path of the user</param>
        public void setInputPath(string userInput)
        {
            inputPath = userInput;  
        }

        /// <summary>
        /// method to set the semantic
        /// </summary>
        /// <param name="doSemantic">if the user seleceted semantic</param>
        public void setSemantic(bool doSemantic)
        {
            this.m_doSemantic = doSemantic;
        }


        /// <summary>
        /// method to set the save of the results
        /// </summary>
        /// <param name="save">if the user seleceted save the results</param>
        public void setSaveResults(bool save)
        {
            this.m_saveResults = save;
        }


        /// <summary>
        /// method to set the output path of the user if changed
        /// </summary>
        /// <param name="userInput"> output path of the user</param>
        public void setOutPutPath(string userOutput)
        {
            outPutPath = userOutput; 

        }

        /// <summary>
        /// method to set the choose of the user about stemming
        /// </summary>
        /// <param name="doStem">the choose of the user about stemming</param>
        public void setStemming(bool doStem)
        {
            m_doStemming = doStem;
        }



        /// <summary>
        /// method to set the Query Input Path
        /// </summary>
        /// <param name="QueryInput">Query Input Path</param>
        public void setQueryInputPath(string QueryInput)
        {
            this.m_queryFileInputPath = QueryInput;
        }

        /// <summary>
        /// method to set the Query Output Path
        /// </summary>
        /// <param name="QueryOutput">Query Output Path</param>
        public void setQueryOutPutPath(string QueryOutput)
        {
            this.m_queryFileOutputPath = QueryOutput;
        }

        /// <summary>
        /// method to clear memory
        /// </summary>
        public void ClearMemory()
        {
            indexer = null;
            readFile = null;
        }


        /// <summary>
        /// method to execute the index model
        /// </summary>
        public abstract void RunIndexing();

        /// <summary>
        /// method to execute the model to results for a query
        /// </summary>
        public abstract void RunQuery(string inputQuery, Dictionary<string, string> filterByCity);

        /// <summary>
        /// method to execute the model to results for a file query
        /// </summary>
        public abstract void RunFileQueries(string path, Dictionary<string, string> filterByCity);

    }
}
