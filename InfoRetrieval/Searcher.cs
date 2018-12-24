using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WnLexicon;

namespace InfoRetrieval
{
    public class Searcher
    {
        public Ranker m_ranker;
        public Dictionary<string, IndexTerm>[] dictionaries { get; set; }
        public Dictionary<string, DocInfo> docInformation { get; set; }

        private bool m_doStemming;
        private string m_inputPath;
        private string m_stopWordsPath;
        private string m_outPutPath;
        private string m_toWriteOutPutPath;
        private bool m_withSemantics;
        private Query tmpQuery;

        public Query query
        {
            get
            {
                return tmpQuery;
            }
            set
            {
                tmpQuery = value;
            }
        }

        public bool DoSemantic
        {
            get
            {
                return m_withSemantics;
            }
            set
            {
                m_withSemantics = value;
            }
        }
        public bool DoStemming
        {
            get
            {
                return m_doStemming;
            }
            set
            {
                m_doStemming = value;
            }
        }

        public string InputPath
        {
            get
            {
                return m_inputPath;
            }
            set
            {
                m_inputPath = value;
            }
        }

        public string toWriteOutPutPath
        {
            get
            {
                return m_toWriteOutPutPath;
            }
            set
            {
                m_toWriteOutPutPath = value;
            }
        }

        public string OutPutPath
        {
            get
            {
                return m_outPutPath;
            }
            set
            {
                m_outPutPath = value;
            }
        }

        public string StopWordsPath
        {
            get
            {
                return m_stopWordsPath;
            }
            set
            {
                m_stopWordsPath = value;
            }
        }

        public static Hashtable m_postingNums = new Hashtable()
        {
            {'a', 1 }, {'b', 2 }, {'c', 3 },{'d', 4 }, //{ "", "0" },
            { 'e', 5 }, {'f', 6 }, {'g', 7 }, {'h', 8 },{'i', 9 },
            { 'j', 10 }, {'k', 11 }, {'l', 12 }, {'m', 13 }, {'n', 14 },
            {'o', 15 }, {'p', 16 },{'q', 17 },{'r', 18 }, {'s', 19 },
            {'t', 20 }, {'u', 21 },{'v', 22 },{'w', 23 }, {'x', 24 },
            { 'y', 25 } ,{'z', 26 }
        };

        public Searcher(string outPutPath)
        {
            this.m_doStemming = false;
            this.m_stopWordsPath = "";
            this.m_inputPath = "";
            this.m_ranker = new Ranker();
            this.docInformation = new Dictionary<string, DocInfo>();
            this.m_outPutPath = outPutPath;
            EvaluatesDocumentsInfo();
        }
        private void EvaluatesDocumentsInfo()
        {
            string[] AllLines, splittedLine, tmpSplite, Entities;
            string docno, tmp, length, title, city;
            if (m_doStemming)
            {
                AllLines = File.ReadAllLines(Path.Combine(Path.Combine(m_outPutPath, "WithStem"), "Documents.txt"));
            }
            else
            {
                AllLines = File.ReadAllLines(Path.Combine(Path.Combine(m_outPutPath, "WithOutStem"), "Documents.txt"));
            }
            for (int i = 0; i < AllLines.Length; i++)
            {
                splittedLine = AllLines[i].Split(new string[] { "(#)" }, StringSplitOptions.None);
                docno = splittedLine[0].Trim(' ');
                tmp = splittedLine[splittedLine.Length - 2].Trim(' ');
                length = tmp.Split(' ')[1].Trim(' ');
                tmp = splittedLine[splittedLine.Length - 1].Trim(' ');
                city = tmp.Split(':')[1].Trim(' ');
                tmp = splittedLine[1].Trim(' ');
                tmpSplite = tmp.Split(new string[] { "TI: " }, StringSplitOptions.None);
                if (tmpSplite.Length > 1)
                {
                    title = tmp.Split(new string[] { "TI: " }, StringSplitOptions.None)[1];
                }
                else
                {
                    title = "";
                }
                DocInfo tmpDocument = new DocInfo(docno, Double.Parse(length), title, city);
                tmp = splittedLine[4].Trim(' ');
                tmpSplite = tmp.Split(new string[] { "[#] " }, StringSplitOptions.None);
                for (int j = 1; j < tmpSplite.Length; j++)
                {
                    Entities = tmpSplite[j].Split(new string[] { "[*]" }, StringSplitOptions.None);
                    tmpDocument.SetEntite(Entities[0], Double.Parse(Entities[1]));
                }
                docInformation.Add(docno, tmpDocument);
            }
        }

        public void updateOutput(bool doStem, string path)
        {
            m_doStemming = doStem;
            if (m_doStemming)
            {
                m_outPutPath = Path.Combine(path, "WithStem");
            }
            else
            {
                m_outPutPath = Path.Combine(path, "WithOutStem");
            }
        }

