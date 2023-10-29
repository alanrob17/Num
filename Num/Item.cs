// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Item.cs" company="Software Inc">
//   A.Robson
// </copyright>
// <summary>
//   Defines the Item type.
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
    /// The item.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the change name.
        /// </summary>
        public string ChangeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether changed.
        /// </summary>
        public bool Changed { get; set; }              
    }
}
