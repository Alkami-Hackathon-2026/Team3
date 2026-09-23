using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Alkami.Contracts
{
    /// <summary>
    /// A wrapper around the member expression that dictates the sorting behavior for a given entity T.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TSorter">The entity-bound sorting specification type</typeparam>
    public class SortFuncWrapper<T, TSorter>
        where T : class, new()
        where TSorter : ISortOrder, new()
    {
        private SortFuncWrapper(Func<IOrderedQueryable<T>, IQueryable<T>, TSorter, IOrderedQueryable<T>> f)
        {
            SortFunc = f;
        }

        /// <summary>
        /// Factory pattern - this class is responsible for producing instances of itself as it knows how to build the wrapped function from the input expression
        /// </summary>
        /// <typeparam name="TKey">The return type of the member expression it wraps</typeparam>
        /// <param name="e">The member expression it must wrap so that it can be used for sorting (specifically, the sorting key selector expression)</param>
        /// <returns>An instance of this class with the specific sort function build as needed, ready to be used/invoked.</returns>
        internal static SortFuncWrapper<T, TSorter> Create<TKey>(Expression<Func<T, TKey>> e)
            => new SortFuncWrapper<T, TSorter>(FSort(e));

        /// <summary>
        /// Returns the wrapped sort function to be applied to a queryable collection of entities.
        /// </summary>
        internal Func<IOrderedQueryable<T>, IQueryable<T>, TSorter, IOrderedQueryable<T>> SortFunc { get; private set; }

        private static Func<IOrderedQueryable<T>, IQueryable<T>, TSorter, IOrderedQueryable<T>> FSort<TKey>(Expression<Func<T, TKey>> e) =>
                (ordQ, orgQ, s) => ordQ == null ? orgQ.SortBy(s, e) : ordQ.SortBy(s, e);
        //DO NOT REFACTOR the above expression to: QueryableExtensions.SortBy(ordQ ?? orgQ, s, e); //THIS WON"T WORK
    }

    /// <summary>
    /// A concurrent dictionary that associates sort fields to entity member expressions that define sorting behavior for a given entity
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TSorter">The entity-bound sorting specification type</typeparam>
    /// <typeparam name="TSortFields">The sorting fields enumeration type</typeparam>
    public class MemberAssociations<T, TSorter, TSortFields> : ConcurrentDictionary<TSortFields, SortFuncWrapper<T, TSorter>>
         where T : class, new()
        where TSortFields : struct
        where TSorter : ISortOrder, new()
    {
        /// <summary>
        /// Enables dictionary initialization using generic values for the member expression type
        /// </summary>
        /// <typeparam name="TKey">The member expression's return type (used implicitly upon initialization). This is not a dictionary TKey!</typeparam>
        /// <param name="f">The sort field (key)</param>
        /// <param name="e">The sort member expression (value)</param>
        public void Add<TKey>(TSortFields f, Expression<Func<T, TKey>> e) =>
            TryAdd(f, SortFuncWrapper<T, TSorter>.Create(e));

    }
}
