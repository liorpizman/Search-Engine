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
        private double existsInTitle;
        public MethodScore(double BM25, double innerProduct, double inTitle)
        {
            this.BM25 = BM25;
            this.InnerProduct = innerProduct;
            this.existsInTitle = inTitle;
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

        public double GetTitleScore()
        {
            return this.existsInTitle;
        }

        public void IncreaseTitleScore(double titleScore)
        {
            existsInTitle += titleScore;
            totalScore += (0.2 * titleScore);
        }

        public void IncreaseBM(double bm25)
        {
            BM25 += bm25;
            totalScore += (0.65 * bm25);
        }

        public void IncreaseInnerProduct(double innerProduct)
        {
            InnerProduct += innerProduct;
            totalScore += (0.15 * innerProduct);
        }

        public void SetSemanticTitleScore(double titleScore)
        {
            existsInTitle += 0.1 * titleScore;
            totalScore += (0.2 * 0.1 * titleScore);
        }

        public void SetSemanticBM(double bm25)
        {
            BM25 += 0.1 * bm25;
            totalScore += (0.65 * 0.1 * bm25);
        }

        public void SetSemanticInnerProduct(double innerProduct)
        {
            InnerProduct += 0.1 * innerProduct;
            totalScore += (0.15 * 0.1 * innerProduct);
        }

        public double GetTotalScore()
        {
            return totalScore;
        }

    }
}
