using System;
using System.Linq;
using System.Linq.Expressions;

namespace Alkami.Contracts
{
    /// <summary>
    /// Extension methods for <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Sorts the by the provided <paramref name="keySelector"/> with the ordering determined by <see cref="ISortOrder"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object being sorted.</typeparam>
        /// <typeparam name="V">The type of the field being sorted upon.</typeparam>
        /// <param name="query">The <see cref="IQueryable{T}"/> to sort.</param>
        /// <param name="sorter">The <see cref="ISortOrder"/>.</param>
        /// <param name="keySelector">The expression to select the field to sort upon.</param>
        /// <returns>The sorted <see cref="IOrderedQueryable{T}"/>.</returns>
        public static IOrderedQueryable<T> SortBy<T, V>(this IQueryable<T> query, ISortOrder sorter, Expression<Func<T, V>> keySelector)
        {
            return sorter.Ascending
                ? query.OrderBy(keySelector)
                : query.OrderByDescending(keySelector);
        }

        /// <summary>
        /// Sorts the by the provided <paramref name="keySelector"/> with the ordering determined by <see cref="ISortOrder"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object being sorted.</typeparam>
        /// <typeparam name="V">The type of the field being sorted upon.</typeparam>
        /// <param name="query">The <see cref="IOrderedQueryable{T}"/> to sort.</param>
        /// <param name="sorter">The <see cref="ISortOrder"/>.</param>
        /// <param name="keySelector">The expression to select the field to sort upon.</param>
        /// <returns>The sorted <see cref="IOrderedQueryable{T}"/>.</returns>
        public static IOrderedQueryable<T> SortBy<T, V>(this IOrderedQueryable<T> query, ISortOrder sorter, Expression<Func<T, V>> keySelector)
        {
            return sorter.Ascending
                ? query.ThenBy(keySelector)
                : query.ThenByDescending(keySelector);
        }
    }
}
