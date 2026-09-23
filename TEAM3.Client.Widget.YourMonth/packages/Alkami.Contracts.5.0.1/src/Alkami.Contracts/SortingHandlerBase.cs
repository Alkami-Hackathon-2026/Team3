using System;
using System.Linq;

namespace Alkami.Contracts
{
    /// <summary>
    /// Extended base class for SortingHandler - all sorting handlers will switch to inherit from this class instead of 
    /// the original SortingHandler, and will require to implement the pairing of sorting fields to the corresponding entity properties
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TSortFields">The sort fields enum type</typeparam>
    /// <typeparam name="TSorter">The entity-specific sorter type that encapsulates sorting order</typeparam>
    public abstract class SortingHandlerBase<T, TSortFields, TSorter>
        : SortingHandlerCommonBase<T, TSortFields, TSorter>
        where T : class, new()
        where TSortFields : struct
        where TSorter : ISortOrder<TSortFields>, new()
    {
        /// <summary>
        /// The readonly property that derived classes must implement; It contains associations between sort fields and entity properties
        /// </summary>
        /// <example>
        /// The code below shows the implementation of this property for the CountrySortingHandler (from the Contacts microservice).
        /// The initializer must use the syntax shown below. Do not use expression syntax ( => ...)  but rather property initializer syntax: {get;} = ...
        /// <code>
        ///  protected override MemberAssociations%lt;CountrySortFields, Country&gt; SortingAssociations {get;}
        ///    = new MemberAssociations&lt;CountrySortFields, Country&gt;
        ///    {
        ///        { CountrySortFields.Id, x => x.Id },
        ///        { CountrySortFields.Name, x => x.Name }
        ///    };
        ///  </code>
        /// </example>
        protected abstract MemberAssociations<T, TSorter, TSortFields> MemberAssociations { get; }

        /// <summary>
        /// Refactored method that applies ordering and paging based on the input query on the entities and the sort ordering.
        /// The sorting field-to-property associations are provided by the SortingAssociations property implemented by the derived sorting handlers.
        /// It is marked as sealed so that subclasses cannot override it.
        /// </summary>
        /// <param name="originalQuery">The query to be run on the generic entities</param>
        /// <param name="orderer">The sort order specification</param>
        /// <returns>A queriable collection of entities upon which sorting behavior was applied</returns>
        protected sealed override IQueryable<T> ApplyOrderingAndPagingInternal(IQueryable<T> originalQuery, TSorter orderer)
        {
            if (MemberAssociations == null)
                return originalQuery; //no sorting behavior specified

            IOrderedQueryable<T> orderedQueryable = null;
            orderer
                .OrderByFields
                .ForEach(field =>
                {
                    if (!MemberAssociations.ContainsKey(field))
                        throw new ArgumentOutOfRangeException(nameof(field), field, "Attempting to sort by a field that is not associated with any entity property. You must add the appropriate sorting association under MemberAssociations.");

                    var e = MemberAssociations[field];
                    orderedQueryable = e.SortFunc(orderedQueryable, originalQuery, orderer);
                });

            return orderedQueryable;
        }

    }
}
