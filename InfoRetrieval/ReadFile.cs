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
    /// <summary>
    /// Class which represents a Reader which reads all documents in the corpus
    /// </summary>
    public class ReadFile : IReadFile
    {
        /// <summary>
        /// fields of ReadFile
        /// </summary>
        public string m_mainPath { get; private set; }
        public string[] m_paths { get; private set; }
        public int m_indexCurrFile { get; private set; }
        public List<string[]> path_Chank { get; private set; }
        public int ChunkSize { get; private set; }

        /// <summary>
        /// constructor of ReadFile
        /// </summary>
        /// <param name="m_mainPath">the path of the corpus</param>
        public ReadFile(string m_mainPath)
        {
            this.m_mainPath = m_mainPath;
            this.m_indexCurrFile = 0;
            path_Chank = new List<string[]>();
            MainRead();
            ChunkSize = Math.Min(120, m_paths.Length);
            InitList();
        }

        /// <summary>
        /// method which splits the corpus to chunks
        /// </summary>
        public void InitList()
        {
            int sumFiles = 0;
            int currentListSize = Math.Min(ChunkSize, m_paths.Length - sumFiles);
            bool doChank = currentListSize != 0;
            for (int i = 0, step = 0; currentListSize != 0; i++, step += ChunkSize)
            {
                path_Chank.Add(new string[currentListSize]);
                for (int j = 0; j < currentListSize; j++)
                {
                    path_Chank[i][j] = m_paths[step + j];
                }
                sumFiles += currentListSize;
                currentListSize = Math.Min(ChunkSize, m_paths.Length - sumFiles);
            }
        }

        /// <summary>
        /// method which reads all documents in a chunck
        /// </summary>
        /// <param name="i">the id of the chunck</param>
        /// <returns>the collection of files</returns>
        public MasterFile ReadChunk(int i)
        {
            string[] currentChunk = path_Chank[i];
            string[] fields = currentChunk[0].Split('\\');
            string currFileName = fields[fields.Length - 1];
            MasterFile masterFile = new MasterFile(currFileName, currentChunk[0]);
            for (int j = 0; j < currentChunk.Length; j++)
            {
                ReadDocuments(currentChunk[j], masterFile);
            }
            return masterFile;
        }

        /// <summary>
        /// method to get all pathes of all files in the corpus
        /// </summary>
        public void MainRead()
        {
            string[] directories = Directory.GetDirectories(Directory.GetDirectories(m_mainPath)[0]);
            m_paths = new string[directories.Length];

            for (int index = 0; index < directories.Length; index++)
            {
                m_paths[index] = Directory.GetFiles(directories[index])[0];
            }
        }

        /// <summary>
        /// method which reads all documents in current master file
        /// </summary>
        /// <param name="file">the path of current file</param>
        /// <param name="masterFile">current master file</param>
        private void ReadDocuments(string file, MasterFile masterFile)
        {
            string content = File.ReadAllText(file);
            string[] docs = content.Split(new[] { "<DOC>" }, StringSplitOptions.None);
            string DOCNO, TEXT;
            for (int i = 1; i < docs.Length; i++)
            {
                DOCNO = GetStringInBetween("<DOCNO>", "</DOCNO>", docs[i]).Trim(' ');
                StringBuilder DATE1 = new StringBuilder(GetDateInBetween(docs[i]).Trim(' '));
                StringBuilder TI = new StringBuilder(GetStringInBetween("<TI>", "</TI>", docs[i]).Trim(' '));
                TEXT = TI.ToString() + " ";
                TEXT += GetStringInBetween("<TEXT>", "</TEXT>", docs[i]);
                StringBuilder City = new StringBuilder(GetCityInBetween(docs[i]));
                StringBuilder language = new StringBuilder(GetLanguageInBetween(docs[i]));
                masterFile.m_documents.Add(DOCNO.ToString(), new Document(DOCNO, DATE1, TI, TEXT, City, language));
                masterFile.m_docAmount++;
            }
        }

        /// <summary>
        /// method to get the string between two tags
        /// </summary>
        /// <param name="strBegin">first tag</param>
        /// <param name="strEnd">second tag</param>
        /// <param name="strSource">the source string</param>
        /// <returns>the string between two tags</returns>
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


        /// <summary>
        /// method to get the string between two tags of date
        /// </summary>
        /// <param name="strSource">the source string</param>
        /// <returns>the string between two tags of date</returns>
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


        /// <summary>
        /// method to get the string between two tags of city
        /// </summary>
        /// <param name="strSource">the source string</param>
        /// <returns>the string between two tags of city</returns>
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

        /// <summary>
        /// method to get the string between two tags of language
        /// </summary>
        /// <param name="strSource">the source string</param>
        /// <returns>the string between two tags of language</returns>
        public static string GetLanguageInBetween(string strSource)
        {
            string[] firstSplit, secondSplit;
            firstSplit = strSource.Split(new[] { "<F P=105>" }, StringSplitOptions.None);
            if (firstSplit.Length < 2)
            {
                return "";
            }
            secondSplit = firstSplit[1].Split(new[] { "</F>" }, StringSplitOptions.None);
            return secondSplit[0].Trim(' ').Split(' ')[0];
        }
    }
}