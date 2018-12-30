using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents the model of the search engine to get inverted index 
    /// </summary>
    public class Model : AModel
    {
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
            this.m_doSemantic = false;
            this.m_saveResults = true;
            this.documentsInformation = new Dictionary<string, DocInfo>();
            this.m_PostingLines = new string[27][];
        }

        /// <summary>
        /// method to execute the model to get inverted index
        /// </summary>
        public override void RunIndexing()
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

            Action<object> DataOfDocsAction = (object obj) =>
            {
                indexer.WriteAdditionalDataOfDocs();
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


            Action<object> LanguagesAction = (object obj) =>
            {
                indexer.WriteLanguagesToFile();
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

            Task dictionaryIndex = Task.Factory.StartNew(indexDictionary, "taskDictionary");
            dictionaryIndex.Wait();

            Task citiesIndex = Task.Factory.StartNew(indexCity, "taskCity");
            citiesIndex.Wait();


            Task DataOfDocsTask = Task.Factory.StartNew(DataOfDocsAction, "taskData");
            DataOfDocsTask.Wait();

            Task LanguagesTask = Task.Factory.StartNew(LanguagesAction, "taskLanguages");
            LanguagesTask.Wait();

        }

        /// <summary>
        /// method to execute the model to results for a query
        /// </summary>
        public override void RunQuery(string inputQuery, Dictionary<string, string> filterByCity)
        {
            if (m_searcher == null)
            {
                string prevPath = "", newPath = "";
                prevPath = System.IO.Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName
                    (Assembly.GetEntryAssembly().Location))));
                newPath = Path.Combine(prevPath, "DictionarySource\\");
                Wnlib.WNCommon.path = @newPath;
                m_searcher = new Searcher(outPutPath, inputPath, m_doStemming);
                m_searcher.m_LocalPostingLines = this.m_PostingLines;
                m_searcher.dictionaries = _dictionaries;
                m_searcher.StopWordsPath = inputPath;
                m_searcher.docInformation = documentsInformation;
            }
            m_searcher.toWriteOutPutPath = this.m_queryFileOutputPath;
            m_searcher.InputPath = this.m_queryFileInputPath;
            m_searcher.OutPutPath = this.outPutPath;
            m_searcher.updateOutput(m_doStemming, outPutPath);
            m_searcher.StopWordsPath = inputPath;

            Query q = m_searcher.ParseNewQuery(inputQuery, m_doSemantic, "-1", filterByCity);
            if (m_saveResults)
            {
                m_searcher.WriteQueryResults(q);
            }
        }

        /// <summary>
        /// method to execute the model to results for a file query
        /// </summary>
        public override void RunFileQueries(string path, Dictionary<string, string> filterByCity)
        {
            if (m_searcher == null)
            {
                string prevPath = "", newPath = "";
                prevPath = System.IO.Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName
                    (Assembly.GetEntryAssembly().Location))));
                newPath = Path.Combine(prevPath, "DictionarySource\\");
                Wnlib.WNCommon.path = @newPath;
                m_searcher = new Searcher(outPutPath, inputPath, m_doStemming);
                m_searcher.m_LocalPostingLines = this.m_PostingLines;
                m_searcher.dictionaries = _dictionaries;
                m_searcher.StopWordsPath = inputPath;
                m_searcher.docInformation = documentsInformation;
            }
            m_searcher.toWriteOutPutPath = this.m_queryFileOutputPath;
            m_searcher.InputPath = this.m_queryFileInputPath;
            m_searcher.OutPutPath = this.outPutPath;
            m_searcher.updateOutput(m_doStemming, outPutPath);
            m_searcher.StopWordsPath = inputPath;

            m_searcher.ParseQueriesFile(m_queryFileInputPath, m_doSemantic, m_saveResults, filterByCity);

        }

        /// <summary>
        /// method to load posting files to memory
        /// </summary>
        public void SetPostingLines()
        {
            string path = outPutPath;
            if (m_doStemming)
            {
                path = Path.Combine(path, "WithStem");
            }
            else
            {
                path = Path.Combine(path, "WithOutStem");
            }
            for (int i = 0; i < 27; i++)
            {
                this.m_PostingLines[i] = File.ReadAllLines(Path.Combine(path, "Posting" + i + ".txt"));
            }
        }

        /// <summary>
        /// method to load dictionary to memory
        /// </summary>
        public void LoadDictionary()
        {
            string[] AllLines, splittedLine;
            string term, df, tfc, PN, LN;
            int postNum;
            Dictionary<string, IndexTerm>[] dictionaries = new Dictionary<string, IndexTerm>[27];
            bool existWithStem = File.Exists(Path.Combine(outPutPath, "WithStem\\Dictionary.txt"));
            bool existWithoutStem = File.Exists(Path.Combine(outPutPath, "WithOutStem\\Dictionary.txt"));
            if (m_doStemming && existWithStem)
            {
                AllLines = File.ReadAllLines(Path.Combine(outPutPath, "WithStem\\Dictionary.txt"));
            }
            else if (!m_doStemming && existWithoutStem)
            {
                AllLines = File.ReadAllLines(Path.Combine(outPutPath, "WithOutStem\\Dictionary.txt"));
            }
            else
            {
                return;
            }
            for (int i = 0; i < 27; i++)
            {
                dictionaries[i] = new Dictionary<string, IndexTerm>();
            }
            for (int j = 0; j < AllLines.Length; j++)
            {
                splittedLine = AllLines[j].Split(new string[] { "(#)" }, StringSplitOptions.None);
                term = splittedLine[0];
                df = splittedLine[1].Split(':')[1];
                tfc = splittedLine[2].Split(':')[1];
                PN = splittedLine[3].Split(':')[1];
                LN = splittedLine[4].Split(':')[1];
                postNum = Int32.Parse(PN);
                IndexTerm temp = new IndexTerm(term, postNum, Int32.Parse(LN));
                temp.IncreaseDf(Int32.Parse(df));
                temp.IncreaseTfc(Int32.Parse(tfc));
                dictionaries[postNum].Add(term, temp);
            }
            _dictionaries = dictionaries;
        }

        /// <summary>
        /// method to load documents information to memory
        /// </summary>
        public void LoadDocumentsInformation()
        {
            string[] AllLines, splittedLine, tmpSplite, Entities;
            string docno, tmp, length, title, city, kFirstWords;
            if (m_doStemming)
            {
                AllLines = File.ReadAllLines(Path.Combine(Path.Combine(outPutPath, "WithStem"), "Documents.txt"));
            }
            else
            {
                AllLines = File.ReadAllLines(Path.Combine(Path.Combine(outPutPath, "WithOutStem"), "Documents.txt"));
            }
            for (int i = 0; i < AllLines.Length; i++)
            {
                splittedLine = AllLines[i].Split(new string[] { "(#)" }, StringSplitOptions.None);
                docno = splittedLine[0].Trim(' ');
                tmp = splittedLine[splittedLine.Length - 2].Trim(' ');
                length = tmp.Split(' ')[1].Trim(' ');
                tmp = splittedLine[splittedLine.Length - 1].Trim(' ');
                city = tmp.Split(':')[1].Trim(' ');
                tmp = splittedLine[2].Trim(' ');
                kFirstWords = tmp.Split(new string[] { "Kwords:" }, StringSplitOptions.None)[1];
                tmp = splittedLine[1].Trim(' ');
                tmpSplite = tmp.Split(new string[] { "TI: " }, StringSplitOptions.None);
                if (tmpSplite.Length > 1)
                {
                    title = tmp.Split(new string[] { "TI: " }, StringSplitOptions.None)[1];
                }
                else
                {
                    title = "";
                }
                DocInfo tmpDocument = new DocInfo(docno, Double.Parse(length), title, city, kFirstWords, m_doStemming, inputPath);
                tmp = splittedLine[5].Trim(' ');
                tmpSplite = tmp.Split(new string[] { "[#] " }, StringSplitOptions.None);
                for (int j = 1; j < tmpSplite.Length; j++)
                {
                    Entities = tmpSplite[j].Split(new string[] { "[*]" }, StringSplitOptions.None);
                    tmpDocument.SetEntite(Entities[0], Double.Parse(Entities[1]));
                }
                documentsInformation.Add(docno, tmpDocument);
            }
        }

        /// <summary>
        /// method to clear documents information from the memory
        /// </summary>
        public void ClearDocumentsInformation()
        {
            documentsInformation.Clear();
        }

        /// <summary>
        /// method to check whether all output files are exist
        /// </summary>
        /// <param name="path">current path to check</param>
        /// <returns>if the files are exist</returns>
        public bool CheckFilesExists(string path)
        {
            string currentPath;
            bool existWithStem = Directory.Exists(Path.Combine(outPutPath, "WithStem"));
            bool existWithoutStem = Directory.Exists(Path.Combine(outPutPath, "WithOutStem"));
            if (!existWithStem && !existWithoutStem)
            {
                return false;
            }
            if (m_doStemming)
            {
                if (!existWithStem)
                {
                    return false;
                }
            }
            else if (!m_doStemming)
            {
                if (!existWithoutStem)
                {
                    return false;
                }
            }
            if (m_doStemming)
            {
                currentPath = Path.Combine(outPutPath, "WithStem");
            }
            else
            {
                currentPath = Path.Combine(outPutPath, "WithOutStem");
            }
            if (!File.Exists(Path.Combine(currentPath, "Cities.txt")))
            {
                return false;
            }
            if (!File.Exists(Path.Combine(currentPath, "DocsData.txt")))
            {
                return false;
            }
            if (!File.Exists(Path.Combine(currentPath, "Dictionary.txt")))
            {
                return false;
            }
            if (!File.Exists(Path.Combine(currentPath, "Documents.txt")))
            {
                return false;
            }
            for (int i = 0; i < 27; i++)
            {

                if (!File.Exists(Path.Combine(currentPath, "Posting" + i + ".txt")))
                {
                    return false;
                }
            }
            return true;
        }

        static void Main(string[] args) { }
    }
}
