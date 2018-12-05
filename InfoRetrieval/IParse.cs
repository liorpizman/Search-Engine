using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Interface for creating a Parse which splits the text into terms
    /// </summary>
    interface IParse
    {
        /// <summary>
        ///  method which splits per document in the collection of the files into terms
        /// </summary>
        /// <param name="file">the path of the first file in the collection</param>
        void ParseMasterFile(MasterFile file);
    }
}
