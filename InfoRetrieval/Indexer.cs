using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace InfoRetrieval
{
    class Indexer
    {
        public int m_indexCounter;
        public string m_outPutPath;
        public bool doStem;
        public Dictionary<int, int> currentLine;
        public Dictionary<string, IndexTerm>[] dictionaries = new Dictionary<string, IndexTerm>[27];
        public Dictionary<string, DocumentTerms> tempDic;
        public StreamWriter Writer;
        public StreamReader Reader;
        public bool StreamHasChanged;
        public Queue<Dictionary<string, DocumentTerms>> IndexQueue;
        public bool CorpusDone;

        public Indexer(bool doStemming, string m_outPutPath)
        {
            //this.Writer = new StreamWriter();
            //this.Reader = new StreamReader();
            this.currentLine = new Dictionary<int, int>();
            this.tempDic = new Dictionary<string, DocumentTerms>();
            this.StreamHasChanged = false;
            this.IndexQueue = new Queue<Dictionary<string, DocumentTerms>>();
            this.CorpusDone = false;
            this.m_indexCounter = 0;
            initDic();
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
            CreatePostingFiles();
        }


        public void AddToQueue(Dictionary<string, DocumentTerms> ToAdd)
        {
            IndexQueue.Enqueue(ToAdd);
        }

        public void SetCorpusDone()
        {
            CorpusDone = true;
        }

        public void MainIndexRun()
        {
            bool FirstWrite = false;
            while (!CorpusDone)
            {
                while (IndexQueue.Count > 0)
                {
                    WriteToPostingFile(IndexQueue.Dequeue(), FirstWrite);
                    FirstWrite = true;
                }
                ////////////// here we need to do thread.sleep(X secounds)
            }
            while (IndexQueue.Count > 0)  /////////to ensure all the files are did post
            {
                WriteToPostingFile(IndexQueue.Dequeue(), FirstWrite);
            }
            MergePosting();
            WriteTheNewIndexFile();
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
        public void CreatePostingFiles()
        {
            StringBuilder data = new StringBuilder();
            for (int i = 0; i < 27; i++)
            {
                CreateTxtFile("Posting" + i + ".txt", data);
                CreateTxtFile("NewPosting" + i + ".txt", data);
            }
        }

        public void CreateNewPostingFiles()
        {
            StringBuilder data = new StringBuilder();
            for (int i = 0; i < 27; i++)
            {
                CreateTxtFile("NewPosting" + i + ".txt", data);
            }
        }

        public void WriteToPostingFile(Dictionary<string, DocumentTerms> documentTermsDic, bool hasWritten)
        {
            tempDic = documentTermsDic;
            InitTerms();
            if (!hasWritten)
            {
                StringBuilder data = new StringBuilder();
                int postNum = -1;
                //StreamWriter sw = new StreamWriter(Path.Combine(m_outPutPath, "Posting.txt"));
                foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
                {
                    currentPostingForFirstWrite(pair.Value.postNum, postNum);
                    postNum = pair.Value.postNum;
                    IndexTerm currentTerm = new IndexTerm(pair.Key, postNum, currentLine[postNum]);
                    // currentTerm.IncreaseTf(pair.Value.m_tfc);
                    currentTerm.IncreaseDf(pair.Value.m_Terms.Count);
                    dictionaries[postNum].Add(pair.Key, currentTerm);
                    Writer.WriteLine(pair.Value.WriteToPostingFileDocDocTerm(false));
                    currentLine[postNum]++;
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
        }

        public void initDic()
        {
            for (int i = 0; i < 27; i++)
            {
                dictionaries[i] = new Dictionary<string, IndexTerm>();
                currentLine[i] = 0;
            }
        }

        public void currentPosting(int termPost, int currentPos)
        {
            bool check = Writer != null && Reader != null;
            if (currentPos == -1 || termPost != currentPos)
            {
                StreamHasChanged = true;
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
            bool check = Writer != null;
            if (currentPos == -1 || termPost != currentPos)
            {
                if (check)
                {
                    Writer.Flush();
                    Writer.Close();
                    // Reader.Close();
                }
                Writer = new StreamWriter(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt"));
                // Reader = new StreamReader(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt")); /////////////////// has to changed
            }
        }

        public void UpdatePosting()
        {
            CreateNewPostingFiles();
            bool last_read = true;
            int PostNumber = -1, LineNumber, currentLineNumber = 0;
            StringBuilder data = new StringBuilder();
            //createTxtFile("NewPosting.txt", data);
            foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
            {
                //StreamWriter outputFile = new StreamWriter(Path.Combine(m_outPutPath, "newPost " + pair.Value.m_Terms[pair.Key].postNum+".txt"));
                ///////////////////////// here need to add the definition of the file we wrote
                currentPosting(pair.Value.postNum, PostNumber);
                if (StreamHasChanged)
                {
                    currentLineNumber = 0;
                    StreamHasChanged = false;
                    last_read = true;
                    //currentLineNumber = currentLine[PostNumber];
                }
                string currentLineInFile;
                PostNumber = pair.Value.postNum;
                LineNumber = pair.Value.line;
                if (LineNumber != Int32.MaxValue) // exist in the posting file
                {
                    while (LineNumber > currentLineNumber)  //LineNumber < currentLineNumber)
                    {
                        //    if ((currentLineInFile = Reader.ReadLine()) != null)
                        //  {
                        currentLineInFile = Reader.ReadLine();
                        Writer.WriteLine(currentLineInFile);
                        currentLineNumber++;
                        // }
                    }
                    currentLineInFile = Reader.ReadLine();
                    Writer.WriteLine(currentLineInFile + pair.Value.WriteToPostingFileDocDocTerm(true));
                    //dictionaries[PostNumber][pair.Key].IncreaseTf(pair.Value.m_tfc);
                    dictionaries[PostNumber][pair.Key].IncreaseDf(pair.Value.m_Terms.Count);
                    currentLineNumber++;
                }
                else
                {
                    if (last_read)
                    {
                        while ((currentLineInFile = Reader.ReadLine()) != null)
                        {
                            //currentLineInFile = Reader.ReadLine();
                            Writer.WriteLine(currentLineInFile);
                            currentLineNumber++;
                        }
                        last_read = false;
                    }
                    IndexTerm currentTerm = new IndexTerm(pair.Key, PostNumber, currentLine[PostNumber]);
                    LineNumber = currentLine[PostNumber];
                    currentLine[PostNumber]++;
                    //currentTerm.IncreaseTf(pair.Value.m_tfc);
                    currentTerm.IncreaseDf(pair.Value.m_Terms.Count);
                    dictionaries[PostNumber].Add(pair.Key, currentTerm);
                    Writer.WriteLine(pair.Value.WriteToPostingFileDocDocTerm(false));
                }
            }
            Writer.Flush();
            Writer.Close();
            Reader.Close();
            Writer = null;
            Reader = null;
            ///in the end:
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
                File.Move(Path.Combine(m_outPutPath, "NewPosting" + i + ".txt"), Path.Combine(m_outPutPath, "Posting" + i + ".txt"));
            }
        }

        public void MergePosting()
        {
            StringBuilder data = new StringBuilder();
            CreateTxtFile("Posting.txt", data);
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

        public void WriteTheNewIndexFile()
        {
            StringBuilder data = new StringBuilder();
            CreateTxtFile("Dictionary.txt", data);
            string current;
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Dictionary.txt"));
            /// here add sort of the dictionary by keys
            //Dictionary<string, IndexTerm> temp;
            /*
            foreach (Dictionary<string, IndexTerm> dic in dictionaries)
            {
                temp = dic.OrderBy(i => i.Key).ToDictionary(p => p.Key, p => p.Value);
                foreach (KeyValuePair<string, IndexTerm> currentTerm in dic)
                {
                    Writer.WriteLine(currentTerm.Value.PrintTerm());  ////////check whose function has the best time 
                    //Writer.WriteLine(currentTerm.Value.PrintTermString()); 
                }
            }
            Writer.Flush();
            Writer.Close();
            */
            foreach (Dictionary<string, IndexTerm> dic in dictionaries)
            {
                var list = dic.Keys.ToList();
                list.Sort();
                // Loop through keys.
                foreach (var key in list)
                {
                    Writer.WriteLine(dic[key].PrintTerm());
                }
            }
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
                StringBuilder data = new StringBuilder();
                //Directory.CreateDirectory(Path.Combine(m_outPutPath, "Documents.txt"));
                CreateTxtFile("Documents.txt", data);
            }
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Documents.txt"));
            //StringBuilder data = new StringBuilder();
            foreach (Document d in masterFiles.m_documents.Values)
            {
                Writer.WriteLine(d.WriteDocumentToIndexFile());
                //data.Append(Environment.NewLine);
            }
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }

        internal class Contributor
        {
            public string Login { get; set; }
            public short Contributions { get; set; }

            public override string ToString()
            {
                return $"{Login,20}: {Contributions} contributions";
            }
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
                            Writer.WriteLine(fields[2].Value + "#" + fields[1].Value + "#" + fields[0].Value[0].ToArray()[0].ToArray()[0] + "#" + fields[3].Value);
                        }
                    }
                }
            }
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }


        public int GetPostNumer(string m_value)       ///////////////// check if this method exist
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

/*
         public void WriteCitiesIndexFile()
        {
            if (!File.Exists(Path.Combine(m_outPutPath, "Cities.txt")))
            {
                StringBuilder data = new StringBuilder();
                CreateTxtFile("Cities.txt", data);
            }
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Cities.txt"));
            var url = "https://restcountries.eu/rest/v2/all?fields=capital;name;currencies;population";
            ///////////////////////////////////////
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
                    //Console.WriteLine(jarr);
                    foreach (JObject content in jarr.Children<JObject>())
                    {
                        foreach (JProperty prop in content.Properties())
                        {
                            if (prop.Name.Equals("currencies"))
                            {
                                foreach (JProperty val in prop.Value[0])
                                {
                                    if (val.Name.Equals("name"))
                                    {
                                        Console.WriteLine("" + val.Value);
                                    }
                                }
                            }
                            //string tempValue = prop.Value.ToString(); // This is not allowed 
                            // Console.WriteLine(tempValue);                                        //here more code in order to save in a database
                        }
                    }
                    // var contributors = JsonConvert.DeserializeObject<List<Contributor>>(contributorsAsJson);
                    //contributors.ForEach(Console.WriteLine);
                }
            }
            ///////////////////////////////////////
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }
*/
