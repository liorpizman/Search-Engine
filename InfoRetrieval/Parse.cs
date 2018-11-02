using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Parse
    {
        public Rules rules;


        public Parse()
        {
            rules = new Rules();
        }

        public void parseDocuments(bool doStem, Dictionary<string, masterFile> m_files)
        {
            foreach (masterFile file in m_files.Values)
            {
                foreach (Document document in file.m_documents.Values)
                {
                    foreach (Regex regex in rules.rulesList)
                    {
                        foreach (Match match in regex.Matches(document.m_TEXT))
                        {
                            if (regex == rules.rulesList[0])         //numbersCase1
                            {

                            }
                            else if (regex == rules.rulesList[1])  //numbersCase2
                            {

                            }
                            else if (regex == rules.rulesList[2])  //percentsCase1
                            {

                            }
                            else if (regex == rules.rulesList[3])  //percentsCase2
                            {

                            }
                            else if (regex == rules.rulesList[4])  //pricesCase1
                            {

                            }
                            else if (regex == rules.rulesList[5])   //pricesCase2
                            {

                            }
                            else if (regex == rules.rulesList[6])   //datesCase1
                            {

                            }
                            else if (regex == rules.rulesList[7])   //datesCase2
                            {

                            }
                            else if (regex == rules.rulesList[8])   //rangesCase1
                            {

                            }
                            else if (regex == rules.rulesList[9])   //rangesCase2
                            {

                            }
                            else if (regex == rules.rulesList[10])  //rangesCase3
                            {

                            }
                            else if (regex == rules.rulesList[11])  //rangesCase4
                            {

                            }
                            else if (regex == rules.rulesList[12])  //namesCase
                            {

                            }
                            else if (regex == rules.rulesList[13])  //measureLength
                            {

                            }
                            else if (regex == rules.rulesList[14])   //measureMass
                            {

                            }
                            else if (regex == rules.rulesList[15])   //measureTime
                            {

                            }
                            else if (regex == rules.rulesList[16])  //measureElectric
                            {

                            }
                        }
                    }
                }
            }
        }





    }
}
