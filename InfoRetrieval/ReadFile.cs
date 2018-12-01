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
    public class ReadFile
    {
        public string m_mainPath;
        public string[] m_paths;
        public int m_indexCurrFile;
        public List<string[]> path_Chank;
        public int ChankSize;

        public ReadFile(string m_mainPath, int ChankSize)
        {
            this.m_mainPath = m_mainPath;
            this.m_indexCurrFile = 0;
            path_Chank = new List<string[]>();
            this.ChankSize = ChankSize;
            MainRead();
            InitList();
        }


        public void InitList()
        {
            int sumFiles = 0;
            int currentListSize = Math.Min(ChankSize, m_paths.Length - sumFiles);
            bool doChank = currentListSize != 0;
            for (int i = 0, step = 0; currentListSize != 0; i++, step += ChankSize)
            {
                path_Chank.Add(new string[currentListSize]);
                for (int j = 0; j < currentListSize; j++)
                {
                    path_Chank[i][j] = m_paths[step + j];
                }
                sumFiles += currentListSize;
                currentListSize = Math.Min(ChankSize, m_paths.Length - sumFiles);
            }
        }

        public masterFile ReadChunk(int i)
        {
            string[] currentChunk = path_Chank[i];
            string[] fields = currentChunk[0].Split('\\');
            masterFile masterFile = new masterFile(fields[fields.Length - 1], currentChunk[0]);
            for (int j = 0; j < currentChunk.Length; j++)
            {
                ReadDocuments(currentChunk[j], masterFile);
            }
            return masterFile;
            /// we can return masterFile and send it to parser and do not sent the whole dictionary

        }

        public void MainRead()
        {
            string[] directories = Directory.GetDirectories(Directory.GetDirectories(m_mainPath)[0]);
            m_paths = new string[directories.Length];
            for (int index = 0; index < directories.Length; index++)
            {
                m_paths[index] = Directory.GetFiles(directories[index])[0];
            }
        }

        public masterFile ReadNewFile(string file)
        {
            string[] fields = file.Split('\\');
            string currFileName = fields[fields.Length - 1];
            masterFile masterFile = new masterFile(currFileName, file);
            ReadDocuments(file, masterFile);
            return masterFile;
            /// we can return masterFile and send it to parser and do not sent the whole dictionary
        }

        private void ReadDocuments(string file, masterFile masterFile)
        {
            string content = File.ReadAllText(file);
            string[] docs = content.Split(new[] { "<DOC>" }, StringSplitOptions.None);
            string DOCNO, TEXT;
            for (int i = 1; i < docs.Length; i++)
            {
                /*
                if ((docs[i].Contains("[TEXT]") && !docs[i].Contains("<TEXT>")) || docs[i].Length < 150)
                {
                    string b = docs[i];
                    int a = docs[i].Length;
                }
                */
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