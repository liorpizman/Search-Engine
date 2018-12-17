using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    public class Searcher
    {
        public Dictionary<string, MethodScore> DocRank;   /// key - DocNo , Value - Rank
        public Ranker m_ranker;
        public Dictionary<string, IndexTerm>[] dictionaries { get; set; }
        public Dictionary<string, int> docLengths { get; set; }

        private bool m_doStemming;
        private string m_inputPath;
        private string m_outPutPath;

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
            this.DocRank = new Dictionary<string, MethodScore>();
            this.m_ranker = new Ranker();
            this.docLengths = new Dictionary<string, int>();
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
                docLengths.Add(docno, Int32.Parse(length));
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

        public void ParseNewQuery(string query)
        {
            Document queryDocument = new Document("DOCNO", new StringBuilder("DATE1"), new StringBuilder("TI"), query, new StringBuilder("CITY"), new StringBuilder("language"));
            Parse parse = new Parse(m_doStemming, m_inputPath);
            parse.ParseDocuments(queryDocument);
            Dictionary<string, DocumentsTerm> queryTerms = parse.m_allTerms;
            int lineInPosting, PostNumber, CurrentqFi;
            foreach (string key in queryTerms.Keys)
            {
                lineInPosting = GetLineInPost(key);
                PostNumber = GetPostNumber(key);
                CurrentqFi = queryTerms[key].m_Terms["DOCNO"].m_tf;
                SelectTermDataForRanking(lineInPosting, PostNumber, CurrentqFi);
            }
        }


        private void SelectTermDataForRanking(int lineInPosting, int PostNumber, int CurrentqFi)
        {
            string[] AllLines, SplitedLine, TermInstances;
            string Line, currentDoc, currentFrequency;
            AllLines = File.ReadAllLines(Path.Combine(m_outPutPath, "Posting" + PostNumber + ".txt"));
            Line = AllLines[lineInPosting];
            TermInstances = Line.Split(new string[] { "[#]" }, StringSplitOptions.None);
            m_ranker.Ni = TermInstances.Length - 1;

            AllLines = File.ReadAllLines(Path.Combine(m_outPutPath, "DocsData.txt"));
            m_ranker.avgDL = Double.Parse(AllLines[0].Split(' ')[1]);
            m_ranker.N = Int32.Parse(AllLines[1].Split(' ')[1]);
            m_ranker.qFi = CurrentqFi;

            for (int i = 1; i < TermInstances.Length; i++)
            {
                SplitedLine = TermInstances[i].Split(new string[] { "(#)" }, StringSplitOptions.None);
                currentDoc = SplitedLine[0];
                currentFrequency = SplitedLine[1];
                m_ranker.dl = GetDocLength(currentDoc); //currentDoc = DOCNO
                m_ranker.Fi = Int32.Parse(currentFrequency);
                if (DocRank.ContainsKey(currentDoc))
                {
                    DocRank[currentDoc].IncreaseBM(m_ranker.CalculateBM25());
                    DocRank[currentDoc].IncreaseCosSim(m_ranker.CalculateCosSim());
                }
                else
                {
                    DocRank.Add(currentDoc, new MethodScore(m_ranker.CalculateBM25(), m_ranker.CalculateCosSim()));
                }
            }

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

        /*
        private int GetCurrentqFi(string term, Dictionary query)
        {
            int counter = 0;
            string[] terms = query.Trim(' ').Split(' ');
            foreach (string t in terms)
            {
                if (t.Equals(term))
                    counter++;
            }
            return counter;
        }
        */

        private int GetDocLength(string DOCNO)
        {
            return docLengths[DOCNO];
        }

        public void GetRelevantDocs()
        {
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
        }

    }
}
