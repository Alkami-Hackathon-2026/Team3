using System;
using System.Linq;
using System.Threading.Tasks;
using Alkami.Exceptions;

namespace Alkami.Contracts
{

    /// <summary>
    /// Common base class for old and new sorting handler base classes.
    /// Once the old SortingHandler base class is deprecated (with true), the content of this class can be moved to the new SortingHandlerBase class
    /// to reduce the number of base classes to a single one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TFields"></typeparam>
    /// <typeparam name="TSortOrderer"></typeparam>    
    public abstract class SortingHandlerCommonBase<T, TFields, TSortOrderer>
        where T : class, new()
        where TFields : struct
        where TSortOrderer : ISortOrder<TFields>, new()
    {
        private const int DefaultMaxResults = 100;
        private const int DefaultPageSize = 1000;

        /// <summary>
        /// The original method to be overridden in the specialized sorting handlers using the explicit sorting behavior specification
        /// </summary>
        /// <param name="originalQuery"></param>
        /// <param name="orderer"></param>
        /// <returns></returns>
        protected abstract IQueryable<T> ApplyOrderingAndPagingInternal(IQueryable<T> originalQuery, TSortOrderer orderer);

        /// <summary>
		/// Sets TotalResults on the provided response and returns an IQueryable that reflects the sorting and paging options on the request.
		/// </summary>
		/// <param name="originalQuery"></param>
		/// <param name="orderer"></param>
		/// <param name="request"></param>
		/// <param name="response"></param>
        /// <param name="totalResultsFetchFunction">A function which will be called to set TotalResults on the response. Please pass () => originalQuery.CountAsync() if your IQueryable is backed by Entity Framework. Otherwise use the appropriate asynchronous call for your backing implementation, or wrap a call to Count() in a Task if no database work is required to get total results.</param>
		/// <exception cref="SortOrderArgumentException"></exception>
        /// <returns></returns>
		public async Task<IQueryable<T>> ApplyOrderingAndPagingAsync<TFilter, TMapper, TSorter>(IQueryable<T> originalQuery, TSortOrderer orderer, BaseGetRequest<TFilter, TMapper, TSorter> request, BaseResponse response, Func<Task<int>> totalResultsFetchFunction)
            where TFilter : IFilter, new()
            where TMapper : IMapping, new()
            where TSorter : ISortOrder, new()
        {
            response.TotalResults = await totalResultsFetchFunction();
            return ProcessOrderingAndPaging(originalQuery, request, response,
                                             orderer, default(TFilter));
        }

        /// <summary>
        /// Sets TotalResults on the provided response and returns an IQueryable that reflects the sorting and paging options on the request.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <typeparam name="TMapper">The type of the mapper.</typeparam>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="totalResultsFetchFunction">A function which will be called to set TotalResults on the response. Please pass () => originalQuery.CountAsync() if your IQueryable is backed by Entity Framework. Otherwise use the appropriate asynchronous call for your backing implementation, or wrap a call to Count() in a Task if no database work is required to get total results.</param>
        /// <returns></returns>
        /// <exception cref="SortOrderArgumentException"></exception>
        public async Task<IQueryable<T>> ApplyOrderingAndPagingAsync<TFilter, TMapper>(IQueryable<T> originalQuery, BaseGetRequest<TFilter, TMapper, TSortOrderer> request, BaseResponse response, Func<Task<int>> totalResultsFetchFunction)
            where TFilter : IFilter, new()
            where TMapper : IMapping, new()
        {
            response.TotalResults = await totalResultsFetchFunction();
            return ProcessOrderingAndPaging(originalQuery, request, response,
                                             request.Sorter, request.Filter);
        }

        private IQueryable<T> ProcessOrderingAndPaging<TFilter>(IQueryable<T> originalQuery,
            BaseRequest request, BaseResponse response,
            TSortOrderer sorter, TFilter filter)
            where TFilter : IFilter, new()
        {
            if (sorter == null || sorter.OrderByFields == null || sorter.OrderByFields.Count < 1)
                throw new SortOrderArgumentException(typeof(TSortOrderer).Name);

            var query = ApplyOrderingAndPagingInternal(originalQuery, sorter);

            if (request.MaxResults == 0)
                request.MaxResults = DefaultMaxResults;

            if (filter != null && (filter.Ids != null) && (filter.Ids.Count > 0))
            {
                // If there is only one, don't do any paging/ordering.
                if (filter.Ids.Count == 1)
                {
                    return query.Take(filter.Ids.Count);
                }

                request.MaxResults = filter.Ids.Count;
            }

            var pageSize = Math.Min(request.MaxResults, DefaultPageSize); // Dont go over 1000...
            query = query.Skip(request.Page * pageSize).Take(pageSize).AsQueryable();
            response.Page = request.Page;
            return query;
        }

    }
}
