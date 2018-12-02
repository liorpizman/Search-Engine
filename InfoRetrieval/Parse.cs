using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents the Parse which splits the text into terms
    ////// </summary>
    class Parse : IParse
    {
        /// <summary>
        /// fields of Parse
        /// </summary>
        public Dictionary<string, DocumentTerms> m_allTerms;
        public Hashtable m_IndexDoc;
        public bool m_doStemming;
        private int _MAX_TF = 0;
        private int countPos = 0;
        private Document tmpDoc = null;
        public HashSet<string> m_stopWords;
        public Hashtable m_months;
        public Hashtable m_nums;
        public Hashtable m_prices;
        public Hashtable m_times;
        public Hashtable m_lengths;
        private Stemmer m_stemmer;

        /// <summary>
        /// constructor of Parse
        /// </summary>
        /// <param name="m_doStemming"></param>
        /// <param name="stopWordsPath"></param>
        public Parse(bool m_doStemming, string stopWordsPath) //
        {
            this.m_doStemming = m_doStemming;
            this.m_stemmer = new Stemmer();
            this.m_allTerms = new Dictionary<string, DocumentTerms>();
            this.m_months = new Hashtable();
            this.m_prices = new Hashtable();
            this.m_nums = new Hashtable();
            this.m_times = new Hashtable();
            this.m_lengths = new Hashtable();
            this.m_stopWords = new HashSet<string>();
            this.m_IndexDoc = new Hashtable();
            AddMonths();
            AddPrices();
            AddNums();
            AddTimes();
            AddLengths();
            AddStopWords(stopWordsPath);
        }
        /// <summary>
        /// HashTable which represents the months of a year
        /// </summary>
        public void AddMonths()
        {
            m_months.Add("jan", "01"); m_months.Add("feb", "02"); m_months.Add("mar", "03"); m_months.Add("apr", "04"); m_months.Add("may", "05");
            m_months.Add("jun", "06"); m_months.Add("jul", "07"); m_months.Add("aug", "08"); m_months.Add("sep", "09"); m_months.Add("oct", "10");
            m_months.Add("nov", "11"); m_months.Add("dec", "12"); m_months.Add("january", "01"); m_months.Add("february", "02"); m_months.Add("march", "03");
            m_months.Add("april", "04"); m_months.Add("june", "06"); m_months.Add("july", "07"); m_months.Add("august", "08"); m_months.Add("september", "09");
            m_months.Add("october", "10"); m_months.Add("november", "11"); m_months.Add("december", "12"); // m_months.Add("may", "05");
        }
        /// <summary>
        /// HashTable which represents the prices 
        /// </summary>
        public void AddPrices()
        {
            m_prices.Add("million", " M Dollars"); m_prices.Add("billion", "000 M Dollars"); m_prices.Add("trillion", "000000 M Dollars");
            m_prices.Add("m", " M Dollars"); m_prices.Add("bn", "000 M Dollars");
        }
        /// <summary>
        /// HashTable which represents the prefixes of the numbers
        /// </summary>
        public void AddNums()
        {
            m_nums.Add("million", "M"); m_nums.Add("billion", "B"); m_nums.Add("trillion", "000B"); m_nums.Add("thousand", "K");
        }
        /// <summary>
        /// HashTable which represents the time measurement
        /// </summary>
        public void AddTimes()
        {
            m_times.Add("second", "sec"); m_times.Add("seconds", "sec"); m_times.Add("minutes", "min"); m_times.Add("minute", "min");
            m_times.Add("hours", "hr"); m_times.Add("hour", "hr");
        }
        /// <summary>
        /// HashTable which represents the length measurement
        /// </summary>
        public void AddLengths()
        {
            m_lengths.Add("meters", "meter"); m_lengths.Add("meter", "meter"); m_lengths.Add("foot", "ft"); m_lengths.Add("kilometere", "km");
            m_lengths.Add("kilometers", "km"); m_lengths.Add("centimeter", "cm"); m_lengths.Add("centimeters", "cm");
        }
        /// <summary>
        /// method to add the stop words to the Parse
        /// </summary>
        /// <param name="filePath">the path of stop words</param>
        public void AddStopWords(string filePath)
        {
            string s = "";
            char[] delimitersToSplit = { '\r', '\n' };
            try
            {
                StreamReader sr = new StreamReader(filePath + "\\stop_words.txt");
                s = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception e) { Console.WriteLine("Exception-(Parse.addStopWords): " + e.Message); }
            m_stopWords = new HashSet<string>(s.Split(delimitersToSplit, StringSplitOptions.RemoveEmptyEntries));
        }
        /// <summary>
        /// method to convert a fraction to double
        /// </summary>
        /// <param name="fraction">the fraction input</param>
        /// <returns>double output</returns>
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
            //Console.WriteLine("error : " + fraction);
            return 1;
        }
        /// <summary>
        /// method to add a term to the dictionary of terms
        /// </summary>
        /// <param name="DOCNO">the id of current document</param>
        /// <param name="current">the value of current term</param>
        public void AddNewTerm(string DOCNO, string current)
        {
            if (m_allTerms.ContainsKey(current))
            {
                //m_allTerms[current].m_tfc++;
                if (m_allTerms[current].m_Terms.ContainsKey(DOCNO))
                {
                    m_allTerms[current].m_Terms[DOCNO].AddNewIndex(countPos);
                }
                else
                {
                    m_allTerms[current].m_Terms.Add(DOCNO, new Term(current, DOCNO, countPos));
                    tmpDoc.m_uniqueCounter++;
                }
                countPos++;
            }
            else
            {
                DocumentTerms documentTerms = new DocumentTerms(current);
                documentTerms.AddToDocumentDictionary(new Term(current, DOCNO, countPos));
                m_allTerms.Add(current, documentTerms);
                //m_allTerms[current].m_Terms[DOCNO].AddNewIndex(countPos);  // added for adding first position
                tmpDoc.m_uniqueCounter++;
                countPos++;
            }
            if (m_allTerms[current].m_Terms[DOCNO].m_tf > _MAX_TF)
            {
                _MAX_TF = m_allTerms[current].m_Terms[DOCNO].m_tf;
            }
        }

        /// <summary>
        /// method to add a lower case term to the dictionary of terms
        /// </summary>
        /// <param name="DOCNO">the id of current document</param>
        /// <param name="current">the value of current term</param>
        public void AddNewLowerCaseTerm(string DOCNO, string current)
        {
            string lower, upper;
            if (m_allTerms.ContainsKey(lower = current.ToLower()))
            {
                //m_allTerms[lower].m_tfc++;
                if (m_allTerms[lower].m_Terms.ContainsKey(DOCNO))
                {
                    m_allTerms[lower].m_Terms[DOCNO].AddNewIndex(countPos);
                }
                else
                {
                    m_allTerms[lower].m_Terms.Add(DOCNO, new Term(lower, DOCNO, countPos));
                    tmpDoc.m_uniqueCounter++;
                }
                countPos++;
            }
            else if (m_allTerms.ContainsKey(upper = current.ToUpper()))
            {
                //m_allTerms[upper].m_tfc++;
                if (m_allTerms[upper].m_Terms.ContainsKey(DOCNO))
                { m_allTerms[upper].m_Terms[DOCNO].AddNewIndex(countPos); }
                else
                {
                    m_allTerms[upper].m_Terms.Add(DOCNO, new Term(upper, DOCNO, countPos));
                    tmpDoc.m_uniqueCounter++;
                }
                DocumentTerms tmpDocumentTerms = m_allTerms[upper];
                tmpDocumentTerms.m_valueOfTerm = lower;
                m_allTerms.Remove(upper);
                m_allTerms.Add(lower, tmpDocumentTerms);
                countPos++;
            }
            else
            {
                DocumentTerms documentTerms = new DocumentTerms(lower);
                documentTerms.AddToDocumentDictionary(new Term(lower, DOCNO, countPos));
                m_allTerms.Add(lower, documentTerms);
                //m_allTerms[lower].m_Terms[DOCNO].AddNewIndex(countPos);  // added for adding first position
                tmpDoc.m_uniqueCounter++;
                countPos++;
            }
            if (m_allTerms[lower].m_Terms[DOCNO].m_tf > _MAX_TF)
            {
                _MAX_TF = m_allTerms[lower].m_Terms[DOCNO].m_tf;
            }
        }

        /// <summary>
        /// method to add an upper case term to the dictionary of terms
        /// </summary>
        /// <param name="DOCNO">the id of current document</param>
        /// <param name="current">the value of current term</param>
        public void AddNewUpperCaseTerm(string DOCNO, string lower)
        {
            string upper;
            if (m_allTerms.ContainsKey(lower))
            {
                //m_allTerms[lower].m_tfc++;
                if (m_allTerms[lower].m_Terms.ContainsKey(DOCNO))
                {
                    m_allTerms[lower].m_Terms[DOCNO].AddNewIndex(countPos);
                }
                else
                {
                    m_allTerms[lower].m_Terms.Add(DOCNO, new Term(lower, DOCNO, countPos));
                    tmpDoc.m_uniqueCounter++;
                }
                countPos++;
                if (m_allTerms[lower].m_Terms[DOCNO].m_tf > _MAX_TF)
                {
                    _MAX_TF = m_allTerms[lower].m_Terms[DOCNO].m_tf;
                }
            }
            else if (m_allTerms.ContainsKey(upper = lower.ToUpper()))
            {
                //m_allTerms[upper].m_tfc++;
                if (m_allTerms[upper].m_Terms.ContainsKey(DOCNO))
                {
                    m_allTerms[upper].m_Terms[DOCNO].AddNewIndex(countPos);
                }
                else
                {
                    m_allTerms[upper].m_Terms.Add(DOCNO, new Term(upper, DOCNO, countPos));
                    tmpDoc.m_uniqueCounter++;
                }
                countPos++;
                if (m_allTerms[upper].m_Terms[DOCNO].m_tf > _MAX_TF)
                {
                    _MAX_TF = m_allTerms[upper].m_Terms[DOCNO].m_tf;
                }
            }
            else
            {
                DocumentTerms documentTerms = new DocumentTerms(upper);
                documentTerms.AddToDocumentDictionary(new Term(upper, DOCNO, countPos));
                m_allTerms.Add(upper, documentTerms);
                //m_allTerms[upper].m_Terms[DOCNO].AddNewIndex(countPos);  // added for adding first position
                tmpDoc.m_uniqueCounter++;
                countPos++;
                if (m_allTerms[upper].m_Terms[DOCNO].m_tf > _MAX_TF)
                {
                    _MAX_TF = m_allTerms[upper].m_Terms[DOCNO].m_tf;
                }
            }
        }

        /// <summary>
        /// method to check whether a string is numeric
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>boolean output</returns>
        public static bool IsIntOrDouble(string str)
        {
            int num1;
            double num2;
            return int.TryParse(str, out num1) && double.TryParse(str, out num2);
        }

        /// <summary>
        /// method to check whether a string is an Integer
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>boolean output</returns>
        public static bool IsInteger(string str)
        {
            return int.TryParse(str, out int n);
        }
        /// <summary>
        /// method to check whether a string is a fraction
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>boolean output</returns>
        public static bool IsFraction(string str)
        {
            string[] nums = str.Split('/');
            if (nums.Length != 2) { return false; }
            return IsIntOrDouble(str.Replace("/", ""));
        }

        /// <summary>
        /// method to convert a value of a number to a new value with prefix
        /// </summary>
        /// <param name="currentValue">input value without prefix</param>
        /// <returns>a new value with prefix</returns>
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

        /// <summary>
        /// method to convert a value of a price to a new value with prefix
        /// </summary>
        /// <param name="currentValue">input value without prefix</param>
        /// <returns>a new value with prefix</returns>
        public string GetNumberAfterConvertToPrice(string currentValue)
        {
            double number = Convert.ToDouble(currentValue.Replace(",", ""));
            if (number < 1000000)
                return currentValue + " Dollars";
            else // (number >= 1000000)
                return (number / 1000000).ToString() + "M Dollars";
        }

        /// <summary>
        /// method to convert a value of a price to a new value with prefix
        /// </summary>
        /// <param name="numeric">numeric string input</param>
        /// <param name="fraction">fraction string input</param>
        /// <returns>a new value with prefix</returns>
        public string GetNumFractionAfterConvertToPrice(string numeric, string fraction)
        {
            double number = Convert.ToDouble(numeric.Replace(",", ""));
            if (number < 1000000)
                return numeric + " " + fraction + " Dollars";
            else // (number >= 1000000)
                return (number / 1000000).ToString() + " " + fraction + "M Dollars";
        }

        /// <summary>
        /// method to convert a value of a price to a new value with prefix
        /// </summary>
        /// <param name="currentValue">input value without prefix</param>
        /// <returns>a new value with prefix</returns>
        public string GetFractionNumberAfterConvertToPrice(string currentValue)
        {
            double number = FractionToDouble(currentValue.Replace(",", ""));
            if (number < 1000000)
                return currentValue + " Dollars";
            else // (number >= 1000000)
                return (number / 1000000).ToString() + "M Dollars";
        }

        /// <summary>
        /// method to check whether a current term suitable to any case in the parse
        /// </summary>
        /// <param name="currValue">the value of the term</param>
        /// <returns>boolean output</returns>
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

        /// <summary>
        /// method which splits per document in the collection of the files into terms
        /// </summary>
        /// <param name="file">the path of the first file in the collection</param>
        public void ParseMasterFile(masterFile file)//
        {
            foreach (Document document in file.m_documents.Values)
            {
                ParseDocuments(document);
            }
            Console.WriteLine(file.m_fileName);
        }

        /// <summary>
        /// method which splits a document into terms and adds all the terms into the dictionary
        /// </summary>
        /// <param name="document">the current document</param>
        public void ParseDocuments(Document document)
        {
            string currValue = "", numValue = "", stemmedValue = "", firstVal = "", secondVal = "", currDOCNO = document.m_DOCNO.Trim(' ');
            char[] delimiterChars = { ' ', '\n' };
            char[] toDelete = { ',', '.', '{', '}', '(', ')', '[', ']', '-', ';', ':', '~', '|', '\\', '"', '?', '!', '@', '\'', '*', '`', '&', '□', '_', '+', '>', '<', '#' }; // add all trim delimiters
            string[] removeChars = new string[] { "?", "@" };
            string[] tokens = tokens = document.m_TEXT.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            string[] splittedNums, splittedWords;
            int tokensSize = tokens.Length;
            _MAX_TF = 0;
            countPos = 0;
            tmpDoc = document;
            for (int tokIndex = 0; tokIndex < tokensSize; tokIndex++)
            {
                currValue = tokens[tokIndex] = tokens[tokIndex].Trim(toDelete).Replace("\"", "").Replace("&", "").Replace("#", "").Replace("!", "").Replace("?", "").Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "").Replace("--", "-").Replace("|", "").Replace("*", "");
                if (currValue.Length >= 2)
                {
                    if (m_stopWords.Contains(currValue.ToLower()) && !string.Equals(currValue, "between", StringComparison.OrdinalIgnoreCase) && !string.Equals(currValue, "may", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (currValue.Contains('-'))
                    {
                        splittedNums = currValue.Split('-');
                        if (splittedNums.Length == 2)                 //number/fraction - number/fraction
                        {
                            if (IsIntOrDouble(splittedNums[0]))
                            {
                                AddNewTerm(currDOCNO, splittedNums[0]);
                            }
                            else if (IsFraction(splittedNums[0]))
                            {
                                if (tokIndex - 1 >= 0 && IsIntOrDouble(numValue = tokens[tokIndex - 1]))
                                {
                                    AddNewTerm(currDOCNO, numValue + " " + splittedNums[0]);
                                }
                            }
                            if (IsIntOrDouble(splittedNums[1]) || IsFraction(splittedNums[1]))
                            {
                                AddNewTerm(currDOCNO, splittedNums[1]);
                            }
                            AddNewTerm(currDOCNO, currValue);
                            continue;
                        }
                    }
                    if (currValue.Contains('/'))
                    {
                        splittedWords = currValue.Split('/');
                        foreach (string word in splittedWords)
                        {
                            if (word.Length > 1)
                            {
                                if (char.IsUpper(word[0]))
                                {
                                    if (m_doStemming)
                                    {
                                        lock (m_stemmer)
                                        {
                                            stemmedValue = m_stemmer.stemTerm(word.ToLower());
                                        }
                                        AddNewUpperCaseTerm(currDOCNO, stemmedValue);
                                    }
                                    else
                                    {
                                        AddNewUpperCaseTerm(currDOCNO, word.ToLower());
                                    }
                                }
                                else
                                {
                                    if (m_doStemming)
                                    {
                                        lock (m_stemmer)
                                        {
                                            stemmedValue = m_stemmer.stemTerm(word);
                                        }
                                        AddNewLowerCaseTerm(currDOCNO, stemmedValue);
                                    }
                                    else
                                    {
                                        AddNewLowerCaseTerm(currDOCNO, word);
                                    }
                                }
                            }
                        }
                        continue;
                    }
                    if (char.IsUpper(currValue[0]))
                    {
                        if (m_doStemming)
                        {
                            lock (m_stemmer)
                            {
                                stemmedValue = m_stemmer.stemTerm(currValue.ToLower());
                            }
                            AddNewUpperCaseTerm(currDOCNO, stemmedValue);
                        }
                        else
                        {
                            AddNewUpperCaseTerm(currDOCNO, currValue.ToLower());
                        }
                        if (!SuitableToAnyCase(currValue))
                        { continue; }
                    } ///////////////// end of upper case
                    if (IsIntOrDouble(currValue))
                    {
                        if (tokIndex + 1 < tokensSize && !m_nums.Contains(tokens[tokIndex + 1].ToLower()))
                        {
                            AddNewTerm(currDOCNO, getNumberAfterConvertToTerm(currValue));
                        }
                        else if ((tokIndex + 1) == tokensSize)
                        {
                            AddNewTerm(currDOCNO, getNumberAfterConvertToTerm(currValue));
                        }
                    } // end of number case
                    else if (string.Equals(currValue, "percent", StringComparison.OrdinalIgnoreCase) || string.Equals(currValue, "percentage", StringComparison.OrdinalIgnoreCase))
                    {
                        if (tokIndex - 1 >= 0 && IsIntOrDouble(numValue = tokens[tokIndex - 1]))
                        {
                            AddNewTerm(currDOCNO, numValue + "%");
                        }
                    } ///////////////// end of percent case
                    else if (string.Equals(currValue, "dollars", StringComparison.OrdinalIgnoreCase))
                    {
                        if (tokIndex - 1 >= 0)
                        {
                            if (IsIntOrDouble(tokens[tokIndex - 1]))  //numeric
                            {
                                AddNewTerm(currDOCNO, GetNumberAfterConvertToPrice(tokens[tokIndex - 1]));
                            }
                            else
                            {
                                if (IsFraction(tokens[tokIndex - 1]))
                                {
                                    if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
                                    {
                                        AddNewTerm(currDOCNO, GetNumFractionAfterConvertToPrice(tokens[tokIndex - 2], tokens[tokIndex - 1]));
                                    }
                                }
                                else if (tokens[tokIndex - 1].Equals("m"))
                                {
                                    if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
                                    {
                                        AddNewTerm(currDOCNO, tokens[tokIndex - 2] + m_prices["m"]);
                                    }
                                }
                                else if (tokens[tokIndex - 1].Equals("bn"))
                                {
                                    if (tokIndex - 2 >= 0 && IsIntOrDouble(tokens[tokIndex - 2]))  //numeric
                                    {
                                        AddNewTerm(currDOCNO, tokens[tokIndex - 2] + m_prices["bn"]);
                                    }
                                }
                            }
                        }
                    }///////////////// end of Dollars case
                    else if (currValue[0] == '$')
                    {
                        currValue = currValue.Replace("$", "");
                        if (IsIntOrDouble(currValue))
                        {
                            if (tokIndex + 1 < tokensSize)
                            {
                                if (string.Equals(tokens[tokIndex + 1], "million", StringComparison.OrdinalIgnoreCase))
                                {
                                    AddNewTerm(currDOCNO, currValue + m_prices["million"]);
                                }
                                else if (string.Equals(tokens[tokIndex + 1], "billion", StringComparison.OrdinalIgnoreCase))
                                {
                                    AddNewTerm(currDOCNO, currValue + m_prices["billion"]);
                                }
                                else if (string.Equals(tokens[tokIndex + 1], "trillion", StringComparison.OrdinalIgnoreCase))
                                {
                                    AddNewTerm(currDOCNO, currValue + m_prices["trillion"]);
                                }
                                else
                                {
                                    AddNewTerm(currDOCNO, GetNumberAfterConvertToPrice(currValue));
                                }
                            }
                            else
                            {
                                AddNewTerm(currDOCNO, GetNumberAfterConvertToPrice(currValue));
                            }
                        }
                    }///////////////// end of $ case
                    else if (IsFraction(currValue))
                    {
                        if (tokIndex - 1 < 0 || !IsInteger(tokens[tokIndex - 1])) // the previous term is not numeric
                        {
                            AddNewTerm(currDOCNO, currValue);
                        }
                        else
                        {
                            AddNewTerm(currDOCNO, tokens[tokIndex - 1].Replace(",", "") + " " + currValue);
                        }
                    } // end of fraction case
                    else if (m_months.Contains(currValue))
                    {
                        int num;
                        if (tokIndex + 1 < tokensSize && IsInteger(numValue = tokens[tokIndex + 1]))
                        {
                            num = Int32.Parse(numValue);
                            if (num >= 32)
                            {
                                AddNewTerm(currDOCNO, num + "-" + m_months[currValue]);
                            }
                            else
                            {
                                if (num < 10) { numValue = "0" + num; }
                                AddNewTerm(currDOCNO, m_months[currValue] + "-" + num);
                            }
                        }
                        if (tokIndex - 1 >= 0 && IsInteger(numValue = tokens[tokIndex - 1]))
                        {
                            num = Int32.Parse(numValue);
                            if (num >= 32)
                            {
                                AddNewTerm(currDOCNO, num + "-" + m_months[currValue]);
                            }
                            else
                            {
                                if (num < 10) { numValue = "0" + num; }
                                AddNewTerm(currDOCNO, m_months[currValue] + "-" + num);
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
                                AddNewTerm(currDOCNO, numValue + m_nums["million"]);
                                break;
                            case "B":
                                AddNewTerm(currDOCNO, numValue + m_nums["billion"]);
                                break;
                            case "K":
                                AddNewTerm(currDOCNO, numValue + m_nums["thousand"]);
                                break;
                            case "000B":
                                AddNewTerm(currDOCNO, numValue + m_nums["trillion"]);
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
                                    AddNewTerm(currDOCNO, tokens[tokIndex - 2] + currValue);
                                }
                            }
                        }
                    }///////////////// end of u.s. dollars case
                    else if (tokIndex + 3 < tokensSize && string.Equals(currValue, "between", StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsIntOrDouble(firstVal = tokens[tokIndex + 1].Trim(toDelete)))
                        {
                            if (string.Equals(tokens[tokIndex + 2], "and", StringComparison.OrdinalIgnoreCase))
                            {
                                if (IsIntOrDouble(secondVal = tokens[tokIndex + 3].Trim(toDelete)))
                                {
                                    AddNewTerm(currDOCNO, "between " + firstVal + " and " + secondVal);
                                }
                            }
                        }
                    }///////////////// end of between case
                    else if (m_times.Contains(currValue))
                    {
                        if (tokIndex - 1 >= 0 && IsIntOrDouble(tokens[tokIndex - 1]))
                        {
                            AddNewTerm(currDOCNO, tokens[tokIndex - 1] + " " + m_times[currValue]);
                        }
                    }///////////////// end of times case  --------------------------- our Rule ------------------------------
                    else if (m_lengths.Contains(currValue))
                    {
                        if (tokIndex - 1 >= 0 && IsIntOrDouble(tokens[tokIndex - 1]))
                        {
                            AddNewTerm(currDOCNO, tokens[tokIndex - 1] + " " + m_lengths[currValue]);
                        }
                    }///////////////// end of lengths case  --------------------------- our Rule ------------------------------
                    else //of all cases
                    {
                        if (m_doStemming)
                        {
                            lock (m_stemmer)
                            {
                                stemmedValue = m_stemmer.stemTerm(currValue);
                            }
                            AddNewLowerCaseTerm(currDOCNO, stemmedValue);
                        }
                        else
                        {
                            AddNewLowerCaseTerm(currDOCNO, currValue);
                        }
                    } // end of all cases
                } // end of the if length >=2
            } // end of for loop 
            tmpDoc.m_maxTF = _MAX_TF;//document.m_maxTF = _MAX_TF;
            m_IndexDoc.Add(currDOCNO, new IndexDoc(_MAX_TF, tmpDoc.m_uniqueCounter, tmpDoc.m_CITY));
        } // end of ParseDocuments function
    }
}

