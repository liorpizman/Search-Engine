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
        public static Dictionary<string, masterFile> m_files = new Dictionary<string, masterFile>();
        public string m_mainPath;
        public string[] m_paths;
        public int m_indexCurrFile;
        public int m_maxFiles = 16;
        public bool readerThreadFinished = false;

        public ReadFile(string m_mainPath)
        {
            this.m_mainPath = m_mainPath;
            this.m_indexCurrFile = 0;
        }

        public void MainRead()
        {
            Parse parse = new Parse(true, m_mainPath);
            string[] directories = Directory.GetDirectories(Directory.GetDirectories(m_mainPath)[0]);
            m_paths = new string[directories.Length];
            //Thread[] threads = new Thread[10];
            for (int index = 0; index < directories.Length; index++)
            {
                m_paths[index] = Directory.GetFiles(directories[index])[0];                  /// check wether one file in one folder
            }
            masterFile currMasterFile = null;
            //Thread readThread = new Thread(new ThreadStart(() => ReadAllFiles()));
            //readThread.Start();
            for (int i = 0; i < m_paths.Length;)
            {
                for (int j = 0; j < m_maxFiles && i < m_paths.Length; j++)
                {
                    currMasterFile = ReadNewFile(m_paths[i]); //currMasterFile = 
                    parse.ParseMasterFile(currMasterFile);
                    i++;
                }
                parse.m_allTerms.Clear();
            }
            //readThread.Join();
        }

        private void ReadAllFiles()
        {
            for (int i = 0; i < m_paths.Length; i++)
            {
                ReadNewFile(m_paths[i]);
            }
        }

        private masterFile ReadNewFile(string file)
        {
            string[] fields = file.Split('\\');
            string currFileName = fields[fields.Length - 1];
            masterFile masterFile = new masterFile(currFileName, file);
            ReadDocuments(file, masterFile);
            m_files.Add(currFileName, masterFile);
            return masterFile;
            /// we can return masterFile and send it to parser and do not sent the whole dictionary
        }

        private void ReadDocuments(string file, masterFile masterFile)
        {
            string content = File.ReadAllText(file);                                      //check run time!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            string[] docs = content.Split(new[] { "<DOC>" }, StringSplitOptions.None);
            string DOCNO, TEXT; //allDocument,
            for (int i = 1; i < docs.Length; i++)
            {
                //allDocument = "<DOC>" + docs[i];
                DOCNO = GetStringInBetween("<DOCNO>", "</DOCNO>", docs[i]);
                StringBuilder DATE1 = new StringBuilder(GetDateInBetween(docs[i]));  // add condition with DATE
                StringBuilder TI = new StringBuilder(GetStringInBetween("<TI>", "</TI>", docs[i]));
                TEXT = GetStringInBetween("<TEXT>", "</TEXT>", docs[i]);                                     // add condition when TEXT does not exist
                StringBuilder City = new StringBuilder(GetCityInBetween(docs[i]));
                masterFile.m_documents.Add(DOCNO.ToString(), new Document(DOCNO, DATE1, TI, TEXT, City));
                masterFile.m_docAmount++;
            }
        }

        public static string GetStringInBetween(string strBegin, string strEnd, string strSource)
        {
            string[] firstSplit, secondSplit;
            firstSplit = strSource.Split(new[] { strBegin }, StringSplitOptions.None);
            if (firstSplit.Length < 2)
            {
                return "";
            }
            secondSplit = firstSplit[1].Split(new[] { strEnd }, StringSplitOptions.None);
            return secondSplit[0];
        }

        public static string GetDateInBetween(string strSource)
        {
            string[] firstSplit, secondSplit;
            if (strSource.Contains("<DATE1>"))
            {
                firstSplit = strSource.Split(new[] { "<DATE1>" }, StringSplitOptions.None);
                secondSplit = firstSplit[1].Split(new[] { "</DATE1>" }, StringSplitOptions.None);
            }
            else
            {
                firstSplit = strSource.Split(new[] { "<DATE>" }, StringSplitOptions.None);
                secondSplit = firstSplit[1].Split(new[] { "</DATE>" }, StringSplitOptions.None);
            }
            return secondSplit[0];
        }

        public static string GetCityInBetween(string strSource)
        {
            string[] firstSplit, secondSplit;
            firstSplit = strSource.Split(new[] { "<F P=104>" }, StringSplitOptions.None);
            if (firstSplit.Length < 2)
            {
                return "";
            }
            secondSplit = firstSplit[1].Split(new[] { "</F>" }, StringSplitOptions.None);
            return secondSplit[0].Trim(' ').Split(' ')[0].ToUpper();
        }
    }
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
