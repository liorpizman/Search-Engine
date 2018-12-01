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
                m2.ReleaseMutex();

                parse.m_allTerms.Clear();

            };

            Console.WriteLine("path_Chank.Count is :" + r.path_Chank.Count);
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


            /*
            for (int _internal = 0; _internal < taskArray.Length; _internal++)
            {
                taskArray[_internal] = Task.Factory.StartNew(taskAction, "taskParse");
            }

            Task.WaitAll(taskArray);
            */





        }
    }
}

/*
Mutex m = new Mutex();

Action<object> parseTask = (object obj) =>
{

    Parse parse = new Parse(stem, r.m_mainPath);

    m.WaitOne();
    masterFile currMasterFile = r.ReadChunk(_external++);
    m.ReleaseMutex();
    //Console.WriteLine("read number  " + _external + " end");



    parse.ParseMasterFile(currMasterFile); //currMasterFile

    indexer.IsLegalEntry();
    indexer.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));
    parse.m_allTerms.Clear();

};

// send doStem from GUI

Action<object> indexTask = (object obj) =>
{
    indexer.MainIndexRun();
};


Task indexerTask = Task.Factory.StartNew(indexTask, "taskIndexer");
Console.WriteLine("path_Chank.Count is :" + r.path_Chank.Count);


//sizeTasks = Math.Min(4, r.path_Chank.Count);
sizeTasks = r.path_Chank.Count;
Task[] taskArray = new Task[sizeTasks];

for (int _internal = 0; _internal < taskArray.Length; _internal++)
{
    taskArray[_internal] = Task.Factory.StartNew(parseTask, "taskParse");
    // _external++;
}
Task.WaitAll(taskArray);


indexer.SetCorpusDone();
indexerTask.Wait();
*/
