using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    public class MethodScore
    {
        private double BM25;
        private double CosSim;
        private double totalScore;
        public MethodScore(double BM25, double CosSim)
        {
            this.BM25 = BM25;
            this.CosSim = CosSim;
            this.totalScore = this.BM25 + this.CosSim;             /// maybe we should normalize it here
        }

        public double GetBM25()
        {
            return BM25;
        }

        public double GetCosSim()
        {
            return CosSim;
        }

        public void IncreaseBM(Double bm25)
        {
            BM25 += bm25;
            totalScore += bm25;
        }

        public void IncreaseCosSim(Double cosSim)
        {
            CosSim += cosSim;
            totalScore += cosSim;
        }

        public double GetTotalScore()
        {
            return totalScore;
        }

    }
}
