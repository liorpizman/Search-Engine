using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents the model of the search engine to get inverted index 
    /// </summary>
    public class Model
    {
        /// <summary>
        /// fields of Model
        /// </summary>
        public int sizeTasks { get; private set; }
        public int _external { get; private set; }
        public ReadFile readFile { get; private set; }
        public Indexer indexer { get; private set; }
        public string inputPath { get; private set; }
        public string outPutPath { get; private set; }
        public bool m_doStemming { get; private set; }
        public bool lastRunStem { get; private set; }

        /// <summary>
        /// constructor of Model
        /// </summary>
        public Model()
        {
            this.sizeTasks = 0;
            this._external = 0;
            this.inputPath = "";
            this.outPutPath = "";
            this.m_doStemming = false;
            this.lastRunStem = false;
        }

        /// <summary>
        /// method to set the input path of the user if changed
        /// </summary>
        /// <param name="userInput"> input path of the user</param>
        public void setInputPath(string userInput)
        {
            inputPath = userInput;   //delete in clear
        }

        /// <summary>
        /// method to set the output path of the user if changed
        /// </summary>
        /// <param name="userInput"> output path of the user</param>
        public void setOutPutPath(string userOutput)
        {
            outPutPath = userOutput;  //delete in clear

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
        /// method to execute the model to get inverted index
        /// </summary>
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
            if (File.Exists(documentsIndexPath))   // delete the documents file if exists
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

            Action<object> updateRuleTask = (object obj) =>
            {
                indexer.MergeSameWords((int)obj);
            };

            Action<object> taskAction = (object obj) =>
            {
                Parse parse = new Parse(m_doStemming, readFile.m_mainPath);

                m1.WaitOne();
                MasterFile currMasterFile = readFile.ReadChunk(_external++);
                m1.ReleaseMutex();

                semaphore.WaitOne();
                parse.ParseMasterFile(currMasterFile);
                semaphore.Release();

                m2.WaitOne();
                indexer.WriteToPostingFile(new Dictionary<string, DocumentsTerm>(parse.m_allTerms), enter);
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
            ////////////////////////////////////////////////////////////////////////////////
            Task[][] m_tasks = new Task[7][];
            for (int i = 0; i < 6; i++)
            {
                m_tasks[i] = new Task[4];
            }
            m_tasks[6] = new Task[3];

            m_tasks[0][0] = Task.Factory.StartNew(() => updateRuleTask(0));
            m_tasks[0][1] = Task.Factory.StartNew(() => updateRuleTask(1));
            m_tasks[0][2] = Task.Factory.StartNew(() => updateRuleTask(2));
            m_tasks[0][3] = Task.Factory.StartNew(() => updateRuleTask(3));
            Task.WaitAll(m_tasks[0]);

            m_tasks[1][0] = Task.Factory.StartNew(() => updateRuleTask(4));
            m_tasks[1][1] = Task.Factory.StartNew(() => updateRuleTask(5));
            m_tasks[1][2] = Task.Factory.StartNew(() => updateRuleTask(6));
            m_tasks[1][3] = Task.Factory.StartNew(() => updateRuleTask(7));
            Task.WaitAll(m_tasks[1]);

            m_tasks[2][0] = Task.Factory.StartNew(() => updateRuleTask(8));
            m_tasks[2][1] = Task.Factory.StartNew(() => updateRuleTask(9));
            m_tasks[2][2] = Task.Factory.StartNew(() => updateRuleTask(10));
            m_tasks[2][3] = Task.Factory.StartNew(() => updateRuleTask(11));
            Task.WaitAll(m_tasks[2]);

            m_tasks[3][0] = Task.Factory.StartNew(() => updateRuleTask(12));
            m_tasks[3][1] = Task.Factory.StartNew(() => updateRuleTask(13));
            m_tasks[3][2] = Task.Factory.StartNew(() => updateRuleTask(14));
            m_tasks[3][3] = Task.Factory.StartNew(() => updateRuleTask(15));
            Task.WaitAll(m_tasks[3]);

            m_tasks[4][0] = Task.Factory.StartNew(() => updateRuleTask(16));
            m_tasks[4][1] = Task.Factory.StartNew(() => updateRuleTask(17));
            m_tasks[4][2] = Task.Factory.StartNew(() => updateRuleTask(18));
            m_tasks[4][3] = Task.Factory.StartNew(() => updateRuleTask(19));
            Task.WaitAll(m_tasks[4]);

            m_tasks[5][0] = Task.Factory.StartNew(() => updateRuleTask(20));
            m_tasks[5][1] = Task.Factory.StartNew(() => updateRuleTask(21));
            m_tasks[5][2] = Task.Factory.StartNew(() => updateRuleTask(22));
            m_tasks[5][3] = Task.Factory.StartNew(() => updateRuleTask(23));
            Task.WaitAll(m_tasks[5]);

            m_tasks[6][0] = Task.Factory.StartNew(() => updateRuleTask(24));
            m_tasks[6][1] = Task.Factory.StartNew(() => updateRuleTask(25));
            m_tasks[6][2] = Task.Factory.StartNew(() => updateRuleTask(26));
            Task.WaitAll(m_tasks[6]);
            ////////////////////////////////////////////////////////////////////////////////
            Task dictionaryIndex = Task.Factory.StartNew(indexDictionary, "taskDictionary");
            dictionaryIndex.Wait();

            Task citiesIndex = Task.Factory.StartNew(indexCity, "taskCity");
            citiesIndex.Wait();
        }
        public void ClearMemory()
        {
            indexer = null;
            readFile = null;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        static void Main(string[] args)
        {
        }

    }
}

/*


string corpusPath = @"D:\documents\users\pezman\SE\data\corpus";
string path750 = @"D:\documents\users\pezman\SE\data\corpus750";
string path250 = @"D:\documents\users\pezman\SE\data\corpus250";
string output = @"D:\documents\users\pezman\SE\outPut";
string inputOneDoc = @"D:\documents\users\pezman\SE\data\oneFile";

bool stem = true;
int sizeTasks, _external = 0;
ReadFile r = new ReadFile(corpusPath);
Indexer indexer = new Indexer(stem, output);
Semaphore semaphore = new Semaphore(2, 2);
Mutex m1 = new Mutex();
Mutex m2 = new Mutex();
bool enter = false;

Action<object> taskAction = (object obj) =>
{
    Parse parse = new Parse(stem, r.m_mainPath);

    m1.WaitOne();
    MasterFile currMasterFile = r.ReadChunk(_external++);
    m1.ReleaseMutex();

    semaphore.WaitOne();
    parse.ParseMasterFile(currMasterFile);
    semaphore.Release();

    m2.WaitOne();
    indexer.WriteToPostingFile(new Dictionary<string, DocumentsTerm>(parse.m_allTerms), enter);
    enter = true;
    indexer.UpdateCitiesAndLanguagesInDocument(currMasterFile);
    indexer.WriteTheNewDocumentsFile(currMasterFile);
    m2.ReleaseMutex();

    parse.m_allTerms.Clear();

};

Action<object> indexDictionary = (object obj) =>
{
    indexer.WriteTheNewIndexFile();
    indexer.SerializeDictionary();
};

Action<object> indexCity = (object obj) =>
{
    indexer.WriteCitiesIndexFile();
};


Console.WriteLine("path_Chunk.Count is :" + r.path_Chank.Count);
            sizeTasks = r.path_Chank.Count;

            Task[] taskArray = new Task[4];
Task[] lastTaskArray = new Task[sizeTasks % 4];

            for (int i = 0; i<sizeTasks;)
            {
                if ((i + 4) <= sizeTasks)
                {
                    for (int _internal = 0; _internal< 4; _internal++, i++)
                    {
                        taskArray[_internal] = Task.Factory.StartNew(taskAction, "taskParse");
                    }
                    Task.WaitAll(taskArray);
                    Console.WriteLine(i + " Task are done");
                }
                else
                {
                    for (int _internal = 0; _internal<(sizeTasks % 4); _internal++, i++)
                    {
                        lastTaskArray[_internal] = Task.Factory.StartNew(taskAction, "taskParse");
                    }
                    Task.WaitAll(lastTaskArray);
                    Console.WriteLine(i + " Task are done");
                }
            }
            ///////////////////////////////////////////////////////////////////////////////////////
            Action<object> updateRuleTask = (object obj) =>
            {
                indexer.MergeSameWords((int)obj);
            };

Task[][] m_tasks = new Task[7][];
            for (int i = 0; i< 6; i++)
            {
                m_tasks[i] = new Task[4];
            }
            m_tasks[6] = new Task[3];

            m_tasks[0][0] = Task.Factory.StartNew(() => updateRuleTask(0));
            m_tasks[0][1] = Task.Factory.StartNew(() => updateRuleTask(1));
            m_tasks[0][2] = Task.Factory.StartNew(() => updateRuleTask(2));
            m_tasks[0][3] = Task.Factory.StartNew(() => updateRuleTask(3));

            Task.WaitAll(m_tasks[0]);
            Console.WriteLine("4 Rule Task are done");

            m_tasks[1][0] = Task.Factory.StartNew(() => updateRuleTask(4));
            m_tasks[1][1] = Task.Factory.StartNew(() => updateRuleTask(5));
            m_tasks[1][2] = Task.Factory.StartNew(() => updateRuleTask(6));
            m_tasks[1][3] = Task.Factory.StartNew(() => updateRuleTask(7));

            Task.WaitAll(m_tasks[1]);
            Console.WriteLine("8 Rule Task are done");

            m_tasks[2][0] = Task.Factory.StartNew(() => updateRuleTask(8));
            m_tasks[2][1] = Task.Factory.StartNew(() => updateRuleTask(9));
            m_tasks[2][2] = Task.Factory.StartNew(() => updateRuleTask(10));
            m_tasks[2][3] = Task.Factory.StartNew(() => updateRuleTask(11));

            Task.WaitAll(m_tasks[2]);
            Console.WriteLine("12 Rule Task are done");

            m_tasks[3][0] = Task.Factory.StartNew(() => updateRuleTask(12));
            m_tasks[3][1] = Task.Factory.StartNew(() => updateRuleTask(13));
            m_tasks[3][2] = Task.Factory.StartNew(() => updateRuleTask(14));
            m_tasks[3][3] = Task.Factory.StartNew(() => updateRuleTask(15));

            Task.WaitAll(m_tasks[3]);
            Console.WriteLine("16 Rule Task are done");

            m_tasks[4][0] = Task.Factory.StartNew(() => updateRuleTask(16));
            m_tasks[4][1] = Task.Factory.StartNew(() => updateRuleTask(17));
            m_tasks[4][2] = Task.Factory.StartNew(() => updateRuleTask(18));
            m_tasks[4][3] = Task.Factory.StartNew(() => updateRuleTask(19));

            Task.WaitAll(m_tasks[4]);
            Console.WriteLine("20 Rule Task are done");

            m_tasks[5][0] = Task.Factory.StartNew(() => updateRuleTask(20));
            m_tasks[5][1] = Task.Factory.StartNew(() => updateRuleTask(21));
            m_tasks[5][2] = Task.Factory.StartNew(() => updateRuleTask(22));
            m_tasks[5][3] = Task.Factory.StartNew(() => updateRuleTask(23));

            Task.WaitAll(m_tasks[5]);
            Console.WriteLine("24 Rule Task are done");

            m_tasks[6][0] = Task.Factory.StartNew(() => updateRuleTask(24));
            m_tasks[6][1] = Task.Factory.StartNew(() => updateRuleTask(25));
            m_tasks[6][2] = Task.Factory.StartNew(() => updateRuleTask(26));

            Task.WaitAll(m_tasks[6]);
            Console.WriteLine("27 Rule Task are done");



            Task dictionaryIndex = Task.Factory.StartNew(indexDictionary, "taskDictionary");
dictionaryIndex.Wait();

            Console.WriteLine(" dictionaryIndex Task is done");


            Task citiesIndex = Task.Factory.StartNew(indexCity, "taskCity");
citiesIndex.Wait();

            Console.WriteLine(" citiesIndex Task is done");

*/
