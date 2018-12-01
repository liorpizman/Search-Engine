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
            string corpusPath = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\corpus";
            string path = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder";
            string pathLab100 = @"D:\documents\users\pezman\newYehuda\test100";
            string pathLabCorpus = @"D:\documents\users\pezman\newYehuda\testFolder";
            string outputOnPc = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\OutPut";

            int sizeTasks, _external = 0;//, partSize, lastPartSize, step;
            masterFile currMasterFile = null;
            Mutex m = new Mutex();
            Mutex m2 = new Mutex();
            Semaphore semaphore = new Semaphore(5, 5);

            bool stem = false;
            ReadFile r = new ReadFile(corpusPath, 75);  // int - ChankSize
            Indexer indexer = new Indexer(stem, outputOnPc, r.path_Chank.Count);  // int - queueSize

            //indexer.WriteCitiesIndexFile(new HashSet<string>() { "Nassau", "Santiago", "Willemstad", "Tbilisi", "Jerusalem" });

            Action<object> indexTask = (object obj) =>
            {
                indexer.MainIndexRun();
            };


            Action<object> parseTask = (object obj) =>
            {
                Parse parse = new Parse(stem, r.m_mainPath);

                m.WaitOne();
                // Console.WriteLine(_external);
                currMasterFile = r.ReadChunk(_external++);
                m.ReleaseMutex();

                parse.ParseMasterFile(currMasterFile); //currMasterFile
                indexer.IsLegalEntry();
                indexer.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));
                parse.m_allTerms.Clear();
            };


            Action<object> ChunkTask = (object obj) =>
            {
                Parse parse = new Parse(stem, r.m_mainPath);

                m.WaitOne();
                // Console.WriteLine(_external);
                currMasterFile = r.ReadChunk(_external++);
                m.ReleaseMutex();

                semaphore.WaitOne();
                parse.ParseMasterFile(currMasterFile); //currMasterFile
                semaphore.Release();

                //indexer.IsLegalEntry();
                //indexer.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));

                m2.WaitOne();
                indexer.WriteToPostingFile(new Dictionary<string, DocumentTerms>(parse.m_allTerms), true);
                m2.ReleaseMutex();

                parse.m_allTerms.Clear();
            };

            sizeTasks = r.path_Chank.Count;
            Console.WriteLine("path_Chank.Count (sizeTasks) : " + sizeTasks);


            //for (; _external < r.m_paths.Length;)
            // while (_external < r.path_Chank.Count)//(_external < r.m_paths.Length)
            //{
            //sizeTasks = Math.Min(4, r.path_Chank.Count - _external);

            Task[] taskArray = new Task[sizeTasks];

            for (int _internal = 0; _internal < taskArray.Length; _internal++) //&& _external < r.path_Chank.Count
            {
                taskArray[_internal] = Task.Factory.StartNew(ChunkTask, "ChunkTask");
            }
            //Task.WaitAny(taskArray);


            Task.WaitAll(taskArray);
            //}
            //Task indexerTask = Task.Factory.StartNew(indexTask, "taskIndexer");

            //indexer.SetCorpusDone();
            //indexerTask.Wait();


        }
    }
}




