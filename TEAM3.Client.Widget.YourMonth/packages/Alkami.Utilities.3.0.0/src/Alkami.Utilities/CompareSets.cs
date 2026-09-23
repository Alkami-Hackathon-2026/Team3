using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace Alkami.Utilities
{
    /// <summary>
    /// Results from the comparison of two sets
    /// </summary>
    /// <typeparam name="T">The type of the items in the sets</typeparam>
    /// <typeparam name="TJoinValue">The type for the lists of non-unique values in each set</typeparam>
    public class CompareSetResult<T, TJoinValue>
    {
        /// <summary>
        /// True if an error has occured during set comparison, false otherwise
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Items in the left set which are not in the right set
        /// </summary>
        public List<T> OnlyInLeft { get; set; }

        /// <summary>
        /// Items in the left set which are not unique
        /// </summary>
        public List<TJoinValue> NonUniqueInLeft { get; set; }

        /// <summary>
        /// Items in the right set which are not in the left set
        /// </summary>
        public List<T> OnlyInRight { get; set; }

        /// <summary>
        /// Items in the right set which are not unique
        /// </summary>
        public List<TJoinValue> NonUniqueInRight { get; set; }

        /// <summary>
        /// A list containing pairs of values which are in both sets
        /// </summary>
        public List<Tuple<T, T>> InBoth { get; set; }
    }

    /// <summary>
    /// Static class for comparing sets
    /// </summary>
    public static class CompareSets
    {
        
        /// <summary>
        /// Populating the lists NonUniqueInLeft and NonUniqueInRight has not been implemented yet
        /// Compare two sets
        /// </summary>
        /// <typeparam name="T">The item type for the sets</typeparam>
        /// <typeparam name="TJoinValue">The type for the lists of non-unique values in each set</typeparam>
        /// <param name="leftSet">The first set to be compared</param>
        /// <param name="rightSet">The second set to be compared</param>
        /// <param name="logger">Current logger</param>
        /// <param name="compareSelector"></param>
        /// <returns>The results of the comparison of the two sets</returns>
        public static CompareSetResult<T, TJoinValue> Compare<T, TJoinValue>(IEnumerable<T> leftSet, IEnumerable<T> rightSet, ILog logger, Func<T, TJoinValue> compareSelector)
        {
            Dictionary<TJoinValue, T> dctLeft;
            Dictionary<TJoinValue, T> dctRight;

            var result = new CompareSetResult<T, TJoinValue>()
            {
                HasError = false,
                OnlyInLeft = new List<T>(),
                NonUniqueInLeft = new List<TJoinValue>(),
                OnlyInRight = new List<T>(),
                NonUniqueInRight = new List<TJoinValue>(),
                InBoth = new List<Tuple<T, T>>(),
            };

            if (leftSet == null)
                leftSet = Enumerable.Empty<T>();
            if (rightSet == null)
                rightSet = Enumerable.Empty<T>();

            try
            {
                dctLeft = leftSet.ToDictionary(compareSelector);
                dctRight = rightSet.ToDictionary(compareSelector);

                result.OnlyInLeft = dctLeft.Keys.Except(dctRight.Keys).Select(x => dctLeft[x]).ToList();
                result.OnlyInRight = dctRight.Keys.Except(dctLeft.Keys).Select(x => dctRight[x]).ToList();
                result.InBoth = dctLeft.Keys.Intersect(dctRight.Keys).Select(x => new Tuple<T, T>(dctLeft[x], dctRight[x])).ToList();

                if (logger.IsTraceEnabled)
                {
                    logger.TraceFormat("CompareSets for {0} - LeftOnly [{1}], RightOnly [{2}], Both [{3}]",
                                       typeof(T).Name,
                                       string.Join(", ", result.OnlyInLeft.Select(x => "{" + compareSelector(x).ToString() + "}")),
                                       string.Join(", ", result.OnlyInRight.Select(x => "{" + compareSelector(x).ToString() + "}")),
                                       string.Join(", ", result.InBoth.Select(x => "{" + compareSelector(x.Item1).ToString() + "}")));
                }
            }
            catch (ArgumentException)
            {
                result.HasError = true;
                result.NonUniqueInLeft = leftSet.ToLookup(compareSelector).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                result.NonUniqueInRight = rightSet.ToLookup(compareSelector).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            }

            return result;
        }
    }
}
