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
        public int indexNumber = 0;
        public string m_outPutPath { get; private set; }
        public bool doStem { get; private set; }
        public Dictionary<int, int> currentLine { get; private set; }
        public Dictionary<string, IndexTerm>[] dictionaries { get; set; }
        public Dictionary<string, DocumentsTerm> tempDic { get; private set; }
        public HashSet<string> m_Cities { get; private set; }
        public HashSet<string> m_Languages { get; private set; }
        public StreamWriter Writer { get; private set; }
        public bool StreamHasChanged { get; private set; }
        public char[] toDelete { get; private set; }
        public int docCounter { get; private set; }
        public int uniqueCorpusCounter { get; private set; }
        public int avgDL { get; private set; }
        public int totalLenDocs { get; private set; }

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
            this.tempDic = new Dictionary<string, DocumentsTerm>();
            this.StreamHasChanged = false;
            this.m_Cities = new HashSet<string>();
            this.m_Languages = new HashSet<string>();
            this.m_indexCounter = 1;
            this.dictionaries = new Dictionary<string, IndexTerm>[27];
            this.uniqueCorpusCounter = 0;
            this.docCounter = 0;
            this.totalLenDocs = 0;
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
        public void WriteToPostingFile(Dictionary<string, DocumentsTerm> documentTermsDic, bool hasWritten)
        {
            tempDic = documentTermsDic;
            if (!hasWritten)
            {
                this.tempDic = tempDic.OrderBy(i => i.Value.postNum).ThenBy(i => i.Value.line).ToDictionary(p => p.Key, p => p.Value);
                StringBuilder data = new StringBuilder();
                int postNum = -1;
                foreach (KeyValuePair<string, DocumentsTerm> pair in tempDic)
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
                    IndexTerm currentTerm = new IndexTerm(pair.Value.m_valueOfTerm, postNum, currentLine[postNum]);
                    currentTerm.IncreaseTfc(pair.Value.countTotalFrequency());
                    currentTerm.IncreaseDf(pair.Value.m_Terms.Count);
                    dictionaries[postNum].Add(pair.Value.m_valueOfTerm, currentTerm);
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
            Writer = new StreamWriter(Path.Combine(m_outPutPath, fileName + termPost + ".txt"));
        }

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
            foreach (KeyValuePair<string, DocumentsTerm> pair in tempDic)
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
                        writeData.AppendLine(lines[indexLine++]);
                        currentLineNumber++;
                    }
                    writeData.AppendLine(lines[indexLine++] + pair.Value.WriteToPostingFileDocDocTerm(true));
                    dictionaries[PostNumber][pair.Value.m_valueOfTerm].IncreaseTfc(pair.Value.countTotalFrequency());
                    dictionaries[PostNumber][pair.Value.m_valueOfTerm].IncreaseDf(pair.Value.m_Terms.Count);
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
                    IndexTerm currentTerm = new IndexTerm(pair.Value.m_valueOfTerm, PostNumber, currentLine[PostNumber]);
                    LineNumberOfTerm = currentLine[PostNumber];
                    currentLine[PostNumber]++;
                    currentTerm.IncreaseTfc(pair.Value.countTotalFrequency());
                    currentTerm.IncreaseDf(pair.Value.m_Terms.Count);
                    dictionaries[PostNumber].Add(pair.Value.m_valueOfTerm, currentTerm);
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
            Writer = null;
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
            StringBuilder data = new StringBuilder();
            for (int j = 0; j < dictionaries.Length; j++)
            {
                uniqueCorpusCounter += dictionaries[j].Count;
                foreach (KeyValuePair<string, IndexTerm> currentTerm in dictionaries[j])
                {
                    data.AppendLine(currentTerm.Value.PrintTerm().ToString());
                }
            }
            Writer.Write(data);
            data.Clear();
            Writer.Flush();
            Writer.Close();
        }

        /// <summary>
        /// method which init all the terms in the current dictionary
        /// </summary>
        public void InitTerms()
        {
            int PostNumber = 0;
            foreach (KeyValuePair<string, DocumentsTerm> pair in tempDic)
            {
                PostNumber = pair.Value.postNum;
                if (dictionaries[PostNumber].ContainsKey(pair.Value.m_valueOfTerm))
                {
                    pair.Value.line = dictionaries[PostNumber][pair.Value.m_valueOfTerm].lineInPost;
                }
                else if (char.IsUpper(pair.Value.m_valueOfTerm[0]))
                {
                    if (dictionaries[PostNumber].ContainsKey(pair.Value.m_valueOfTerm.ToLower()))
                    {
                        pair.Value.m_valueOfTerm = pair.Value.m_valueOfTerm.ToLower();
                        pair.Value.line = dictionaries[PostNumber][pair.Value.m_valueOfTerm.ToLower()].lineInPost;
                    }
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
        public void WriteTheNewDocumentsFile(MasterFile masterFiles)
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
            foreach (Document document in masterFiles.m_documents.Values)
            {
                Writer.WriteLine(document.WriteDocumentToIndexFile());
                totalLenDocs += document.m_length;
            }

            Writer.Flush();
            Writer.Close();
            Writer = null;
        }

        /// <summary>
        /// method which saves all the cities exist in the tags
        /// </summary>
        /// <param name="masterFile">a collection of files</param>
        public void UpdateCitiesAndLanguagesInDocument(MasterFile masterFile)
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
        /// method to write all languages to file
        /// </summary>
        public void WriteLanguagesToFile()
        {
            if (!File.Exists(Path.Combine(m_outPutPath, "Languages.txt")))
            {
                CreateEmptyTxtFile("Languages.txt");
                Writer = new StreamWriter(Path.Combine(m_outPutPath, "Languages.txt"));
            }
            else
            {
                Writer = File.AppendText(Path.Combine(m_outPutPath, "Languages.txt"));
            }
            foreach (String language in m_Languages)
            {
                Writer.WriteLine(language);
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
            StringBuilder data = new StringBuilder();
            int currLineInPosting = 0, position, j, numOfLines = 0;
            string extension = "";
            for (int i = 0; i < Terms.Length - 2; i++, numOfLines++)
            {
                j = i;
                while (string.Equals(Terms[i].m_value, Terms[j + 1].m_value, StringComparison.OrdinalIgnoreCase) && j < Terms.Length - 2)
                {
                    if (!LinesToMerge.ContainsKey(Terms[i].lineInPost))
                    {
                        List<int> toAdd = new List<int>();
                        toAdd.Add(Terms[j + 1].lineInPost);
                        LinesToMerge.Add(Terms[i].lineInPost, toAdd);
                    }
                    else
                    {
                        LinesToMerge[Terms[i].lineInPost].Add(Terms[j + 1].lineInPost);
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
                foreach (int pos in mergeLines.Value)
                {
                    position = Lines[pos].IndexOf("[#]");
                    extension = Lines[pos].Substring(position + 3);
                    Lines[mergeLines.Key] = Lines[mergeLines.Key] + extension;
                    Lines[pos] = null;
                }
            }
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i] != null)
                {
                    position = Lines[i].IndexOf("[#]");
                    if (position == -1 || position == 0)
                    {
                        continue;
                    }
                    extension = Lines[i].Substring(0, position);
                    dictionaries[dicNumber][extension].lineInPost = currLineInPosting;
                    data.AppendLine(Lines[i]);
                    currLineInPosting++;
                }
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            CreateTxtFile("Posting" + dicNumber + ".txt", data);
        }

        /// <summary>
        /// method to add the right prefix to current number
        /// </summary>
        /// <param name="currentValue">the current number</param>
        /// <returns>the number with the prefix</returns>
        public string AddPerfixToNumber(string currentValue)
        {
            double number = Convert.ToDouble(currentValue.Replace(",", ""));
            if (number >= 1000 && number < 1000000)
            {
                return Math.Round((number / 1000), 2).ToString() + "K";
            }
            else if (number >= 1000000 && number < 1000000000)
            {
                return Math.Round((number / 1000000), 2).ToString() + "M";
            }
            else if (number >= 1000000000)
            {
                return Math.Round((number / 1000000000), 2).ToString() + "B";
            }
            else //number under 1000
            {
                return Math.Round(number, 2).ToString();
            }
        }

        /// <summary>
        /// method which gets the cities from API
        /// </summary>
        public void WriteCitiesIndexFile()
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
            int postNum;
            string city = "", line = "";
            using (var s = webRequest.GetResponse().GetResponseStream())
            {
                using (var sr = new StreamReader(s))
                {
                    var contributorsAsJson = sr.ReadToEnd();
                    JArray jarr = JArray.Parse(contributorsAsJson);
                    HashSet<string> m_States = new HashSet<string>();
                    foreach (JObject content in jarr.Children<JObject>())
                    {
                        JProperty[] fields = content.Properties().ToArray();
                        city = "" + fields[2].Value;
                        postNum = GetPostNumber(city);
                        city = (city).ToUpper();
                        if (m_Cities.Contains(city))
                        {
                            if (!m_States.Contains(fields[1].Value + ""))
                            {
                                m_States.Add(fields[1].Value + "");
                            }
                            if (dictionaries[postNum].ContainsKey(city))
                            {
                                line = "PN: " + postNum + " LN: " + dictionaries[postNum][city].lineInPost;
                            }
                            else
                            {
                                line = "not exist in Posting";
                            }
                            Writer.WriteLine(city + "(#)" + fields[1].Value + "(#)" + fields[0].Value[0].ToArray()[0].ToArray()[0] + "(#)" + AddPerfixToNumber(fields[3].Value.ToString()) + "(#)" + line);
                            m_Cities.Remove(city);
                        }
                    }
                    foreach (string m_city in m_Cities)
                    {
                        city = m_city.ToUpper();
                        postNum = GetPostNumber(city);
                        if (dictionaries[postNum].ContainsKey(city))
                        {
                            line = "PN: " + postNum + " LN: " + dictionaries[postNum][city].lineInPost;
                        }
                        else
                        {
                            line = "not exist in Posting";
                        }
                        Writer.WriteLine(city + "(#)" + line);
                    }
                    Console.WriteLine("counter of states: " + m_States.Count);
                    m_Cities.Clear();
                }
            }
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }

        /// <summary>
        /// method which calculates the avgDL
        /// </summary>
        /// <returns>AvgDL</returns>
        public double GetAvgDL()
        {
            return (totalLenDocs / docCounter);
        }


        /// <summary>
        /// method to write the documents' additional data
        /// </summary>
        public void WriteAdditionalDataOfDocs()
        {
            if (!File.Exists(Path.Combine(m_outPutPath, "DocsData.txt")))
            {
                CreateEmptyTxtFile("DocsData.txt");
                Writer = new StreamWriter(Path.Combine(m_outPutPath, "DocsData.txt"));
            }
            else
            {
                Writer = File.AppendText(Path.Combine(m_outPutPath, "DocsData.txt"));
            }
            Writer.WriteLine("avgDL: " + GetAvgDL());
            Writer.WriteLine("docsCounter: " + docCounter);
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }
    }
}

