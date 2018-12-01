using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    [Serializable]
    public class IndexTerm
    {
        public int df; //number of documents that contain the term
        public int tf; //total frequency in corpus
        public string m_value; //value of the term
        //public int postNum;
        public int lineInPost;
        //public bool updated;

        public IndexTerm(string m_value, int postNum, int lineInPost)
        {
            this.df = 0;
            this.tf = 0;
            //updated = true;
            //initPostNumer();
            this.m_value = m_value;
            //this.postNum = postNum;
            this.lineInPost = lineInPost;
        }

        public void IncreaseTf(int increase)
        {
            this.tf += increase;
        }
        public void IncreaseDf(int increase)
        {
            this.df += increase;
        }

        public StringBuilder PrintTerm()
        {
            //return new StringBuilder(m_value + "#" + "df: " + df + "#" + "tf: " + tf);(#)
            return new StringBuilder(m_value + "(#)" + df + "(#)" + tf);
        }

    }
}
