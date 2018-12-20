using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    public class Ranker
    {
        private const double K1 = 1.5; // in a range of [1.2,2.0]
        private const double K2 = 100; // in a range of [0,1000]
        private const double b = 0.75;  //constant

        public double Ri { get; set; } //amount of relevant documents containing term i (set 0 if no relevancy info is known)
        public double Ni { get; set; }  //amount of documents that contains the term qi (from the query) 
        public double N { get; set; }  //amount of documents in the corpus
        public double R { get; set; }  //amount of relevant documents for this query (in the corpus) , (set 0 if no relevancy info is know)
        public double Fi { get; set; }  //frequency of term i int the document 
        public double qFi { get; set; }  //frequency of term i in the query
        public double avgDL { get; set; }  //average document length (in the corpus)
        public double dl { get; set; }  //document length


        public double Tfi { get; set; } // frequency of term i in document (normallized by length of the document)
        public double idf { get; set; } // inverse document frequency (how much the term specializes the document)

        public double titleLen { get; set; } // amount of terms in the tile
        public double tileFi { get; set; }//frequency of term i in the title

        public Ranker()
        {
            Ri = 0;
            Ni = 0;
            N = 0;
            R = 0;
            Fi = 0;
            qFi = 0;
            avgDL = 0;
            dl = 0;

            Tfi = 0;
            idf = 0;
        }

        public double CalculateBM25()
        {
            double a = 1, x = 1, c = 1;
            a = (Ri + 0.5) / (R - Ri + 0.5);
            a = a / ((Ni - Ri + 0.5) / (N - Ni - R + Ri + 0.5));
            x = (K1 + 1) * Fi;
            x = x / ((K1 * ((1 - b) + b * (dl / avgDL))) + Fi);
            c = (K2 + 1) * qFi;
            c = c / (K2 + qFi);
            return (Math.Log(a) * x * c);
        }

        public double CalculateInnerProduct()
        {
            Tfi = Fi / dl;
            idf = Math.Log(N / Ni);
            return Tfi * idf;
        }

        public double CalculateTitleRank()
        {
            return (tileFi / titleLen);
        }

    }
}
