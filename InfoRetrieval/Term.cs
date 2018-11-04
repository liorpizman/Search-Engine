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
        public int m_amount;
        public string m_DOCNO;
        public ArrayList m_positions;

        public Term(string m_value, string docno)
        {
            this.m_value = m_value;
            this.m_amount = 0;
            this.m_positions = new ArrayList();
            this.m_DOCNO = docno;
        }

        public void increaseAmount()
        {
            this.m_amount++;
        }
    }
}
