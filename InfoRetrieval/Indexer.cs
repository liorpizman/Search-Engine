using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Indexer
    {

        public Indexer() { }


        // fileName = "fileNameToCreate.txt" (including .txt)
        // mydocpath = @"C:\Users\Lior\Desktop\current semester\Information retrieval\Project\moodle data\testFolder"; // change to path from GUI
        // dataToFile = "add here the data structure!"
        //
        public void createTxtFile(string fileName, string mydocpath, string dataToFile)
        {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(mydocpath, fileName)))
            {
                outputFile.WriteLine(dataToFile);
            }
        }




    }
}
