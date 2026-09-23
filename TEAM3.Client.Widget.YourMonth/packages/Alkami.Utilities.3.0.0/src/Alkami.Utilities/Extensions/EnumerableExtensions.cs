// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Alkami Technology, Inc.">
//   Copyright 2013 Alkami Technology, Inc.  All rights reserved.
// </copyright>
// <summary>
//   A utility class that provides static methods as shortcuts for handling enumerations
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Alkami.Utilities.Extensions
{
    /// <summary>
    /// A utility class that provides static methods as shortcuts for handling enumerations
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Provides expression based for-each functionality to IEnumerable{T} objects.
        /// </summary>
        /// <typeparam name="T">The generic type of the collection.</typeparam>
        /// <param name="collection">The collection to iterate over.</param>
        /// <param name="action">The action to perform on each element of the collection.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            //if (collection == null)
            //	throw new ArgumentNullException("collection");
            //if (action == null)
            //	throw new ArgumentNullException("action");

            foreach (var c in collection)
            {
                action(c);
            }
        }

        /// <summary>
        /// Returns any duplicates found based on the lamda expression.
        /// </summary>
        /// <typeparam name="TSource">The source</typeparam>
        /// <typeparam name="TKey">The Key</typeparam>
        /// <param name="source">The source enumeration to order.</param>
        /// <param name="keySelector">The key to select on</param>
        /// <returns>An enumeration of duplicate entities</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static IEnumerable<TSource> DuplicatesBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
                return null;
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => !seenKeys.Add(keySelector(element)));
        }

        /// <summary>
        /// Returns distinct objects based on the lamda expression.
        /// </summary>
        /// <typeparam name="TSource">The source</typeparam>
        /// <typeparam name="TKey">The Key</typeparam>
        /// <param name="source">The source enumeration to order.</param>
        /// <param name="keySelector">The key to select on</param>
        /// <returns>An enumeration of distinct entities</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();

            return source.Where(element => knownKeys.Add(keySelector(element)));
        }

        /// <summary>
        /// Returns the sole element from a single-element sequence. If the sequence
        /// has no elements or if it has more than 1 element the return value is
        /// default(T).
        /// </summary>
        /// <typeparam name="T">Element type of the collection.</typeparam>
        /// <param name="items">Collection to enumerate.</param>
        /// <param name="tooManyItems">Set to true upon return if the sequence has too many items.</param>
        /// <returns>
        /// Returns the sole item from the collection or null if the collection has 0 or 
        /// more than 1 items.
        /// </returns>
        public static T TryGetSingleOrDefault<T>(this IEnumerable<T> items, out bool tooManyItems)
        {
            T result = default(T);
            bool foundItem = false;

            foreach (T item in items)
            {
                // If an item has already been found, the sequence contains more than
                // one element. In that case just return default(T).
                if (foundItem)
                {
                    tooManyItems = true;
                    return (default(T));
                }

                result = item;
                foundItem = true;
            }

            // The sequence has 0 or 1 item.
            tooManyItems = false;

            return (result);
        }

        /// <summary>
        /// Determines if the specified collection is null or contains no elements.
        /// </summary>
        /// <typeparam name="T">Collection type.</typeparam>
        /// <param name="items">Collection to check. This parameter can be null.</param>
        /// <returns>
        /// Returns true if the collection is null or if it has no elements.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return !items?.Any<T>() ?? true;
        }

        /// <summary>
        /// Splits a 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            if (chunkSize < 1)
                throw new ArgumentException("The chunk size must be at least one.");

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var storage = new List<T>(chunkSize);
                    storage.Clear();
                    storage.Add(enumerator.Current);

                    for (var j = 0; j < chunkSize - 1 && enumerator.MoveNext(); j++)
                        storage.Add(enumerator.Current);

                    yield return storage;
                }
            }
        }

        /// <summary>
        /// Orders an enumerable by some selected value in the supplied direction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="selector">Produce a value by which to sort the enumerable</param>
        /// <param name="ascending">Whether the enumerable should be in ascending or descending order</param>
        /// <returns></returns>
        public static IEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector, bool ascending)
        {
            return ascending ? enumerable.OrderBy(selector) : enumerable.OrderByDescending(selector);
        }
    }
}