        public void ParseNewQuery(string query, bool withSemantic, string id, bool saveResults, HashSet<string> filterByCity)
        {
            Query q, semanticQuery;
            if (id.Equals("-1"))
            {
                q = new Query(query);
            }
            else
            {
                q = new Query(query, id);
            }
            this.m_withSemantics = withSemantic;

            IterateOverQuery(q, filterByCity);
            if (withSemantic)
            {
                // q.content += UpdateQueryBySemantics(query);
                semanticQuery = new Query(UpdateQueryBySemantics(query));
                IterateOverQuery(semanticQuery, filterByCity);
                foreach (string key in semanticQuery.m_docsRanks.Keys)
                {
                    if (q.m_docsRanks.ContainsKey(key))
                    {
                        q.m_docsRanks[key].SetSemanticBM(semanticQuery.m_docsRanks[key].GetBM25());
                        q.m_docsRanks[key].SetSemanticInnerProduct(semanticQuery.m_docsRanks[key].GetInnerProduct());
                        q.m_docsRanks[key].SetSemanticTitleScore(semanticQuery.m_docsRanks[key].GetTitleScore());
                    }
                }
            }
            if (saveResults)
            {
                WriteQueryResults(q);
            }
            tmpQuery = q;
        }

        private void IterateOverQuery(Query q, HashSet<string> filterByCity)
        {
            Document queryDocument = new Document("DOCNO", new StringBuilder("DATE1"), new StringBuilder("TI"), q.content, new StringBuilder("CITY"), new StringBuilder("language"));
            Parse parse = new Parse(m_doStemming, m_stopWordsPath);
            parse.ParseDocuments(queryDocument);
            Dictionary<string, DocumentsTerm> queryTerms = new Dictionary<string, DocumentsTerm>(parse.m_allTerms);
            int lineInPosting, PostNumber, CurrentqFi;
            string upper, lower, currKey;
            foreach (string key in parse.m_allTerms.Keys)
            {
                currKey = key;
                PostNumber = GetPostNumber(currKey);
                if (!dictionaries[PostNumber].ContainsKey(currKey))
                {
                    upper = currKey.ToUpper();
                    lower = currKey.ToLower();
                    if (dictionaries[PostNumber].ContainsKey(upper))
                    {
                        currKey = upper;
                        queryTerms.Add(currKey, queryTerms[key]);
                        queryTerms.Remove(key);
                    }
                    else if (dictionaries[PostNumber].ContainsKey(lower))
                    {
                        currKey = lower;
                        queryTerms.Add(currKey, queryTerms[key]);
                        queryTerms.Remove(key);
                    }
                    else
                    {
                        continue;
                    }
                }
                //queryTerms = parse.m_allTerms;              
                lineInPosting = GetLineInPost(currKey);
                CurrentqFi = queryTerms[currKey].m_Terms["DOCNO"].m_tf;
                SelectTermDataForRanking(lineInPosting, PostNumber, CurrentqFi, q, filterByCity);
            }
        }


        public void ParseQueriesFile(string path, bool doSemantic, bool saveResults, HashSet<string> filterByCity)
        {
            Dictionary<string, string> m_queries = new Dictionary<string, string>();
            string text = File.ReadAllText(path);
            string[] queries = text.Split(new[] { "<top>" }, StringSplitOptions.None);
            string queryID, queryContent;
            char[] toRemove = { ' ', '\n' };
            for (int i = 1; i < queries.Length; i++)
            {
                queryID = GetStringInBetween("<num>", "<title>", queries[i]).Trim(toRemove);
                queryID = queryID.Split(new[] { ": " }, StringSplitOptions.None)[1].Trim(toRemove);
                queryContent = GetStringInBetween("<title>", "<desc>", queries[i]).Trim(toRemove);
                m_queries.Add(queryID, queryContent);
            }
            foreach (string id in m_queries.Keys)
            {
                ParseNewQuery(m_queries[id], doSemantic, id, saveResults, filterByCity);
            }
        }

        /// <summary>
        /// method to get the string between two tags
        /// </summary>
        /// <param name="strBegin">first tag</param>
        /// <param name="strEnd">second tag</param>
        /// <param name="strSource">the source string</param>
        /// <returns>the string between two tags</returns>
        private string GetStringInBetween(string strBegin, string strEnd, string strSource)
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

