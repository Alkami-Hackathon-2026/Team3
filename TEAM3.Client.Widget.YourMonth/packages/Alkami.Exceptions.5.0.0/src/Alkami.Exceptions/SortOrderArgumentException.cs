using System;

namespace Alkami.Exceptions
{
    /// <summary>
    /// The <see cref="SortOrderArgumentException"/> indicates that an invalid sort order was supplied.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class SortOrderArgumentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SortOrderArgumentException"/> class.
        /// </summary>
        /// <param name="sortOrderType">Type of the sort order.</param>
        public SortOrderArgumentException(string sortOrderType)
            : base(string.Format("The provided SortOrder of type, {0}, was either null or had no fields to sort by.", sortOrderType ?? "<Null>"))
        {
        }
    }
}