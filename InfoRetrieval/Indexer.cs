using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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
    /// <summary>
    /// Class which represents an indexer for creating posting and indexing files 
    /// </summary>
    public class Indexer
    {
        /// <summary>
        /// fields of Indexer
        /// </summary>
        public int m_indexCounter;
        public string m_outPutPath;
        public bool doStem;
        public Dictionary<int, int> currentLine;
        public Dictionary<string, IndexTerm>[] dictionaries = new Dictionary<string, IndexTerm>[27];
        public Dictionary<string, DocumentTerms> tempDic;
        public HashSet<string> m_Cities;
        public StreamWriter Writer;
        public StreamReader Reader;
        public bool StreamHasChanged;
        public char[] toDelete;

        public int indexNumber = 0;

        public static Hashtable m_postingNums = new Hashtable()
        {
            {'a', 0 }, {'b', 1 }, {'c', 2 },{'d', 3 }, //{ "", "26" },
            { 'e', 4 }, {'f', 5 }, {'g', 6 }, {'h', 7 },{'i', 8 },
            { 'j', 9 }, {'k', 10 }, {'l', 11 }, {'m', 12 }, {'n', 13 },
            {'o', 14 }, {'p', 15 },{'q', 16 },{'r', 17 }, {'s', 18 },
            {'t', 19 }, {'u', 20 },{'v', 21 },{'w', 22 }, {'x', 23 },
            { 'y', 24 } ,{'z', 25 }
        };


        /// <summary>
        /// constructor of Indexer
        /// </summary>
        /// <param name="doStemming">boolean input for stemming</param>
        /// <param name="m_outPutPath">the path of the output</param>
        public Indexer(bool doStemming, string m_outPutPath)
        {
            this.toDelete = new char[] { ',', '.', '{', '}', '(', ')', '[', ']', '-', ';', ':', '~', '|', '\\', '"', '?', '!', '@', '\'', '*', '`', '&', '□', '_', '+' };
            this.currentLine = new Dictionary<int, int>();
            this.tempDic = new Dictionary<string, DocumentTerms>();
            this.StreamHasChanged = false;
            this.m_Cities = new HashSet<string>();
            this.m_indexCounter = 1;
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

        /// <summary>
        /// method which creates a new text file
        /// </summary>
        /// <param name="fileName">the name of the file</param>
        /// <param name="dataToFile">the data to write to the file</param>
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

        /// <summary>
        /// methods which creates posting files
        /// </summary>
        public void CreatePostingFiles()
        {
            StringBuilder data = new StringBuilder();
            for (int i = 0; i < 27; i++)
            {
                CreateTxtFile("Posting" + i + ".txt", data);
                CreateTxtFile("NewPosting" + i + ".txt", data);
            }
        }

        /// <summary>
        /// methods which creates new posting files
        /// </summary>
        public void CreateNewPostingFiles()
        {
            StringBuilder data = new StringBuilder();
            for (int i = 0; i < 27; i++)
            {
                CreateTxtFile("NewPosting" + i + ".txt", data);
            }
        }

        /// <summary>
        /// method to write to posting files
        /// </summary>
        /// <param name="documentTermsDic">input dictionary of the Parse class</param>
        /// <param name="hasWritten">boolean input for first writing</param>
        public void WriteToPostingFile(Dictionary<string, DocumentTerms> documentTermsDic, bool hasWritten)
        {
            tempDic = documentTermsDic;
            if (!hasWritten)
            {
                this.tempDic = tempDic.OrderBy(i => i.Value.postNum).ThenBy(i => i.Value.line).ToDictionary(p => p.Key, p => p.Value);
                StringBuilder data = new StringBuilder();
                int postNum = -1;
                foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
                {
                    SwitchWriterForFirstPosting(pair.Value.postNum, postNum);
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
            Console.WriteLine("-----------" + m_indexCounter++);
        }

        /// <summary>
        /// mwthod to init the array which holds all the dictionaries of each post number
        /// </summary>
        public void initDic()
        {
            for (int i = 0; i < 27; i++)
            {
                dictionaries[i] = new Dictionary<string, IndexTerm>();
                currentLine[i] = 0;
            }
        }

        /// <summary>
        /// method to switch the reader and writer streams
        /// </summary>
        /// <param name="termPost">the post number of the current term</param>
        /// <param name="currentPos">the position of the stream</param>
        public void SwitchWriterAndReaderForPosting(int termPost, int currentPos)
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
                Reader = new StreamReader(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt"));
            }
        }

        /// <summary>
        /// method to switch the writer stream
        /// </summary>
        /// <param name="termPost">the post number of the current term</param>
        /// <param name="currentPos">the position of the stream</param>
        public void SwitchWriterForFirstPosting(int termPost, int currentPos)
        {
            bool check = Writer != null;
            if (currentPos == -1 || termPost != currentPos)
            {
                if (check)
                {
                    Writer.Flush();
                    Writer.Close();
                }
                Writer = new StreamWriter(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt"));
            }
        }

        /// <summary>
        /// method which updates current posting files
        /// </summary>
        public void UpdatePosting()
        {
            InitTerms();
            CreateNewPostingFiles();
            bool last_read = true;
            int PostNumber = -1, LineNumber, currentLineNumber = 0;
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
            {
                SwitchWriterAndReaderForPosting(pair.Value.postNum, PostNumber);
                if (StreamHasChanged)
                {
                    currentLineNumber = 0;
                    StreamHasChanged = false;
                    last_read = true;
                }
                string currentLineInFile;
                PostNumber = pair.Value.postNum;
                LineNumber = pair.Value.line;
                if (LineNumber != Int32.MaxValue) // exist in the posting file
                {
                    while (LineNumber > currentLineNumber)
                    {
                        currentLineInFile = Reader.ReadLine();
                        Writer.WriteLine(currentLineInFile);
                        currentLineNumber++;
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

        /// <summary>
        /// method to delete all old files
        /// </summary>
        public void DeleteOldFiles()
        {
            for (int i = 0; i < 27; i++)
            {
                if (File.Exists(Path.Combine(m_outPutPath, "Posting" + i + ".txt")))
                {
                    File.Delete(Path.Combine(m_outPutPath, "Posting" + i + ".txt"));
                }
                File.Move(Path.Combine(m_outPutPath, "NewPosting" + i + ".txt"), Path.Combine(m_outPutPath, "Posting" + i + ".txt"));
                if (File.Exists(Path.Combine(m_outPutPath, "NewPosting" + i + ".txt")))
                {
                    File.Delete(Path.Combine(m_outPutPath, "NewPosting" + i + ".txt"));
                }
            }
        }

        /// <summary>
        /// method to write the index file
        /// </summary>
        public void WriteTheNewIndexFile()
        {
            StringBuilder data = new StringBuilder();
            CreateTxtFile("Dictionary.txt", data);
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Dictionary.txt"));
            Dictionary<string, IndexTerm> temp;
            foreach (Dictionary<string, IndexTerm> dic in dictionaries)
            {
                temp = dic.OrderBy(i => i.Value.m_value).ToDictionary(p => p.Key, p => p.Value);
                foreach (KeyValuePair<string, IndexTerm> currentTerm in temp)
                {
                    Writer.WriteLine(currentTerm.Value.PrintTerm());
                }
            }
            Writer.Flush();
            Writer.Close();
        }

        /// <summary>
        /// method which init all the terms in the current dictionary
        /// </summary>
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
            this.tempDic = tempDic.OrderBy(i => i.Value.postNum).ThenBy(i => i.Value.line).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// method which get the suitable post number of a current term
        /// </summary>
        /// <param name="str">the value of the current term</param>
        /// <returns>the post number of a current term</returns>
        public int GetPostNumber(string str)
        {
            if (str.Equals(""))
            {
                return 26;
            }
            char c = char.ToLower(str[0]);
            if (char.IsLetter(c))
            {
                return (int)m_postingNums[c];
            }
            else
            {
                return 26;
            }
        }

        /// <summary>
        /// method to write the documents' file to disk
        /// </summary>
        /// <param name="masterFiles">current collection of files</param>
        public void WriteTheNewDocumentsFile(masterFile masterFiles)
        {
            /////////////////////////////////////////////////////////////////////////////   delete all files, before run the model
            StringBuilder data = new StringBuilder();
            if (!File.Exists(Path.Combine(m_outPutPath, "Documents.txt")))
            {
                CreateTxtFile("Documents.txt", data);
                Writer = new StreamWriter(Path.Combine(m_outPutPath, "Documents.txt"));
            }
            else
            {
                Writer = File.AppendText(Path.Combine(m_outPutPath, "Documents.txt"));
            }
            foreach (Document d in masterFiles.m_documents.Values)
            {
                Writer.WriteLine(d.WriteDocumentToIndexFile());
            }
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }

        /// <summary>
        /// method which saves all the cities exist in the tags
        /// </summary>
        /// <param name="masterFile">a collection of files</param>
        public void UpdateCitiesPositionInDocument(masterFile masterFile)
        {
            string city;
            foreach (Document document in masterFile.m_documents.Values)
            {
                city = document.m_CITY.ToString().Trim(toDelete);
                if (!city.Equals("") && !m_Cities.Contains(city) && !(city.Any(char.IsDigit)))
                {
                    m_Cities.Add(city);
                }
            }
        }

        /// <summary>
        /// method to write the Cities index file
        /// </summary>
        public void WriteCitiesIndexFile()
        {
            int postNum;
            string currency, state, population;
            StringBuilder cityData = new StringBuilder();
            if (!File.Exists(Path.Combine(m_outPutPath, "Cities.txt")))
            {
                StringBuilder data = new StringBuilder();
                CreateTxtFile("Cities.txt", data);
            }
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Cities.txt"));
            foreach (string city in m_Cities)
            {
                postNum = GetPostNumber(city);

                var webRequest = WebRequest.Create("http://getcitydetails.geobytes.com/GetCityDetails?fqcn=" + city) as HttpWebRequest;
                if (webRequest != null)
                {
                    using (var s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var contributorsAsJson = sr.ReadToEnd();
                            JObject jObject = JObject.Parse(contributorsAsJson);
                            cityData.Append(city + "(#)");
                            currency = (string)jObject.SelectToken("geobytescurrencycode");
                            state = (string)jObject.SelectToken("geobytescountry");
                            population = (string)jObject.SelectToken("geobytespopulation");
                            if (!currency.Equals(""))
                            {
                                cityData.Append(currency + "(#)"); //currency
                            }
                            if (!state.Equals(""))
                            {
                                cityData.Append(state + "(#)"); // state
                            }
                            if (!population.Equals(""))
                            {
                                cityData.Append(population + "(#)"); // population
                            }

                            if (dictionaries[postNum].ContainsKey(city))
                            {
                                cityData.Append(" postNum: " + postNum + " lineNumber: " + dictionaries[postNum][city].lineInPost); // positions pointer
                                Writer.WriteLine(cityData);
                                cityData.Clear();
                            }
                            else
                            {
                                cityData.Append(" not exist in Posting"); // positions
                                Writer.WriteLine(cityData);
                                cityData.Clear();
                            }
                        }
                    }
                }
            }
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }

        /// <summary>
        /// method to load the dictionary from the disk
        /// </summary>
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
                string[] lineDetails = currentLine.Split('#'); //(#)
                                                               ///////////////////////////////// have to add the post number of the index
                string[] lineNumber = lineDetails[3].Split(':');
                string[] df = lineDetails[1].Split(':');
                string[] tf = lineDetails[2].Split(':');
                IndexTerm currentTerm = new IndexTerm(lineDetails[0], 3, Int32.Parse(lineNumber[1]));
                // Console.WriteLine(allLines[i]);
                currentTerm.tf = Int32.Parse(df[1]);
                currentTerm.df = Int32.Parse(tf[1]);
                dictionaries[postNum].Add(currentTerm.m_value, currentTerm);
            }
        }



    }
}
