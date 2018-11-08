using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class ReadFile
    {
        public Dictionary<string, masterFile> m_files;
        public string m_mainPath;
        public int m_filesAmount;
        //public string[] m_paths;


        public ReadFile(string m_mainPath)
        {
            this.m_files = new Dictionary<string, masterFile>();
            this.m_mainPath = m_mainPath;
            this.m_filesAmount = 0;
        }

        public void mainRead()
        {
            string[] directories = Directory.GetDirectories(m_mainPath);
            //Console.WriteLine("----------directories:--------------");
            foreach (string directory in directories)
            {
                //Console.WriteLine(directory);
            }
            subRead(directories);
        }

        private void subRead(string[] directories)
        {
            //Console.WriteLine("----------files:--------------");
            foreach (string directory in directories)
            {
                string[] files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    readFile(file);
                    //Console.WriteLine(file);
                    m_filesAmount++;
                }
            }
            // Console.WriteLine("----------totatl files : " + m_filesAmount + "--------------");
        }

        private void readFile(string file)
        {
            int pos = file.LastIndexOf("\\") + 1;
            string currFileName = file.Substring(pos, file.Length - pos);
            masterFile masterFile = new masterFile(currFileName, file);
            readDocuments(file, masterFile);
            m_files.Add(currFileName, masterFile);
        }

        private void readDocuments(string file, masterFile masterFile)
        {
            string content = File.ReadAllText(file);
            int pos;
            while (content != String.Empty)
            {
                string allDocument = GetStringInBetween("<DOC>", "</DOC>", content, false, false);
                string DOCNO = GetStringInBetween("<DOCNO>", "</DOCNO>", allDocument, false, false);
                //Console.WriteLine("DOCNO : " + DOCNO);
                string DATE1 = GetStringInBetween("<DATE1>", "</DATE1>", allDocument, false, false);
                //Console.WriteLine("DATE1 : " + DATE1);
                string TI = GetStringInBetween("<TI>", "</TI>", allDocument, false, false);
                //Console.WriteLine("TI : " + TI);
                string TEXT = GetStringInBetween("<TEXT>", "</TEXT>", allDocument, false, false);
                //Console.WriteLine("TEXT : " + TEXT);
                //Console.WriteLine(content);
                //Console.ReadLine();
                Document document = new Document(DOCNO, DATE1, TI, TEXT);
                masterFile.m_documents.Add(DOCNO, document);
                masterFile.m_docAmount++;
                pos = content.IndexOf("</DOC>");
                if (pos != -1)
                    content = content.Substring(pos + 6);
                if (!content.Contains("<DOC>"))
                    content = String.Empty;
            }
        }

        public static string GetStringInBetween(string strBegin, string strEnd, string strSource, bool includeBegin, bool includeEnd)
        {
            string[] result = { string.Empty, string.Empty };
            int iIndexOfBegin = strSource.IndexOf(strBegin);

            if (iIndexOfBegin != -1)
            {
                // include the Begin string if desired 
                if (includeBegin)
                    iIndexOfBegin -= strBegin.Length;

                strSource = strSource.Substring(iIndexOfBegin + strBegin.Length);

                int iEnd = strSource.IndexOf(strEnd);
                if (iEnd != -1)
                {
                    // include the End string if desired 
                    if (includeEnd)
                        iEnd += strEnd.Length;
                    result[0] = strSource.Substring(0, iEnd);
                    // advance beyond this segment 
                    if (iEnd + strEnd.Length < strSource.Length)
                        result[1] = strSource.Substring(iEnd + strEnd.Length);
                }
            }
            else
                // stay where we are 
                result[1] = strSource;
            return result[0];
        }




        static void Main(string[] args)
        {
            ReadFile r = new ReadFile(@"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder");
            r.mainRead();
            //string file = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder";
            //int pos = file.LastIndexOf("\\") + 1;
            //string currFileName = file.Substring(pos, file.Length - pos);
            //masterFile masterFile = new masterFile(currFileName, file);
            Parse parse = new Parse(true);
            parse.parseDocuments(r.m_files["FB396001"]);
            Console.ReadLine();
        }
        
    }
}
