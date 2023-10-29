// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArgList.cs" company="Software Inc.">
//   A.Robson
// </copyright>
// <summary>
//   The argument list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Num
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The argument list.
    /// </summary>
    public class ArgList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgList"/> class.
        /// </summary>
        /// <param name="subFolder">The sub folder.</param>
        /// <param name="changeFileName">Change file name.</param>
        /// <param name="removeBrackets">Remove brackets and content from music files.</param>
        public ArgList(bool subFolder, bool changeFileName, bool removeBrackets)
        {
            this.SubFolder = subFolder;
            this.ChangeFileName = changeFileName;
            this.RemoveBrackets = removeBrackets;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to search sub folders.
        /// </summary>
        public bool SubFolder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to change file names.
        /// </summary>
        public bool ChangeFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove brackets from music files.
        /// </summary>
        public bool RemoveBrackets { get; set; }
    }
}
