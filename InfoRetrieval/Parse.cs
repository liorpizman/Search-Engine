using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Parse
    {
        public Rules m_rules;
        public HashSet<string> m_stopWords;
        public bool m_doStemming;
        public Dictionary<string, DocumentTerms> m_allTerms;

        public Parse(bool doStem)
        {
            m_rules = new Rules();
            m_stopWords = new HashSet<string>();
            m_doStemming = doStem;
            m_allTerms = new Dictionary<string, DocumentTerms>();
        }

        public void addStopWords()
        {
            String line;
            try
            {
                string path = Directory.GetCurrentDirectory();
                StreamReader sr = new StreamReader(path + "\\stop_words.txt");
                line = sr.ReadLine();
                while (line != null)
                {
                    line = sr.ReadLine();
                    m_stopWords.Add(line);
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception-(Parse.addStopWords): " + e.Message);
            }
        }


        public void parseFiles(Dictionary<string, masterFile> m_files)
        {
            addStopWords();
            foreach (masterFile file in m_files.Values)
            {
                parseDocuments(file);
            }
        }


        public double FractionToDouble(string fraction)
        {
            double result;
            if (double.TryParse(fraction, out result))
                return result;
            string[] split = fraction.Split(new char[] { ' ', '/' });
            if (split.Length == 2 || split.Length == 3)
            {
                int a, b;
                if (int.TryParse(split[0], out a) && int.TryParse(split[1], out b))
                {
                    if (split.Length == 2)
                        return (double)a / b;
                    int c;
                    if (int.TryParse(split[2], out c))
                        return a + (double)b / c;
                }
            }
            throw new FormatException("Parse.FractionToDouble exception");
        }

        /*
        static void Main(string[] args)
        {
            Parse parse = new Parse(true);
            Console.WriteLine(FractionToDouble("345,142.453") / 1000 + "K");
            Console.ReadLine();
        }
        */
        public string numberAfterParse(Match match, int toDecrease, string tail, double multiple)
        {
            string current = match.Value.Substring(0, match.Value.Length - toDecrease);
            double number = FractionToDouble(current) * multiple;
            current = number.ToString() + tail;
            return current;
        }

        public void addNumberTerm(string DOCNO, string current, int index)
        {
            if (m_allTerms.ContainsKey(current))
            {
                m_allTerms[current].m_totalAmount++;
                if (m_allTerms[current].m_Terms.ContainsKey(DOCNO))
                {
                    m_allTerms[current].m_Terms[DOCNO].m_positions.Add(index);
                    m_allTerms[current].m_Terms[DOCNO].m_amount++;
                }
                else
                {
                    m_allTerms[current].m_Terms.Add(DOCNO, new Term(current, DOCNO));
                }
            }
            else
            {
                DocumentTerms documentTerms = new DocumentTerms(DOCNO);
                documentTerms.addToDocumentDictionary(new Term(current, DOCNO));
                m_allTerms.Add(current, documentTerms);
            }
        }

        public void parseDocuments(masterFile file)
        {
            Stemmer stemmer = new Stemmer();
            double number;
            string current;
            foreach (Document document in file.m_documents.Values)
            {
                foreach (Regex regex in m_rules.rulesList)
                {
                    foreach (Match match in regex.Matches(document.m_TEXT))
                    {
                        if (!m_stopWords.Contains(match.Value))
                        {
                            if (regex == m_rules.rulesList[0])        //numbersCase
                            {
                                if (match.Value.Contains("Thousand") || match.Value.Contains("thousand"))
                                {
                                    current = numberAfterParse(match, 9, "K", 1);
                                    addNumberTerm(document.m_DOCNO, current, match.Index);
                                }
                                else if (match.Value.Contains("Million") || match.Value.Contains("million"))
                                {
                                    current = numberAfterParse(match, 8, "M", 1);
                                    addNumberTerm(document.m_DOCNO, current, match.Index);
                                }
                                else if (match.Value.Contains("Billion") || match.Value.Contains("billion"))
                                {
                                    current = numberAfterParse(match, 8, "B", 1);
                                    addNumberTerm(document.m_DOCNO, current, match.Index);
                                }
                                else if (match.Value.Contains("Trillion") || match.Value.Contains("trillion"))
                                {
                                    current = numberAfterParse(match, 9, "B", 1000);
                                    addNumberTerm(document.m_DOCNO, current, match.Index);
                                }
                                else
                                {
                                    number = FractionToDouble(match.Value.Replace(",", ""));
                                    if (number >= 1000 && number < 1000000)
                                    {
                                        addNumberTerm(document.m_DOCNO, (number / 1000).ToString() + "K", match.Index);
                                    }
                                    else if (number >= 1000000 && number < 1000000000)
                                    {
                                        addNumberTerm(document.m_DOCNO, (number / 1000000).ToString() + "M", match.Index);
                                    }
                                    else if (number >= 1000000000)
                                    {
                                        addNumberTerm(document.m_DOCNO, (number / 1000000000).ToString() + "B", match.Index);
                                    }
                                    else //number under 1000
                                    {
                                        addNumberTerm(document.m_DOCNO, number.ToString(), match.Index);
                                    }
                                }
                            }
                            else if (regex == m_rules.rulesList[1])  //percentsCase
                            {
                                if (match.Value.Contains("Percents") || match.Value.Contains("percents"))
                                {
                                    current = numberAfterParse(match, 9, "%", 1);
                                }
                                else if (match.Value.Contains("Percent") || match.Value.Contains("percent"))
                                {
                                    current = numberAfterParse(match, 8, "%", 1);
                                }
                                else if (match.Value.Contains("Percentage") || match.Value.Contains("percentage"))
                                {
                                    current = numberAfterParse(match, 11, "%", 1);
                                }
                                else //if (match.Value.Contains("%"))
                                {
                                    current = match.Value;
                                }
                                addNumberTerm(document.m_DOCNO, current, match.Index);
                            }
                            else if (regex == m_rules.rulesList[2])  //pricesCase1
                            {
                                if (match.Value.Contains("Dollars") || match.Value.Contains("dollars"))
                                {
                                    current = match.Value.Substring(0, match.Value.Length - 8);
                                    if (match.Value.Contains("m"))
                                    {
                                        current = numberAfterParse(match, 1, " M Dollars", 1);  // if the m is close to number
                                    }
                                    else if (match.Value.Contains("million U.S."))
                                    {
                                        current = numberAfterParse(match, 13, " M Dollars", 1);
                                    }
                                    else if (match.Value.Contains("bn"))
                                    {
                                        current = numberAfterParse(match, 2, " M Dollars", 1000); // if the m is close to number
                                    }
                                    else if (match.Value.Contains("billion U.S."))
                                    {
                                        current = numberAfterParse(match, 13, " M Dollars", 1000);
                                    }
                                    else if (match.Value.Contains("trillion U.S."))
                                    {
                                        current = numberAfterParse(match, 14, " M Dollars", 1000000);
                                    }
                                    addNumberTerm(document.m_DOCNO, current, match.Index);
                                }
                            }
                            else if (regex == m_rules.rulesList[3])   //pricesCase2
                            {
                                if (match.Value.Contains("$"))
                                {
                                    current = match.Value.Substring(1);
                                    if (match.Value.Contains("Million") || match.Value.Contains("million"))
                                    {
                                        current = numberAfterParse(match, 8, " M Dollars", 1);
                                        addNumberTerm(document.m_DOCNO, current, match.Index);
                                    }
                                    else if (match.Value.Contains("Billion") || match.Value.Contains("billion"))
                                    {
                                        current = numberAfterParse(match, 8, " M Dollars", 1000);
                                        addNumberTerm(document.m_DOCNO, current, match.Index);
                                    }
                                    else if (match.Value.Contains("Trillion") || match.Value.Contains("trillion"))
                                    {
                                        current = numberAfterParse(match, 9, " M Dollars", 1000000);
                                        addNumberTerm(document.m_DOCNO, current, match.Index);
                                    }
                                    else
                                    {
                                        number = FractionToDouble(current.Replace(",", ""));
                                        if (number < 1000000)
                                        {
                                            addNumberTerm(document.m_DOCNO, current + " Dollars", match.Index);
                                        }
                                        else
                                        {
                                            addNumberTerm(document.m_DOCNO, (number / 1000000).ToString() + " M Dollars", match.Index);
                                        }
                                    }
                                }
                            }
                            else if (regex == m_rules.rulesList[4])   //datesCase1
                            {

                            }
                            else if (regex == m_rules.rulesList[5])   //datesCase2
                            {

                            }
                            else if (regex == m_rules.rulesList[6])   //rangesCase1
                            {

                            }
                            else if (regex == m_rules.rulesList[7])   //rangesCase2
                            {

                            }
                            else if (regex == m_rules.rulesList[8])  //rangesCase3
                            {

                            }
                            else if (regex == m_rules.rulesList[9])  //rangesCase4
                            {

                            }
                            else if (regex == m_rules.rulesList[10])  //namesCase
                            {

                            }
                            else if (regex == m_rules.rulesList[11])  //measureLength
                            {

                            }
                            else if (regex == m_rules.rulesList[12])   //measureMass
                            {

                            }
                            else if (regex == m_rules.rulesList[13])   //measureTime
                            {

                            }
                            else if (regex == m_rules.rulesList[14])  //measureElectric
                            {

                            }
                        }
                    }
                }
            }
        }
    }

}
