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
            string pathLab = @"d:\documents\users\pezman\Documents\bbbbbbbbbbbbb\testFolder";
            ReadFile r = new ReadFile(corpusPath);
            r.MainRead();

            int sizeTasks, _external = 0;
            masterFile currMasterFile = null;

            Action<object> parseTask = (object obj) =>
            {
                Parse parse = new Parse(true, r.m_mainPath);
                currMasterFile = r.ReadNewFile(r.m_paths[_external++]);
                //currMasterFile = m_files[m_paths[readIndex++]];
                parse.ParseMasterFile(currMasterFile); //currMasterFile
                parse.m_allTerms.Clear();
            };

            for (; _external < r.m_paths.Length;)
            {
                sizeTasks = Math.Min(15, r.m_paths.Length - _external);
                Task[] taskArray = new Task[sizeTasks];
                for (int _internal = 0; _internal < sizeTasks && _external < r.m_paths.Length; _internal++)
                {
                    taskArray[_internal] = Task.Factory.StartNew(parseTask, "taskName");
                }
                Task.WaitAll(taskArray);
            }
            //Indexer.SetCorpusDone();

        }
    }
}
