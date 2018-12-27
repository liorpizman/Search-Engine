using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents Ranker for each Query
    /// </summary>
    public class Ranker
    {
        /// <summary>
        /// fields of Ranker
        /// </summary>
        private double K1 = 1.2; // in a range of [1.2,2.0]
        private const double K2 = 0; // in a range of [0,1000]
        private const double b = 0.65;  //constant

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
        public double titleFi { get; set; }//frequency of term i in the title

        public double kFirstWordsTotal { get; set; }
        public double termInKfirstWordsTotal { get; set; }

        /// <summary>
        /// Constructor of Ranker
        /// </summary>
        public Ranker()
        {
            Ri = 0;
            Ni = 0;
            N = 0;
            R = 0;
            Fi = 0;
            qFi = 0;
            K1 -= 0.2;
            avgDL = 0;
            dl = 0;

            Tfi = 0;
            idf = 0;

            titleFi = 0;

            kFirstWordsTotal = 30;// K=30 first words
            termInKfirstWordsTotal = 0;
        }

        /// <summary>
        /// method to calculate BM25
        /// </summary>
        /// <returns>BM25 score</returns>
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

        /// <summary>
        /// method to calculate InnerProduct
        /// </summary>
        /// <returns>InnerProduct score</returns>
        public double CalculateInnerProduct()
        {
            Tfi = Fi / dl;
            idf = Math.Log(N / Ni);
            return Tfi * idf;
        }

        /// <summary>
        /// method to calculate Title Rank
        /// </summary>
        /// <returns>Title Rank</returns>
        public double CalculateTitleRank()
        {
            return (titleFi / titleLen);
        }

        /// <summary>
        /// method to calculate K First Words Rank
        /// </summary>
        /// <returns>K First Words Rank</returns>
        public double CalculateKFirstWordsRank()
        {
            return (termInKfirstWordsTotal / kFirstWordsTotal);
        }

        /// <summary>
        /// method to calculate Semantics Rank
        /// </summary>
        /// <returns>Semantics Rank</returns>
        public double CalculateSemanticsRank(double rank, double NormalizeFactor)
        {
            return (rank / NormalizeFactor);
        }

        /// <summary>
        /// method to calculate Entities Rank
        /// </summary>
        /// <returns>Entities Rank</returns>
        public double CalculateEntitiesRank(double rank, double NormalizeFactor)
        {
            return (rank / NormalizeFactor);
        }
    }
}
