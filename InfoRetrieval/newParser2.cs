using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Parse
    {
        public bool m_doStemming;
        public HashSet<string> m_stopWords;
        public Dictionary<string, DocumentTerms> m_allTerms;
        public Dictionary<string, string> m_months;

        public Parse(bool m_doStemming, string stopWordsPath)
        {
            this.m_doStemming = m_doStemming;
            this.m_allTerms = new Dictionary<string, DocumentTerms>();
            this.m_months = new Dictionary<string, string>();
            addMonths();
            m_stopWords = new HashSet<string>();
            addStopWords(stopWordsPath);
        }

        public void addMonths()
        {
            m_months.Add("jan", "01");
            m_months.Add("feb", "02");
            m_months.Add("mar", "03");
            m_months.Add("apr", "04");
            m_months.Add("may", "05");
            m_months.Add("jun", "06");
            m_months.Add("jul", "07");
            m_months.Add("aug", "08");
            m_months.Add("sep", "09");
            m_months.Add("oct", "10");
            m_months.Add("nov", "11");
            m_months.Add("dec", "12");
            //////////////////////////
            m_months.Add("january", "01");
            m_months.Add("february", "02");
            m_months.Add("march", "03");
            m_months.Add("april", "04");
            //m_months.Add("may", "05");
            m_months.Add("june", "06");
            m_months.Add("july", "07");
            m_months.Add("august", "08");
            m_months.Add("september", "09");
            m_months.Add("october", "10");
            m_months.Add("november", "11");
            m_months.Add("december", "12");
        }

        public void addStopWords(string filePath)
        {
            string line;
            try
            {
                //string path = Directory.GetCurrentDirectory();
                StreamReader sr = new StreamReader(filePath + "\\stop_words.txt");
                line = sr.ReadLine();
                while (line != null)
                {
                    m_stopWords.Add(line);
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception-(Parse.addStopWords): " + e.Message);
            }
        }

        public static double FractionToDouble(string fraction)// convert 3 4/5 ------------> 3.8
        {
            /*
            double result;
            string[] nums;
            string[] f;
            //if (Convert.ToDouble(fraction))
            //{
            //
           // }
            if (fraction.Contains('/'))
            {
                if (fraction.Contains(' '))
                {
                    nums = fraction.Split(' ');
                    result = (double)(Convert.ToInt32(nums[0]));
                    f = nums[1].Split('/');
                    result += ((double)(Convert.ToInt32(f[0])) / (double)(Convert.ToInt32(f[1])));
                }
                else
                {
                    f = fraction.Split('/');
                    result = ((double)(Convert.ToInt32(f[0])) / (double)(Convert.ToInt32(f[1])));
                }
            }
            else
            {
                result = (double)(Convert.ToInt32(fraction));
            }
            return result;
            */

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
            if (fraction.Equals(""))
            {
                int a = 1;
            }
            Console.WriteLine("error : " + fraction);
            //throw new FormatException("Parse.FractionToDouble exception"); //---------------------------// delete when we submit the project!
            if (fraction.Contains("3.5."))
            {
                int a = 1;
            }
            return 0;

        }


        public void addNewTerm(string DOCNO, string current, int index)
        {
            if (m_allTerms.ContainsKey(current))
            {
                m_allTerms[current].m_tfc++;
                if (m_allTerms[current].m_Terms.ContainsKey(DOCNO))
                {
                    m_allTerms[current].m_Terms[DOCNO].addNewIndex(index);
                }
                else
                {
                    m_allTerms[current].m_Terms.Add(DOCNO, new Term(current, DOCNO));
                }
            }
            else
            {
                DocumentTerms documentTerms = new DocumentTerms(current);
                documentTerms.addToDocumentDictionary(new Term(current, DOCNO));
                m_allTerms.Add(current, documentTerms);
            }
        }

        public static bool IsNumeric(string str)
        {
            return str.Replace(",", "").All(char.IsDigit);
            /*
            try
            {
                str = str.Replace(",", "").Trim();
                int foo = int.Parse(str);
                return (true);
            }
            catch (FormatException)
            {
                return (false);
            }
            */
        }



        public static bool isDouble(string str)
        {
            str = str.Replace(",", "").Replace(".", "");
            return str.All(char.IsDigit) && !str.Equals("");
            /*
            try
            {
                str = str.Replace(",", "").Trim();
                Convert.ToDouble(str);
                return (true);
            }
            catch (FormatException)
            {
                return (false);
            }
            */
        }

        public static bool isFraction(string str)
        {
            if (!str.Contains("/"))
            {
                return false;
            }
            str = str.Replace("/", "");
            return IsNumeric(str) || isDouble(str);
        }

        public string getNumberAfterConvertToTerm(string s)
        {
            string tmp = s.Replace(",", "");
            double number = FractionToDouble(tmp);
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


        public string getNumberAfterConvertToPrice(string currentValue)   // it is removes ',' from number <1000000
        {
            double number = FractionToDouble(currentValue.Replace(",", ""));
            if (number < 1000000)
                return currentValue + " Dollars";
            else if (number >= 1000000)
                return (number / 1000000).ToString() + "M Dollars";
            else
                return number.ToString() + " Dollars";
        }

        public string removeWhiteSpaces(string str)
        {
            return str.Replace(",", "");
        }

        public static bool isAvalidWord(string input)
        {
            return input.All(char.IsLetter);
        }

        public string convertNumberBeforeBillion(string num)
        {
            return (Convert.ToDouble(num.Replace(",", "")) * 1000).ToString();
        }

        public string convertNumberBeforeTrillion(string num)
        {
            return (Convert.ToDouble(num.Replace(",", "")) * 1000000).ToString();
        }


        public void parseDocuments(masterFile file)
        {
            Stemmer stemmer = new Stemmer();
            string currentValue = "", nextValue = "", thirdValue = "", fourthValue = "";
            string[] words;
            StringBuilder firstTerm = new StringBuilder();
            StringBuilder secondTerm = new StringBuilder();
            StringBuilder thirdTerm = new StringBuilder();
            double number = 0;
            string[] tokens;
            int countPos = -1;
            char[] delimiterChars = { ' ', '\n', ':', '\t', '{', '}', '(', ')', '[', ']', '!', '@', '#', '^', '&', '*', '+', '=', '_', '?', ';', '~', '"', '`', '<', '>' };//, "'" };
            foreach (Document document in file.m_documents.Values)
            {
                //m_tmpDictionary = new Dictionary<string, int>();
                tokens = document.m_TEXT.ToString().Replace("--", " ").Split(delimiterChars);//Split(' ');delimiterChars
                //var watch = System.Diagnostics.Stopwatch.StartNew();
                for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
                {
                    if (tokens[tokenIndex].Equals("") || tokens[tokenIndex].Equals(",") || tokens[tokenIndex].Equals("."))
                    {
                        continue;
                    }
                    if (!tokens[tokenIndex].ToLower().Contains("between") && m_stopWords.Contains(tokens[tokenIndex]))
                    {
                        continue;
                    }
                    if (tokens[tokenIndex][0] == '-' && !(tokens[tokenIndex].Equals("--")))
                    {
                        tokens[tokenIndex] = tokens[tokenIndex].Substring(1);
                    }
                    //tokens[tokenIndex] = tokens[tokenIndex].Replace("..", "");
                    if (tokens[tokenIndex].Length > 1)
                    {
                        if (tokens[tokenIndex][tokens[tokenIndex].Length - 1] == '-' && !(tokens[tokenIndex].Equals("--")))//|| tokens[tokenIndex][tokens[tokenIndex].Length - 1] == '.')
                        {
                            tokens[tokenIndex] = tokens[tokenIndex].Substring(0, tokens[tokenIndex].Length - 1);
                        }
                    }
                    /*)
                    Console.WriteLine(tokenIndex + "   :" + tokens[tokenIndex]);
                    if (tokens[tokenIndex].Contains("-733-6534"))
                    {
                        Console.WriteLine("-----");
                    }
                    */
                    currentValue = tokens[tokenIndex];
                    countPos++;
                    if (isAvalidWord(currentValue = removeWhiteSpaces(currentValue)))                           // case all words - just simple letters
                    {
                        if (m_doStemming)
                        {
                            currentValue = stemmer.stemTerm(currentValue.ToLower());
                        }
                        addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                    }///////////////////////////////////////////////////////////////////////////////////////////////////first is a number
                    else if (IsNumeric(currentValue) || isDouble(currentValue))
                    {
                        if ((tokenIndex + 1) < tokens.Length) // we have 2nd index
                        {
                            nextValue = tokens[++tokenIndex].ToLower();
                            nextValue = removeWhiteSpaces(nextValue);
                            if (isFraction(nextValue))
                            {
                                if ((tokenIndex + 1) < tokens.Length) // we have 3th 
                                {
                                    thirdValue = tokens[++tokenIndex].ToLower();
                                    thirdValue = removeWhiteSpaces(thirdValue);
                                    if (thirdValue.Equals("dollars"))
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), currentValue + " " + nextValue + " Dollars", countPos);
                                    }
                                    else
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), currentValue + " " + nextValue, countPos);  // (2 3/4 ) number + fraction
                                        tokenIndex--;
                                    }
                                }
                                else
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue + " " + nextValue, countPos);  // (2 3/4 ) number + fraction
                                }
                            }
                            else if (nextValue.Equals("dollars"))
                            {
                                currentValue = getNumberAfterConvertToPrice(currentValue);
                                addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                            }
                            else if (nextValue.Equals("m"))
                            {
                                if ((tokenIndex + 1) < tokens.Length) // we have 3th 
                                {
                                    thirdValue = tokens[++tokenIndex].ToLower();
                                    thirdValue = removeWhiteSpaces(thirdValue);
                                    if (thirdValue.Equals("dollars"))
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), currentValue + " M Dollars", countPos);
                                    }
                                    else
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), currentValue + "m", countPos);  // number+meters 40m
                                        tokenIndex--;
                                    }
                                }
                                else
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue + "m", countPos);  // number+meters 40m
                                }
                            }
                            else if (nextValue.Equals("bn"))
                            {
                                if ((tokenIndex + 1) < tokens.Length) // we have 3th 
                                {
                                    thirdValue = tokens[++tokenIndex].ToLower();
                                    thirdValue = removeWhiteSpaces(thirdValue);
                                    if (thirdValue.Equals("dollars"))
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), convertNumberBeforeBillion(currentValue) + " M Dollars", countPos);
                                    }
                                    else
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);  // just the number 40
                                        tokenIndex--;
                                    }
                                }
                                else
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);  // just the number 40
                                }
                            }
                            else if (nextValue.Equals("thousand"))
                            {
                                addNewTerm(document.m_DOCNO.ToString(), currentValue + "K", countPos);
                            }
                            else if (nextValue.Equals("million"))
                            {
                                if ((tokenIndex + 1) < tokens.Length && (tokenIndex + 2) < tokens.Length) // we have 3th and 4th index
                                {
                                    thirdValue = tokens[++tokenIndex].ToLower();
                                    thirdValue = removeWhiteSpaces(thirdValue);
                                    fourthValue = tokens[++tokenIndex].ToLower();
                                    fourthValue = removeWhiteSpaces(fourthValue);
                                    if (thirdValue.Equals("u.s.") && fourthValue.Equals("dollars"))
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), currentValue + " M Dollars", countPos);
                                    }
                                    else
                                    {
                                        addNewTerm(document.m_DOCNO.ToString().ToString(), currentValue + "M", countPos);
                                        tokenIndex = tokenIndex - 2;
                                    }
                                }
                                else
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue + "M", countPos);
                                }
                            }
                            else if (nextValue.Equals("billion"))
                            {
                                if ((tokenIndex + 1) < tokens.Length && (tokenIndex + 2) < tokens.Length) // we have 3th and 4th index
                                {
                                    thirdValue = tokens[++tokenIndex].ToLower();
                                    thirdValue = removeWhiteSpaces(thirdValue);
                                    fourthValue = tokens[++tokenIndex].ToLower();
                                    fourthValue = removeWhiteSpaces(fourthValue);
                                    if (thirdValue.Equals("u.s.") && fourthValue.Equals("dollars"))
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), convertNumberBeforeBillion(currentValue) + " M Dollars", countPos);
                                    }
                                    else
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), currentValue + "B", countPos);
                                        tokenIndex = tokenIndex - 2;
                                    }
                                }
                                else
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue + "B", countPos);
                                }
                            }
                            else if (nextValue.Equals("trillion"))
                            {
                                if ((tokenIndex + 1) < tokens.Length && (tokenIndex + 2) < tokens.Length) // we have 3th and 4th index
                                {
                                    thirdValue = tokens[++tokenIndex].ToLower();
                                    thirdValue = removeWhiteSpaces(thirdValue);
                                    fourthValue = tokens[++tokenIndex].ToLower();
                                    fourthValue = removeWhiteSpaces(fourthValue);
                                    if (thirdValue.Equals("u.s.") && fourthValue.Equals("dollars"))
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), convertNumberBeforeTrillion(currentValue) + " M Dollars", countPos);
                                    }
                                    else
                                    {
                                        addNewTerm(document.m_DOCNO.ToString(), convertNumberBeforeBillion(currentValue) + "B", countPos);
                                        tokenIndex = tokenIndex - 2;
                                    }
                                }
                                else
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), convertNumberBeforeBillion(currentValue) + "B", countPos);
                                }
                            }
                            else if (nextValue.Equals("percent"))
                            {
                                addNewTerm(document.m_DOCNO.ToString(), currentValue + "%", countPos);
                            }
                            else if (nextValue.Equals("percentage"))
                            {
                                addNewTerm(document.m_DOCNO.ToString(), currentValue + "%", countPos);
                            }
                            else if (nextValue.Equals("percents"))
                            {
                                addNewTerm(document.m_DOCNO.ToString(), currentValue + "%", countPos);
                            }
                            else if (m_months.ContainsKey(nextValue))                    // case:  number_months
                            {
                                currentValue = m_months[nextValue] + "-" + currentValue;
                                addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                            }
                            else //here we should add here simple numbers without prefix
                            {
                                currentValue = getNumberAfterConvertToTerm(currentValue);
                                addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                                tokenIndex--;
                            }
                        }
                        else                                                      // just a number
                        {
                            ////////////---------------------- here all cases 1k 1m 1b big function of all numbers
                            currentValue = getNumberAfterConvertToTerm(currentValue);
                            addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                        }
                    }/////////////////////////////////////////////////////////////////////////////////////////////////////     
                    else if (currentValue[0] == '$')// when $ first char we cant remove "," from numbers!!!!
                    {
                        currentValue = tokens[tokenIndex].Substring(1);
                        if (IsNumeric(currentValue) || isDouble(currentValue))
                        {
                            if ((tokenIndex + 1) < tokens.Length) // we have 2nd index
                            {
                                nextValue = tokens[++tokenIndex].ToLower();
                                nextValue = removeWhiteSpaces(nextValue);
                                if (nextValue.Equals("million"))
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue + " M Dollars", countPos);
                                }
                                else if (nextValue.Equals("billion"))
                                {
                                    currentValue = convertNumberBeforeBillion(currentValue);
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue + " M Dollars", countPos);
                                }
                                else if (nextValue.Equals("trillion"))
                                {
                                    currentValue = convertNumberBeforeTrillion(currentValue);
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue + " M Dollars", countPos);
                                }
                                else // case $price+anyOtherWord
                                {
                                    currentValue = getNumberAfterConvertToPrice(currentValue);
                                    addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                                    tokenIndex--;
                                }
                            }
                            else                                                      // just a $number
                            {
                                ////////////---------------------- here  a case $450,000 is the last term in doc
                                currentValue = getNumberAfterConvertToPrice(currentValue);
                                addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                            }
                        }
                        else                                      // any word with $ at first char and next chars are not digits
                        {
                            addNewTerm(document.m_DOCNO.ToString(), tokens[tokenIndex], countPos);
                        }
                    }/////////////////////////////////////////////////////////////////////////////////////////////////////
                    else if (currentValue[currentValue.Length - 1] == '%')
                    {
                        currentValue = tokens[tokenIndex].Substring(1, tokens[tokenIndex].Length - 1);
                        if (IsNumeric(currentValue) || isDouble(currentValue))                            //case 6%
                        {
                            addNewTerm(document.m_DOCNO.ToString(), tokens[tokenIndex], countPos);
                        }
                        else                                                                               // case abc%
                        {
                            addNewTerm(document.m_DOCNO.ToString(), tokens[tokenIndex], countPos);
                        }
                    }/////////////////////////////////////////////////////////////////////////////////////////////////////
                    else if (m_months.ContainsKey(currentValue.ToLower()))
                    {
                        if ((tokenIndex + 1) < tokens.Length) // we have 2nd index
                        {
                            nextValue = tokens[++tokenIndex].ToLower();
                            nextValue = removeWhiteSpaces(nextValue);
                            if (IsNumeric(nextValue) || isDouble(nextValue))
                            {
                                if ((Convert.ToDouble(nextValue)) <= 31.0)
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), m_months[currentValue] + "-" + nextValue, countPos);
                                }
                                else
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), nextValue + "-" + m_months[currentValue], countPos);
                                }
                            }
                            else
                            {
                                tokenIndex--;
                                addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                            }
                        }
                        else              //last word in doc is a month
                        {
                            addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                        }
                    }/////////////////////////////////////////////////////////////////////////////////////////////////////
                    else if (currentValue.Contains("-") && !currentValue.Contains("--"))
                    {
                        words = currentValue.Split('-');
                        //////////////////////////////////added now
                        if (words.Length >= 2 && (words[0].Equals("") || words[1].Equals("")))
                        {
                            string a = currentValue;
                            string a1 = words[0];
                            string a2 = words[1];
                        }
                        ////////////////////////////////////
                        if (words.Length == 2)                 // word/number/fraction - word/number/fraction
                        {
                            if (isFraction(words[0]))
                            {
                                if (tokenIndex - 1 >= 0) //there is a word before the range
                                {
                                    if (IsNumeric(tokens[tokenIndex - 1]) || isDouble(tokens[tokenIndex - 1])) // case  2 3/8 - number/word/fraction
                                    {
                                        firstTerm.Append(tokens[tokenIndex - 1]);
                                        firstTerm.Append(" ");
                                        firstTerm.Append(words[0]);
                                    }
                                    else
                                    {
                                        firstTerm.Append(words[0]);
                                    }
                                }
                                else  // no word before range
                                {
                                    firstTerm.Append(words[0]);
                                }
                            }
                            else if (IsNumeric(words[0]) || isDouble(words[0]))
                            {
                                firstTerm.Append(getNumberAfterConvertToTerm(words[0]));
                            }
                            else
                            {
                                firstTerm.Append(words[0]);
                            }
                            if (IsNumeric(words[1]) || isDouble(words[1]))
                            {
                                if ((tokenIndex + 1) < tokens.Length) //there is a word atfer the range
                                {
                                    if (isFraction(tokens[tokenIndex + 1])) // case  2 3/8 - number/word/fraction
                                    {
                                        secondTerm.Append(words[1]);
                                        secondTerm.Append(" ");
                                        secondTerm.Append(tokens[tokenIndex + 1]);
                                    }
                                    else
                                    {
                                        secondTerm.Append(words[0]);
                                    }
                                }
                                else  // no word before range
                                {
                                    secondTerm.Append(words[1]);
                                }
                            }
                            else if (IsNumeric(words[1]) || isDouble(words[1]))
                            {
                                firstTerm.Append(getNumberAfterConvertToTerm(words[1]));
                            }
                            else
                            {
                                secondTerm.Append(words[1]);
                            }
                            addNewTerm(document.m_DOCNO.ToString(), firstTerm.ToString(), countPos);  // case firstTerm
                            addNewTerm(document.m_DOCNO.ToString(), secondTerm.ToString(), countPos);  // case secondTerm
                            firstTerm.Append("-");
                            firstTerm.Append(secondTerm);
                            addNewTerm(document.m_DOCNO.ToString(), firstTerm.ToString(), countPos); // case firstTerm-secondTerm
                        }
                        else if (words.Length == 3)
                        {
                            if (isAvalidWord(words[0]) && isAvalidWord(words[1]) && isAvalidWord(words[2]))
                            {
                                firstTerm.Append(words[0]);
                                firstTerm.Append("-");
                                firstTerm.Append(words[1]);
                                firstTerm.Append("-");
                                firstTerm.Append(words[2]);
                                addNewTerm(document.m_DOCNO.ToString(), firstTerm.ToString(), countPos); // case firstTerm-secondTerm-thirdTerm
                            }
                            else
                            {
                                //////////////////////////////////////////////////////////////////////// num- num - num all cases we need this???
                                if (IsNumeric(words[0]) || isDouble(words[0]) || isFraction(words[0]))
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), getNumberAfterConvertToTerm(words[0]), countPos); // case firstTerm
                                }
                                if (IsNumeric(words[1]) || isDouble(words[1]) || isFraction(words[1]))
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), getNumberAfterConvertToTerm(words[1]), countPos); // case secondTerm
                                }
                                if (IsNumeric(words[2]) || isDouble(words[2]) || isFraction(words[2]))
                                {
                                    addNewTerm(document.m_DOCNO.ToString(), getNumberAfterConvertToTerm(words[0]), countPos); // case thirsTerm
                                }
                                //////////////////////////////////////////////////////////////////////////////////should we save num-num-num???
                            }
                        }
                        else
                        {
                            addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                            //////////////////////////////////////////////////////////////////////////////////should we save word-word-word-word???
                        }
                        firstTerm.Clear();
                        secondTerm.Clear();
                    }

                    else if (currentValue.ToLower().Equals("between") && (tokenIndex + 3) < tokens.Length)
                    {
                        if (tokens[tokenIndex + 2].ToLower().Equals("and") || tokens[tokenIndex + 3].ToLower().Equals("and"))
                        {
                            currentValue = tokens[++tokenIndex].ToLower();
                            if (IsNumeric(currentValue) || isDouble(currentValue))
                            {
                                if (isFraction(tokens[tokenIndex + 1]))
                                {
                                    firstTerm.Append(getNumberAfterConvertToTerm(currentValue + " " + tokens[tokenIndex + 1]));
                                    tokenIndex++;
                                }
                                else
                                {
                                    firstTerm.Append(getNumberAfterConvertToTerm(currentValue));
                                }
                            }
                            else if (isFraction(currentValue))
                            {
                                firstTerm.Append(getNumberAfterConvertToTerm(tokens[tokenIndex]));
                            }
                            else  //between word and word
                            {
                                tokenIndex--;
                                continue;
                            }
                            //////////////////////////////////////
                            tokenIndex += 2; // go to "and" term 
                            /////////////////////////////////////
                            currentValue = tokens[tokenIndex].ToLower();
                            if (IsNumeric(currentValue) || isDouble(currentValue))
                            {
                                if (isFraction(tokens[tokenIndex + 1]))
                                {
                                    secondTerm.Append(getNumberAfterConvertToTerm(currentValue + " " + tokens[tokenIndex + 1]));
                                    tokenIndex++;
                                }
                                else
                                {
                                    secondTerm.Append(getNumberAfterConvertToTerm(currentValue));
                                }
                            }
                            else if (isFraction(currentValue))
                            {
                                secondTerm.Append(getNumberAfterConvertToTerm(tokens[tokenIndex]));
                            }
                            else  //between num and word
                            {
                                tokenIndex--;
                                continue;
                            }
                            addNewTerm(document.m_DOCNO.ToString(), firstTerm.ToString(), countPos);  // case firstTerm
                            addNewTerm(document.m_DOCNO.ToString(), secondTerm.ToString(), countPos);  // case secondTerm
                            thirdTerm.Append("Between ");
                            thirdTerm.Append(firstTerm);
                            thirdTerm.Append(" and ");
                            thirdTerm.Append(secondTerm);
                            addNewTerm(document.m_DOCNO.ToString(), thirdTerm.ToString(), countPos); // case Between firstTerm and secondTerm
                            firstTerm.Clear();
                            secondTerm.Clear();
                            thirdTerm.Clear();
                        }
                    }
                    else if (currentValue.All(char.IsLetterOrDigit))  // add case of flight45
                    {
                        addNewTerm(document.m_DOCNO.ToString(), currentValue, countPos);
                    }
                    else
                    {
                        //Console.WriteLine("Not included in the dictionary:     ----(" + currentValue + ")----");
                        continue;
                    }
                }
                //watch.Stop();
                //Console.WriteLine(watch.ElapsedMilliseconds);
                //int a = 1;
            }
        }

    }
}
