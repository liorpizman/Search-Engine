using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Ranker
    {
        private const double K1 = 1.2; // in a range of [1.2,2.0]
        private const double K2 = 100; // in a range of [0,1000]
        private const double b = 0.75;  //constant

        private int Ri = 0; //amount of relevant documents containing term i (set ti 0 if no relevancy info is know)
        private int Ni = 0;//amount of documents that contains the term qi from the query 
        private int N = 0; //amount of documents in the corpus
        private int R = 0; //amount of relevant documents for this query
        private int Fi = 0; //frequency of term i int the document under consideration
        private int qFi = 0; //frequency of term i in the query
        private double avgDL = 0;//average document length
        private int dl = 0; //document length

        public Ranker()
        {

        }

        public double CalculateBM25()
        {
            double a = 1, b = 1, c = 1;
            a = (Ri + 0.5) / (R - Ri + 0.5);
            a = a / ((Ni - Ri + 0.5) / (N - Ni - R + Ri + 0.5));
            b = (K1 + 1) * Fi;
            b = b / ((K1 * ((1 - b) + b * (dl / avgDL))) + Fi);
            c = (K2 + 1) * qFi;
            c = c / (K2 + qFi);
            return (Math.Log(a) * b * c);
        }

        public double CalculateCosSim()
        {
            return 0;
        }

    }
}
