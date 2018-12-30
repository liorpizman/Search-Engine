using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents the score of current query by six different methods
    /// </summary>
    public class MethodScore
    {
        /// <summary>
        /// fields of MethodScore
        /// </summary>
        private const double factor = 0.33;
        private double BM25;
        private double InnerProduct;
        private double totalScore;
        private double existsInTitle;
        private double kFirstWords;
        private double description;
        private double entities;

        /// <summary>
        /// constructor of MethodScore
        /// </summary>
        /// <param name="BM25">BM25 score</param>
        /// <param name="innerProduct">inner Product score</param>
        /// <param name="inTitle">in Title score</param>
        /// <param name="kFirstWords">k First Words score</param>
        public MethodScore(double BM25, double innerProduct, double inTitle, double kFirstWords)
        {
            this.BM25 = BM25;
            this.InnerProduct = innerProduct;
            this.existsInTitle = inTitle;
            this.kFirstWords = kFirstWords;
            this.description = 0;
            this.entities = 0;
            this.totalScore = (0.5 * this.BM25) + (0 * this.InnerProduct) + (0 * this.existsInTitle) + (0.5 * this.description) + (0 * this.kFirstWords) + (0 * this.entities); ;
        }

        /// <summary>
        /// getter for BM25 score
        /// </summary>
        /// <returns>BM25 score</returns>
        public double GetBM25()
        {
            return this.BM25;
        }

        /// <summary>
        /// getter for Description score
        /// </summary>
        /// <returns>Description score</returns>
        public double GetDescription()
        {
            return this.description;
        }

        /// <summary>
        /// getter for Inner Product score
        /// </summary>
        /// <returns>Inner Product score</returns>
        public double GetInnerProduct()
        {
            return this.InnerProduct;
        }

        /// <summary>
        /// getter for Title Score
        /// </summary>
        /// <returns>Title Score</returns>
        public double GetTitleScore()
        {
            return this.existsInTitle;
        }

        /// <summary>
        /// getter for K first Words Score
        /// </summary>
        /// <returns>K first Words Score</returns>
        public double GetKfirstWordsScore()
        {
            return this.kFirstWords;
        }

        /// <summary>
        /// getter for Entities Score
        /// </summary>
        /// <returns>Entities Score</returns>
        public double GetEntitiesScore()
        {
            return this.entities;
        }

        /// <summary>
        /// increase of Entities Score
        /// </summary>
        public void IncreaseEntitiesScore(double entitiesIncrease)
        {
            this.entities += entitiesIncrease;
        }

        /// <summary>
        /// increase of Description Score
        /// </summary>
        public void IncreaseDescription(double descIncrease)
        {
            this.description += descIncrease;
        }

        /// <summary>
        /// increase of Title Score
        /// </summary>
        public void IncreaseTitleScore(double titleScore)
        {
            this.existsInTitle += titleScore;
        }

        /// <summary>
        /// increase of BM25 Score
        /// </summary>
        public void IncreaseBM(double bm25)
        {
            this.BM25 += bm25;
        }

        /// <summary>
        /// increase of KfirstWords Score
        /// </summary>
        public void IncreaseKfirstWords(double kFirst)
        {
            this.kFirstWords += kFirst;
        }

        /// <summary>
        /// increase of InnerProduct Score
        /// </summary>
        public void IncreaseInnerProduct(double innerProduct)
        {
            this.InnerProduct += innerProduct;
        }

        /// <summary>
        /// increase of Semantic Title Score
        /// </summary>
        public void SetSemanticTitleScore(double titleScore)
        {
            this.existsInTitle += factor * titleScore;
        }

        /// <summary>
        /// increase of Semantic BM25 Score
        /// </summary>
        public void SetSemanticBM(double bm25)
        {
            this.BM25 += factor * bm25;
        }

        /// <summary>
        /// increase of Semantic InnerProduct Score
        /// </summary>
        public void SetSemanticInnerProduct(double innerProduct)
        {
            this.InnerProduct += factor * innerProduct;
        }

        /// <summary>
        /// increase of Semantic K first Words Score
        /// </summary>
        public void SetSemanticKfirstWords(double kFirst)
        {
            this.kFirstWords += factor * kFirst;
        }

        /// <summary>
        /// method to get total score
        /// </summary>
        /// <returns>the total score</returns>
        public double GetTotalScore()
        {
            if (this.description == 0)
            {
                return this.BM25;
            }
            if (this.BM25 == 0)
            {
                return this.description;
            }
            return (0.5 * this.BM25) + (0 * this.InnerProduct) + (0 * this.existsInTitle) + (0.5 * this.description) +
                (0 * this.kFirstWords) + (0 * this.entities);
        }
    }
}