/*
 * 
 *          masterFile currMasterFile1 = null;
            masterFile currMasterFile2 = null;
            masterFile currMasterFile3 = null;
            masterFile currMasterFile4 = null;
            Mutex m1 = new Mutex();
            Mutex m2 = new Mutex();
            Mutex m3 = new Mutex();
            Mutex m4 = new Mutex();
Indexer indexer1 = new Indexer(stem, System.IO.Path.Combine(outputOnPc, "indexer1"));
Indexer indexer2 = new Indexer(stem, System.IO.Path.Combine(outputOnPc, "indexer2"));
Indexer indexer3 = new Indexer(stem, System.IO.Path.Combine(outputOnPc, "indexer3"));
Indexer indexer4 = new Indexer(stem, System.IO.Path.Combine(outputOnPc, "indexer4"));

sizeTasks = r.path_Chank.Count;
            partSize = 450;
            lastPartSize = sizeTasks - 3 * partSize;

Action<object> parse1Task = (object obj) =>
{
    Parse parse = new Parse(stem, r.m_mainPath);

    m1.WaitOne();
    // Console.WriteLine(_external);
    currMasterFile1 = r.ReadChunk(_external++);
    m1.ReleaseMutex();

    parse.ParseMasterFile(currMasterFile1); //currMasterFile
    indexer1.IsLegalEntry();
    indexer1.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));
    parse.m_allTerms.Clear();
};

Action<object> parse2Task = (object obj) =>
{
    Parse parse = new Parse(stem, r.m_mainPath);

    m2.WaitOne();
    // Console.WriteLine(_external);
    currMasterFile2 = r.ReadChunk(_external++);
    m2.ReleaseMutex();

    parse.ParseMasterFile(currMasterFile2); //currMasterFile
    indexer2.IsLegalEntry();
    indexer2.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));
    parse.m_allTerms.Clear();
};

Action<object> parse3Task = (object obj) =>
{
    Parse parse = new Parse(stem, r.m_mainPath);

    m3.WaitOne();
    // Console.WriteLine(_external);
    currMasterFile3 = r.ReadChunk(_external++);
    m3.ReleaseMutex();

    parse.ParseMasterFile(currMasterFile3); //currMasterFile
    indexer3.IsLegalEntry();
    indexer3.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));
    parse.m_allTerms.Clear();
};

Action<object> parse4Task = (object obj) =>
{
    Parse parse = new Parse(stem, r.m_mainPath);

    m4.WaitOne();
    // Console.WriteLine(_external);
    currMasterFile4 = r.ReadChunk(_external++);
    m4.ReleaseMutex();

    parse.ParseMasterFile(currMasterFile4); //currMasterFile
    indexer4.IsLegalEntry();
    indexer4.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));
    parse.m_allTerms.Clear();
};

Action<object> index1Task = (object obj) =>
{
    indexer1.MainIndexRun();
};

Action<object> index2Task = (object obj) =>
{
    indexer2.MainIndexRun();
};

Action<object> index3Task = (object obj) =>
{
    indexer3.MainIndexRun();
};

Action<object> index4Task = (object obj) =>
{
    indexer4.MainIndexRun();
};

Task[] indexersTasks = new Task[4];
int _internal = 0;

//Task[] taskArray = new Task[sizeTasks];
Task[] taskArray1 = new Task[9];
Task[] taskArray2 = new Task[9];
Task[] taskArray3 = new Task[9];
Task[] taskArray4 = new Task[10];

Console.WriteLine("path_Chank.Count (sizeTasks) : " + sizeTasks);
            //////////////////////////////////////////////////////////////////////////////////////
            indexersTasks[0] = Task.Factory.StartNew(index1Task, "task1Indexer");
            for (_internal = 0; _internal< 9; _internal++)
            {
                taskArray1[_internal] = Task.Factory.StartNew(parse1Task, "task1Parse");
            }
            Task.WaitAll(taskArray1);
            indexer1.SetCorpusDone();
            //////////////////////////////////////////////////////////////////////////////////////
            indexersTasks[1] = Task.Factory.StartNew(index2Task, "task2Indexer");
            for (_internal = 0; _internal< 9; _internal++)
            {
                taskArray2[_internal] = Task.Factory.StartNew(parse2Task, "task2Parse");
            }
            Task.WaitAll(taskArray2);
            indexer2.SetCorpusDone();
            //////////////////////////////////////////////////////////////////////////////////////
            indexersTasks[2] = Task.Factory.StartNew(index3Task, "task3Indexer");
            for (_internal = 0; _internal< 9; _internal++)
            {
                taskArray3[_internal] = Task.Factory.StartNew(parse3Task, "task3Parse");
            }
            Task.WaitAll(taskArray3);
            indexer3.SetCorpusDone();
            //////////////////////////////////////////////////////////////////////////////////////
            indexersTasks[3] = Task.Factory.StartNew(index4Task, "task4Indexer");
            for (_internal = 0; _internal< 10; _internal++)
            {
                taskArray4[_internal] = Task.Factory.StartNew(parse4Task, "task4Parse");
            }
            Task.WaitAll(taskArray4);
            indexer4.SetCorpusDone();
            //////////////////////////////////////////////////////////////////////////////////////

            Task.WaitAll(indexersTasks);
*/
