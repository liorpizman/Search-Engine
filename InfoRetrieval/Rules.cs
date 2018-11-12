using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Rules
    {
        public Regex[] rulesList;
        //43.45Percentage does not work!!! 
        // change the words - case sensetive issue

        public Rules()
        {
            rulesList = new Regex[16];
            //Regex numbersCase1 = new Regex(@"^(\d{1,3},(\d{3},)*\d{3}(\.\d+)?)(\s\d+\/\d+)? *((?i:Thousand)|(?i:thousand)|(?i:Million)|(?i:million)|(?i:Billion)|(?i:billion)|(?i:Trillion)|(?i:trillion))?$");
            // Regex percentsCase1 = new Regex(@"^(\d{1,3},(\d{3},)*\d{3}(\.\d+)?)(\s\d+\/\d+)? +((?i:Percents)|(?i:percents)|(?i:Percent)|(?i:percent)|(?i:Percentage)|(?i:percentage)|(?i:%))?$");

            //include all numbers in a format: *****.*** or ***  **/**  Thousand /etc. and {1-3},***,***.*** or  {1-3},***,***  **/**  Thousand /etc.
            Regex numbersCase = new Regex(@"(\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)? *((?i:Thousand)|(?i:thousand)|(?i:Million)|(?i:million)|(?i:Billion)|(?i:billion)|(?i:Trillion)|(?i:trillion))?");

            //include all percents in a format: *****.*** or ***  **/**  Percents /etc. and {1-3},***,***.*** or  {1-3},***,***  **/**  Percents /etc.
            Regex percentsCase = new Regex(@"(\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)? +((?i:Percents)|(?i:percents)|(?i:Percent)|(?i:percent)|(?i:Percentage)|(?i:percentage)|(?i:%)\s)");

            //include all prices in a format: {1-3},***,***.*** or  {1-3},***,***  **/**  dollars /etc.
            Regex pricesCase1 = new Regex(@"(\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)? *((?i:m)|(?i:bn)|(?i:billion U.S.)|(?i:million U.S.)|(?i:trillion U.S.)\s)? +((?i:dollars)|(?i:Dollars)\s)");
            //include all prices in a format: $ {1-3},***,***.*** or  $ {1-3},***,***  **/** 
            Regex pricesCase2 = new Regex(@"\$(\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)? *((?i:million)|(?i:billion)|(?i:trillion)\s)?");

            //	January February March April May June July August September October November December
            //	Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec

            //check in the function that the day in under or equal 31
            //include all dates in a format: */**/**** month
            Regex datesCase1 = new Regex(@"(\d\d\d\d|\d\d|\d) +((?i:January)|(?i:February)|(?i:March)|(?i:April)|(?i:May)|(?i:June)|(?i:July)|(?i:August)|(?i:September)|(?i:October)|(?i:November)|(?i:December)|
                                        (?i:Jan)|(?i:Feb)|(?i:Mar)|(?i:Apr)|(?i:May)|(?i:Jun)|(?i:Jul)|(?i:Aug)|(?i:Sep)|(?i:Oct)|(?i:Nov)|(?i:Dec)|(?i:JANUARY)|(?i:FEBRUARY)|(?i:MARCH)|(?i:APRIL)|(?i:MAY)|
                                        (?i:JUNE)|(?i:JULY)|(?i:AUGUST)|(?i:SEPTEMBER)|(?i:OCTOBER)|(?i:NOVEMBER)|(?i:DECEMBER)\s)");
            //include all dates in a format: month */**/****
            Regex datesCase2 = new Regex(@"((?i:January)|(?i:February)|(?i:March)|(?i:April)|(?i:May)|(?i:June)|(?i:July)|(?i:August)|(?i:September)|(?i:October)|(?i:November)|(?i:December)|
                                        (?i:Jan)|(?i:Feb)|(?i:Mar)|(?i:Apr)|(?i:May)|(?i:Jun)|(?i:Jul)|(?i:Aug)|(?i:Sep)|(?i:Oct)|(?i:Nov)|(?i:Dec)|(?i:JANUARY)|(?i:FEBRUARY)|(?i:MARCH)|
                                        (?i:APRIL)|(?i:MAY)|(?i:JUNE)|(?i:JULY)|(?i:AUGUST)|(?i:SEPTEMBER)|(?i:OCTOBER)|(?i:NOVEMBER)|(?i:DECEMBER)\s) +(\d\d\d\d|\d\d|\d)");

            //((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?) ---> add it if we want to accept 4325.32 and  4,325.32
            // (check in dateCase2 and upper)

            //include all ranges in a format: number-number
            Regex rangesCase1 = new Regex(@"((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)-((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)");
            //((\d{1,3},(\d{3},)*\d{3}(\.\d+)?)(\s\d+\/\d+)?-(\d{1,3},(\d{3},)*\d{3}(\.\d+)?)(\s\d+\/\d+)?)|((\d+)(\.\d+)?(\s\d+\/\d+)?-(\d+)(\.\d+)?(\s\d+\/\d+)?)$");
            //include all ranges in a format: word-word or word-word-word
            //Regex rangesCase2 = new Regex(@"(\s)([A-Za-z]+-[A-Za-z]+-[A-Za-z]+)(\s)|(\s)([A-Za-z]+-[A-Za-z]+)(\s)");
            Regex rangesCase2 = new Regex(@"([A-Za-z]+-[A-Za-z]+-[A-Za-z]+)|([A-Za-z]+-[A-Za-z]+)");
            //include all ranges in a format: number-word or word-number
            //Regex rangesCase3 = new Regex(@"(\s)((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)-[A-Za-z]+?(\s)|(\s)[A-Za-z]+?-((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)(\s)");
            Regex rangesCase3 = new Regex(@"((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)-[A-Za-z]+?|[A-Za-z]+?-((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)");
            //include all ranges in a format: Between number and number
            Regex rangesCase4 = new Regex(@"(?i:Between) +((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?) +(?i:and) +((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)");


            //our rule: include all names in a format: Aaaaa Aaaaa  (A - Capital letter , a - lower-case letter)- FirstName LastName
            Regex namesCase = new Regex(@"[A-Z][A-Za-z']+\s?[A-Z][A-Za-z']+$");

            //our rule: include all measure Units in a format: Number MeasureUnit
            Regex measureLength = new Regex(@"((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)\s((?i:m)|(?i:meter)|(?i:metere)|(?i:inch)|(?i:foot)|(?i:kilometere)|(?i:kilometer)|(?i:mile)|(?i:yard)|(?i:furlong)|(?i:micrometere)|(?i:decametre)|(?i:mm)|(?i:cm)|(?i:dm)|(?i:km)|(?i:millimeter)|(?i:centimeters)|(?i:centimeter)|(?i:meters)|(?i:kilometers)|(?i:millimeters))\s");
            Regex measureMass = new Regex(@"((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)\s((?i:g)|(?i:gram)|(?i:grams)|(?i:kg)|(?i:kilogram)|(?i:kilograms)|(?i:mg)|(?i:milligram)|(?i:milligrams)|(?i:cg)|(?i:centigram)|(?i:centigrams)|(?i:dg)|(?i:decigram)|(?i:decigrams)|(?i:ton)|(?i:pound)|(?i:pounds))\s");
            Regex measureTime = new Regex(@"((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)\s((?i:s)|(?i:sec)|(?i:second)|(?i:seconds)|(?i:ms)|(?i:nanosecond)|(?i:nanoseconds)|(?i:ns)|(?i:minute)|(?i:hour)|(?i:day)|(?i:week)|(?i:month)|(?i:year)|(?i:decade)|(?i:century)|(?i:millennium)|(?i:mega-anuum)|(?i:minutes))\s");
            Regex measureElectric = new Regex(@"((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)\s((?i:ampere)|(?i:volt)|(?i:Ohm)|(?i:Siemen)|(?i:Farad)|(?i:Coulomb)|(?i:Henry)|(?i:Watts)|(?i:Hertz))\s");

            Regex words = new Regex(@"[A-Za-z]+");

            rulesList[0] = numbersCase;

            rulesList[1] = percentsCase;

            rulesList[2] = pricesCase1;
            rulesList[3] = pricesCase2;

            rulesList[4] = datesCase1;
            rulesList[5] = datesCase2;

            rulesList[6] = rangesCase1;
            rulesList[7] = rangesCase2;
            rulesList[8] = rangesCase3;
            rulesList[9] = rangesCase4;

            rulesList[10] = namesCase;

            rulesList[11] = measureLength;
            rulesList[12] = measureMass;
            rulesList[13] = measureTime;
            rulesList[14] = measureElectric;

            rulesList[15] = words;
        }

        /*
        static void Main(string[] args)
        {
            Rules rules = new Rules();

            //test
            MatchCollection[] arr = new MatchCollection[16];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = rules.rulesList[i].Matches("4 minutes be or a not to be 12 percents Between 23 and 234 ert swd4 sdf3 sdfd sdfgs");
                if (arr[i].Count > 0)
                {
                    Console.Write(i + " --- count:       "+ arr[i].Count);
                    for (int j = 0; j < arr[i].Count; j++)
                    {
                        Console.Write(" --- value: " + arr[i][j].Value);
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(i + " --- count:       " + arr[i].Count);
                }
            }
            
           // Regex words = new Regex(@"[A-Za-z]+?");
           // if (!words.IsMatch("ghf gfh fgh g word1-word2 gd word1-word2-word3 fgh word1-word2-word3-word4 fgh"))
           //     Console.WriteLine("fail!");
           // else
           //     Console.WriteLine("sucess!");
            
            Console.ReadLine();
      
    }
    */
    }

}
