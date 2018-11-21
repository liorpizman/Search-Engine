using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class ReadFile
    {
        //public ConcurrentDictionary<string, masterFile> m_files;
        public Dictionary<string, masterFile> m_files;
        public string m_mainPath;
        //public int m_filesAmount;
        public bool m_doStem;
        public string[] m_paths;
        public int m_indexCurrFile;
        public int m_maxFiles = 16;
        TimeSpan readFileTimeSpan;
        TimeSpan parseTimeSpan;
        //public int m_maxDoucments = 6500;
        //public Parse tmpP;                                                                                                  //// test now ?????????

        public ReadFile(string m_mainPath, bool doStem)
        {
            //this.m_files = new ConcurrentDictionary<string, masterFile>();
            this.m_files = new Dictionary<string, masterFile>();
            this.m_mainPath = m_mainPath;
            //this.m_filesAmount = 0;
            this.m_doStem = doStem;
            this.m_indexCurrFile = 0;
            //tmpP = new Parse(m_doStem, m_mainPath); ;                                                                            //// test now ?????????
        }


        public void mainRead()
        {
            Parse parse = new Parse(m_doStem, m_mainPath);
            string[] directories = Directory.GetDirectories(Directory.GetDirectories(m_mainPath)[0]);
            m_paths = new string[directories.Length];
            //Parse[] parsers = new Parse[m_maxFiles];
            //Thread[] threads = new Thread[m_maxFiles];
            for (int index = 0; index < directories.Length; index++)
            {
                m_paths[index] = Directory.GetFiles(directories[index])[0];  /// check wether one file in one folder
                //parsers[index] = new Parse(m_doStem, m_mainPath);
            }
            masterFile currMasterFile = null;
            for (int i = 0; i < m_paths.Length;)
            {
                for (int j = 0; j < m_maxFiles && i < m_paths.Length; j++)
                {
                    //var currIndex = i;
                    //DateTime st1 = DateTime.Now;
                    //currMasterFile = readFile(m_paths[currIndex]);
                    // readFileTimeSpan = readFileTimeSpan.Add(DateTime.Now - st1);
                    /*
                    st1 = DateTime.Now;
                    parse.parseDocuments(currMasterFile);
                    parseTimeSpan = parseTimeSpan.Add(DateTime.Now - st1);
                    */
                    currMasterFile = readFile(m_paths[i]);
                    //m_filesAmount++;
                    /*
                    parsers[j] = new Parse(m_doStem, m_mainPath);
                    Parse p = parsers[j];
                    threads[j] = new Thread(() => p.parseDocuments(currMasterFile));
                    threads[j].Start();
                    */
                    parse.parseDocuments(currMasterFile);
                    i++;
                }
                /*
                for (int m = 0; m < m_maxFiles && m < m_paths.Length; m++)
                {
                    Parse p = parsers[m];
                    p.m_allTerms.Clear();
                }
                for (int iThread = 0; iThread < threads.Length; iThread++)
                {
                    threads[iThread].Join();
                }
                */
                //parse.m_allTerms.Clear();
            }
            //Console.WriteLine("Read file Time Span: " + readFileTimeSpan);
            //Console.WriteLine("Parse Time Span: " + parseTimeSpan);



            //tmpP = parse;                                                          //// test now ?????????
        }

        private masterFile readFile(string file)
        {
            /*
            int pos = file.LastIndexOf("\\") + 1;
            string currFileName = file.Substring(pos, file.Length - pos);
            */
            string[] fields = file.Split('\\');
            string currFileName = fields[fields.Length - 1];
            masterFile masterFile = new masterFile(currFileName, file);
            readDocuments(file, masterFile);
            m_files.Add(currFileName, masterFile);
            return masterFile;
            /// we can return masterFile and send it to parser and do not sent the whole dictionary
        }

        private void readDocuments(string file, masterFile masterFile)
        {
            string content = File.ReadAllText(file);                    //check run time!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            string[] docs = content.Split(new[] { "<DOC>" }, StringSplitOptions.None);
            string allDocument;
            for (int i = 1; i < docs.Length; i++)
            {
                allDocument = "<DOC>" + docs[i];
                StringBuilder DOCNO = new StringBuilder(GetStringInBetween("<DOCNO>", "</DOCNO>", docs[i]));
                StringBuilder DATE1 = new StringBuilder(GetStringInBetween("<DATE1>", "</DATE1>", docs[i]));
                StringBuilder TI = new StringBuilder(GetStringInBetween("<TI>", "</TI>", docs[i]));
                StringBuilder TEXT = new StringBuilder(GetStringInBetween("<TEXT>", "</TEXT>", docs[i]));
                //Document document = new Document(DOCNO, DATE1, TI, TEXT);
                //Document document = CreateDocument(docs[i]);
                masterFile.m_documents.Add(DOCNO.ToString(), new Document(DOCNO, DATE1, TI, TEXT));
                masterFile.m_docAmount++;
            }
        }

        public static string GetStringInBetween(string strBegin, string strEnd, string strSource)
        {
            string[] firstSplit, secondSplit;
            firstSplit = strSource.Split(new[] { strBegin }, StringSplitOptions.None);
            secondSplit = firstSplit[1].Split(new[] { strEnd }, StringSplitOptions.None);
            return secondSplit[0];
        }

        /*
        private Document CreateDocument(string strSource)
        {
            string[] docData, text, ti, date, docno;
            docData = strSource.Split(new[] { "</DOC>" }, StringSplitOptions.None);
            text = docData[0].Split(new[] { "</TEXT>" }, StringSplitOptions.None);
            text = text[0].Split(new[] { "<TEXT>" }, StringSplitOptions.None);

            ti = text[0].Split(new[] { "</TI>" }, StringSplitOptions.None);
            ti = ti[0].Split(new[] { "<TI>" }, StringSplitOptions.None);

            date = ti[0].Split(new[] { "</DATE>" }, StringSplitOptions.None);
            date = date[0].Split(new[] { "<DATE>" }, StringSplitOptions.None);
            StringBuilder DATE1 = new StringBuilder(date[1]);
            docno = date[0].Split(new[] { "</DCONO>" }, StringSplitOptions.None);
            docno = docno[0].Split(new[] { "<DCONO>" }, StringSplitOptions.None);
            return new Document(new StringBuilder(docno[1]), DATE1, new StringBuilder(ti[1]), new StringBuilder(text[1]));
        }
        */

        static void Main(string[] args)
        {
            string corpusPath = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\corpus";
            string path = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder";
            string pathLab = @"d:\documents\users\pezman\Documents\bbbbbbbbbbbbb\testFolder";
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            ReadFile r = new ReadFile(corpusPath, true);
            r.mainRead();

            //Indexer indexer = new Indexer(true, path);
            //indexer.writeToDocumentsFile(r.m_files);
            //indexer.writeToIndexFile(r.tmpP.m_allTerms);
            //indexer.writeToPostingFile(r.tmpP.m_allTerms);
            /*
            string check = "sdf sfsd";
            string[] s = check.Split(' ');
            for(int i=0; i< s.Length; i++)
            {
                Console.WriteLine("_"+s[i]+"_");
            }
            */
            //watch.Stop();
            //Console.WriteLine(watch.ElapsedMilliseconds);
            //Console.ReadLine();
            //Indexer indexer = new Indexer();
            //indexer.createTxtFile(path);
            //Console.ReadLine();
        }

    }
}