        private void SelectTermDataForRanking(int lineInPosting, int PostNumber, int CurrentqFi, Query q, HashSet<string> filterByCity)
        {
            string[] AllLines, SplitedLine, TermInstances;
            string Line, currentDoc, currentFrequency, title;
            AllLines = File.ReadAllLines(Path.Combine(m_outPutPath, "Posting" + PostNumber + ".txt"));
            Line = AllLines[lineInPosting];
            TermInstances = Line.Split(new string[] { "[#]" }, StringSplitOptions.None);
            m_ranker.Ni = TermInstances.Length - 1;

            AllLines = File.ReadAllLines(Path.Combine(m_outPutPath, "DocsData.txt"));
            m_ranker.avgDL = Double.Parse(AllLines[0].Split(' ')[1]);
            m_ranker.N = Double.Parse(AllLines[1].Split(' ')[1]);
            m_ranker.qFi = CurrentqFi;

            for (int i = 1; i < TermInstances.Length; i++)
            {
                SplitedLine = TermInstances[i].Split(new string[] { "(#)" }, StringSplitOptions.None);
                currentDoc = SplitedLine[0];
                if (filterByCity.Count > 0)
                {
                    if (!filterByCity.Contains(docInformation[currentDoc].city))
                    {
                        continue;
                    }
                }
                currentFrequency = SplitedLine[1];
                m_ranker.dl = GetDocLength(currentDoc); //currentDoc = DOCNO
                m_ranker.Fi = Double.Parse(currentFrequency);
                title = docInformation[currentDoc].docTitle;
                m_ranker.titleLen = title.Split(' ').Length;
                m_ranker.tileFi = CountInstancesOfTermInTitle(TermInstances[0], title);

                if (q.m_docsRanks.ContainsKey(currentDoc))
                {
                    q.m_docsRanks[currentDoc].IncreaseBM(m_ranker.CalculateBM25());
                    q.m_docsRanks[currentDoc].IncreaseInnerProduct(m_ranker.CalculateInnerProduct());
                    q.m_docsRanks[currentDoc].IncreaseTitleScore(m_ranker.CalculateTitleRank());
                }
                else
                {
                    q.m_docsRanks.Add(currentDoc, new MethodScore(m_ranker.CalculateBM25(), m_ranker.CalculateInnerProduct(), m_ranker.CalculateTitleRank()));
                }
            }

        }

        private double CountInstancesOfTermInTitle(string term, string title)
        {
            return CountStringOccurrences(title.ToLower(), term.ToLower());
        }

        private double CountStringOccurrences(string text, string pattern)
        {
            double count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index)) != -1)
            {
                index += pattern.Length;
                count++;
            }
            return count;
        }

        private string UpdateQueryBySemantics(string query)
        {
            string[] wordsBeforeSemantic, adjWords, advWords, nounWords, verbWords;
            StringBuilder extendQuery = new StringBuilder();
            wordsBeforeSemantic = query.Split(' ');
            for (int i = 0; i < wordsBeforeSemantic.Length; i++)
            {
                adjWords = Lexicon.FindSynonyms(wordsBeforeSemantic[i], Wnlib.PartsOfSpeech.Adj, false);
                advWords = Lexicon.FindSynonyms(wordsBeforeSemantic[i], Wnlib.PartsOfSpeech.Adv, false);
                nounWords = Lexicon.FindSynonyms(wordsBeforeSemantic[i], Wnlib.PartsOfSpeech.Noun, false);
                verbWords = Lexicon.FindSynonyms(wordsBeforeSemantic[i], Wnlib.PartsOfSpeech.Verb, false);
                extendQuery.Append(updateByPartOfSpeech(adjWords));
                extendQuery.Append(updateByPartOfSpeech(advWords));
                extendQuery.Append(updateByPartOfSpeech(nounWords));
                extendQuery.Append(updateByPartOfSpeech(verbWords));
            }
            return extendQuery.ToString();
        }

        private string updateByPartOfSpeech(string[] partOfSpeech)
        {
            StringBuilder extendQuery = new StringBuilder();
            if (partOfSpeech != null)
            {
                for (int i = 0; i < 5 && i < partOfSpeech.Length; i++)
                {
                    extendQuery.Append(" " + partOfSpeech[i]);
                }
                return extendQuery.ToString();
            }
            return string.Empty;
        }

        private int GetLineInPost(string term)
        {
            return dictionaries[GetPostNumber(term)][term].lineInPost; ;
        }

        private int GetPostNumber(string term)
        {
            if (term.Equals(""))
            {
                return 0;
            }
            char c = char.ToLower(term[0]);
            if (char.IsLetter(c))
            {
                return (int)m_postingNums[c];
            }
            else
            {
                return 0;
            }
        }

        private double GetDocLength(string DOCNO)
        {
            return docInformation[DOCNO].docLength;
        }

        public void WriteQueryResults(Query q)
        {
            StreamWriter Writer;
            if (!File.Exists(Path.Combine(m_toWriteOutPutPath, "QueryRanksResults.txt")))
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(m_toWriteOutPutPath, "QueryRanksResults.txt")))
                {
                    outputFile.WriteLine(new StringBuilder());
                }
                Writer = new StreamWriter(Path.Combine(m_toWriteOutPutPath, "QueryRanksResults.txt"));
            }
            else
            {
                Writer = File.AppendText(Path.Combine(m_toWriteOutPutPath, "QueryRanksResults.txt"));
            }
            Writer.Write(q.GetQueryData());
            Writer.Flush();
            Writer.Close();
        }

    }
}

