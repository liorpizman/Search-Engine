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
        private string[][] m_PostingLines { get; set; }


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

        public Searcher(string outPutPath, string inputPath, bool m_doStemming)
        {
            this.m_doStemming = m_doStemming;
            this.m_stopWordsPath = inputPath;
            this.m_inputPath = "";
            this.m_ranker = new Ranker();
            this.docInformation = new Dictionary<string, DocInfo>();
            this.m_outPutPath = outPutPath;
            this.m_PostingLines = new string[27][];
            SetPostingLines();
        }

        private void SetPostingLines()
        {
            string path = m_outPutPath;
            if (m_doStemming)
            {
                path = Path.Combine(path, "WithStem");
            }
            else
            {
                path = Path.Combine(path, "WithOutStem");
            }
            for (int i = 0; i < 27; i++)
            {
                this.m_PostingLines[i] = File.ReadAllLines(Path.Combine(path, "Posting" + i + ".txt"));
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

        public Query ParseWithDescription(string query, string description, bool withSemantic, string id, Dictionary<string, string> filterByCity)
        {
            Query qDescription = new Query(""), qQuery;
            qDescription = ParseNewDescription(description, id, filterByCity);
            qQuery = ParseNewQuery(query, withSemantic, id, filterByCity);
            foreach (string docno in qQuery.m_docsRanks.Keys)
            {
                if (qDescription.m_docsRanks.ContainsKey(docno))
                {
                    qQuery.m_docsRanks[docno].IncreaseDescription(qDescription.m_docsRanks[docno].GetTotalScore());
                }
            }
            tmpQuery = qQuery;
            return qQuery;
        }

        public Query ParseNewDescription(string description, string id, Dictionary<string, string> filterByCity)
        {
            Query qDescription;
            if (id.Equals("-1"))
            {
                qDescription = new Query(description);
            }
            else
            {
                qDescription = new Query(description, id);
            }
            IterateOverQuery(qDescription, filterByCity);
            return qDescription;
        }

        public Query ParseNewQuery(string query, bool withSemantic, string id, Dictionary<string, string> filterByCity)
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
            tmpQuery = q;
            return q;
        }

        private void IterateOverQuery(Query q, Dictionary<string, string> filterByCity)
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


        public void ParseQueriesFile(string path, bool doSemantic, bool saveResults, Dictionary<string, string> filterByCity)
        {
            Dictionary<string, queryInfo> m_queries = new Dictionary<string, queryInfo>();
            string text = File.ReadAllText(path);
            string[] queries = text.Split(new[] { "<top>" }, StringSplitOptions.None);
            string queryID, queryContent, description;
            char[] toRemove = { ' ', '\n' };
            for (int i = 1; i < queries.Length; i++)
            {
                queryID = GetStringInBetween("<num>", "<title>", queries[i]).Trim(toRemove);
                queryID = queryID.Split(new[] { ": " }, StringSplitOptions.None)[1].Trim(toRemove);

                description = GetStringInBetween("<desc>", "<narr>", queries[i]).Trim(toRemove);
                description = description.Split(new[] { "Description: " }, StringSplitOptions.None)[1].Trim(toRemove);

                queryContent = GetStringInBetween("<title>", "<desc>", queries[i]).Trim(toRemove);
                m_queries.Add(queryID, new queryInfo(queryContent, description));
            }
            foreach (string id in m_queries.Keys)
            {
                //ParseNewQuery(m_queries[id], doSemantic, id, filterByCity);
                Query q = ParseWithDescription(m_queries[id].m_queryContent, m_queries[id].m_queryDescription, doSemantic, id, filterByCity);
                if (saveResults)
                {
                    WriteQueryResults(q);
                }
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

        private void SelectTermDataForRanking(int lineInPosting, int PostNumber, int CurrentqFi, Query q, Dictionary<string, string> filterByCity)
        {
            string[] AllLines, SplitedLine, TermInstances;
            string Line, currentDoc, currentFrequency, title, upper, lower;
            int cityPostNumber;
            //AllLines = File.ReadAllLines(Path.Combine(m_outPutPath, "Posting" + PostNumber + ".txt"));
            //Line = AllLines[lineInPosting];
            Line = m_PostingLines[PostNumber][lineInPosting];
            TermInstances = Line.Split(new string[] { "[#]" }, StringSplitOptions.None);
            m_ranker.Ni = TermInstances.Length - 1;

            AllLines = File.ReadAllLines(Path.Combine(m_outPutPath, "DocsData.txt"));
            m_ranker.avgDL = Double.Parse(AllLines[0].Split(' ')[1]);
            m_ranker.N = Double.Parse(AllLines[1].Split(' ')[1]);
            m_ranker.qFi = CurrentqFi;

            foreach (string city in filterByCity.Keys)
            {
                cityPostNumber = GetPostNumber(city);
                upper = city.ToUpper();
                lower = city.ToLower();
                if (dictionaries[cityPostNumber].ContainsKey(upper))
                {
                    lineInPosting = dictionaries[cityPostNumber][upper].lineInPost;
                    //AllLines = File.ReadAllLines(Path.Combine(m_outPutPath, "Posting" + cityPostNumber + ".txt"));
                    Line = m_PostingLines[cityPostNumber][lineInPosting];
                    //Line = AllLines[lineInPosting];
                    filterByCity[upper] = Line;
                }
                else if (dictionaries[cityPostNumber].ContainsKey(lower))
                {
                    lineInPosting = dictionaries[cityPostNumber][lower].lineInPost;
                    //AllLines = File.ReadAllLines(Path.Combine(m_outPutPath, "Posting" + cityPostNumber + ".txt"));
                    Line = m_PostingLines[cityPostNumber][lineInPosting];
                    //Line = AllLines[lineInPosting];
                    filterByCity[lower] = Line;
                }
            }
            bool containsDoc = false;
            for (int i = 1; i < TermInstances.Length; i++)
            {
                SplitedLine = TermInstances[i].Split(new string[] { "(#)" }, StringSplitOptions.None);
                currentDoc = SplitedLine[0];
                if (filterByCity.Count > 0)
                {
                    foreach (string city in filterByCity.Keys) //checks whether the city in the text
                    {
                        containsDoc = containsDoc || filterByCity[city].Contains(currentDoc);
                    }
                    if (!containsDoc && !filterByCity.ContainsKey(docInformation[currentDoc].city)) // checks whether the city is in the text or in the tag
                    {
                        continue;
                    }
                }
                currentFrequency = SplitedLine[1];
                m_ranker.dl = GetDocLength(currentDoc); //currentDoc = DOCNO
                m_ranker.Fi = Double.Parse(currentFrequency);
                title = docInformation[currentDoc].docTitle;
                m_ranker.titleLen = title.Split(' ').Length;
                //m_ranker.titleFi = CountInstancesOfTermInString(TermInstances[0], title);----- not in use right now
                //m_ranker.termInKfirstWordsTotal = RankByFirstKWords(TermInstances[0], currentDoc);----- not in use right now
                if (q.m_docsRanks.ContainsKey(currentDoc))
                {
                    //q.m_docsRanks[currentDoc].IncreaseKfirstWords(m_ranker.CalculateKFirstWordsRank()); ----- not in use right now
                    q.m_docsRanks[currentDoc].IncreaseBM(m_ranker.CalculateBM25());
                    q.m_docsRanks[currentDoc].IncreaseInnerProduct(m_ranker.CalculateInnerProduct());
                    q.m_docsRanks[currentDoc].IncreaseTitleScore(m_ranker.CalculateTitleRank());
                }
                else
                {
                    q.m_docsRanks.Add(currentDoc, new MethodScore(m_ranker.CalculateBM25(), m_ranker.CalculateInnerProduct(), m_ranker.CalculateTitleRank(), m_ranker.CalculateKFirstWordsRank()));
                }
            }

        }

        private double RankByFirstKWords(string term, string docno)
        {
            return CountInstancesOfTermInString(term, docInformation[docno].m_KWords);
        }

        private double CountInstancesOfTermInString(string term, string title)
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



    public class queryInfo
    {
        public string m_queryContent { get; set; }
        public string m_queryDescription { get; set; }

        public queryInfo(string queryContent, string queryDescription)
        {
            this.m_queryContent = queryContent;
            this.m_queryDescription = queryDescription;
        }
    }
}

