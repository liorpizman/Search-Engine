using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    class IndexDoc
    {
        public int m_mxTF { get; private set; }
        public int m_uniqueCouner { get; private set; }
        public StringBuilder m_City { get; private set; }

        public IndexDoc(int m_mxTF, int m_uniqueCouner, StringBuilder m_City)
        {
            this.m_mxTF = m_mxTF;
            this.m_uniqueCouner = m_uniqueCouner;
            this.m_City = m_City;
        }
    }
}
