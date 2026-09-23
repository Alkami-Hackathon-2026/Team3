using System.Collections.Generic;

namespace Alkami.Contracts
{
    /// <summary>
    /// base interface for controlling paramter types
    /// </summary>
    public interface ISortOrder
    {
        /// <summary>
        /// Indicates the direction of the sort
        /// </summary>
        bool Ascending { get; set; }
    }

    /// <summary>
    /// The data contract used to allow callers to control their sorting fields
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface ISortOrder<TField> : ISortOrder where TField : struct
    {
        /// <summary>
        /// A list of Enums representing the availble fields to sort by
        /// </summary>
        List<TField> OrderByFields { get; set; }
    }
}