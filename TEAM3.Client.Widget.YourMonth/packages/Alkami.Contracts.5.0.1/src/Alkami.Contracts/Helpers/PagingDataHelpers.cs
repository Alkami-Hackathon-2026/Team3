using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Alkami.Contracts
{
    public static class PagingDataHelpers
    {
        /// <summary>
        /// Apply the Paging parameters base on <see cref="BaseRequest"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="query">Query for object</param>
        /// <param name="orderBy">Order By function</param>
        /// <param name="request">The <see cref="BaseRequest"/> to get Page and MaxResults</param>
        /// <param name="response">The <see cref="BaseResponse"/></param>
        /// <param name="totalResultsFetchFunction">The function to get the TotalResults</param>
        /// <returns>Returns the query with methods</returns>
        public static async Task<IQueryable<T>> ApplyPagingAsync<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderBy, BaseRequest request, BaseResponse response, Func<Task<int>> totalResultsFetchFunction)
        {
            response.TotalResults = await totalResultsFetchFunction();
            if (request.MaxResults == 0)
            {
                request.MaxResults = 100;
            }

            int num = Math.Min(request.MaxResults, 1000); // Dont go over 1000...
            query = query.OrderBy(orderBy).Skip(request.Page * num).Take(num)
                .AsQueryable();
            return query;
        }

        /// <summary>
        /// Apply the Paging parameters base on <see cref="BaseGetRequest{TFilter, TMapper, TSorter}"/> 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TFilter"></typeparam>
        /// <typeparam name="TMapper"></typeparam>
        /// <typeparam name="TSorter"></typeparam>
        /// <param name="query">Query for object</param>
        /// <param name="orderBy">Order By function</param>
        /// <param name="request">The <see cref="BaseGetRequest{TFilter, TMapper, TSorter}"/> to get Page and MaxResults</param>
        /// <param name="response">The <see cref="BaseResponse"/></param>
        /// <param name="totalResultsFetchFunction">The function to get the TotalResults</param>
        /// <returns>Returns the query with methods</returns>
        public static async Task<IQueryable<T>> ApplyPagingAsync<T, TKey, TFilter, TMapper, TSorter>(this IQueryable<T> query, Expression<Func<T, TKey>> orderBy, BaseGetRequest<TFilter, TMapper, TSorter> request, BaseResponse response, Func<Task<int>> totalResultsFetchFunction) where TFilter : IFilter, new() where TMapper : IMapping, new() where TSorter : ISortOrder, new()
        {
            response.TotalResults = await totalResultsFetchFunction();
            if (request.MaxResults == 0)
            {
                request.MaxResults = 100;
            }

            if (request.Filter != null && request.Filter.Ids != null && request.Filter.Ids.Count > 0)
            {
                if (request.Filter.Ids.Count == 1)
                {
                    return query.Take(request.Filter.Ids.Count);
                }

                request.MaxResults = request.Filter.Ids.Count;
            }

            int num = Math.Min(request.MaxResults, 1000); // Dont go over 1000...
            query = query.OrderBy(orderBy).Skip(request.Page * num).Take(num)
                .AsQueryable();
            return query;
        }
    }
}
