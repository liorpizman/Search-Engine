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
        private double InnerProduct;
        private double totalScore;
        public MethodScore(double BM25, double innerProduct)
        {
            this.BM25 = BM25;
            this.InnerProduct = innerProduct;
            this.totalScore = (0.75 * BM25) + (0.25 * InnerProduct);
            /// maybe we should normalize it here
        }

        public double GetBM25()
        {
            return this.BM25;
        }

        public double GetInnerProduct()
        {
            return this.InnerProduct;
        }

        public void IncreaseBM(Double bm25)
        {
            BM25 += bm25;
            totalScore += (0.75 * bm25);
        }

        public void IncreaseInnerProduct(Double innerProduct)
        {
            InnerProduct += innerProduct;
            totalScore += (0.25 * innerProduct);
        }

        public double GetTotalScore()
        {
            return totalScore;
        }

    }
}
