using System;
using System.Collections.Generic;
using System.IO;
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
        public ReadFile readFile;
        public Indexer indexer;
        public string inputPath;
        public string outPutPath;
        public bool m_doStemming;
        public bool lastRunStem;

        public Model()
        {
            this.sizeTasks = 0;
            this._external = 0;
            this.inputPath = "";
            this.outPutPath = "";
            this.m_doStemming = false;
            this.lastRunStem = false;
        }

        public void setInputPath(string userInput)
        {
            inputPath = userInput;   //delete in clear
        }

        public void setOutPutPath(string userOutput)
        {
            outPutPath = userOutput;  //delete in clear

        }

        public void setStemming(bool doStem)
        {
            m_doStemming = doStem;
        }


        public void Run()
        {
            int sizeTasks, _external = 0;
            readFile = new ReadFile(inputPath);
            indexer = new Indexer(m_doStemming, outPutPath);
            Semaphore semaphore = new Semaphore(2, 2);
            Mutex m1 = new Mutex();
            Mutex m2 = new Mutex();
            bool enter = false;
            this.lastRunStem = m_doStemming;

            string documentsIndexPath = Path.Combine(indexer.m_outPutPath, "Documents.txt");
            if (File.Exists(documentsIndexPath))
            {
                File.Delete(documentsIndexPath);
            }

            Action<object> indexCity = (object obj) =>
            {
                indexer.WriteCitiesIndexFile();
            };

            Action<object> indexDictionary = (object obj) =>
            {
                indexer.WriteTheNewIndexFile();
                indexer.SerializeDictionary();
            };

            Action<object> taskAction = (object obj) =>
            {
                Parse parse = new Parse(m_doStemming, readFile.m_mainPath);

                m1.WaitOne();
                masterFile currMasterFile = readFile.ReadChunk(_external++);
                m1.ReleaseMutex();

                semaphore.WaitOne();
                parse.ParseMasterFile(currMasterFile);
                semaphore.Release();

                m2.WaitOne();
                indexer.WriteToPostingFile(new Dictionary<string, DocumentTerms>(parse.m_allTerms), enter);
                enter = true;
                indexer.UpdateCitiesAndLanguagesInDocument(currMasterFile);
                indexer.WriteTheNewDocumentsFile(currMasterFile);
                m2.ReleaseMutex();

                parse.m_allTerms.Clear();

            };

            sizeTasks = readFile.path_Chank.Count;
            Task[] taskArray = new Task[4];
            Task[] lastTaskArray = new Task[sizeTasks % 4];

            for (int i = 0; i < sizeTasks;)
            {
                if ((i + 4) <= sizeTasks)
                {
                    for (int _internal = 0; _internal < 4; _internal++, i++)
                    {
                        taskArray[_internal] = Task.Factory.StartNew(taskAction, "taskParse");
                    }
                    Task.WaitAll(taskArray);
                }
                else
                {
                    for (int _internal = 0; _internal < (sizeTasks % 4); _internal++, i++)
                    {
                        lastTaskArray[_internal] = Task.Factory.StartNew(taskAction, "taskParse");
                    }
                    Task.WaitAll(lastTaskArray);
                }
            }

            Task citiesIndex = Task.Factory.StartNew(indexCity, "taskCity");
            citiesIndex.Wait();

            Task dictionaryIndex = Task.Factory.StartNew(indexDictionary, "taskDictionary");
            dictionaryIndex.Wait();
        }

    }
}
/*
public void RunCitiesIndex()
{
    Action<object> indexCity = (object obj) =>
    {
        indexer.WriteCitiesIndexFile();
    };

    Task citiesIndex = Task.Factory.StartNew(indexCity, "taskCity");
    citiesIndex.Wait();
}

public void RunDictionaryIndex()
{

    Action<object> indexDictionary = (object obj) =>
    {
        indexer.WriteTheNewIndexFile();
        indexer.SerializeDictionary();
    };

    Task dictionaryIndex = Task.Factory.StartNew(indexDictionary, "taskDictionary");
    dictionaryIndex.Wait();
}
*/
