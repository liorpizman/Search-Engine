using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class TOdelete
    {
        Regex aCase1;
        Regex aCase2;

        public TOdelete()
        {
            aCase1 = new Regex(@"^(\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)? *((?i:m)|(?i:bn)|(?i:billion U.S.)|(?i:million U.S.)|(?i:trillion U.S.))? +((?i:dollars)|(?i:Dollars))?$");
            //include all prices in a format: $ {1-3},***,***.*** or  $ {1-3},***,***  **/** 
            aCase2 = new Regex(@"^\$(\d+|(\d{1,3}(,\d{3})*))(\.\d+)?(\s\d+\/\d+)? *((?i:million)|(?i:billion)|(?i:trillion))?$");
        }

        /*
        static void Main(string[] args)
        {
            TOdelete t = new TOdelete();

            //test
            if (!t.aCase2.IsMatch("1234$123"))
                Console.WriteLine("fail!");
            else
                Console.WriteLine("sucess!");
            Console.ReadLine();
        }
        */
    }
}
