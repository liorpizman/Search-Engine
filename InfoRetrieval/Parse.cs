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
                    if (line.Contains("\n"))
                        line = line.Replace("\n", "");
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
            string f = fraction;                                                                        // for test
            Console.WriteLine(fraction);                                                                // for test
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
        public string numberAfterParse(string matchValue, int toDecrease, string tail, double multiple)
        {
            string current = matchValue.Substring(0, matchValue.Length - toDecrease);
            double number = FractionToDouble(current) * multiple;
            current = number.ToString() + tail;
            return current;
        }

        public string convertDate(Match match, int indexOfMonth, bool includesYear) //indexOfMonth - means the index of the word in document
        {
            string[] splitted = match.Value.Split(' ');
            string convertedDate = "";
            if (includesYear)  // for year 1994-05
            {
                if (indexOfMonth == 0)
                {
                    convertedDate += splitted[1] + "-";
                }
                else
                {
                    convertedDate += splitted[0] + "-";
                }
            }
            if (splitted[indexOfMonth].Equals("Jan") || splitted[indexOfMonth].Equals("January") || splitted[indexOfMonth].Equals("JANUARY"))
            {
                convertedDate += "01";
            }
            else if (splitted[indexOfMonth].Equals("Feb") || splitted[indexOfMonth].Equals("February") || splitted[indexOfMonth].Equals("FEBRUARY"))
            {
                convertedDate += "02";
            }
            else if (splitted[indexOfMonth].Equals("Mar") || splitted[indexOfMonth].Equals("March") || splitted[indexOfMonth].Equals("MARCH"))
            {
                convertedDate += "03";
            }
            else if (splitted[indexOfMonth].Equals("Apr") || splitted[indexOfMonth].Equals("April") || splitted[indexOfMonth].Equals("APRIL"))
            {
                convertedDate += "04";
            }
            else if (splitted[indexOfMonth].Equals("May") || splitted[indexOfMonth].Equals("MAY"))
            {
                convertedDate += "05";
            }
            else if (splitted[indexOfMonth].Equals("Jun") || splitted[indexOfMonth].Equals("June") || splitted[indexOfMonth].Equals("JUNE"))
            {
                convertedDate += "06";
            }
            else if (splitted[indexOfMonth].Equals("Jul") || splitted[indexOfMonth].Equals("July") || splitted[indexOfMonth].Equals("JULY"))
            {
                convertedDate += "07";
            }
            else if (splitted[indexOfMonth].Equals("Aug") || splitted[indexOfMonth].Equals("August") || splitted[indexOfMonth].Equals("AUGUST"))
            {
                convertedDate += "08";
            }
            else if (splitted[indexOfMonth].Equals("Sep") || splitted[indexOfMonth].Equals("September") || splitted[indexOfMonth].Equals("SEPTEMBER"))
            {
                convertedDate += "09";
            }
            else if (splitted[indexOfMonth].Equals("Oct") || splitted[indexOfMonth].Equals("October") || splitted[indexOfMonth].Equals("OCTOBER"))
            {
                convertedDate += "10";
            }
            else if (splitted[indexOfMonth].Equals("Nov") || splitted[indexOfMonth].Equals("November") || splitted[indexOfMonth].Equals("NOVEMBER"))
            {
                convertedDate += "11";
            }
            else if (splitted[indexOfMonth].Equals("Dec") || splitted[indexOfMonth].Equals("December") || splitted[indexOfMonth].Equals("DECEMBER"))
            {
                convertedDate += "12";
            }
            if (!includesYear)  // for month 05-14
            {
                if (indexOfMonth == 0)
                {
                    convertedDate += "-" + splitted[1];
                }
                else
                {
                    convertedDate += "-" + splitted[0];
                }
            }
            return convertedDate;
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

        public string convertNumberToTerm(string matchValue, string current, string DOCNO, double number, int matchIndex)
        {
            matchValue = matchValue.ToLower();
            if (matchValue.Contains("Thousand") || matchValue.Contains("thousand"))        // can remove upper case First Case
            {
                current = numberAfterParse(matchValue, 9, "K", 1);
                addNumberTerm(DOCNO, current, matchIndex);
                return current;
            } 
            else if (matchValue.Contains("Million") || matchValue.Contains("million"))      // can remove upper case First Case
            {
                current = numberAfterParse(matchValue, 8, "M", 1);
                addNumberTerm(DOCNO, current, matchIndex);
                return current;
            }
            else if (matchValue.Contains("Billion") || matchValue.Contains("billion"))       // can remove upper case First Case
            {
                current = numberAfterParse(matchValue, 8, "B", 1);
                addNumberTerm(DOCNO, current, matchIndex);
                return current;
            }
            else if (matchValue.Contains("Trillion") || matchValue.Contains("trillion"))       // can remove upper case First Case
            {
                current = numberAfterParse(matchValue, 9, "B", 1000);
                addNumberTerm(DOCNO, current, matchIndex);
                return current;
            }
            number = FractionToDouble(matchValue.Replace(",", ""));
            if (number >= 1000 && number < 1000000)
            {
                addNumberTerm(DOCNO, (number / 1000).ToString() + "K", matchIndex);
                return (number / 1000).ToString() + "K";
            }
            else if (number >= 1000000 && number < 1000000000)
            {
                addNumberTerm(DOCNO, (number / 1000000).ToString() + "M", matchIndex);
                return (number / 1000000).ToString() + "M";
            }
            else if (number >= 1000000000)
            {
                addNumberTerm(DOCNO, (number / 1000000000).ToString() + "B", matchIndex);
                return (number / 1000000000).ToString() + "B";
            }
            else //number under 1000
            {
                addNumberTerm(DOCNO, number.ToString(), matchIndex);
                return number.ToString();
            }
        }

        public string convertNumberForRange(string currNumber)
        {
            double number = FractionToDouble(currNumber.Replace(",", ""));
            if (number >= 1000 && number < 1000000)
                return (number / 1000).ToString() + "K";
            else if (number >= 1000000 && number < 1000000000)
                return (number / 1000000).ToString() + "M";
            else if (number >= 1000000000)
                return (number / 1000000000).ToString() + "B";
            else //number under 1000
                return number.ToString();
        }

        public static bool IsNumeric(string str)
        {
            try
            {
                str = str.Trim();
                int foo = int.Parse(str);
                return (true);
            }
            catch (FormatException)
            {
                return (false);
            }
        }

        public static bool isDouble(string str)
        {
            try
            {
                str = str.Trim();
                Convert.ToDouble(str);
                return (true);
            }
            catch (FormatException)
            {
                return (false);
            }
        }

        public void parseDocuments(masterFile file)
        {
            Stemmer stemmer = new Stemmer();
            double number = 0;
            string current = "";
            string[] splittedRange;
            foreach (Document document in file.m_documents.Values)
            {
                foreach (Regex regex in m_rules.rulesList)
                {
                    foreach (Match match in regex.Matches(document.m_TEXT))
                    {
                        if (!m_stopWords.Contains(match.Value))
                        {
                            // --------------------------------------------- > For Debug Purpose

                            if (match.Value.Contains("$100 MILLION")) //15 MILLION--According
                            {
                                Console.WriteLine(match.Value);
                            }

                            //----------------------------------------------- > For Debug Purpose
                            if (regex == m_rules.rulesList[0])        //numbersCase
                            {
                                convertNumberToTerm(match.Value, current, document.m_DOCNO, number, match.Index);
                            }
                            else if (regex == m_rules.rulesList[1])  //percentsCase
                            {
                                if (match.Value.Contains("Percents") || match.Value.Contains("percents"))
                                {
                                    current = numberAfterParse(match.Value, 9, "%", 1);
                                }
                                else if (match.Value.Contains("Percent") || match.Value.Contains("percent"))
                                {
                                    current = numberAfterParse(match.Value, 8, "%", 1);
                                }
                                else if (match.Value.Contains("Percentage") || match.Value.Contains("percentage"))
                                {
                                    current = numberAfterParse(match.Value, 11, "%", 1);
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
                                        current = numberAfterParse(match.Value, 1, " M Dollars", 1);  // if the m is close to number
                                    }
                                    else if (match.Value.Contains("million U.S."))
                                    {
                                        current = numberAfterParse(match.Value, 13, " M Dollars", 1);
                                    }
                                    else if (match.Value.Contains("bn"))
                                    {
                                        current = numberAfterParse(match.Value, 2, " M Dollars", 1000); // if the m is close to number
                                    }
                                    else if (match.Value.Contains("billion U.S."))
                                    {
                                        current = numberAfterParse(match.Value, 13, " M Dollars", 1000);
                                    }
                                    else if (match.Value.Contains("trillion U.S."))
                                    {
                                        current = numberAfterParse(match.Value, 14, " M Dollars", 1000000);
                                    }
                                    addNumberTerm(document.m_DOCNO, current, match.Index);
                                }
                            }
                            else if (regex == m_rules.rulesList[3])   //pricesCase2
                            {
                                if (match.Value.Contains("$"))
                                {
                                    current = match.Value.Substring(1);
                                    current = current.ToLower();
                                    if (current.Contains("Million") || current.Contains("million"))             // can remove upper case First Case
                                    {
                                        current = numberAfterParse(current, 8, " M Dollars", 1); // match.Value ---> current
                                        addNumberTerm(document.m_DOCNO, current, match.Index);
                                    }
                                    else if (current.Contains("Billion") || current.Contains("billion"))        // can remove upper case First Case
                                    {
                                        current = numberAfterParse(current, 8, " M Dollars", 1000); // match.Value ---> current
                                        addNumberTerm(document.m_DOCNO, current, match.Index);
                                    }
                                    else if (current.Contains("Trillion") || current.Contains("trillion"))      // can remove upper case First Case
                                    {
                                        current = numberAfterParse(current, 9, " M Dollars", 1000000); // match.Value ---> current
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
                            else if (regex == m_rules.rulesList[4] || regex == m_rules.rulesList[5])   //datesCase1 && datesCase2
                            {
                                if (regex == m_rules.rulesList[4])
                                {
                                    if (match.Value.Split(' ')[0].Length == 4) // checks whether it is year
                                    {
                                        current = convertDate(match, 1, true);
                                    }
                                    else
                                    {
                                        current = convertDate(match, 1, false);
                                    }
                                }
                                else
                                {
                                    if (match.Value.Split(' ')[0].Length == 4) // checks whether it is year
                                    {
                                        current = convertDate(match, 0, true);
                                    }
                                    else
                                    {
                                        current = convertDate(match, 0, false);
                                    }
                                }
                                addNumberTerm(document.m_DOCNO, current, match.Index);
                            }
                            else if (regex == m_rules.rulesList[6])   //rangesCase1
                            {
                                //addNumberTerm(document.m_DOCNO, match.Value, match.Index); // to save 4000-8000
                                splittedRange = match.Value.Split('-');
                                //addNumberTerm(document.m_DOCNO, splittedRange[0], match.Index); //to save 4000
                                //addNumberTerm(document.m_DOCNO, splittedRange[1], match.Index); //to save 8000

                                current = convertNumberForRange(splittedRange[0]); //4K //current = convertNumberToTerm(splittedRange[0], splittedRange[0], document.m_DOCNO, number, match.Index, false); //4K
                                current += "-";
                                current += convertNumberForRange(splittedRange[1]); //8k //current += convertNumberToTerm(splittedRange[1], splittedRange[1], document.m_DOCNO, number, match.Index, false); //8K
                                addNumberTerm(document.m_DOCNO, current, match.Index); ////4k-8k 

                            }
                            else if (regex == m_rules.rulesList[7])   //rangesCase2  word1-word2 or word1-word2-word3
                            {
                                addNumberTerm(document.m_DOCNO, match.Value, match.Index); //word1-word2 or word1-word2-word3
                                splittedRange = match.Value.Split('-');
                                addNumberTerm(document.m_DOCNO, splittedRange[0], match.Index); //word1
                                addNumberTerm(document.m_DOCNO, splittedRange[1], match.Index); //word2
                                if (splittedRange.Length == 3)
                                {
                                    addNumberTerm(document.m_DOCNO, splittedRange[2], match.Index); //word3
                                }
                            }
                            else if (regex == m_rules.rulesList[8])  //rangesCase3
                            {
                                //addNumberTerm(document.m_DOCNO, current, match.Index); //4000-word or word-4000
                                if (match.Value[0] == ' ')
                                    splittedRange= match.Value.Substring(1).Split('-'); /// correct for (space)86.3-percent
                                else
                                    splittedRange = match.Value.Split('-');
                                splittedRange[0] = splittedRange[0].Replace(",", "");
                                splittedRange[1] = splittedRange[1].Replace(",", "");

                                if (IsNumeric(splittedRange[0]) || isDouble(splittedRange[0])) ////4000-word
                                {
                                    addNumberTerm(document.m_DOCNO, splittedRange[1], match.Index); //word
                                    current = convertNumberForRange(splittedRange[0]);  //4k current = convertNumberToTerm(match.Value, splittedRange[0], document.m_DOCNO, number, match.Index, false); //4K
                                    addNumberTerm(document.m_DOCNO, current + "-" + splittedRange[1], match.Index); //4000-word

                                }
                                else  //word-4000
                                {
                                    addNumberTerm(document.m_DOCNO, splittedRange[0], match.Index); //word
                                    current = convertNumberForRange(splittedRange[1]);  //4k current = convertNumberToTerm(match.Value, splittedRange[1], document.m_DOCNO, number, match.Index, false); //4K
                                    addNumberTerm(document.m_DOCNO, splittedRange[0] + "-" + current, match.Index); //word-4000
                                }
                            }
                            else if (regex == m_rules.rulesList[9])  //rangesCase4
                            {
                                addNumberTerm(document.m_DOCNO, match.Value, match.Index); //Between 4000 and 8000
                                // check if we need to save Between 4k and 8k
                                splittedRange = match.Value.Split(' ');
                                current = convertNumberForRange(splittedRange[1]); //4K -number1 //current = convertNumberToTerm(match.Value, splittedRange[1], document.m_DOCNO, number, match.Index, false); //4K -number1
                                current += "-";
                                current += convertNumberForRange(splittedRange[3]); //8K -number2 //current += convertNumberToTerm(match.Value, splittedRange[3], document.m_DOCNO, number, match.Index, false); //8K -number2
                                //convertNumberToTerm(match.Value, current, document.m_DOCNO, number, match.Index, false); //4k-8k
                                addNumberTerm(document.m_DOCNO, current, match.Index); ////4k-8k 
                            }
                            else if (regex == m_rules.rulesList[10])  //namesCase
                            {
                                addNumberTerm(document.m_DOCNO, match.Value, match.Index); //FirstName LastName
                                splittedRange = match.Value.Split(' ');
                                addNumberTerm(document.m_DOCNO, splittedRange[0], match.Index); //FirstName 
                                addNumberTerm(document.m_DOCNO, splittedRange[1], match.Index); //LastName
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
                                addNumberTerm(document.m_DOCNO, match.Value, match.Index);
                            }
                            else if (regex == m_rules.rulesList[15])  //all words
                            {
                                addNumberTerm(document.m_DOCNO, match.Value, match.Index);
                            }
                        }
                    }
                }
            }
        }
    }

}
/* numbers case 1
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
*/
