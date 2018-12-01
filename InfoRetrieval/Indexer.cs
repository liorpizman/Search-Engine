using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace InfoRetrieval
{
    public class Indexer
    {
        public int m_indexCounter;
        public string m_outPutPath;
        public bool m_doStem;
        public Dictionary<int, int> currentLine;
        public Dictionary<string, IndexTerm>[] dictionaries;
        public Dictionary<string, DocumentTerms> tempDic;
        public StreamWriter Writer;
        //public StreamReader Reader;
        public bool m_StreamHasChanged;
        public Queue<Dictionary<string, DocumentTerms>> IndexQueue;
        public bool m_CorpusDone;
        public Semaphore m_semaphore;

        public Indexer(bool doStemming, string m_outPutPath, int queueSize)
        {
            this.m_semaphore = new Semaphore(queueSize, queueSize);
            this.currentLine = new Dictionary<int, int>();
            this.dictionaries = new Dictionary<string, IndexTerm>[27];
            this.tempDic = new Dictionary<string, DocumentTerms>();
            this.m_StreamHasChanged = false;
            this.IndexQueue = new Queue<Dictionary<string, DocumentTerms>>();
            this.m_CorpusDone = false;
            this.m_indexCounter = 1;
            this.m_doStem = doStemming;
            this.m_outPutPath = m_outPutPath;
            if (m_doStem)
            {
                this.m_outPutPath = Path.Combine(m_outPutPath, "WithStem");
            }
            else
            {
                this.m_outPutPath = Path.Combine(m_outPutPath, "WithOutStem");
            }
            InitDic();
            CreatePostingFiles();
        }

        public void IsLegalEntry()
        {
            m_semaphore.WaitOne();
        }

        public void AddToQueue(Dictionary<string, DocumentTerms> ToAdd)
        {
            IndexQueue.Enqueue(ToAdd);
        }

        public void SetCorpusDone()
        {
            this.m_CorpusDone = true;
        }

        public void MainIndexRun()
        {
            ////////////////////////////////////// we should check if all cleared before run it[Serializable]
            bool FirstWrite = false;


            //while (!m_CorpusDone)
            //{
            while (IndexQueue.Count > 0)
            {
                WriteToPostingFile(IndexQueue.Dequeue(), FirstWrite);
                m_semaphore.Release();
                FirstWrite = true;
            }
            //}


            /*
            while (IndexQueue.Count > 0)  /////////to ensure all the files are did post
            {  WriteToPostingFile(IndexQueue.Dequeue(), FirstWrite);}
            */
            ////////////MergePosting();///////////////-check-it//////////////
            WriteTheNewIndexFile();
            //WriteTheNewIndexBinFile();
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

        public void CreateEmptyTxtFile(string fileName)
        {
            if (!Directory.Exists(m_outPutPath))
            {
                Directory.CreateDirectory(m_outPutPath);
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(m_outPutPath, fileName)))
            {
                outputFile.WriteLine();
            }
        }

        public void CreatePostingFiles()
        {
            //StringBuilder data = new StringBuilder();
            for (int i = 0; i < 27; i++)
            {
                //CreateTxtFile("Posting" + i + ".txt", data);
                CreateEmptyTxtFile("Posting" + i + ".txt");
                //CreateTxtFile("NewPosting" + i + ".txt", data);
                CreateEmptyTxtFile("NewPosting" + i + ".txt");
            }
        }

        public void CreateNewPostingFiles()
        {
            //StringBuilder data = new StringBuilder();
            for (int i = 0; i < 27; i++)
            {
                //CreateTxtFile("NewPosting" + i + ".txt", data);
                CreateEmptyTxtFile("NewPosting" + i + ".txt");
            }
        }

        public void WriteToPostingFile(Dictionary<string, DocumentTerms> documentTermsDic, bool hasWritten)
        {
            tempDic = documentTermsDic;
            //InitTerms();  
            if (!hasWritten)
            {
                StringBuilder data = new StringBuilder();
                this.tempDic = tempDic.OrderBy(i => i.Value.postNum).ThenBy(i => i.Value.line).ToDictionary(p => p.Key, p => p.Value);
                int postNum = -1;
                //StreamWriter sw = new StreamWriter(Path.Combine(m_outPutPath, "Posting.txt"));
                foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
                {
                    if (StreamShouldBeSwitched(pair.Value.postNum, postNum))
                    {
                        if (Writer != null)
                        {
                            Writer.Write(data);  //write all data in one step
                            data.Clear();       // clear data from string builder
                            Writer.Flush();
                            Writer.Close();
                        }
                        m_StreamHasChanged = true;
                        Writer = new StreamWriter(Path.Combine(m_outPutPath, "Posting" + pair.Value.postNum + ".txt"));
                        //SwitchWriterForFirstPosting(pair.Value.postNum, postNum);
                    }
                    postNum = pair.Value.postNum;
                    IndexTerm currentTerm = new IndexTerm(pair.Key, postNum, currentLine[postNum]);
                    //currentTerm.IncreaseTf(pair.Value.m_tfc);
                    currentTerm.IncreaseDf(pair.Value.m_Terms.Count);
                    dictionaries[postNum].Add(pair.Key, currentTerm);
                    //Writer.WriteLine(pair.Value.WriteToPostingFileDocDocTerm(false));
                    data.Append(pair.Value.WriteToPostingFileDocDocTerm(false));
                    data.Append(Environment.NewLine);
                    currentLine[postNum]++;
                }
                if (Writer != null)
                {
                    Writer.Write(data);
                    data.Clear();
                }
                tempDic.Clear();
                Writer.Flush();
                Writer.Close();
                Writer = null;
            }
            else
            {
                UpdatePosting();
                DeleteOldFiles();
            }
            Console.WriteLine("counter:   " + m_indexCounter++);
        }

        public void InitDic()
        {
            for (int i = 0; i < 27; i++)
            {
                dictionaries[i] = new Dictionary<string, IndexTerm>();
                currentLine[i] = 0;
            }
        }

        public bool StreamShouldBeSwitched(int termPost, int currentPos)
        {
            if (currentPos == -1 || termPost != currentPos)
            {
                return true;
            }
            return false;
        }

        public void SwitchWriterAndReaderForPosting(int termPost, int currentPos)
        {
            if (currentPos == -1 || termPost != currentPos)
            {
                m_StreamHasChanged = true;
                if (Writer != null)
                {
                    Writer.Flush();
                    Writer.Close();
                }
                /*
                if (Reader != null)
                {
                    Reader.Close();
                }
                */
                Writer = new StreamWriter(Path.Combine(m_outPutPath, "NewPosting" + termPost + ".txt"));
                //Reader = new StreamReader(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt"));
            }
        }

        public void SwitchWriterForFirstPosting(int termPost, int currentPos)
        {
            if (currentPos == -1 || termPost != currentPos)
            {
                if (Writer != null)
                {
                    Writer.Flush();
                    Writer.Close();
                }
                Writer = new StreamWriter(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt"));
            }
        }

        public void UpdatePosting()
        {
            InitTerms();
            CreateNewPostingFiles();
            char[] splitBy = { '\r', '\n' };
            bool finshedUpdatePrevTerms = true;
            string currentLineInFile, readData, path;
            int PostNumber = -1, LineNumber, currentLineNumber = 0, i = 0;
            StringBuilder writeData = new StringBuilder();
            string[] lines = null;
            foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
            {
                ///////////////////////// here need to add the definition of the file we wrote
                if (StreamShouldBeSwitched(pair.Value.postNum, PostNumber))
                {
                    if (Writer != null)
                    {
                        Writer.Write(writeData);
                        writeData.Clear();
                        Writer.Flush();
                        Writer.Close();
                    }
                    m_StreamHasChanged = true;
                    Writer = new StreamWriter(Path.Combine(m_outPutPath, "NewPosting" + pair.Value.postNum + ".txt"));
                    //SwitchWriterAndReaderForPosting(pair.Value.postNum, PostNumber);
                }
                if (m_StreamHasChanged)
                {
                    path = Path.Combine(m_outPutPath, "Posting" + pair.Value.postNum + ".txt");
                    readData = File.ReadAllText(path);
                    lines = readData.Split(splitBy, StringSplitOptions.RemoveEmptyEntries);
                    readData = null;
                    i = 0;
                    currentLineNumber = 0;
                    m_StreamHasChanged = false;
                    finshedUpdatePrevTerms = true;
                    //currentLineNumber = currentLine[PostNumber];
                }
                PostNumber = pair.Value.postNum;
                LineNumber = pair.Value.line;
                if (LineNumber != Int32.MaxValue) // exist in the posting file
                {
                    while (LineNumber > currentLineNumber)  //LineNumber < currentLineNumber)
                    {
                        //    if ((currentLineInFile = Reader.ReadLine()) != null)
                        //  {
                        //currentLineInFile = Reader.ReadLine();
                        currentLineInFile = lines[i++];
                        //Writer.WriteLine(currentLineInFile);
                        writeData.Append(currentLineInFile);
                        writeData.Append(Environment.NewLine);
                        currentLineNumber++;
                        // }
                    }
                    //currentLineInFile = Reader.ReadLine();
                    currentLineInFile = lines[i++];
                    //Writer.WriteLine(currentLineInFile + pair.Value.WriteToPostingFileDocDocTerm(true));
                    writeData.Append(currentLineInFile + pair.Value.WriteToPostingFileDocDocTerm(true));
                    writeData.Append(Environment.NewLine);
                    //dictionaries[PostNumber][pair.Key].IncreaseTf(pair.Value.m_tfc);
                    dictionaries[PostNumber][pair.Key].IncreaseDf(pair.Value.m_Terms.Count);
                    currentLineNumber++;
                }
                else
                {
                    if (finshedUpdatePrevTerms)
                    {
                        /*
                        while ((currentLineInFile = Reader.ReadLine()) != null)
                        {
                            Writer.WriteLine(currentLineInFile);
                            //currentLineNumber++;
                        }
                        */
                        while (i < lines.Length)
                        {
                            currentLineInFile = lines[i++];
                            writeData.Append(currentLineInFile);
                            writeData.Append(Environment.NewLine);
                        }
                        finshedUpdatePrevTerms = false;
                    }
                    IndexTerm currentTerm = new IndexTerm(pair.Key, PostNumber, currentLine[PostNumber]);
                    //LineNumber = currentLine[PostNumber];
                    currentLine[PostNumber]++;
                    //currentTerm.IncreaseTf(pair.Value.m_tfc);
                    currentTerm.IncreaseDf(pair.Value.m_Terms.Count);
                    dictionaries[PostNumber].Add(pair.Key, currentTerm);
                    //Writer.WriteLine(pair.Value.WriteToPostingFileDocDocTerm(false));
                    writeData.Append(pair.Value.WriteToPostingFileDocDocTerm(false));
                    writeData.Append(Environment.NewLine);
                }
            }
            if (Writer != null)
            {
                Writer.Write(writeData);
                writeData.Clear();
            }
            Writer.Flush();
            Writer.Close();
            //Reader.Close();
            Writer = null;
            //Reader = null;
            //in the end:
            tempDic.Clear();
        }

        public void DeleteOldFiles()
        {
            for (int i = 0; i < 27; i++)
            {

                if (File.Exists(Path.Combine(m_outPutPath, "Posting" + i + ".txt")))
                {
                    File.Delete(Path.Combine(m_outPutPath, "Posting" + i + ".txt"));
                }

                // rename src file -> dst file
                File.Move(Path.Combine(m_outPutPath, "NewPosting" + i + ".txt"), Path.Combine(m_outPutPath, "Posting" + i + ".txt"));

                if (File.Exists(Path.Combine(m_outPutPath, "NewPosting" + i + ".txt")))
                {
                    File.Delete(Path.Combine(m_outPutPath, "NewPosting" + i + ".txt"));
                }
            }
        }

        /*
        public void MergePosting()
        {
            //StringBuilder data = new StringBuilder();
            //CreateTxtFile("Posting.txt", data);
            CreateEmptyTxtFile("Posting.txt");
            string current;
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Posting.txt"));
            for (int i = 0; i < 27; i++)
            {
                Reader = new StreamReader(Path.Combine(m_outPutPath, "Posting" + i + ".txt"));
                while ((current = Reader.ReadLine()) != null)
                {
                    Writer.WriteLine(current);
                }
                Reader.Close();
            }
            Writer.Flush();
            Writer.Close();
        }
        */


        /*
        public void WriteTheNewIndexBinFile()
        {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            Stream stream = new FileStream(Path.Combine(m_outPutPath, "Dictionary.bin"), FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, dictionaries);
            stream.Close();
        }
        */


        public void WriteTheNewIndexFile()
        {
            //StringBuilder data = new StringBuilder();
            //CreateTxtFile("Dictionary.txt", data);
            CreateEmptyTxtFile("Dictionary.txt");
            //string current;
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Dictionary.txt"));
            // here add sort of the dictionary by keys
            Dictionary<string, IndexTerm> temp;
            foreach (Dictionary<string, IndexTerm> dic in dictionaries)
            {
                temp = dic.OrderBy(i => i.Key).ToDictionary(p => p.Key, p => p.Value);
                foreach (KeyValuePair<string, IndexTerm> currentTerm in temp)
                {
                    Writer.WriteLine(currentTerm.Value.PrintTerm());  ////////check whose function has the best time 
                                                                      //Writer.WriteLine(currentTerm.Value.PrintTermString()); 
                }
            }
            Writer.Flush();
            Writer.Close();
            // 
            // foreach (Dictionary<string, IndexTerm> dic in dictionaries)
            // {
            //     var list = dic.Keys.ToList();
            //     list.Sort();
            //     // Loop through keys.
            //     foreach (var key in list)
            //     {
            //         Writer.WriteLine(dic[key].PrintTerm());
            //  }
            //

        }



        public void InitTerms()
        {
            foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
            {
                int PostNumber = pair.Value.postNum;
                if (dictionaries[PostNumber].ContainsKey(pair.Key))
                {
                    pair.Value.line = dictionaries[PostNumber][pair.Key].lineInPost;
                }
            }
            ///////////////// order without internal order - check it!!!
            this.tempDic = tempDic.OrderBy(i => i.Value.postNum).ThenBy(i => i.Value.line).ToDictionary(p => p.Key, p => p.Value); ;
        }


        public void WriteTheNewDocumentsFile(masterFile masterFiles)
        {
            if (!File.Exists(Path.Combine(m_outPutPath, "Documents.txt")))
            {
                //StringBuilder data = new StringBuilder();
                //Directory.CreateDirectory(Path.Combine(m_outPutPath, "Documents.txt"));
                //CreateTxtFile("Documents.txt", data);
                CreateEmptyTxtFile("Documents.txt");

            }
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Documents.txt"));
            //StringBuilder data = new StringBuilder();
            foreach (Document doc in masterFiles.m_documents.Values)
            {
                Writer.WriteLine(doc.WriteDocumentToIndexFile());
            }
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }


        public void WriteCitiesIndexFile(HashSet<string> m_Cities)
        {
            if (!File.Exists(Path.Combine(m_outPutPath, "Cities.txt")))
            {
                StringBuilder data = new StringBuilder();
                CreateTxtFile("Cities.txt", data);
            }
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Cities.txt"));
            var webRequest = WebRequest.Create("https://restcountries.eu/rest/v2/all?fields=capital;name;currencies;population") as HttpWebRequest;
            if (webRequest == null)
            {
                return;
            }
            webRequest.ContentType = "application/json";
            webRequest.UserAgent = "Nothing";
            using (var s = webRequest.GetResponse().GetResponseStream())
            {
                using (var sr = new StreamReader(s))
                {
                    var contributorsAsJson = sr.ReadToEnd();
                    JArray jarr = JArray.Parse(contributorsAsJson);
                    foreach (JObject content in jarr.Children<JObject>())
                    {//fields[0] - currencies ,fields[1] - name ,fields[2] - capital ,fields[3] - population ,
                        JProperty[] fields = content.Properties().ToArray();
                        if (m_Cities.Contains("" + fields[2].Value))
                        {//fields[0].Value[0].ToArray()[0] 
                            Writer.WriteLine(fields[2].Value + "(#)" + fields[1].Value + "(#)" + fields[0].Value[0].ToArray()[0].ToArray()[0] + "(#)" + fields[3].Value);
                        }
                    }
                }
            }
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }

        /*
        public void LoadDictionary()
        {
            if (!File.Exists(Path.Combine(m_outPutPath, "Dictionary.txt")))
            {
                return;
            }
            else
            {
                Reader = new StreamReader(Path.Combine(m_outPutPath, "Dictionary.txt"));
            }
            for (int i = 0; i < dictionaries.Length; i++)
            {
                dictionaries[i].Clear();
            }
            int postNum = 0;
            string[] allLines = System.IO.File.ReadAllLines(Path.Combine(m_outPutPath, "Dictionary.txt"));
            for (int i = 0; i < allLines.Length; i++)
            {
                string currentLine = allLines[i];
                string[] lineDetails = currentLine.Split('#');
                ///////////////////////////////// have to add the post number of the index
                string[] lineNumber = lineDetails[3].Split(':');
                string[] df = lineDetails[1].Split(':');
                string[] tf = lineDetails[2].Split(':');
                IndexTerm currentTerm = new IndexTerm(lineDetails[0], 3, Int32.Parse(lineNumber[1]));
                Console.WriteLine(allLines[i]);
                currentTerm.tf = Int32.Parse(df[1]);
                currentTerm.df = Int32.Parse(tf[1]);
                dictionaries[postNum].Add(currentTerm.m_value, currentTerm);
            }
        }
        */
    }
}
