using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class Term
    {
        public string m_value;
        public int m_tf;  // num of instances of m_value in current m_DOCNO
        public string m_DOCNO;
        public StringBuilder m_positions; //maybe list
        public int postNum;
        public int lineInPost;

        public Term(string m_value, string docno, int newPOS)
        {
            this.m_value = m_value;
            this.m_tf = 1;
            this.m_positions = new StringBuilder(newPOS);
            //add poisition index add in the constructor for the first step
            this.m_DOCNO = docno;
            InitPostNumer();  //////////////////// check if we need it
            lineInPost = 0;   //////////////////// check if we need it
        }

        public void AddNewIndex(int newPos)
        {
            m_positions.Append(newPos + " ");
            this.m_tf++;
        }

        public StringBuilder WriteDocumentToPostingFileTerm()
        {
            StringBuilder data = new StringBuilder();
            /*
            if (set)
            {
                SetLine(line);
                data.Append(m_value);                      //term value
            }
            */
            //data.Append(m_value);
            data.Append("\t");
            data.Append(m_DOCNO);                      //document num of current term 
            data.Append("\t");
            data.Append(m_tf);                     // tf - term frequency in current doc
            data.Append("\tpositions: ");
            data.Append(m_positions);                  //all the indexes of the current term
            return data;
        }

        public void SetLine(int line)
        {
            lineInPost = line;
        }

        public void InitPostNumer()
        {
            if (m_value == "")
            {
                postNum = 26;
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
                    postNum = 26;
                }
            }

        }
    }
}
