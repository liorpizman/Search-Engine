using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class IndexTerm
    {
        public int df; //number of documents that contain the term
        public int tf; //total frequency in corpus
        public string m_value; //value of the term
        public int postNum;
        public int lineInPost;
        //public bool updated;

        public IndexTerm(string m_value, int postNum, int lineInPost)
        {
            this.df = 0;
            this.tf = 0;
            //updated = true;
            //initPostNumer();
            this.m_value = m_value;
            this.postNum = postNum;
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
            return new StringBuilder(m_value + "\t" + "df: " + df + "\t" + "tf: " + tf);
        }

        /*
        public string PrintTermString()
        {
            string data = "";
            data = m_value + "\tdf: " + df + "\ttf: " + tf;
            return data;
        }
        */



        public void InitPostNumer()
        {
            if (m_value == "")
            {
                postNum = 0;
            }
            else
            {
                char c = m_value.ToLower()[0];
                if (c == 'a')
                    postNum = 0;
                else if (c == 'b')
                    postNum = 1;
                else if (c == 'c')
                    postNum = 2;
                else if (c == 'd')
                    postNum = 3;
                else if (c == 'e')
                    postNum = 4;
                else if (c == 'f')
                    postNum = 5;
                else if (c == 'g')
                    postNum = 6;
                else if (c == 'h')
                    postNum = 7;
                else if (c == 'i')
                    postNum = 8;
                else if (c == 'j')
                    postNum = 9;
                else if (c == 'k')
                    postNum = 10;
                else if (c == 'l')
                    postNum = 11;
                else if (c == 'm')
                    postNum = 12;
                else if (c == 'n')
                    postNum = 13;
                else if (c == 'o')
                    postNum = 14;
                else if (c == 'p')
                    postNum = 15;
                else if (c == 'q')
                    postNum = 16;
                else if (c == 'r')
                    postNum = 17;
                else if (c == 's')
                    postNum = 18;
                else if (c == 't')
                    postNum = 19;
                else if (c == 'u')
                    postNum = 20;
                else if (c == 'v')
                    postNum = 21;
                else if (c == 'w')
                    postNum = 22;
                else if (c == 'x')
                    postNum = 23;
                else if (c == 'y')
                    postNum = 24;
                else if (c == 'z')
                    postNum = 25;
                else
                {
                    postNum = 0;
                }
            }

        }
    }
}
