using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            string outputOnPc = @"D:\documents\users\pezman\newYehuda\OutPutLab";

            ReadFile r = new ReadFile(pathLab100);
            r.MainRead();

            int sizeTasks, _external = 0;
            masterFile currMasterFile = null;

            //Parse.m_doStemming = true;             // add boolean according to gui 
            // Parse.AddStopWords(path);              //add static stop words;


            Indexer indexer = new Indexer(true, outputOnPc);
            //indexer.WriteCitiesIndexFile(new HashSet<string>() { "Nassau", "Santiago", "Willemstad", "Tbilisi", "Jerusalem" });

            Action<object> parseTask = (object obj) =>
            {
                Parse parse = new Parse(true, r.m_mainPath);                   // send doStem from GUI
                currMasterFile = r.ReadNewFile(r.m_paths[_external++]);
                //currMasterFile = m_files[m_paths[readIndex++]];
                parse.ParseMasterFile(currMasterFile); //currMasterFile
                indexer.AddToQueue(new Dictionary<string, DocumentTerms>(parse.m_allTerms));
                parse.m_allTerms.Clear();
            };

            // send doStem from GUI

            Action<object> indexTask = (object obj) =>
            {
                indexer.MainIndexRun();
            };


            Task indexerTask = Task.Factory.StartNew(indexTask, "taskIndexer");

            //for (; _external < r.m_paths.Length;)
            while (_external < r.m_paths.Length)
            {
                sizeTasks = Math.Min(15, r.m_paths.Length - _external);
                Task[] taskArray = new Task[sizeTasks];
                for (int _internal = 0; _internal < sizeTasks && _external < r.m_paths.Length; _internal++)
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
