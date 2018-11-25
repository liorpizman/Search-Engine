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
        public string m_outPutPath = @"C:\Users\Yehuda Pashay\Desktop\שנה ג'\אחזור\עבודה-מנוע חיפוש\מנוע 22.11\corp\output"; // change to path from GUI
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
                    data.Append(d.WriteDocumentToIndexFile());
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

        public void writeToPostingFile(Dictionary<string, DocumentTerms> documentTermsDic, bool hasWritten)
        {
            if (!hasWritten)
            {
                initDic();
                int postNum = 0;
                StringBuilder data = new StringBuilder();
                foreach (KeyValuePair<string, DocumentTerms> pair in documentTermsDic)
                {
                    postNum = pair.Value.m_Terms[pair.Key].postNum;
                   // data.Append(pair.Value.m_Terms[pair.Key].WriteDocumentToPostingFile(currentLine[postNum],true)); //writing all terms in current document term
                    data.Append(Environment.NewLine);
                    ////////////////////////////////////////////////
                    dictionarysArray[postNum].Add(pair.Value.m_valueOfTerm, pair.Value.m_Terms[pair.Key]);
                    currentLine[postNum]++;

                }
                createTxtFile("Posting.txt", data);
            }
            else
            {
                UpdatePosting(documentTermsDic);
            }

        }




        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static int[] currentLine = new int[27];
        private static Dictionary<string, Term>[] dictionarysArray = new Dictionary<string, Term>[27];

        public void initDic()
        {
            for (int i = 0; i < 27; i++)
            {
                dictionarysArray[i]= new Dictionary<string, Term>();
                currentLine[i] = 0;
            }
        }

        public void UpdatePosting(Dictionary<string, DocumentTerms> tmpTermsDic)
        {
            int PostNumber,LineNumber;
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, DocumentTerms> pair in tmpTermsDic)
            {
                //StreamWriter outputFile = new StreamWriter(Path.Combine(m_outPutPath, "newPost " + pair.Value.m_Terms[pair.Key].postNum+".txt"));
                StreamReader Reader = new StreamReader(Path.Combine(m_outPutPath, "Posting.txt")); /////////////////// has to changed
                string currentLineInFile;

                PostNumber = pair.Value.m_Terms[pair.Key].postNum;
                if (dictionarysArray[PostNumber].ContainsKey(pair.Value.m_valueOfTerm)){
                    LineNumber = dictionarysArray[PostNumber][pair.Value.m_valueOfTerm].lineInPost;
                }
                else
                {
                    dictionarysArray[PostNumber].Add(pair.Key, pair.Value.m_Terms[pair.Key]);
                    LineNumber = -1;
                }

                if (LineNumber == -1)
                {
                    while((currentLineInFile = Reader.ReadLine()) != null){
                        data.Append(currentLine);
                    }
         //           data.Append(pair.Value.m_Terms[pair.Key].WriteDocumentToPostingFile(currentLine[PostNumber],true));
                    data.Append(Environment.NewLine);
                    currentLine[PostNumber]++;

                    createTxtFile("newPost.txt", data);

                }
                else
                {
                    int i = 0;
                    for (; i < LineNumber; i++)
                    {
                        currentLineInFile = Reader.ReadLine();
                        data.Append(currentLine);
                        data.Append(Environment.NewLine);


                    }
                    currentLineInFile = Reader.ReadLine();
         //           data.Append(currentLineInFile = currentLineInFile+( pair.Value.m_Terms[pair.Key].WriteDocumentToPostingFile(currentLine[PostNumber],false)));
                    while ((currentLineInFile = Reader.ReadLine()) != null)
                    {
                        data.Append(currentLineInFile);
                        data.Append(Environment.NewLine);
                    }
                    createTxtFile("newPost.txt", data);
                }
            }
        }















    }
}
