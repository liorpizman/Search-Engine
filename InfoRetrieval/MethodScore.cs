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
        private double kFirstWords;
        private double description;
        public MethodScore(double BM25, double innerProduct, double inTitle, double kFirstWords)
        {
            this.BM25 = BM25;
            this.InnerProduct = innerProduct;
            this.existsInTitle = inTitle;
            this.kFirstWords = kFirstWords;
            this.description = 0;
            this.totalScore = (0.5* BM25) + (0 * InnerProduct) + (0 * existsInTitle) + (0.5 * description) + (0* kFirstWords);
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

        public double GetKfirstWordsScore()
        {
            return this.kFirstWords;
        }

        public void IncreaseDescription(double descIncrease)
        {
            this.description += descIncrease;
        }
        public void IncreaseTitleScore(double titleScore)
        {
            this.existsInTitle += titleScore;
            //totalScore += (0.1 * titleScore);
        }

        public void IncreaseBM(double bm25)
        {
            this.BM25 += bm25;
            //totalScore += (0.6 * bm25);
        }

        public void IncreaseKfirstWords(double kFirst)
        {
            this.kFirstWords += kFirst;
            //totalScore += (0.2 * kFirst);
        }

        public void IncreaseInnerProduct(double innerProduct)
        {
            this.InnerProduct += innerProduct;
            //totalScore += (0.1 * innerProduct);
        }

        public void SetSemanticTitleScore(double titleScore)
        {
            this.existsInTitle += 0.1 * titleScore;
            //totalScore += (0.1 * 0.1 * titleScore);
        }

        public void SetSemanticBM(double bm25)
        {
            this.BM25 += 0.1 * bm25;
            //totalScore += (0.8 * 0.1 * bm25);
        }

        public void SetSemanticInnerProduct(double innerProduct)
        {
            this.InnerProduct += 0.1 * innerProduct;
            //totalScore += (0.1 * 0.1 * innerProduct);
        }

        public void SetSemanticKfirstWords(double kFirst)
        {
            this.kFirstWords += 0.1 * kFirst;
            //totalScore += (0.1 * 0.1 * kFirst);
        }

        public double GetTotalScore()
        {
            return (0.5 * this.BM25) + (0 * this.InnerProduct) + (0 * this.existsInTitle) + (0.5 * this.description) + (0 * this.kFirstWords);
        }

    }
}
