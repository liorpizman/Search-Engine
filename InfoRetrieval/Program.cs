using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            string corpusPath = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\corpus";
            string path = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder";
            string pathLab100 = @"D:\documents\users\pezman\newYehuda\test100";
            string pathLabCorpus = @"D:\documents\users\pezman\newYehuda\testFolder";
            string outputOnPc = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\OutPut";
            */
            string corpusPath = @"D:\documents\users\pezman\SE\corpus";
            string path750 = @"D:\documents\users\pezman\SE\corpus750";
            string outputOnPc = @"D:\documents\users\pezman\SE\OutPut";
            string path250 = @"D:\documents\users\pezman\SE\corpus250";

            bool stem = false;
            int sizeTasks, _external = 0;
            ReadFile r = new ReadFile(corpusPath);
            Indexer indexer = new Indexer(stem, outputOnPc);
            Semaphore semaphore = new Semaphore(2, 2);
            Mutex m1 = new Mutex();
            Mutex m2 = new Mutex();
            bool enter = false;


            Action<object> taskAction = (object obj) =>
            {
                Parse parse = new Parse(stem, r.m_mainPath);

                m1.WaitOne();
                masterFile currMasterFile = r.ReadChunk(_external++);
                m1.ReleaseMutex();

                semaphore.WaitOne();
                parse.ParseMasterFile(currMasterFile);
                semaphore.Release();

                m2.WaitOne();
                indexer.WriteToPostingFile(new Dictionary<string, DocumentTerms>(parse.m_allTerms), enter);
                enter = true;
                indexer.UpdateCitiesPositionInDocument(currMasterFile);
                indexer.WriteTheNewDocumentsFile(currMasterFile);
                m2.ReleaseMutex();

                parse.m_allTerms.Clear();

            };

            Action<object> indexDictionary = (object obj) =>
            {
                indexer.WriteTheNewIndexFile();
            };

            Action<object> indexCity = (object obj) =>
            {
                indexer.WriteCitiesIndexFile();
            };


            Console.WriteLine("path_Chunk.Count is :" + r.path_Chank.Count);
            sizeTasks = r.path_Chank.Count;



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
                    Console.WriteLine(i + " Task are done");
                }
                else
                {
                    for (int _internal = 0; _internal < (sizeTasks % 4); _internal++, i++)
                    {
                        lastTaskArray[_internal] = Task.Factory.StartNew(taskAction, "taskParse");
                    }
                    Task.WaitAll(lastTaskArray);
                    Console.WriteLine(i + " Task are done");
                }
            }


            Task citiesIndex = Task.Factory.StartNew(indexCity, "taskCity");
            citiesIndex.Wait();

            Console.WriteLine(" citiesIndex Task is done");

            Task dictionaryIndex = Task.Factory.StartNew(indexDictionary, "taskDictionary");
            dictionaryIndex.Wait();

            Console.WriteLine(" dictionaryIndex Task is done");
        }
    }
}

