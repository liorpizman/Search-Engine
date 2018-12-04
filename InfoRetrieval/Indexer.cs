using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        public HashSet<string> m_Languages;
        public StreamWriter Writer;
        //public StreamReader Reader;
        public bool StreamHasChanged;
        public char[] toDelete;
        public int indexNumber = 0;            //delete it - it is for testing
        public int docCounter = 0;
        public int uniqueCorpusCounter = 0;

        public static Hashtable m_postingNums = new Hashtable()
        {
            {'a', 1 }, {'b', 2 }, {'c', 3 },{'d', 4 }, //{ "", "0" },
            { 'e', 5 }, {'f', 6 }, {'g', 7 }, {'h', 8 },{'i', 9 },
            { 'j', 10 }, {'k', 11 }, {'l', 12 }, {'m', 13 }, {'n', 14 },
            {'o', 15 }, {'p', 16 },{'q', 17 },{'r', 18 }, {'s', 19 },
            {'t', 20 }, {'u', 21 },{'v', 22 },{'w', 23 }, {'x', 24 },
            { 'y', 25 } ,{'z', 26 }
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
            this.m_Languages = new HashSet<string>();
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
        /// method which creates an empty text file
        /// </summary>
        /// <param name="fileName">the name of the file</param>
        public void CreateEmptyTxtFile(string fileName)
        {
            if (!Directory.Exists(m_outPutPath))
            {
                Directory.CreateDirectory(m_outPutPath);
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(m_outPutPath, fileName)))
            {
                outputFile.WriteLine(new StringBuilder());
            }
        }

        /// <summary>
        /// methods which creates posting files
        /// </summary>
        public void CreatePostingFiles()
        {
            for (int i = 0; i < 27; i++)
            {
                CreateEmptyTxtFile("Posting" + i + ".txt");
                CreateEmptyTxtFile("NewPosting" + i + ".txt");
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
                CreateEmptyTxtFile("NewPosting" + i + ".txt");
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
                    if (StreamShouldBeChanged(pair.Value.postNum, postNum))
                    {
                        if (Writer != null)
                        {
                            Writer.Write(data);
                            data.Clear();
                            Writer.Flush();
                            Writer.Close();
                        }
                        SwitchWriterForPosting(pair.Value.postNum, "Posting");
                    }
                    postNum = pair.Value.postNum;
                    IndexTerm currentTerm = new IndexTerm(pair.Key, postNum, currentLine[postNum]);
                    currentTerm.IncreaseTfc(pair.Value.countTotalFrequency());
                    currentTerm.IncreaseDf(pair.Value.m_Terms.Count);
                    dictionaries[postNum].Add(pair.Key, currentTerm);
                    //Writer.WriteLine(pair.Value.WriteToPostingFileDocDocTerm(false));
                    data.AppendLine(pair.Value.WriteToPostingFileDocDocTerm(false).ToString());
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
        /// method to check wether a stream should be switched
        /// </summary>
        /// <param name="termPost">the post number of the term</param>
        /// <param name="currentPos">current writer</param>
        /// <returns></returns>
        public bool StreamShouldBeChanged(int termPost, int currentPos)
        {
            if (currentPos == -1 || termPost != currentPos)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// method to switch the reader and writer streams
        /// </summary>
        /// <param name="termPost">the post number of the current term</param>
        public void SwitchWriterForPosting(int termPost, string fileName)
        {
            //bool check = Writer != null );//&& Reader != null;
            //if (currentPos == -1 || termPost != currentPos)
            //{
            StreamHasChanged = true;
            Writer = new StreamWriter(Path.Combine(m_outPutPath, fileName + termPost + ".txt")); //fileName- newPosting for all and Posting for first
                                                                                                 //Reader = new StreamReader(Path.Combine(m_outPutPath, "Posting" + termPost + ".txt"));
                                                                                                 //}
        }
        /*
        /// <summary>
        /// method to switch the writer stream
        /// </summary>
        /// <param name="termPost">the post number of the current term</param>
        /// <param name="currentPos">the position of the stream</param>
        public void SwitchWriterForFirstPosting(int termPost, int currentPos, string fileName)
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
        */
        /// <summary>
        /// method which updates current posting files
        /// </summary>
        public void UpdatePosting()
        {
            InitTerms();
            CreateNewPostingFiles();
            bool finishedUpdatePrevTerms = true;
            int PostNumber = -1, LineNumberOfTerm, currentLineNumber = 0, indexLine = 0;
            StringBuilder writeData = new StringBuilder();
            string[] lines = null;
            foreach (KeyValuePair<string, DocumentTerms> pair in tempDic)
            {
                if (StreamShouldBeChanged(pair.Value.postNum, PostNumber))
                {
                    if (Writer != null)
                    {
                        Writer.Write(writeData);
                        writeData.Clear();
                        Writer.Flush();
                        Writer.Close();
                    }
                    SwitchWriterForPosting(pair.Value.postNum, "NewPosting");
                }
                if (StreamHasChanged)
                {
                    lines = File.ReadAllLines(Path.Combine(m_outPutPath, "Posting" + pair.Value.postNum + ".txt"));
                    currentLineNumber = 0;
                    indexLine = 0;
                    StreamHasChanged = false;
                    finishedUpdatePrevTerms = true;
                }
                PostNumber = pair.Value.postNum;
                LineNumberOfTerm = pair.Value.line;
                if (LineNumberOfTerm != Int32.MaxValue) // exist in the posting file
                {
                    while (LineNumberOfTerm > currentLineNumber)
                    {
                        //currentLineInFile = Reader.ReadLine();
                        writeData.AppendLine(lines[indexLine++]);
                        currentLineNumber++;
                    }
                    //currentLineInFile = Reader.ReadLine();
                    writeData.AppendLine(lines[indexLine++] + pair.Value.WriteToPostingFileDocDocTerm(true));
                    //dictionaries[PostNumber][pair.Key].IncreaseTf(pair.Value.m_tfc);
                    dictionaries[PostNumber][pair.Key].IncreaseTfc(pair.Value.countTotalFrequency());
                    dictionaries[PostNumber][pair.Key].IncreaseDf(pair.Value.m_Terms.Count);
                    currentLineNumber++;
                }
                else
                {
                    if (finishedUpdatePrevTerms)
                    {
                        while (indexLine < lines.Length)
                        {
                            writeData.AppendLine(lines[indexLine++]);
                            currentLineNumber++;
                        }
                        finishedUpdatePrevTerms = false;
                    }
                    IndexTerm currentTerm = new IndexTerm(pair.Key, PostNumber, currentLine[PostNumber]);
                    LineNumberOfTerm = currentLine[PostNumber];
                    currentLine[PostNumber]++;
                    //currentTerm.IncreaseTf(pair.Value.m_tfc);
                    currentTerm.IncreaseTfc(pair.Value.countTotalFrequency());
                    currentTerm.IncreaseDf(pair.Value.m_Terms.Count);
                    dictionaries[PostNumber].Add(pair.Key, currentTerm);
                    writeData.AppendLine(pair.Value.WriteToPostingFileDocDocTerm(false).ToString());
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
            CreateEmptyTxtFile("Dictionary.txt");
            Writer = new StreamWriter(Path.Combine(m_outPutPath, "Dictionary.txt"));
            Dictionary<string, IndexTerm> temp;
            //foreach (Dictionary<string, IndexTerm> dic in dictionaries)
            //{
            for (int j = 0; j < dictionaries.Length; j++)
            {
                uniqueCorpusCounter += dictionaries[j].Count;
                //temp = dic.OrderBy(i => i.Value.m_value).ToDictionary(p => p.Key, p => p.Value);
                //  dictionaries[j] = dictionaries[j].OrderBy(i => i.Value.m_value).ToDictionary(p => p.Key, p => p.Value);  ///////// check if we need it
                foreach (KeyValuePair<string, IndexTerm> currentTerm in dictionaries[j])
                {
                    Writer.WriteLine(currentTerm.Value.PrintTerm());
                }
            }
            //}
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
                return 0;
            }
            char c = char.ToLower(str[0]);
            if (char.IsLetter(c))
            {
                return (int)m_postingNums[c];
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// method to write the documents' file to disk
        /// </summary>
        /// <param name="masterFiles">current collection of files</param>
        public void WriteTheNewDocumentsFile(masterFile masterFiles)
        {
            if (!File.Exists(Path.Combine(m_outPutPath, "Documents.txt")))
            {
                CreateEmptyTxtFile("Documents.txt");
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
        public void UpdateCitiesAndLanguagesInDocument(masterFile masterFile)
        {
            string city, language;
            docCounter += masterFile.m_documents.Count;
            foreach (Document document in masterFile.m_documents.Values)
            {
                city = document.m_CITY.ToString().Trim(toDelete);
                if (!city.Equals("") && !m_Cities.Contains(city) && !(city.Any(char.IsDigit)))
                {
                    m_Cities.Add(city);
                }
                language = document.m_language.ToString().Trim(toDelete);
                if (!language.Equals("") && !m_Languages.Contains(language) && !(language.Any(char.IsDigit)))
                {
                    m_Languages.Add(language);
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
                CreateEmptyTxtFile("Cities.txt");
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
        /// method to serialize the dictionary of all terms into bin file
        /// </summary>
        public void SerializeDictionary()
        {
            string path = Path.Combine(m_outPutPath, "Dictionary.bin");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            FileStream fs = new FileStream(Path.Combine(m_outPutPath, "Dictionary.bin"), FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, dictionaries);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// method to update upper and lower case rule
        /// </summary>
        /// <param name="dicNumber">the index of the dictionary</param>
        public void MergeSameWords(int dicNumber)
        {
            string path = Path.Combine(m_outPutPath, "Posting" + dicNumber + ".txt");
            dictionaries[dicNumber] = dictionaries[dicNumber].OrderBy(i => i.Value.m_value).ToDictionary(p => p.Key, p => p.Value);
            IndexTerm[] Terms = dictionaries[dicNumber].Values.ToArray();
            Dictionary<int, List<int>> LinesToMerge = new Dictionary<int, List<int>>();
            int j;
            for (int i = 0; i < Terms.Length - 2; i++)
            {
                j = i;
                while (string.Equals(Terms[i].m_value, Terms[j + 1].m_value, StringComparison.OrdinalIgnoreCase) && j < Terms.Length - 2)
                {
                    if (!LinesToMerge.ContainsKey(Terms[i].lineInPost))
                    {
                        List<int> toAdd = new List<int>();
                        toAdd.Add(Terms[j + 1].lineInPost);
                        LinesToMerge.Add(Terms[i].lineInPost, toAdd);
                        // Console.WriteLine(Terms[i].m_value + " ---- " + Terms[j + 1].m_value);
                    }
                    else
                    {
                        LinesToMerge[Terms[i].lineInPost].Add(Terms[j + 1].lineInPost);
                        // Console.WriteLine(Terms[i].m_value + " ---- " + Terms[j + 1].m_value);
                    }
                    dictionaries[dicNumber][Terms[i].m_value].IncreaseDf(Terms[j + 1].df);
                    dictionaries[dicNumber][Terms[i].m_value].IncreaseTfc(Terms[j + 1].tfc);
                    dictionaries[dicNumber].Remove(Terms[j + 1].m_value);
                    j++;
                }
                i = j;
            }
            string[] Lines = File.ReadAllLines(path);
            foreach (KeyValuePair<int, List<int>> mergeLines in LinesToMerge)
            {
                string extension = "";
                int position;
                foreach (int pos in mergeLines.Value)
                {
                    position = Lines[pos].IndexOf("(#)");
                    extension = Lines[pos].Substring(position + 3);
                    Lines[mergeLines.Key] = Lines[mergeLines.Key] + extension;
                    Lines[pos] = null;
                }

            }
            StringBuilder data = new StringBuilder();
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i] != null)
                {
                    data.AppendLine(Lines[i]);
                }
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            CreateTxtFile("Posting" + dicNumber + ".txt", data);
        }
    }

}

