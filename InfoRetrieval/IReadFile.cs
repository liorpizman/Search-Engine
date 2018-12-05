using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoRetrieval
{
    /// <summary>
    /// Interface for creating a Reader which reads all documents in the corpus
    /// </summary>
    interface IReadFile
    {
        /// <summary>
        /// method which splits the corpus to chunks
        /// </summary>
        void InitList();
        /// <summary>
        /// method which reads all documents in a chunck
        /// </summary>
        /// <param name="i">the id of the chunck</param>
        /// <returns>the collection of files</returns>
        MasterFile ReadChunk(int i);
    }
}
