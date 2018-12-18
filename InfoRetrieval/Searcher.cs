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
        public Dictionary<string, double> docLengths { get; set; }

        private bool m_doStemming;
        private string m_inputPath;
        private string m_outPutPath;
        private bool m_withSemantics;

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
            this.m_inputPath = "";
            this.m_ranker = new Ranker();
            this.docLengths = new Dictionary<string, double>();
            this.m_outPutPath = outPutPath;
            CalculateDocsLengths();
        }

        private void CalculateDocsLengths()
        {
            string[] AllLines, splittedLine;
            string docno, tmp, length;
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
                docLengths.Add(docno, Double.Parse(length));
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

        public void ParseNewQuery(string query, bool withSemantic, string id, bool saveResults)
        {
            Query q;
            if (id.Equals("-1"))
            {
                q = new Query(query);
            }
            else
            {
                q = new Query(query, id);
            }
            this.m_withSemantics = withSemantic;
            if (withSemantic)
            {
                q.content += UpdateQueryBySemantics(query);
            }
            Document queryDocument = new Document("DOCNO", new StringBuilder("DATE1"), new StringBuilder("TI"), q.content, new StringBuilder("CITY"), new StringBuilder("language"));
            Parse parse = new Parse(m_doStemming, m_inputPath);
            parse.ParseDocuments(queryDocument);
            Dictionary<string, DocumentsTerm> queryTerms = parse.m_allTerms;
            int lineInPosting, PostNumber, CurrentqFi;
            foreach (string key in queryTerms.Keys)
            {
                PostNumber = GetPostNumber(key);
                if (!dictionaries[PostNumber].ContainsKey(key))
                {
                    continue;
                }
                lineInPosting = GetLineInPost(key);
                CurrentqFi = queryTerms[key].m_Terms["DOCNO"].m_tf;
                SelectTermDataForRanking(lineInPosting, PostNumber, CurrentqFi, q);
            }
            if (saveResults)
            {
                WriteQueryResults(q);
            }
        }


        public void ParseQueriesFile(string path, bool doSemantic, bool saveResults)
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
                ParseNewQuery(m_queries[id], doSemantic, id, saveResults);
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

        private void SelectTermDataForRanking(int lineInPosting, int PostNumber, int CurrentqFi, Query q)
        {
            string[] AllLines, SplitedLine, TermInstances;
            string Line, currentDoc, currentFrequency;
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
                currentFrequency = SplitedLine[1];
                m_ranker.dl = GetDocLength(currentDoc); //currentDoc = DOCNO
                m_ranker.Fi = Double.Parse(currentFrequency);
                if (q.m_docsRanks.ContainsKey(currentDoc))
                {
                    q.m_docsRanks[currentDoc].IncreaseBM(m_ranker.CalculateBM25());
                    q.m_docsRanks[currentDoc].IncreaseInnerProduct(m_ranker.CalculateInnerProduct());
                }
                else
                {
                    q.m_docsRanks.Add(currentDoc, new MethodScore(m_ranker.CalculateBM25(), m_ranker.CalculateInnerProduct()));
                }
            }

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
            return docLengths[DOCNO];
        }

        public void WriteQueryResults(Query q)
        {
            StreamWriter Writer;
            if (!File.Exists(Path.Combine(m_outPutPath, "QueryRanksResults.txt")))
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(m_outPutPath, "QueryRanksResults.txt")))
                {
                    outputFile.WriteLine(new StringBuilder());
                }
                Writer = new StreamWriter(Path.Combine(m_outPutPath, "QueryRanksResults.txt"));
            }
            else
            {
                Writer = File.AppendText(Path.Combine(m_outPutPath, "QueryRanksResults.txt"));
            }
            Writer.Write(q.GetQueryData());
            Writer.Flush();
            Writer.Close();
        }

    }
}
/*
Dictionary<string, double> DocResults = new Dictionary<string, double>();
int i = 1;
foreach (string key in DocRank.Keys)
{
    if (i > 50)
    {
        break;
    }
    DocResults.Add(key, DocRank[key].GetTotalScore());
    i++;

}
if (!Directory.Exists(m_outPutPath))
{
    Directory.CreateDirectory(m_outPutPath);
}
using (StreamWriter outputFile = new StreamWriter(Path.Combine(m_outPutPath, "RanksResults.txt")))
{
    outputFile.WriteLine(new StringBuilder());
}
StreamWriter Writer = new StreamWriter(Path.Combine(m_outPutPath, "RanksResults.txt"));
StringBuilder data = new StringBuilder();
DocResults = DocResults.OrderByDescending(j => j.Value).ToDictionary(p => p.Key, p => p.Value);
foreach (string key in DocResults.Keys)
{
    data.AppendLine(key + "   " + DocResults[key]);
}
Writer.Write(data);
data.Clear();
Writer.Flush();
Writer.Close();
*/
