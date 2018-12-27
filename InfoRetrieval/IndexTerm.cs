using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Class which represents the data of a term for indexing
    ////// </summary>
    [Serializable]
    public class IndexTerm
    {
        /// <summary>
        /// fiels of IndexTerm
        /// </summary>
        public int df { get; private set; }//number of documents that contain the term
        public int tfc { get; private set; } //total frequency in corpus
        public string m_value { get; private set; } //value of the term
        public int postNum { get; private set; }
        public int lineInPost { get; set; }

        /// <summary>
        /// constructor of IndexTerm
        /// </summary>
        /// <param name="m_value">the value of the term</param>
        /// <param name="postNum">the suitable post number of the current term</param>
        /// <param name="lineInPost">the line of the term in the posting file</param>
        public IndexTerm(string m_value, int postNum, int lineInPost)
        {
            this.df = 0;
            this.tfc = 0;
            this.m_value = m_value;
            this.postNum = postNum;
            this.lineInPost = lineInPost;
        }

        /// <summary>
        /// method for increase TF of a term
        /// </summary>
        /// <param name="increase">number of instances that should be add</param>
        public void IncreaseTfc(int increase)
        {
            this.tfc += increase;
        }

        /// <summary>
        /// method for increase DF of a term
        /// </summary>
        /// <param name="increase">number of instances that should be add</param>
        public void IncreaseDf(int increase)
        {
            this.df += increase;
        }

        /// <summary>
        ///  method for writing to index file
        /// </summary>
        /// <returns>stringbuilder for writing to index file</returns>
        public StringBuilder PrintTerm()
        {
            return new StringBuilder(m_value + "(#)" + "df:" + df + "(#)" + "tfc:" + tfc + "(#)" + "PN:" + postNum + "(#)" + "LN:" + lineInPost);
        }

    }
}
