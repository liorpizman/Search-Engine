using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class NewIndexer
    {

        public static int m_indexCounter;
        public string m_outPutPath = @"C:\Users\Yehuda Pashay\Desktop\שנה ג'\אחזור\עבודה-מנוע חיפוש\מנוע 22.11\corp\output"; // change to path from GUI
        public bool doStem;

        public static int[] currentLine = new int[27];
        public static Dictionary<string, IndexTerm>[] dictionarys = new Dictionary<string, IndexTerm>[27];
        public Dictionary<string, DocumentTerms> tempDic;
        StreamWriter Writer;
        StreamReader Reader; /////////////////// has to changed

        public NewIndexer(bool doStemming, string m_outPutPath)
        {
            tempDic = new Dictionary<string, DocumentTerms>();
            initDic();
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
        public void CreateTxtFile(string fileName, StringBuilder dataToFile)
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
            CreateTxtFile("Documents.txt", data);

        }


        public void WriteToIndexFile(Dictionary<string, DocumentTerms> documentTermsDic)
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
            CreateTxtFile("Dictionary.txt", data);
        }

        public void CreatePostingFiles()
        {
            StringBuilder data = new StringBuilder();
            for (int i = 0; i < 27; i++)
            {
                CreateTxtFile("Posting" + i + ".txt", data);
                CreateTxtFile("NewPosting" + i + ".txt", data);
            }

        }

        public void writeToPostingFile(Dictionary<string, DocumentTerms> documentTermsDic, bool hasWritten)
        {



            //StreamWriter sw = null;
            //StreamReader sr = null;
            // sw = new StreamWriter(postingFile);
            // sr = new StreamReader(tempPostingFile);
            CreatePostingFiles();
            tempDic = documentTermsDic;
            InitTerms();
            if (!hasWritten)
            {
                StringBuilder data = new StringBuilder();
                //createTxtFile("Posting.txt", data);
                //initDic(); ////////////////////////
                int postNum = -1;
                //StreamWriter sw = new StreamWriter(Path.Combine(m_outPutPath, "Posting.txt"));
                foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
                {
                    currentPostingForFirstWrite(pair.Value.postNum, postNum);
                    postNum = pair.Value.postNum;
                    IndexTerm currentTerm = new IndexTerm(pair.Key, postNum, currentLine[postNum]);
                    currentLine[postNum]++;
                    currentTerm.IncreaseTf(pair.Value.m_tfc);
                    currentTerm.IncreaseDf();
                    dictionarys[postNum].Add(pair.Key, currentTerm);
                    Writer.WriteLine(pair.Value.WriteToPostingFileDocDocTerm(false));

                }
                tempDic.Clear();
                Writer.Flush();
                Writer.Close();
            }
            else
            {
                UpdatePosting();
            }
        }



        public void initDic()
        {
            for (int i = 0; i < 27; i++)
            {
                dictionarys[i] = new Dictionary<string, IndexTerm>();
                currentLine[i] = 0;
            }
        }

        public void currentPosting(int termPost, int currentPos)
        {
            bool check = Writer != null && Reader != null;
            if (currentPos == -1 || termPost != currentPos)
            {
                if (check)
                {
                    Writer.Flush();
                    Writer.Close();
                    Reader.Close();
                }

                Writer = new StreamWriter(Path.Combine(m_outPutPath, "NewPosting" + termPost + ".txt"));
                Reader = new StreamReader(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt")); /////////////////// has to changed
            }

        }

        public void currentPostingForFirstWrite(int termPost, int currentPos)
        {
            bool check = Writer != null && Reader != null;
            if (currentPos == -1 || termPost != currentPos)
            {
                if (check)
                {
                    Writer.Flush();
                    Writer.Close();
                    Reader.Close();
                }

                Writer = new StreamWriter(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt"));
                // Reader = new StreamReader(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt")); /////////////////// has to changed
            }

        }

        public void UpdatePosting()
        {
            int PostNumber = -1, LineNumber, currentLineNumber = 0;
            StringBuilder data = new StringBuilder();
            //createTxtFile("NewPosting.txt", data);

            foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
            {
                //StreamWriter outputFile = new StreamWriter(Path.Combine(m_outPutPath, "newPost " + pair.Value.m_Terms[pair.Key].postNum+".txt"));

                ///////////////////////// here need to add the definition of the file we wrote
                currentPosting(pair.Value.postNum, PostNumber);
                string currentLineInFile;
                currentLineNumber = currentLine[PostNumber];

                PostNumber = pair.Value.postNum;
                LineNumber = pair.Value.line;

                if (LineNumber != -1) // exist in the posting file
                {
                    while (LineNumber < currentLineNumber)
                    {
                        if ((currentLineInFile = Reader.ReadLine()) != null)
                        {
                            Writer.WriteLine(currentLineInFile);
                            currentLineNumber++;
                        }
                    }
                    currentLineInFile = Reader.ReadLine();
                    Writer.WriteLine(currentLineInFile + pair.Value.WriteToPostingFileDocDocTerm(true));
                    dictionarys[PostNumber][pair.Key].IncreaseTf(pair.Value.m_tfc);
                    dictionarys[PostNumber][pair.Key].IncreaseDf();
                    //currentLineNumber++;
                }
                else
                {
                    IndexTerm currentTerm = new IndexTerm(pair.Key, PostNumber, currentLine[PostNumber]);
                    LineNumber = currentLine[PostNumber];
                    currentLine[PostNumber]++;
                    currentTerm.IncreaseTf(pair.Value.m_tfc);
                    currentTerm.IncreaseDf();
                    dictionarys[PostNumber].Add(pair.Key, currentTerm);
                    Writer.WriteLine(pair.Value.WriteToPostingFileDocDocTerm(false));

                }


                /*
   
                PostNumber = GetPostNumer(pair.Key);
                if (dictionarys[PostNumber].ContainsKey(pair.Value.m_valueOfTerm))
                {
                    //LineNumber = dictionarys[PostNumber][pair.Key].lineInPost;
                    dictionarys[PostNumber][pair.Key].IncreaseTf(pair.Value.m_tfc);
                    dictionarys[PostNumber][pair.Key].IncreaseDf();
                }
                else
                {
                    IndexTerm currentTerm = new IndexTerm(pair.Key, PostNumber, currentLine[PostNumber]);
                    LineNumber = currentLine[PostNumber];
                    currentLine[PostNumber]++;
                    currentTerm.IncreaseTf(pair.Value.m_tfc);
                    currentTerm.IncreaseDf();
                    dictionarys[PostNumber].Add(pair.Key, currentTerm);

                }

                while(LineNumber<)


                if (LineNumber == -1)
                {
                    while ((currentLineInFile = Reader.ReadLine()) != null)
                    {
                        data.Append(currentLine);
                    }
                    data.Append(pair.Value.m_Terms[pair.Key].writeDocumentToPostingFile(currentLine[PostNumber], true));
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
                    data.Append(currentLineInFile = currentLineInFile + (pair.Value.m_Terms[pair.Key].writeDocumentToPostingFile(currentLine[PostNumber], false)));
                    while ((currentLineInFile = Reader.ReadLine()) != null)
                    {
                        data.Append(currentLineInFile);
                        data.Append(Environment.NewLine);
                    }
                    createTxtFile("newPost.txt", data);
                    */
            }
            Writer.Flush();
            Writer.Close();
            Reader.Close();
            ///in the end:
            tempDic.Clear();
        }


        private void InitTerms()
        {
            foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
            {
                int PostNumber = pair.Value.postNum;
                if (dictionarys[PostNumber].ContainsKey(pair.Key))
                {
                    pair.Value.line = dictionarys[PostNumber][pair.Key].lineInPost;
                }
            }
            this.tempDic = tempDic.OrderBy(i => i.Value.postNum).ThenBy(i => i.Value.line).ToDictionary(p => p.Key, p => p.Value); ;
        }



        public int GetPostNumer(string m_value)
        {
            if (m_value == "")
            {
                return 26;
            }
            else
            {
                char c = m_value.ToLower()[0];
                if (c == 'a')
                    return 0;
                else if (c == 'b')
                    return 1;
                else if (c == 'c')
                    return 2;
                else if (c == 'd')
                    return 3;
                else if (c == 'e')
                    return 4;
                else if (c == 'f')
                    return 5;
                else if (c == 'g')
                    return 6;
                else if (c == 'h')
                    return 7;
                else if (c == 'i')
                    return 8;
                else if (c == 'j')
                    return 9;
                else if (c == 'k')
                    return 10;
                else if (c == 'l')
                    return 11;
                else if (c == 'm')
                    return 12;
                else if (c == 'n')
                    return 13;
                else if (c == 'o')
                    return 14;
                else if (c == 'p')
                    return 15;
                else if (c == 'q')
                    return 16;
                else if (c == 'r')
                    return 17;
                else if (c == 's')
                    return 18;
                else if (c == 't')
                    return 19;
                else if (c == 'u')
                    return 20;
                else if (c == 'v')
                    return 21;
                else if (c == 'w')
                    return 22;
                else if (c == 'x')
                    return 23;
                else if (c == 'y')
                    return 24;
                else if (c == 'z')
                    return 25;
                else
                {
                    return 26;
                }
            }

        }
    }









}

