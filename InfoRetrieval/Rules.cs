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
            rulesList = new Regex[17];

            //include all numbers in a format: {1-3},***,***.*** or  {1-3},***,***  **/**  Thousand /etc.
            Regex numbersCase1 = new Regex(@"^(\d{1,3},(\d{3},)*\d{3}(\.\d+)?)(\s\d+\/\d+)? *((?i:Thousand)|(?i:thousand)|(?i:Million)|(?i:million)|(?i:Billion)|(?i:billion)|(?i:Trillion)|(?i:trillion))?$");
            //include all numbers in a format: *****.*** or ***  **/**  Thousand /etc.
            Regex numbersCase2 = new Regex(@"^(\d+)(\.\d+)?(\s\d+\/\d+)? *((?i:Thousand)|(?i:thousand)|(?i:Million)|(?i:million)|(?i:Billion)|(?i:billion)|(?i:Trillion)|(?i:trillion))?$");

            //include all percents in a format: {1-3},***,***.*** or  {1-3},***,***  **/**  Percents /etc.
            Regex percentsCase1 = new Regex(@"^(\d{1,3},(\d{3},)*\d{3}(\.\d+)?)(\s\d+\/\d+)? +((?i:Percents)|(?i:percents)|(?i:Percent)|(?i:percent)|(?i:Percentage)|(?i:percentage)|(?i:%))?$");
            //include all percents in a format: *****.*** or ***  **/**  Percents /etc.
            Regex percentsCase2 = new Regex(@"^(\d+)(\.\d+)?(\s\d+\/\d+)? +((?i:Percents)|(?i:percents)|(?i:Percent)|(?i:percent)|(?i:Percentage)|(?i:percentage)|(?i:%))?$");

            //include all prices in a format: {1-3},***,***.*** or  {1-3},***,***  **/**  dollars /etc.
            Regex pricesCase1 = new Regex(@"^\d{1,3}(,\d{3})*(\.\d+)?(\s\d+\/\d+)? *((?i:m)|(?i:bn)|(?i:billion U.S.)|(?i:million U.S.)|(?i:trillion U.S.))? +((?i:dollars)|(?i:Dollars))?$");
            //include all prices in a format: $ {1-3},***,***.*** or  $ {1-3},***,***  **/** 
            Regex pricesCase2 = new Regex(@"^\$\d{1,3}(,\d{3})*(\.\d+)?(\s\d+\/\d+)? *((?i:million)|(?i:billion)|(?i:trillion))?$");

            //	January February March April May June July August September October November December
            //	Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec

            //check in the function that the day in under or equal 31
            //include all dates in a format: */**/**** month
            Regex datesCase1 = new Regex(@"^(\d\d\d\d|\d\d|\d) +((?i:January)|(?i:February)|(?i:March)|(?i:April)|(?i:May)|(?i:June)|(?i:July)|(?i:August)|(?i:September)|(?i:October)|(?i:November)|(?i:December)|
                                        (?i:Jan)|(?i:Feb)|(?i:Mar)|(?i:Apr)|(?i:May)|(?i:Jun)|(?i:Jul)|(?i:Aug)|(?i:Sep)|(?i:Oct)|(?i:Nov)|(?i:Dec))$");
            //include all dates in a format: month */**/****
            Regex datesCase2 = new Regex(@"^((?i:January)|(?i:February)|(?i:March)|(?i:April)|(?i:May)|(?i:June)|(?i:July)|(?i:August)|(?i:September)|(?i:October)|(?i:November)|(?i:December)|
                                        (?i:Jan)|(?i:Feb)|(?i:Mar)|(?i:Apr)|(?i:May)|(?i:Jun)|(?i:Jul)|(?i:Aug)|(?i:Sep)|(?i:Oct)|(?i:Nov)|(?i:Dec)) +(\d\d\d\d|\d\d|\d)$");

            //((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?) ---> add it if we want to accept 4325.32 and  4,325.32
            // (check in dateCase2 and upper)

            //include all ranges in a format: number-number
            Regex rangesCase1 = new Regex(@"^((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)-((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)$");//((\d{1,3},(\d{3},)*\d{3}(\.\d+)?)(\s\d+\/\d+)?-(\d{1,3},(\d{3},)*\d{3}(\.\d+)?)(\s\d+\/\d+)?)|((\d+)(\.\d+)?(\s\d+\/\d+)?-(\d+)(\.\d+)?(\s\d+\/\d+)?)$");
                                                                                                                                                //include all ranges in a format: word-word or word-word-word
            Regex rangesCase2 = new Regex(@"^[A-Za-z]+?-[A-Za-z]+?$|^[A-Za-z]+?-[A-Za-z]+?-[A-Za-z]+?$");
            //include all ranges in a format: number-word or word-number
            Regex rangesCase3 = new Regex(@"^((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)-[A-Za-z]+?$|^[A-Za-z]+?-((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)$");
            //include all ranges in a format: Between number and number
            Regex rangesCase4 = new Regex(@"^(?i:Between) +((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?) +(?i:and) +((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)$");


            //our rule: include all names in a format: Aaaaa Aaaaa  (A - Capital letter , a - lower-case letter)- FirsName LastName
            Regex namesCase = new Regex(@"^[A-Z][A-Za-z']+\s?[A-Z][A-Za-z']+$");

            //our rule: include all measure Units in a format: Number MeasureUnit
            Regex measureLength = new Regex(@"^((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)\s(m|meter|metere|inch|foot|kilometere|kilometer|mile|yard|furlong|micrometere|decametre|mm|cm|dm|km|millimeter|centimeters|centimeter|meters|kilometers|millimeters)$");
            Regex measureMass = new Regex(@"^((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)\s(g|gram|grams|kg|kilogram|kilograms|mg|milligram|milligrams|cg|centigram|centigrams|dg|decigram|decigrams|ton|pound|pounds)$");
            Regex measureTime = new Regex(@"^((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)\s(s|sec|second|seconds|ms|nanosecond|nanoseconds|ns|minute|hour|day|week|month|year|decade|century|millennium|mega-anuum)$");
            Regex measureElectric = new Regex(@"^((\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)?)\s(ampere|volt|Ohm|Siemen|Farad|Coulomb|Henry|Watts|Hertz)$");

            rulesList[0] = numbersCase1;
            rulesList[1] = numbersCase2;

            rulesList[2] = percentsCase1;
            rulesList[3] = percentsCase2;

            rulesList[4] = pricesCase1;
            rulesList[5] = pricesCase2;

            rulesList[6] = datesCase1;
            rulesList[7] = datesCase2;

            rulesList[8] = rangesCase1;
            rulesList[9] = rangesCase2;
            rulesList[10] = rangesCase3;
            rulesList[11] = rangesCase4;

            rulesList[12] = namesCase;

            rulesList[13] = measureLength;
            rulesList[14] = measureMass;
            rulesList[15] = measureTime;
            rulesList[16] = measureElectric;
        }

        /*
        static void Main(string[] args)
        {
            Rules rules = new Rules();

            //test
            if (!rules.rangesCase1.IsMatch("3424 3/5-456243423"))
                Console.WriteLine("fail!");
            else
                Console.WriteLine("sucess!");
            Console.ReadLine();
        }
        */
    }
}
