using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Indexer
    {
        public static int m_indexCounter;
        public string m_outPutPath = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder"; // change to path from GUI
        public bool doStem;

        public Indexer(bool doStemming, string m_outPutPath)
        {
            m_indexCounter = 0;
            this.doStem = doStemming;
            this.m_outPutPath = m_outPutPath;
            if (doStem)
            {
                this.m_outPutPath = Path.Combine(m_outPutPath, "WithStem");
            }
            else
            {
                this.m_outPutPath = Path.Combine(m_outPutPath, "WithOutStem");
            }
        }


        // fileName = "fileNameToCreate.txt" (including .txt)
        // mydocpath = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder"; // change to path from GUI
        // dataToFile = "add here the data structure!"
        public void createTxtFile(string fileName, StringBuilder dataToFile)
        {
            if (!Directory.Exists(m_outPutPath))
            {
                Directory.CreateDirectory(m_outPutPath);
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(m_outPutPath, fileName)))
            {
                outputFile.WriteLine(dataToFile);
            }
        }


        public void writeToDocumentsFile(Dictionary<string, masterFile> masterFilesDictionary)
        {
            StringBuilder data = new StringBuilder();
            foreach (masterFile m in masterFilesDictionary.Values)
            {
                foreach (Document d in m.m_documents.Values)
                {
                    data.Append(d.writeDocumentToIndexFile());
                    data.Append(Environment.NewLine);
                    //data.Append(Environment.NewLine);
                }
            }
            createTxtFile("Documents.txt", data);

        }


        public void writeToIndexFile(Dictionary<string, DocumentTerms> documentTermsDic)
        {
            StringBuilder data = new StringBuilder();
            foreach (string termValue in documentTermsDic.Keys)
            {
                data.Append(termValue);                                   // value of the term
                data.Append("\t");
                data.Append(documentTermsDic[termValue].m_Terms.Count);   // df - N docs - num of docs the term exists
                data.Append("\t");
                data.Append(documentTermsDic[termValue].m_tfc);           // tfc - num of term instances in all files
                data.Append(Environment.NewLine);
            }
            createTxtFile("Dictionary.txt", data);
        }

        public void writeToPostingFile(Dictionary<string, DocumentTerms> documentTermsDic)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, DocumentTerms> pair in documentTermsDic)
            {
                data.Append(pair.Value.m_Terms[pair.Key].writeDocumentToPostingFile()); //writing all terms in current document term
                data.Append(Environment.NewLine);
            }
            createTxtFile("Posting.txt", data);
        }
    }
}
