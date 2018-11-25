using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Program
    {
        static void Main(string[] args)
        {

            string corpusPath = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\corpus";
            string path = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder";
            string pathLab = @"d:\documents\users\pezman\Documents\bbbbbbbbbbbbb\testFolder";
            ReadFile r = new ReadFile(path);
            r.MainRead();
            //Parse parse = new Parse(true, path);
        }
    }
}
