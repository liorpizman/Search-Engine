using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    public class Model
    {
        public int sizeTasks;
        public int _external;
        public masterFile currMasterFile;
        public Mutex m;
        public ReadFile readFile;
        public Indexer indexer;
        public string inputPath;
        public string outPutPath;
        public bool m_doStemming;

        public Model()
        {
            currMasterFile = null;
            sizeTasks = 0;
            _external = 0;
            m = new Mutex();
            inputPath = "";
            outPutPath = "";
            this.m_doStemming = false;
        }

        public void setInputPath(string userInput)
        {
            inputPath = userInput;   //delete in clear
        }

        public void setOutPutPath(string userOutput)
        {
            outPutPath = userOutput;//delete in clear

        }

        public void setStemming(bool doStem)
        {
            m_doStemming = doStem;
        }

        public void Run()
        {
            readFile = new ReadFile(inputPath, 50);  // int - ChankSize
            indexer = new Indexer(m_doStemming, outPutPath, 10); // int - queueSize

            Action<object> parseTask = (object obj) =>
                                    {
                                        Parse parse = new Parse(true, readFile.m_mainPath);
                                        //////////// mutex 
                                        m.WaitOne();
                                        currMasterFile = readFile.ReadChunk(_external++);
                                        m.ReleaseMutex();
                                        //////////// mutex 
                                        parse.ParseMasterFile(currMasterFile);
                                        indexer.IsLegalEntry();
                                        indexer.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));
                                        parse.m_allTerms.Clear();
                                    };

            Action<object> indexTask = (object obj) =>
                                    {
                                        indexer.MainIndexRun();
                                    };

            Task indexerTask = Task.Factory.StartNew(indexTask, "taskIndexer");

            //for (; _external < r.m_paths.Length;)
            while (_external < readFile.path_Chank.Count)//(_external < r.m_paths.Length)
            {
                sizeTasks = Math.Min(4, readFile.path_Chank.Count - _external);
                Task[] taskArray = new Task[sizeTasks];
                for (int _internal = 0; _internal < sizeTasks && _external < readFile.path_Chank.Count; _internal++)
                {
                    taskArray[_internal] = Task.Factory.StartNew(parseTask, "taskParse");
                }
                Task.WaitAll(taskArray);
            }

            indexer.SetCorpusDone();
            indexerTask.Wait();
        }
    }
}
