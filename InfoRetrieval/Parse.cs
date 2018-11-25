using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Parse
    {
        private bool m_doStemming;
        public HashSet<string> m_stopWords;
        public Dictionary<string, DocumentTerms> m_allTerms;
        public Dictionary<string, DocumentTerms> m_UpperCaseLetter;
        public Hashtable m_months;
        public Hashtable m_nums;
        public Hashtable m_prices;
        private Stemmer m_stemmer;

        // for test run time
        public static int _count_BigLetter = 0;
        public static int _count_percentage = 0;
        public static int _count_Dollars = 0;
        public static int _count_signDollar = 0;
        public static int _count_us = 0;
        public static int _count_month = 0;
        public static int _count_between = 0;
        public static int _count_thousand = 0;
        public static int _count_million = 0;
        public static int _count_billion = 0;
        public static int _count_trillion = 0;
        public static int _count_number = 0;
        public static int _count_fraction = 0;
        public static int _count_else = 0;

        public Parse(bool m_doStemming, string stopWordsPath) //
        {
            this.m_doStemming = m_doStemming;
            this.m_stemmer = new Stemmer();
            this.m_allTerms = new Dictionary<string, DocumentTerms>();
            this.m_UpperCaseLetter = new Dictionary<string, DocumentTerms>();
            this.m_months = new Hashtable();
            this.m_prices = new Hashtable();
            this.m_nums = new Hashtable();
            this.m_stopWords = new HashSet<string>();
            AddMonths();
            AddPrices();
            AddNums();
            AddStopWords(stopWordsPath);
        }

        public void AddMonths()
        {
            m_months.Add("jan", "01"); m_months.Add("feb", "02"); m_months.Add("mar", "03"); m_months.Add("apr", "04"); m_months.Add("may", "05");
            m_months.Add("jun", "06"); m_months.Add("jul", "07"); m_months.Add("aug", "08"); m_months.Add("sep", "09"); m_months.Add("oct", "10");
            m_months.Add("nov", "11"); m_months.Add("dec", "12"); m_months.Add("january", "01"); m_months.Add("february", "02"); m_months.Add("march", "03");
            m_months.Add("april", "04"); m_months.Add("june", "06"); m_months.Add("july", "07"); m_months.Add("august", "08"); m_months.Add("september", "09");
            m_months.Add("october", "10"); m_months.Add("november", "11"); m_months.Add("december", "12"); // m_months.Add("may", "05");
        }

        public void AddPrices()
        {
            m_prices.Add("million", " M Dollars"); m_prices.Add("billion", "000 M Dollars"); m_prices.Add("trillion", "000000 M Dollars");
            m_prices.Add("m", " M Dollars"); m_prices.Add("bn", "000 M Dollars");
        }

        public void AddNums()
        {
            m_nums.Add("million", "M"); m_nums.Add("billion", "B"); m_nums.Add("trillion", "000B"); m_nums.Add("thousand", "K");
        }

        public void AddStopWords(string filePath)
        {
            try
            {
                StreamReader sr = new StreamReader(filePath + "\\stop_words.txt");
                string line = sr.ReadLine();
                while (line != null)
                {
                    m_stopWords.Add(line);
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception e) { Console.WriteLine("Exception-(Parse.addStopWords): " + e.Message); }
        }

        public static double FractionToDouble(string fraction)// convert 3 4/5 ------------> 3.8
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
            Console.WriteLine("error : " + fraction);
            //throw new FormatException("Parse.FractionToDouble exception"); //---------------------------// delete when we submit the project!
            return -345678.34;
        }


        public void AddNewTerm(string DOCNO, string current, int index)
        {
            if (m_allTerms.ContainsKey(current))
            {
                m_allTerms[current].m_tfc++;
                if (m_allTerms[current].m_Terms.ContainsKey(DOCNO))
                { m_allTerms[current].m_Terms[DOCNO].AddNewIndex(index); }
                else
                { m_allTerms[current].m_Terms.Add(DOCNO, new Term(current, DOCNO)); }
            }
            else
            {
                DocumentTerms documentTerms = new DocumentTerms(current);
                documentTerms.AddToDocumentDictionary(new Term(current, DOCNO));
                m_allTerms.Add(current, documentTerms);
                m_allTerms[current].m_Terms[DOCNO].AddNewIndex(index);  // added for adding first position
            }
        }

        public void AddNewUpperCaseTerm(string DOCNO, string current, int index)
        {
            if (m_UpperCaseLetter.ContainsKey(current))
            {
                m_UpperCaseLetter[current].m_tfc++;
                if (m_UpperCaseLetter[current].m_Terms.ContainsKey(DOCNO))
                { m_UpperCaseLetter[current].m_Terms[DOCNO].AddNewIndex(index); }
                else
                { m_UpperCaseLetter[current].m_Terms.Add(DOCNO, new Term(current, DOCNO)); }
            }
            else
            {
                DocumentTerms documentTerms = new DocumentTerms(current);
                documentTerms.AddToDocumentDictionary(new Term(current, DOCNO));
                m_UpperCaseLetter.Add(current, documentTerms);
                m_UpperCaseLetter[current].m_Terms[DOCNO].AddNewIndex(index);  // added for adding first position
            }
        }

        public static bool IsIntOrDouble(string str)
        {
            int num1;
            double num2;
            return int.TryParse(str, out num1) && double.TryParse(str, out num2);
        }

        public static bool IsInteger(string str)
        {
            return int.TryParse(str, out int n);
        }

        public static bool IsFraction(string str)
        {
            string[] nums = str.Split('/');
            if (nums.Length != 2) { return false; }
            return IsIntOrDouble(str.Replace("/", ""));
        }

        public string getNumberAfterConvertToTerm(string currentValue)
        {
            string tmp = currentValue.Replace(",", "");
            double number = Convert.ToDouble(tmp);
            if (number >= 1000 && number < 1000000)
            {
                return (number / 1000).ToString() + "K";
            }
            else if (number >= 1000000 && number < 1000000000)
            {
                return (number / 1000000).ToString() + "M";
            }
            else if (number >= 1000000000)
            {
                return (number / 1000000000).ToString() + "B";
            }
            else //number under 1000
            {
                return tmp;
            }
        }

        public string GetNumberAfterConvertToPrice(string currentValue)
        {
            double number = Convert.ToDouble(currentValue.Replace(",", ""));
            if (number < 1000000)
                return currentValue + " Dollars";
            else // (number >= 1000000)
                return (number / 1000000).ToString() + "M Dollars";
        }

        public string GetNumFractionAfterConvertToPrice(string numeric, string fraction)
        {
            double number = Convert.ToDouble(numeric.Replace(",", ""));
            if (number < 1000000)
                return numeric + " " + fraction + " Dollars";
            else // (number >= 1000000)
                return (number / 1000000).ToString() + " " + fraction + "M Dollars";
        }

        public string GetFractionNumberAfterConvertToPrice(string currentValue)
        {
            double number = FractionToDouble(currentValue.Replace(",", ""));
            if (number < 1000000)
                return currentValue + " Dollars";
            else // (number >= 1000000)
                return (number / 1000000).ToString() + "M Dollars";
        }

        public void ParseMasterFile(masterFile file)
        {
            foreach (Document document in file.m_documents.Values)
            {
                ParseDocuments(document);
            }
        }

        public bool SuitableToAnyCase(string currValue)
        {
            if (string.Equals(currValue, "u.s", StringComparison.OrdinalIgnoreCase) || string.Equals(currValue, "dollars", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(currValue, "between", StringComparison.OrdinalIgnoreCase) || string.Equals(currValue, "thousand", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(currValue, "million", StringComparison.OrdinalIgnoreCase) || string.Equals(currValue, "trillion", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(currValue, "billion", StringComparison.OrdinalIgnoreCase) || m_months.Contains(currValue.ToLower()))
            {
                return true;
            }
            return false;
        }

        public void ParseDocuments(Document document)
        {
            string currValue = "", numValue = "", stemmedValue = "", firstVal = "", secondVal = "", currDOCNO = document.m_DOCNO;
            StringBuilder firstTerm = new StringBuilder();
            StringBuilder secondTerm = new StringBuilder();
            StringBuilder thirdTerm = new StringBuilder();
            char[] delimiterChars = { ' ', '\n' };
            char[] toDelete = { ',', '.', '{', '}', '(', ')', '[', ']', '-', ';', ':', '~', '|', '\\', '"' };
            string[] tokens = tokens = document.m_TEXT.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            string[] splittedNums;
            int countPos = -1, tokensSize = tokens.Length;
            for (int tokIndex = 0; tokIndex < tokensSize; tokIndex++)
            {
                currValue = tokens[tokIndex] = tokens[tokIndex].Trim(toDelete);     // must be before the check of length
                if (currValue.Length >= 2)
                {
                    if (m_stopWords.Contains(currValue.ToLower()) && !string.Equals(currValue, "between", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    countPos++; // must be after all continue commands
                    if (currValue.Contains('-'))
                    {
                        splittedNums = currValue.Split('-');
                        if (splittedNums.Length == 2)                 //number/fraction - number/fraction
                        {
                            if (IsIntOrDouble(splittedNums[0]))
                            {
                                AddNewTerm(currDOCNO, splittedNums[0], countPos);
                            }
                            else if (IsFraction(splittedNums[0]))
                            {
                                if (tokIndex - 1 >= 0 && IsIntOrDouble(numValue = tokens[tokIndex - 1]))
                                {
                                    AddNewTerm(currDOCNO, numValue + " " + splittedNums[0], countPos);
                                }
                            }
                            if (IsIntOrDouble(splittedNums[1]) || IsFraction(splittedNums[1]))
                            {
                                AddNewTerm(currDOCNO, splittedNums[1], countPos);
                            }
                            AddNewTerm(currDOCNO, currValue, countPos);
                            continue;
                        }
                    }
                    if (char.IsUpper(currValue[0]))
                    {
                        if (m_doStemming)
                        {
                            stemmedValue = m_stemmer.stemTerm(currValue.ToUpper());
                            AddNewUpperCaseTerm(currDOCNO, stemmedValue, countPos);
                        }
                        else
                        {
                            AddNewUpperCaseTerm(currDOCNO, currValue.ToUpper(), countPos);
                        }
                        //_count_BigLetter++;
                        //m_UpperCaseLetter.Add(currValue.ToUpper());                        // there are not duplicates of strings here!
                        if (!SuitableToAnyCase(currValue))
                        { continue; }
                    } ///////////////// end of upper case
                    if (IsIntOrDouble(currValue))
                    {
                        //_count_number++;
                        if (tokIndex + 1 < tokensSize && !m_nums.Contains(tokens[tokIndex + 1].ToLower()))
                        {
                            AddNewTerm(currDOCNO, getNumberAfterConvertToTerm(currValue), countPos);
                        }
                        else if ((tokIndex + 1) == tokensSize)
                        {
                            AddNewTerm(currDOCNO, getNumberAfterConvertToTerm(currValue), countPos);
                        }
                    } // end of number case
                    else if (string.Equals(currValue, "percent", StringComparison.OrdinalIgnoreCase) || string.Equals(currValue, "percentage", StringComparison.OrdinalIgnoreCase))
                    {
                        //_count_percentage++;
                        if (tokIndex - 1 >= 0 && IsIntOrDouble(numValue = tokens[tokIndex - 1]))
                        {
                            AddNewTerm(currDOCNO, numValue + "%", countPos);
                        }
                    } ///////////////// end of percent case
                    else if (string.Equals(currValue, "dollars", StringComparison.OrdinalIgnoreCase))//currValue.Equals("Dollars"))
                    {
                        //_count_Dollars++;
                        if (tokIndex - 1 >= 0)
                        {
                            if (IsIntOrDouble(tokens[tokIndex - 1]))  //numeric
                            {
                                AddNewTerm(currDOCNO, GetNumberAfterConvertToPrice(tokens[tokIndex - 1]), countPos);
                            }
                            else
                            {
                                if (IsFraction(tokens[tokIndex - 1]))
                                {
                                    if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
                                    {
                                        AddNewTerm(currDOCNO, GetNumFractionAfterConvertToPrice(tokens[tokIndex - 2], tokens[tokIndex - 1]), countPos);
                                    }
                                }
                                else if (tokens[tokIndex - 1].Equals("m"))
                                {
                                    if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
                                    {
                                        AddNewTerm(currDOCNO, tokens[tokIndex - 2] + m_prices["m"], countPos);
                                    }
                                }
                                else if (tokens[tokIndex - 1].Equals("bn"))
                                {
                                    if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
                                    {
                                        AddNewTerm(currDOCNO, tokens[tokIndex - 2] + m_prices["bn"], countPos);
                                    }
                                }
                            }
                        }
                    }///////////////// end of Dollars case
                    else if (currValue[0] == '$')
                    {
                        //_count_signDollar++;
                        currValue = currValue.Replace("$", "");
                        if (IsIntOrDouble(currValue))
                        {
                            if (tokIndex + 1 < tokensSize)
                            {
                                if (string.Equals(tokens[tokIndex + 1], "million", StringComparison.OrdinalIgnoreCase))
                                {
                                    AddNewTerm(currDOCNO, currValue + m_prices["million"], countPos);
                                }
                                else if (string.Equals(tokens[tokIndex + 1], "billion", StringComparison.OrdinalIgnoreCase))
                                {
                                    AddNewTerm(currDOCNO, currValue + m_prices["billion"], countPos);
                                }
                                else if (string.Equals(tokens[tokIndex + 1], "trillion", StringComparison.OrdinalIgnoreCase))
                                {
                                    AddNewTerm(currDOCNO, currValue + m_prices["trillion"], countPos);
                                }
                                else
                                {
                                    AddNewTerm(currDOCNO, GetNumberAfterConvertToPrice(currValue), countPos);
                                }
                            }
                            else
                            {
                                AddNewTerm(currDOCNO, GetNumberAfterConvertToPrice(currValue), countPos);
                            }
                        }
                    }///////////////// end of $ case
                    else if (IsFraction(currValue))
                    {
                        //_count_fraction++;
                        if (tokIndex - 1 < 0 || !IsInteger(tokens[tokIndex - 1])) // the previous term is not numeric
                        {
                            AddNewTerm(currDOCNO, currValue, countPos);
                        }
                        else
                        {
                            AddNewTerm(currDOCNO, tokens[tokIndex - 1].Replace(",", "") + " " + currValue, countPos);
                        }
                    } // end of fraction case
                    else if (m_months.Contains(currValue))
                    {
                        //_count_month++;
                        int num;
                        if (tokIndex + 1 < tokensSize && IsInteger(numValue = tokens[tokIndex + 1]))
                        {
                            num = Int32.Parse(numValue);
                            if (num >= 32)
                            {
                                AddNewTerm(currDOCNO, num + "-" + m_months[currValue], countPos);
                            }
                            else
                            {
                                if (num < 10) { numValue = "0" + num; }
                                AddNewTerm(currDOCNO, m_months[currValue] + "-" + num, countPos);
                            }
                        }
                        if (tokIndex - 1 >= 0 && IsInteger(numValue = tokens[tokIndex - 1]))
                        {
                            num = Int32.Parse(numValue);
                            if (num >= 32)
                            {
                                AddNewTerm(currDOCNO, num + "-" + m_months[currValue], countPos);
                            }
                            else
                            {
                                if (num < 10) { numValue = "0" + num; }
                                AddNewTerm(currDOCNO, m_months[currValue] + "-" + num, countPos);
                            }
                        }
                    }///////////////// end of months case
                    else if (tokIndex - 1 >= 0 && m_nums.Contains(currValue = currValue.ToLower()))
                    {
                        if (IsIntOrDouble(tokens[tokIndex - 1]))
                        {
                            numValue = tokens[tokIndex - 1];
                        }
                        else if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))
                        {
                            numValue = tokens[tokIndex - 2];
                        }
                        switch (m_nums[currValue])
                        {
                            case "M":
                                AddNewTerm(currDOCNO, numValue + m_nums["million"], countPos);
                                break;
                            case "B":
                                AddNewTerm(currDOCNO, numValue + m_nums["billion"], countPos);
                                break;
                            case "K":
                                AddNewTerm(currDOCNO, numValue + m_nums["thousand"], countPos);
                                break;
                            case "000B":
                                AddNewTerm(currDOCNO, numValue + m_nums["trillion"], countPos);
                                break;
                        }
                    }///////////////// end of num+prefix case
                    else if (string.Equals(currValue, "u.s", StringComparison.OrdinalIgnoreCase))
                    {
                        //_count_us++;
                        if (tokIndex + 1 < tokensSize && tokIndex - 2 >= 0 && string.Equals(tokens[tokIndex + 1], "dollars", StringComparison.OrdinalIgnoreCase))
                        {
                            if (m_nums.Contains(currValue = tokens[tokIndex - 1].ToLower()))
                            {
                                switch (m_nums[currValue])
                                {
                                    case "M":
                                        currValue = "" + m_prices["million"];
                                        break;
                                    case "B":
                                        currValue = "" + m_prices["billion"];
                                        break;
                                    case "000B":
                                        currValue = "" + m_prices["trillion"];
                                        break;
                                }
                                if (IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
                                {
                                    AddNewTerm(currDOCNO, tokens[tokIndex - 2] + currValue, countPos);
                                }
                            }
                        }
                    }///////////////// end of u.s. dollars case
                    else if (tokIndex + 3 < tokensSize && string.Equals(currValue, "between", StringComparison.OrdinalIgnoreCase))
                    {
                        //_count_between++;
                        if (IsIntOrDouble(firstVal = tokens[tokIndex + 1].Trim(toDelete)))
                        {
                            if (string.Equals(tokens[tokIndex + 2], "and", StringComparison.OrdinalIgnoreCase))
                            {
                                if (IsIntOrDouble(secondVal = tokens[tokIndex + 3].Trim(toDelete)))
                                {
                                    AddNewTerm(currDOCNO, "between " + firstVal + " and " + secondVal, countPos);
                                }
                            }
                        }
                    }///////////////// end of between case
                    else //of all cases
                    {
                        //_count_else++;
                        if (m_doStemming)// && !currValue.Contains('-'))
                        {
                            stemmedValue = m_stemmer.stemTerm(currValue.ToLower());
                            AddNewTerm(currDOCNO, stemmedValue, countPos);
                        }
                        else
                        {
                            AddNewTerm(currDOCNO, currValue.ToLower(), countPos);
                        }
                    } // end of all cases
                } // end of the if length >=2
            } // end of for loop
        } // end of ParseDocuments function
    }
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// external if's
/*
else if (tokIndex - 1 >= 0 && string.Equals(currValue, "thousand", StringComparison.OrdinalIgnoreCase))
{
    //_count_thousand++;
    AddNewTerm(currDOCNO, tokens[tokIndex - 1] + m_nums["thousand"], countPos);
}///////////////// end of Thousand case
else if (tokIndex - 1 >= 0 && string.Equals(currValue, "million", StringComparison.OrdinalIgnoreCase))
{
    //_count_million++;
    AddNewTerm(currDOCNO, tokens[tokIndex - 1] + m_nums["million"], countPos);
}///////////////// end of Million case
else if (tokIndex - 1 >= 0 && string.Equals(currValue, "billion", StringComparison.OrdinalIgnoreCase))
{
    //_count_billion++;
    AddNewTerm(currDOCNO, tokens[tokIndex - 1] + m_nums["billion"], countPos);
}///////////////// end of Billion case
else if (tokIndex - 1 >= 0 && string.Equals(currValue, "trillion", StringComparison.OrdinalIgnoreCase))
{
    //_count_trillion++;
    AddNewTerm(currDOCNO, tokens[tokIndex - 1] + m_nums["trillion"], countPos);
}///////////////// end of Trillion case
*/

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// internal if's of u.s

/*
if (tokIndex - 1 >= 0 && string.Equals(tokens[tokIndex - 1], "million", StringComparison.OrdinalIgnoreCase))
{
if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
{
    AddNewTerm(currDOCNO, tokens[tokIndex - 2] + m_prices["million"], countPos);
}
}
else if (tokIndex - 1 >= 0 && string.Equals(tokens[tokIndex - 1], "billion", StringComparison.OrdinalIgnoreCase))
{
if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
{
    AddNewTerm(currDOCNO, tokens[tokIndex - 2] + m_prices["billion"], countPos);
}
}
else if (tokIndex - 1 >= 0 && string.Equals(tokens[tokIndex - 1], "trillion", StringComparison.OrdinalIgnoreCase))
{
if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
{
    AddNewTerm(currDOCNO, tokens[tokIndex - 2] + m_prices["trillion"], countPos);
}
}
*/
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
